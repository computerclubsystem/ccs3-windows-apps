using Ccs3ClientAppWindowsService.Messages;
using Ccs3ClientAppWindowsService.Messages.Device;
using Ccs3ClientAppWindowsService.Messages.Device.Declarations;
using Ccs3ClientAppWindowsService.Messages.LocalClient;
using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccs3ClientAppWindowsService;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private StartProcessAsCurrentUserResult? _startProcessAsCurrentUserResult;
    private WebSocketConnector _serverWsConnector = new();
    private readonly string _serviceName = "Ccs3ClientAppWindowsService";
    private readonly CertificateHelper _certificateHelper = new();
    private readonly RegistryHelper _registryHelper = new();
    private readonly RestartWindowsHelper _restartWindowsHelper = new();
    private bool _disableClientAppProcessStartLogs = false;
    private JsonSerializerOptions _jsonSerializerOptions;
    private ServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody> _deviceConfigNotificationMsg;
    private ServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody> _deviceCurrentStatusNotificationMsg;
    private System.Threading.Timer _timer;
    private readonly WorkerState _state = new WorkerState();
    private readonly WebSocketServerManager _wsServerManager;

    public Worker(
        ILogger<Worker> logger,
        WebSocketServerManager wsServerManager
    ) {
        _logger = logger;
        _wsServerManager = wsServerManager;
        _timer = new Timer(TimerCallbackFunction, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void SetAppStoppingCancellationToken(CancellationToken token) {
        _state.AppCancellationToken = token;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        PrepareState();
        //_state.CancellationToken = stoppingToken;
        _state.CancellationToken = _state.AppCancellationToken;
        _state.CancellationToken.Register(() => {
            _logger.LogDebug(new EventId(101), "CancellationToken canceled");
        });
        _state.AppCancellationToken.Register(() => {
            _logger.LogDebug(new EventId(101), "AppCancellationToken canceled");
        });
        stoppingToken.Register(() => {
            _logger.LogDebug(new EventId(101), "stoppingToken canceled");
        });
        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("CCS3 worker started at: {time}", GetNow());
        }
        _jsonSerializerOptions = CreateJsonSerializerOptions();
        _disableClientAppProcessStartLogs = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CAWS_DEBUG_DISABLE_CLIENT_APP_PROCESS_START_LOGS) == "true";
        StartWebSocketConnector(stoppingToken);
        while (!stoppingToken.IsCancellationRequested) {
            try {
                StartClientAppIfNotStarted();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error on StartClientAppIfNotStarted");
            }
            if (!stoppingToken.IsCancellationRequested) {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        _logger.LogInformation(new EventId(500), "Cancellation requested");
        // Give 5 seconds to stop all client processes
        var startTime = GetNow();
        _logger.LogInformation(new EventId(7, "Killing client instances"), "Killing client instances");
        while (true) {
            bool instanceStopped = KillClientAppProcess();
            if (!instanceStopped) {
                // Nothing was stopped - probably it does not run - exit
                break;
            }
            // Client process instance stopped - check if we are off the time limit
            var diff = GetNow() - startTime;
            if (diff > TimeSpan.FromSeconds(5)) {
                // 5 seconds passed - stop retries
                break;
            }
            // If the timeout has not passed and an instance was killed, will try again
            // to kill other instances if any
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogWarning("StopAsync");
        return base.StopAsync(cancellationToken);
    }

    private bool KillClientAppProcess() {
        int processId;
        if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.ProcInfo.dwProcessId != 0) {
            processId = (int)_startProcessAsCurrentUserResult.ProcInfo.dwProcessId;
        } else {
            processId = 0;
        }
        try {
            Process? proc = null;
            if (processId != 0) {
                _logger.LogWarning("Getting process with id {0}", processId);
                proc = Process.GetProcessById(processId);
            }
            if (proc == null) {
                _logger.LogWarning("Process with id {0} not found.", processId);
                string clientExecutablePath = GetClientExecutablePath();
                List<string> allProcessPaths = GetAllProcessesExecutablePaths();
                _logger.LogDebug("All processes paths: {0}{1}", Environment.NewLine, string.Join(Environment.NewLine, allProcessPaths));
                proc = GetProcessByExecutablePath(clientExecutablePath);
                if (proc == null) {
                    _logger.LogWarning("Process with path {0} not found.", clientExecutablePath);
                }
            }
            if (proc != null) {
                proc.Kill(true);
                return true;
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Can't kill the client process");
        }
        return false;
    }

    public async Task HandleConnectedWebSocket(WebSocket webSocket) {
        // TODO: This does not receive _state.CancellationToken.IsCancellationRequested set to true soon enought to be able to stop listening
        //       _state.CancellationToken.IsCancellationRequested becomes true after the Kestrel detects stop timeout and shuts down the host
        //       This is too late for the app and it crashes
        // TODO: Remove this return; when a solution for timeouts on stop cause app crash
        ExecuteIfDebugIsEnabled(() => {
            _logger.LogDebug(new EventId(100), "Local client WebSocket connected");
        });
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        ClientWebSocketState wsState = new() { WebSocket = webSocket };
        _state.WebSockets.TryAdd(webSocket, wsState);
        SendConfigurationNotificationMessage(webSocket);
        // 100 Kb buffer
        var buffer = new byte[100 * 1024];
        WebSocketReceiveResult? receiveResult = null;
        while (!_state.CancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open) {
            try {
                ExecuteIfDebugIsEnabled(() => {
                    _logger.LogDebug(new EventId(100), "Waiting to receive data on local client WebSocket connection. IsCancellationRequested: {0}", _state.CancellationToken.IsCancellationRequested);
                });
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _state.CancellationToken);
                ExecuteIfDebugIsEnabled(() => {
                    _logger.LogDebug(new EventId(100), "Data received IsCancellationRequested: {0}", _state.CancellationToken.IsCancellationRequested);
                });
                if (receiveResult.MessageType == WebSocketMessageType.Close || receiveResult.Count == 0) {
                    ExecuteIfDebugIsEnabled(() => {
                        _logger.LogDebug(new EventId(100), "Received data indicates close");
                    });
                    break;
                } else {
                    if (receiveResult.EndOfMessage) {
                        string stringData = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        ExecuteIfDebugIsEnabled(() => {
                            _logger.LogDebug(new EventId(100), "Received message from local client: {0}", stringData);
                        });
                        ProcessLocalClientConnectorReceivedData(wsState, stringData);
                    } else {
                        // TODO: Not entire message is received - collect the data and process only after the entire message is recevied
                        ExecuteIfDebugIsEnabled(() => {
                            _logger.LogDebug(new EventId(100), "Partial message received from local client: {0}", buffer);
                        });
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(new EventId(100), ex, "Error on receiving from local client WebSocket. IsCancellationRequested: {0}", _state.CancellationToken.IsCancellationRequested);
                break;
            }
        }
        ExecuteIfDebugIsEnabled(() => {
            _logger.LogDebug(
                new EventId(100),
                "Closing local client WebSocket. WebSocket state: {0}, CancellationRequested: {1}, Result message type: {2}, Result CloseStatus: {3}, Result CloseStatusDescription: {4}",
                webSocket.State,
                _state.CancellationToken.IsCancellationRequested,
                receiveResult?.MessageType,
                receiveResult?.CloseStatus,
                receiveResult?.CloseStatusDescription
            );
        });
        wsState.WebSocket = null;
        _state.WebSockets.TryRemove(webSocket, out _);
    }

    private void ProcessLocalClientConnectorReceivedData(ClientWebSocketState wsState, string stringData) {
        PartialMessage partialMsg = DeserializePartialMessage(stringData);
        if (partialMsg?.Header?.Type == null) {
            // TODO: Can't process the message
            return;
        }
        // TODO: Should we set LastMessageReceivedAt only for known messages ?
        wsState.LastMessageReceivedAt = GetNow();
        string msgType = partialMsg.Header.Type;
        switch (msgType) {
            case LocalClientToDeviceRequestMessageType.CreateSignInCode: {
                    var msg = DeserializeLocalClientToDeviceRequestMessage<LocalClientToDeviceCreateSignInCodeRequestMessage, LocalClientToDeviceCreateSignInCodeRequestMessageBody>(stringData);
                    var toServerReqMsg = DeviceToServerCreateSignInCodeRequestMessageHelper.CreateMessage();
                    SendDeviceToServerRequestMessage(toServerReqMsg, toServerReqMsg.Header);
                    break;
                }
            case LocalClientToDeviceRequestMessageType.Ping: {
                    // We don't need to process the ping message - LastMessageReceivedAt was already set
                    break;
                }
            case LocalClientToDeviceRequestMessageType.StartOnPrepaidTariff: {
                    var msg = DeserializeLocalClientToDeviceRequestMessage<LocalClientToDeviceStartOnPrepaidTariffRequestMessage, LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody>(stringData);
                    var toServerReqMsg = DeviceToServerStartOnPrepaidTariffRequestMessageHelper.CreateMessage();
                    toServerReqMsg.Body.TariffId = msg.Body.TariffId;
                    toServerReqMsg.Body.PasswordHash = msg.Body.PasswordHash;
                    SendDeviceToServerRequestMessage(toServerReqMsg, toServerReqMsg.Header);
                    break;
                }
            case LocalClientToDeviceRequestMessageType.EndDeviceSessionByCustomer: {
                    var msg = DeserializeLocalClientToDeviceRequestMessage<LocalClientToDeviceEndDeviceSessionByCustomerRequestMessage, LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageBody>(stringData);
                    var toServerReqMsg = DeviceToServerEndDeviceSessionRequestMessageHelper.CreateMessage();
                    SendDeviceToServerRequestMessage(toServerReqMsg, toServerReqMsg.Header);
                    break;
                }
            case LocalClientToDeviceRequestMessageType.ChangePrepaidTariffPasswordByCustomer: {
                    var msg = DeserializeLocalClientToDeviceRequestMessage<LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessage, LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody>(stringData);
                    var toServerReqMsg = DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessageHelper.CreateMessage();
                    toServerReqMsg.Body.CurrentPasswordHash = msg.Body.CurrentPasswordHash;
                    toServerReqMsg.Body.NewPasswordHash = msg.Body.NewPasswordHash;
                    SendDeviceToServerRequestMessage(toServerReqMsg, toServerReqMsg.Header);
                    break;
                }
            case LocalClientToDeviceRequestMessageType.RestartNow: {
                    _restartWindowsHelper.Restart();
                    break;
                }
        }
    }

    private TMessage DeserializeLocalClientToDeviceRequestMessage<TMessage, TBody>(string jsonString) where TMessage : LocalClientToDeviceRequestMessage<TBody>, new() {
        var deserialized = JsonSerializer.Deserialize<LocalClientToDeviceRequestMessage<object>>(jsonString, _jsonSerializerOptions);
        // TODO: Can we use reflection to infer the TBody ?
        var deserializedMsg = CreateTypedLocalClientToDeviceRequestMessageFromGenericMessage<TBody>(deserialized);
        TMessage result = new TMessage();
        result.Header = deserializedMsg.Header;
        result.Body = deserializedMsg.Body;
        return result;
    }

    private LocalClientToDeviceRequestMessage<TBody> CreateTypedLocalClientToDeviceRequestMessageFromGenericMessage<TBody>(LocalClientToDeviceRequestMessage<object> msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        LocalClientToDeviceRequestMessage<TBody> deserializedMsg = new LocalClientToDeviceRequestMessage<TBody>();
        deserializedMsg.Header = msg.Header;
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private async void SendConfigurationNotificationMessage(WebSocket ws) {
        DeviceToLocalClientConfigurationNotificationMessage msg = DeviceToLocalClientConfigurationNotificationMessageHelper.CreateMessage();
        msg.Body.PingInterval = _state.LocalClientPingInterval;
        if (_deviceConfigNotificationMsg?.Body?.FeatureFlags is not null) {
            msg.Body.FeatureFlags = new DeviceToLocalClientConfigurationNotificationMessageFeatureFlags {
                CodeSignIn = _deviceConfigNotificationMsg.Body.FeatureFlags.CodeSignIn
            };
        }
        await SendDeviceToLocalClientNotificationMessage(msg, ws);
    }

    private async void SendConfigurationNotificationMessageToAllLocalClients() {
        DeviceToLocalClientConfigurationNotificationMessage msg = DeviceToLocalClientConfigurationNotificationMessageHelper.CreateMessage();
        msg.Body.PingInterval = _state.LocalClientPingInterval;
        if (_deviceConfigNotificationMsg?.Body?.FeatureFlags is not null) {
            msg.Body.FeatureFlags = new DeviceToLocalClientConfigurationNotificationMessageFeatureFlags {
                CodeSignIn = _deviceConfigNotificationMsg.Body.FeatureFlags.CodeSignIn
            };
        }
        string serialized = SerializeDeviceToLocalClientNotificationMessage(msg);
        SendToAllLocalClients(serialized);
    }

    private void StartWebSocketConnector(CancellationToken cancellationToken) {
        string? pcConnectorBaseUrl = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_BASE_URL);
        Uri.TryCreate(pcConnectorBaseUrl, UriKind.Absolute, out Uri? pcConnectorBaseUri);
        if (pcConnectorBaseUri is null || pcConnectorBaseUri.Scheme != Uri.UriSchemeWss) {
            _logger.LogCritical("The environment variable {0} value '{1}' is invalid Uri. It must be valid wss://<name-or-ip-address>:<port>", Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_BASE_URL, pcConnectorBaseUrl);
            return;
        }
        X509Certificate2? clientCert = GetLocalClientCertificate();
        if (clientCert is null) {
            _logger.LogCritical("Can't find client certificate for connections to the service");
            return;
        }
        string? pcConnectorCertificateThumbprint = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_CERTIFICATE_THUMBPRINT);
        if (string.IsNullOrEmpty(pcConnectorCertificateThumbprint)) {
            _logger.LogCritical("Can't find pc-connector certificate thumbprint. Environment variable {0} value is empty", Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_CERTIFICATE_THUMBPRINT);
            return;
        }
        _logger.LogInformation("Using pc-connector certificate thumprint {0}", pcConnectorCertificateThumbprint);

        _serverWsConnector.Connected += _serverWsConnector_Connected;
        _serverWsConnector.Disconnected += _serverWsConnector_Disconnected;
        _serverWsConnector.ConnectError += _serverWsConnector_ConnectError;
        _serverWsConnector.ReceiveError += _serverWsConnector_ReceiveError;
        _serverWsConnector.ValidatingRemoteCertificate += _serverWsConnector_ValidatingRemoteCertificate;
        _serverWsConnector.DataReceived += _serverWsConnector_DataReceived;
        _serverWsConnector.SendDataError += _serverWsConnector_SendDataError;
        WebSocketConnectorConfig wsConnectorConfig = new() {
            ClientCertificate = clientCert,
            CancellationToken = cancellationToken,
            ReconnectDelay = TimeSpan.FromSeconds(3),
            ServerUri = pcConnectorBaseUri,
            ServerCertificateThumbprint = pcConnectorCertificateThumbprint,
        };
        _serverWsConnector.Initialize(wsConnectorConfig);
        _serverWsConnector.Start();
    }

    private void _serverWsConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        _logger.LogInformation("Disconnected from the server");
    }

    private void _serverWsConnector_ReceiveError(object? sender, ReceiveErrorEventArgs e) {
        _logger.LogWarning(e.Exception, "Server data receive error");
    }

    private void _serverWsConnector_Connected(object? sender, ConnectedEventArgs e) {
        _logger.LogInformation("Connected to the server");
    }

    private void _serverWsConnector_SendDataError(object? sender, SendDataErrorEventArgs e) {
        _logger.LogWarning(e.Exception, "WebSocket send data error");
    }

    private void _serverWsConnector_DataReceived(object? sender, DataReceivedEventArgs e) {
        bool traceEnabled = _logger.IsEnabled(LogLevel.Trace);
        if (traceEnabled) {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogDebug("WebSocket data received. Bytes length: {0}, bytes: {1}",
                    e.Data.Length,
                    BitConverter.ToString(e.Data.ToArray())
                );
            });
        }
        try {
            string stringData = Encoding.UTF8.GetString(e.Data.ToArray());
            if (traceEnabled) {
                _logger.LogDebug("Received string '{0}'", stringData);
            }
            try {
                ProcessServerToDeviceReceivedData(stringData);
            } catch (Exception ex) {
                _logger.LogError(ex, "Can't process received data '{0}'", stringData);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Cannot process received data {0}", e.Data);
        }
    }

    private void ProcessServerToDeviceReceivedData(string stringData) {
        PartialMessage msg = DeserializePartialMessage(stringData);
        string msgType = msg.Header.Type;
        switch (msgType) {
            case ServerToDeviceReplyMessageType.CreateSignInCode:
                // TODO: For now we will process this here
                ServerToDeviceReplyMessage<ServerToDeviceCreateSignInCodeReplyMessageBody> createSignInCodeReplyMsg = CreateTypedMessageFromGenericServerToDeviceReplyMessage<ServerToDeviceCreateSignInCodeReplyMessageBody>(msg);
                ProcessServerToDeviceCreateSignInCodeReplyMessage(createSignInCodeReplyMsg);
                break;
            case ServerToDeviceNotificationMessageType.Restart:
                ServerToDeviceNotificationMessage<ServerToDeviceRestartNotificationMessageBody> restartNotificationMsg = CreateTypedMessageFromGenericServerToDeviceNotificationMessage<ServerToDeviceRestartNotificationMessageBody>(msg);
                ProcessServerToDeviceRestartNotificationMessage(restartNotificationMsg);
                break;
            case ServerToDeviceNotificationMessageType.Shutdown:
                ServerToDeviceNotificationMessage<ServerToDeviceShutdownNotificationMessageBody> shutdownNotificationMsg = CreateTypedMessageFromGenericServerToDeviceNotificationMessage<ServerToDeviceShutdownNotificationMessageBody>(msg);
                ProcessServerToDeviceShutdownNotificationMessage(shutdownNotificationMsg);
                break;
            case ServerToDeviceReplyMessageType.ChangePrepaidTariffPasswordByCustomer:
                // TODO: For now we will process this here
                ServerToDeviceReplyMessage<ServerToDeviceChangePrepaidTariffPasswordByCustomerReplyMessageBody> changePrepaidTariffPasswordByCustomerReplyMsg = CreateTypedMessageFromGenericServerToDeviceReplyMessage<ServerToDeviceChangePrepaidTariffPasswordByCustomerReplyMessageBody>(msg);
                ProcessServerToDeviceChangePrepaidTariffPasswordByCustomerReplyMessage(changePrepaidTariffPasswordByCustomerReplyMsg);
                break;
            case ServerToDeviceReplyMessageType.StartOnPrepaidTariff:
                // TODO: For now we will process this here
                ServerToDeviceReplyMessage<ServerToDeviceStartOnPrepaidTariffReplyMessageBody> startOnPrepaidTariffReplyMsg = CreateTypedMessageFromGenericServerToDeviceReplyMessage<ServerToDeviceStartOnPrepaidTariffReplyMessageBody>(msg);
                ProcessServerToDeviceStartOnPrepaidTariffReplyMessage(startOnPrepaidTariffReplyMsg);
                break;
            case ServerToDeviceNotificationMessageType.CurrentStatus:
                ServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody> deviceSetStatusNotificationMsg = CreateTypedMessageFromGenericServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody>(msg);
                ProcessServerToDeviceCurrentStatusNotificationMessage(deviceSetStatusNotificationMsg);
                break;
            case ServerToDeviceNotificationMessageType.DeviceConfiguration:
                ServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody> deviceConfigurationMsg = CreateTypedMessageFromGenericServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody>(msg);
                ProcessDeviceConfigurationNotificationMessage(deviceConfigurationMsg);
                break;

        }
        //JsonSerializer.Deserialize<object>(deserialized.Body as JsonElement);
        //const bodyJsonElement = deserialized.Body as JsonElement;
    }

    private void ProcessServerToDeviceChangePrepaidTariffPasswordByCustomerReplyMessage(ServerToDeviceReplyMessage<ServerToDeviceChangePrepaidTariffPasswordByCustomerReplyMessageBody> msg) {
        var dtlcMsg = DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageHelper.CreateMessage();
        TransferReplyHeader(msg.Header, dtlcMsg.Header);
        var serialized = SerializeDeviceToLocalClientReplyMessage(dtlcMsg);
        SendToAllLocalClients(serialized);
    }

    private void ProcessServerToDeviceCreateSignInCodeReplyMessage(ServerToDeviceReplyMessage<ServerToDeviceCreateSignInCodeReplyMessageBody> msg) {
        var dtlcMsg = DeviceToLocalClientCreateSignInCodeReplyMessageHelper.CreateMessage();
        TransferReplyHeader(msg.Header,dtlcMsg.Header);
        dtlcMsg.Body.Url = msg.Body.Url;
        dtlcMsg.Body.RemainingSeconds = msg.Body.RemainingSeconds;
        dtlcMsg.Body.IdentifierType = msg.Body.IdentifierType;
        dtlcMsg.Body.Code = msg.Body.Code;
        var serialized = SerializeDeviceToLocalClientReplyMessage(dtlcMsg);
        SendToAllLocalClients(serialized);
    }

    private void ProcessServerToDeviceStartOnPrepaidTariffReplyMessage(ServerToDeviceReplyMessage<ServerToDeviceStartOnPrepaidTariffReplyMessageBody> msg) {
        var dtlcMsg = DeviceToLocalClientStartOnPrepaidTariffReplyMessageHelper.CreateMessage();
        TransferReplyHeader(msg.Header, dtlcMsg.Header);
        dtlcMsg.Body.Success = msg.Body.Success;
        dtlcMsg.Body.PasswordDoesNotMatch = msg.Body.PasswordDoesNotMatch;
        dtlcMsg.Body.AlreadyInUse = msg.Body.AlreadyInUse;
        dtlcMsg.Body.NotAvailableForThisDeviceGroup = msg.Body.NotAvailableForThisDeviceGroup;
        dtlcMsg.Body.NoRemainingTime = msg.Body.NoRemainingTime;
        dtlcMsg.Body.RemainingSeconds = msg.Body.RemainingSeconds;
        var serialized = SerializeDeviceToLocalClientReplyMessage(dtlcMsg);
        SendToAllLocalClients(serialized);
    }

    private void TransferReplyHeader(ReplyMessageHeader source, ReplyMessageHeader destination) {
        destination.CorrelationId = source.CorrelationId;
        destination.MessageErrors = source.MessageErrors;
        destination.Failure = source.Failure;
        destination.Type = source.Type;
    }

    private void ProcessDeviceConfigurationNotificationMessage(ServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody> msg) {
        _deviceConfigNotificationMsg = msg;
        ProcessDeviceConfigurationMessageChanged();
    }

    private void ProcessDeviceConfigurationMessageChanged() {
        SendConfigurationNotificationMessageToAllLocalClients();
    }

    private void HandleStartedToStoppedTransition() {
        _state.StartedToStoppedTransitionDate = DateTimeOffset.Now;
        // Disable Task manager
        _registryHelper.ChangeTaskManagerAvailability(false);
    }

    private void HandleStoppedToStartedTransition() {
        _state.StartedToStoppedTransitionDate = null;
        // Enable task manager
        _registryHelper.ChangeTaskManagerAvailability(true);
    }

    private void ProcessServerToDeviceRestartNotificationMessage(ServerToDeviceNotificationMessage<ServerToDeviceRestartNotificationMessageBody> msg) {
        this._restartWindowsHelper.Restart();
    }

    private void ProcessServerToDeviceShutdownNotificationMessage(ServerToDeviceNotificationMessage<ServerToDeviceShutdownNotificationMessageBody> msg) {
        this._restartWindowsHelper.Shutdown();
    }

    private void ProcessServerToDeviceCurrentStatusNotificationMessage(ServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody> msg) {
        // If this is the first message and it is for stopped - apply restrictions as it was stopped
        if (_deviceCurrentStatusNotificationMsg == null) {
            // This is the first current status notification message
            if (msg.Body.Started == false) {
                // The first status notification message indicates the computer must be stopped
                // Perform the "stopped computer" activities - in this case just disable the task manager
                _registryHelper.ChangeTaskManagerAvailability(false);
            } else if (msg.Body.Started == true) {
                // The first status notification message indicates the computer must be started
                // Perform the "started computer" activities - in this case just enable the task manager
                _registryHelper.ChangeTaskManagerAvailability(true);
            }
        }
        _deviceCurrentStatusNotificationMsg = msg;
        if (_state.StartedState == true && msg.Body.Started == false) {
            // Was started but now it is stopped
            HandleStartedToStoppedTransition();
        } else if (_state.StartedState == false && msg.Body.Started == true) {
            // Was stopped but now it is started
            _state.SessionWillEndSoonMessageSentAt = DateTimeOffset.MinValue;
            HandleStoppedToStartedTransition();
        }

        _state.StartedState = msg.Body.Started;
        var msgToSend = DeviceToLocalClientCurrentStatusNotificationMessageHelper.CreateMessage();
        msgToSend.Body = new DeviceToLocalClientCurrentStatusNotificationMessageBody {
            Started = msg.Body.Started,
            CanBeStoppedByCustomer = msg.Body.CanBeStoppedByCustomer,
            TariffId = msg.Body.TariffId,
            Amounts = msg.Body.Amounts,
            ContinuationTariffShortInfo = msg.Body.ContinuationTariffShortInfo,
        };
        string serialized = SerializeDeviceToLocalClientNotificationMessage(msgToSend);
        SendToAllLocalClients(serialized);

        bool hasContinuation = msg.Body.ContinuationTariffShortInfo?.Id > 0;
        if (_state.StartedState == true &&
            _deviceConfigNotificationMsg is not null &&
            !hasContinuation &&
            _deviceConfigNotificationMsg.Body.SecondsBeforeNotifyingCustomerForSessionEnd > 0 &&
            msg.Body.Amounts.RemainingSeconds < _deviceConfigNotificationMsg.Body.SecondsBeforeNotifyingCustomerForSessionEnd &&
            (DateTimeOffset.Now - _state.SessionWillEndSoonMessageSentAt).TotalSeconds > (_deviceConfigNotificationMsg.Body.SecondsBeforeNotifyingCustomerForSessionEnd + 10)
        ) {
            var sessionEndsSoonMsg = DeviceToLocalClientSessionWillEndSoonNotificationMessageHelper.CreateMessage();
            sessionEndsSoonMsg.Body.RemainingSeconds = msg.Body.Amounts?.RemainingSeconds ?? 0;
            sessionEndsSoonMsg.Body.NotificationSoundFile = _deviceConfigNotificationMsg.Body.SessionEndNotificationSoundFilePath;
            string serializedSessionEndsSoonMsg = SerializeDeviceToLocalClientNotificationMessage(sessionEndsSoonMsg);
            SendToAllLocalClients(serializedSessionEndsSoonMsg);
            _state.SessionWillEndSoonMessageSentAt = DateTimeOffset.Now;
        }
    }

    private async void SendToAllLocalClients(string data) {
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        ArraySegment<byte> arraySegment = new ArraySegment<byte>(bytes);
        foreach (var kvp in _state.WebSockets.Where(x => x.Key.State == WebSocketState.Open)) {
            var ws = kvp.Key;
            try {
                ExecuteIfDebugIsEnabled(() => {
                    _logger.LogDebug(new EventId(100), "Sending data to client WebSocket: {0}", data);
                });
                await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, _state.CancellationToken);
            } catch (Exception ex) {
                this._logger.LogError(new EventId(100), ex, "Can't send data {0} to local client", data);
            }
        }
    }

    private ServerToDeviceNotificationMessage<TBody> CreateTypedMessageFromGenericServerToDeviceNotificationMessage<TBody>(PartialMessage msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        ServerToDeviceNotificationMessage<TBody> deserializedMsg = new ServerToDeviceNotificationMessage<TBody>();
        deserializedMsg.Header = new ServerToDeviceNotificationMessageHeader {
            Type = msg.Header.Type,
        };
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private ServerToDeviceReplyMessage<TBody> CreateTypedMessageFromGenericServerToDeviceReplyMessage<TBody>(PartialMessage msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        ServerToDeviceReplyMessage<TBody> deserializedMsg = new ServerToDeviceReplyMessage<TBody>();
        deserializedMsg.Header = new ServerToDeviceReplyMessageHeader {
            Type = msg.Header.Type,
            CorrelationId = msg.Header.CorrelationId,
            MessageErrors = msg.Header.MessageErrors,
            Failure = msg.Header.Failure,
        };
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    //private Message<TBody> CreateTypedMessageFromGenericMessage<TBody>(Message<object> msg) {
    //    string bodyString = msg.Body.ToString()!;
    //    TBody? body = DeserializeBody<TBody>(bodyString);
    //    Message<TBody> deserializedMsg = new Message<TBody>();
    //    deserializedMsg.Header = msg.Header;
    //    deserializedMsg.Body = body!;
    //    return deserializedMsg;
    //}

    private PartialMessage DeserializePartialMessage(string jsonString) {
        var deserialized = JsonSerializer.Deserialize<PartialMessage>(jsonString, _jsonSerializerOptions);
        return deserialized;
    }

    private TBody DeserializeBody<TBody>(string body) {
        var deserialized = JsonSerializer.Deserialize<TBody>(body, _jsonSerializerOptions);
        return deserialized;
    }

    private string SerializeMessage(object msg) {
        var serializedString = JsonSerializer.Serialize(msg, _jsonSerializerOptions);
        return serializedString;
    }

    private string SerializeDeviceToLocalClientNotificationMessage<TBody>(DeviceToLocalClientNotificationMessage<TBody> msg) {
        var serializedString = JsonSerializer.Serialize(msg, _jsonSerializerOptions);
        return serializedString;
    }

    private string SerializeDeviceToLocalClientReplyMessage<TBody>(DeviceToLocalClientReplyMessage<TBody> msg) {
        var serializedString = JsonSerializer.Serialize(msg, _jsonSerializerOptions);
        return serializedString;
    }

    private void _serverWsConnector_ValidatingRemoteCertificate(object? sender, ValidatingRemoteCertificateArgs e) {
        _logger.LogInformation(
            "Validating remote server certificate{0}{1}{2}",
            Environment.NewLine,
            e.Certificate,
            Environment.NewLine,
            e.SslPolicyError
        );
    }

    private void _serverWsConnector_ConnectError(object? sender, ConnectErrorEventArgs e) {
        _logger.LogWarning(e.Exception, "WebSocket connect error");
    }

    private X509Certificate2? GetLocalClientCertificate() {
        string? caCertThumbprint = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT);
        if (caCertThumbprint is null) {
            _logger.LogCritical("The value of the environment variable '{0}' is missing. It must contain the value of the CA certificate issuer thumbprint", Ccs3EnvironmentVariableNames.CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT);
            return null;
        }
        string certificateCNToSearchFor = Environment.MachineName;
        var certResult = _certificateHelper.GetPersonalCertificateIssuedByTrustedRootCA(caCertThumbprint, certificateCNToSearchFor);
        if (!certResult.IssuerFound) {
            _logger.LogCritical("Trusted root certificate with thumbprint '{0}' was not found", caCertThumbprint);
            return null;
        }
        if (certResult.PersonalCertificate == null) {
            _logger.LogCritical("Local service certificate with subject '{0}' issued by certificate with thumbprint {1} was not found", certificateCNToSearchFor, caCertThumbprint);
            return null;
        }
        return certResult.PersonalCertificate;
    }

    private string GetClientExecutablePath() {
        var clientAppProcessExecutableFullPath = Path.GetFullPath(Path.Combine(".", "Ccs3ClientApp\\Ccs3ClientApp.exe"));
        return clientAppProcessExecutableFullPath;
    }

    private void StartClientAppIfNotStarted() {
#if DEBUG
        return;
#endif
        // TODO: What if the process already runs ? Should we store its id and use it later when we want to kill it ?
        //       And what about multiple instances of the process ?
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        if (_deviceConfigNotificationMsg == null) {
            // Still did not received initial messages from server - probably this device 
            // is not part of the system / not active / no connection to the server
            return;
        }
        if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.Success) {
            // The proces seems started
            return;
        }

        // TODO: We could use environment variable
        // Get path using current path - the client app is in subfolder of current service executable path
        var clientAppProcessExecutableFullPath = GetClientExecutablePath();

        var sessions = ClientAppProcessController.GetSessions();
        LogClientAppProcessData(() => {
            if (_logger.IsEnabled(LogLevel.Debug)) {
                StringBuilder sb = new();
                foreach (var session in sessions) {
                    sb.AppendLine("Session id: " + session.SessionID);
                    sb.AppendLine("Session name: " + session.pWinStationName);
                    sb.AppendLine("Session state: " + session.State);
                    sb.AppendLine("-----------------");
                }
                _logger.LogDebug("Sessions: {sessions}", sb.ToString());
            }
        });
        ClientAppProcessController.WTS_SESSION_INFO activeSession = sessions.FirstOrDefault(x => x.State == ClientAppProcessController.WTS_CONNECTSTATE_CLASS.WTSActive);
        if (activeSession.SessionID > 0) {
            LogClientAppProcessData(() => {
                _logger.LogDebug("ActiveSession: {0}", activeSession.SessionID);
            });
            // TODO: Check if the app already runs
            Process? clientAppProcess = GetProcessByExecutablePath(clientAppProcessExecutableFullPath, (int)activeSession.SessionID);
            if (clientAppProcess == null) {
                LogClientAppProcessData(() => {
                    _logger.LogDebug("It seems that process with path {0} does not run in session {1}", clientAppProcessExecutableFullPath, activeSession.SessionID);
                });
                if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.ProcInfo.hProcess != 0) {
                    ClientAppProcessController.CloseProcInfoHandles(_startProcessAsCurrentUserResult.ProcInfo);
                }
                LogClientAppProcessData(() => {
                    _logger.LogDebug("Trying to start the process: {clientAppProcessExecutableFullPath}", clientAppProcessExecutableFullPath);
                });
                _startProcessAsCurrentUserResult = ClientAppProcessController.StartProcessAsCurrentUser(clientAppProcessExecutableFullPath, null, null, true, _logger);
                if (_startProcessAsCurrentUserResult.Success) {
                    LogClientAppProcessData(() => {
                        _logger.LogDebug("Process handle: {hProcess}", _startProcessAsCurrentUserResult.ProcInfo.hProcess);
                    });
                    LogClientAppProcessData(() => {
                        _logger.LogDebug("Process {clientAppProcessExecutableFullPath} started. PID: {pid}", clientAppProcessExecutableFullPath, _startProcessAsCurrentUserResult.ProcInfo.dwProcessId);
                    });
                    // TODO: Should we call WaitForInputIdle to know when the process has finished initialization ?
                    if (_startProcessAsCurrentUserResult.ProcInfo.hProcess != 0) {
                        // Process pc = Process.GetProcessById((int)_startProcessAsCurrentUserResult.ProcInfo?.dwProcessId);
                        // TODO: Provide cancellation token
                        WaitForProcessToExit(_startProcessAsCurrentUserResult.ProcInfo.hProcess).ContinueWith(task => {
                            _startProcessAsCurrentUserResult = null;
                            LogClientAppProcessData(() => {
                                _logger.LogInformation("Client app process has exited");
                            });
                        });
                        LogClientAppProcessData(() => {
                            _logger.LogInformation("Waiting for the ClientApp process to exit");
                        });
                    }
                } else {
                    LogClientAppProcessData(() => {
                        _logger.LogWarning(
                            "Can't start the process {clientAppProcessExecutableFullPath}. WinAPI errors: {lastErrors}, dwProcessId: {dwProcessId}, hProcess: {hProcess}",
                            clientAppProcessExecutableFullPath,
                            _startProcessAsCurrentUserResult.LastErrors,
                            _startProcessAsCurrentUserResult.ProcInfo.dwProcessId,
                            _startProcessAsCurrentUserResult.ProcInfo.hProcess
                        );
                    });
                }
            }
        }
    }

    private void TimerCallbackFunction(object? state) {
        PingServer();
        CheckForRestartAfterStopped();
    }

    private void CheckForRestartAfterStopped() {
        if (_state.Restarting) {
            SendSecondsToRestartMessage(0);
            return;
        }
        if (_state.StartedState == false
            && _deviceConfigNotificationMsg != null
            && _state.StartedToStoppedTransitionDate != null
            && _deviceConfigNotificationMsg.Body.SecondsAfterStoppedBeforeRestart != null
            && _deviceConfigNotificationMsg.Body.SecondsAfterStoppedBeforeRestart > 0) {
            var now = DateTimeOffset.Now;
            var diff = now - _state.StartedToStoppedTransitionDate.Value;
            var secondsConfigValue = _deviceConfigNotificationMsg.Body.SecondsAfterStoppedBeforeRestart.Value;
            int remainingSeconds = secondsConfigValue - (int)diff.TotalSeconds;
            SendSecondsToRestartMessage(remainingSeconds);
            if (diff.TotalSeconds > secondsConfigValue) {
                _state.Restarting = true;
                _restartWindowsHelper.Restart();
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 0) {
                    _logger.LogWarning("Can't restart. LastWin32Error: " + lastWin32Error);
                }
            }
        }
    }

    private void SendSecondsToRestartMessage(int remainingSeconds) {
        var notificationMsg = DeviceToLocalClientSecondsBeforeRestartNotificationMessageHelper.CreateMessage();
        notificationMsg.Body.Seconds = remainingSeconds;
        var serialized = SerializeDeviceToLocalClientNotificationMessage(notificationMsg);
        SendToAllLocalClients(serialized);
    }

    private void PingServer() {
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        if (_deviceConfigNotificationMsg is null) {
            return;
        }
        if (_deviceConfigNotificationMsg.Body is null
            || _deviceConfigNotificationMsg.Body.PingInterval <= 0) {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogDebug("Can't ping server. Device configuration ping interval {0} is invalid", _deviceConfigNotificationMsg.Body?.PingInterval);
            });
            return;
        }

        try {
            TimeSpan diff = GetNow() - _state.LastServicePingDateTime;
            if (diff.TotalMilliseconds >= _deviceConfigNotificationMsg.Body.PingInterval) {
                _state.LastServicePingDateTime = DateTime.UtcNow;
                SendDeviceToServerPingNotificationMessage();
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Can't ping the server");
        }
    }

    private void SendDeviceToServerPingNotificationMessage() {
        DeviceToServerPingNotificationMessage pingMsg = DeviceToServerPingNotificationMessageHelper.CreateMessage();
        SendDeviceToServerNotificationMessage(pingMsg);
    }

    private void SendDeviceToServerNotificationMessage(object msg) {
        string serialized = SerializeMessage(msg);
        ReadOnlyMemory<byte> buffer = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastDataSentAt = GetNow();
        _serverWsConnector.SendData(buffer);
    }

    private void SendDeviceToServerRequestMessage(object msg, DeviceToServerRequestMessageHeader messageHeader) {
        if (string.IsNullOrWhiteSpace(messageHeader.CorrelationId)) {
            messageHeader.CorrelationId = Guid.NewGuid().ToString();
        }
        string serialized = SerializeMessage(msg);
        ReadOnlyMemory<byte> buffer = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastDataSentAt = GetNow();
        _serverWsConnector.SendData(buffer);
    }

    // TODO:
    private void SendDeviceToServerRequestMessageAndWaitForReply(object msg, DeviceToServerRequestMessageHeader messageHeader) {
        if (string.IsNullOrWhiteSpace(messageHeader.CorrelationId)) {
            messageHeader.CorrelationId = Guid.NewGuid().ToString();
        }
        string serialized = SerializeMessage(msg);
        ReadOnlyMemory<byte> buffer = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastDataSentAt = GetNow();
        Subject<string> mySubject = new Subject<string>();
        _serverWsConnector.SendData(buffer);
    }

    private async Task<bool> SendDeviceToLocalClientReplyMessage<TBody>(DeviceToLocalClientReplyMessage<TBody> msg, WebSocket ws) {
        string serialized = SerializeDeviceToLocalClientReplyMessage(msg);
        ReadOnlyMemory<byte> bytes = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastLocalClientDataSentAt = GetNow();
        try {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogDebug(new EventId(100), "Sending local client reply message: {0}", serialized);
            });
            await ws.SendAsync(bytes, WebSocketMessageType.Text, true, _state.CancellationToken);
            return true;
        } catch (Exception ex) {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogError(new EventId(100), ex, "Can't send DeviceToLocalClientReplyMessage message");
            });
            //SendDataError?.Invoke(this, new SendDataErrorEventArgs { Exception = ex });
            return false;
        }
        //_wsConnector.SendData(buffer);
    }

    private async Task<bool> SendDeviceToLocalClientNotificationMessage<TBody>(DeviceToLocalClientNotificationMessage<TBody> msg, WebSocket ws) {
        string serialized = SerializeDeviceToLocalClientNotificationMessage(msg);
        ReadOnlyMemory<byte> bytes = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastLocalClientDataSentAt = GetNow();
        try {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogDebug(new EventId(100), "Sending local client notification message: {0}", serialized);
            });
            await ws.SendAsync(bytes, WebSocketMessageType.Text, true, _state.CancellationToken);
            return true;
        } catch (Exception ex) {
            ExecuteIfDebugIsEnabled(() => {
                _logger.LogError(new EventId(100), ex, "Can't send LocalClientNotificationMessage message");
            });
            //SendDataError?.Invoke(this, new SendDataErrorEventArgs { Exception = ex });
            return false;
        }
        //_wsConnector.SendData(buffer);
    }

    public DateTimeOffset GetNow() {
        return DateTimeOffset.Now;
    }

    private void ExecuteIfDebugIsEnabled(Action action) {
        if (!_logger.IsEnabled(LogLevel.Debug)) {
            return;
        }
        action();
    }

    private void PrepareState() {
        _state.LastServicePingDateTime = GetNow();
        _state.LocalClientPingInterval = 5000;
        _state.WebSockets = new ConcurrentDictionary<WebSocket, ClientWebSocketState>();
    }

    private void LogClientAppProcessData(Action logAction) {
        if (_disableClientAppProcessStartLogs) {
            return;
        }
        logAction();
    }

    private List<string> GetAllProcessesExecutablePaths() {
        List<string> result = new();
        var processes = Process.GetProcesses();
        foreach (var proc in processes) {
            // Access to some MainModule throws AccessDenied exception
            try {
                if (proc.MainModule is not null) {
                    result.Add(proc.MainModule.FileName);
                }
            } catch { }
        }
        return result;
    }

    private Process? GetProcessByExecutablePath(string executablePath, int sessionId = -1) {
        IEnumerable<Process> processes;
        if (sessionId == -1) {
            processes = Process.GetProcesses();
        } else {
            processes = Process.GetProcesses().Where(x => x.SessionId == sessionId);
        }
        Process? processByExecutablePath = null;
        foreach (var proc in processes) {
            // Access to some MainModule throws AccessDenied exception
            try {
                if (proc.MainModule is not null && string.Equals(proc.MainModule.FileName, executablePath, StringComparison.OrdinalIgnoreCase)) {
                    processByExecutablePath = proc;
                    break;
                }
            } catch { }
        }
        return processByExecutablePath;
    }

    private Task WaitForProcessToExit(nint processHandle) {
        if (processHandle == 0) {
            return Task.CompletedTask;
        }
        var task = Task.Run(() => {
            EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
            ewh.SafeWaitHandle = new SafeWaitHandle(_startProcessAsCurrentUserResult!.ProcInfo.hProcess, true);
            bool waitOneResult = ewh.WaitOne();
        }, _state.CancellationToken);
        return task;
    }

    private JsonSerializerOptions CreateJsonSerializerOptions() {
        JsonSerializerOptions options = new() {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.Strict,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        return options;
    }

    private static class Ccs3EnvironmentVariableNames {
        public static readonly string CCS3_PC_CONNECTOR_SERVICE_BASE_URL = "CCS3_PC_CONNECTOR_SERVICE_BASE_URL";
        public static readonly string CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT = "CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT";
        public static readonly string CCS3_PC_CONNECTOR_SERVICE_CERTIFICATE_THUMBPRINT = "CCS3_PC_CONNECTOR_SERVICE_CERTIFICATE_THUMBPRINT";
        public static readonly string CCS3_CAWS_DEBUG_DISABLE_CLIENT_APP_PROCESS_START_LOGS = "CCS3_CAWS_DEBUG_DISABLE_CLIENT_APP_PROCESS_START_LOGS";
    }

    private class ClientWebSocketState {
        public WebSocket WebSocket { get; set; }
        public DateTimeOffset? LastMessageReceivedAt { get; set; }
    }

    private class WorkerState {
        public DateTimeOffset LastServicePingDateTime { get; set; }
        public DateTimeOffset? LastDataSentAt { get; set; }
        public DateTimeOffset? LastLocalClientDataSentAt { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public CancellationToken AppCancellationToken { get; set; }
        public int LocalClientPingInterval { get; set; }
        public ConcurrentDictionary<WebSocket, ClientWebSocketState> WebSockets { get; set; }
        public bool? StartedState { get; set; }
        public DateTimeOffset? StartedToStoppedTransitionDate { get; set; }
        public bool Restarting { get; set; }
        public DateTimeOffset SessionWillEndSoonMessageSentAt { get; set; }
    }
}
