using Ccs3ClientAppWindowsService.Messages;
using Ccs3ClientAppWindowsService.Messages.LocalClient;
using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccs3ClientAppWindowsService;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private StartProcessAsCurrentUserResult? _startProcessAsCurrentUserResult;
    private WebSocketConnector _wsConnector = new();
    private readonly string _serviceName = "Ccs3ClientAppWindowsService";
    private readonly CertificateHelper _certificateHelper = new();
    private bool _disableClientAppProcessStartLogs = false;
    private JsonSerializerOptions _jsonSerializerOptions;
    private Message<DeviceConfigurationNotificationMessageBody> _deviceConfigMsg;
    private Message<DeviceSetStatusNotificationMessageBody> _deviceSetStatusMsg;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        PrepareState();
        _state.CancellationToken = stoppingToken;
        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Worker running at: {time}", GetNow());
        }
        _jsonSerializerOptions = CreateJsonSerializerOptions();
        _disableClientAppProcessStartLogs = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CAWS_DEBUG_DISABLE_CLIENT_APP_PROCESS_START_LOGS) == "true";
        // TODO: Bring this back
        //StartWebSocketConnector(stoppingToken);
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
        if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.ProcInfo.dwThreadId != 0) {
            processId = (int)_startProcessAsCurrentUserResult.ProcInfo.dwThreadId;
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
        return;
        ExecuteIfTraceIsEnabled(() => {
            _logger.LogTrace("Local client WebSocket connected");
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
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _state.CancellationToken);
                if (receiveResult.MessageType == WebSocketMessageType.Close || receiveResult.Count == 0) {
                    break;
                } else {
                    if (receiveResult.EndOfMessage) {
                        string stringData = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        ExecuteIfTraceIsEnabled(() => {
                            _logger.LogTrace(new EventId(5, "Received message from local client WebSocket"), "Received message from local client: {0}", stringData);
                        });
                        ProcessLocalClientWebSocketMessage(wsState, stringData);
                    } else {
                        // TODO: Not entire message is received - collect the data and process only after the entire message is recevied
                        ExecuteIfTraceIsEnabled(() => {
                            _logger.LogTrace("Partial message received from local client: {0}", buffer);
                        });
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error on receiving from local client WebSocket");
                break;
            }
        }
        ExecuteIfTraceIsEnabled(() => {
            _logger.LogTrace(
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

    private void ProcessLocalClientWebSocketMessage(ClientWebSocketState wsState, string stringData) {
        LocalClientPartialMessage partialMsg = DeserializeLocalClientPartialMessage(stringData);
        if (partialMsg?.Header?.Type == null) {
            // TODO: Can't process the message
            return;
        }
        // TODO: Should we set LastMessageReceivedAt only for known messages ?
        wsState.LastMessageReceivedAt = GetNow();
        string msgType = partialMsg.Header.Type;
        switch (msgType) {
            case LocalClientRequestMessageType.Ping:
                //var msg = DeserializeLocalClientRequestMessage<LocalClientPingRequestMessage, LocalClientPingRequestMessageBody>(stringData);
                // We don't need to process the ping message - LastMessageReceivedAt was already set
                break;
        }
    }

    private TMessage DeserializeLocalClientRequestMessage<TMessage, TBody>(string jsonString) where TMessage : LocalClientRequestMessage<TBody>, new() {
        var deserialized = JsonSerializer.Deserialize<LocalClientRequestMessage<object>>(jsonString, _jsonSerializerOptions);
        // TODO: Can we use reflection to infer the TBody ?
        var deserializedMsg = CreateTypedLocalClientRequestMessageFromGenericMessage<TBody>(deserialized);
        TMessage result = new TMessage();
        result.Header = deserializedMsg.Header;
        result.Body = deserializedMsg.Body;
        return result;
    }

    private LocalClientRequestMessage<TBody> CreateTypedLocalClientRequestMessageFromGenericMessage<TBody>(LocalClientRequestMessage<object> msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        LocalClientRequestMessage<TBody> deserializedMsg = new LocalClientRequestMessage<TBody>();
        deserializedMsg.Header = msg.Header;
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private LocalClientPartialMessage DeserializeLocalClientPartialMessage(string jsonString) {
        var deserialized = JsonSerializer.Deserialize<LocalClientPartialMessage>(jsonString, _jsonSerializerOptions);
        return deserialized;
    }

    private async void SendConfigurationNotificationMessage(WebSocket ws) {
        LocalClientConfigurationNotificationMessage msg = LocalClientConfigurationNotificationMessageHelper.CreateMessage();
        msg.Body.PingInterval = _state.LocalClientPingInterval;
        await SendLocalClientNotificationMessage(msg, ws);
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

        _wsConnector.Connected += _wsConnector_Connected;
        _wsConnector.Disconnected += _wsConnector_Disconnected;
        _wsConnector.ConnectError += _wsConnector_ConnectError;
        _wsConnector.ReceiveError += _wsConnector_ReceiveError;
        _wsConnector.ValidatingRemoteCertificate += _wsConnector_ValidatingRemoteCertificate;
        _wsConnector.DataReceived += _wsConnector_DataReceived;
        _wsConnector.SendDataError += _wsConnector_SendDataError;
        WebSocketConnectorConfig wsConnectorConfig = new() {
            ClientCertificate = clientCert,
            CancellationToken = cancellationToken,
            ReconnectDelay = TimeSpan.FromSeconds(3),
            ServerUri = pcConnectorBaseUri,
            ServerCertificateThumbprint = pcConnectorCertificateThumbprint,
        };
#if DEBUG
        //wsConnectorConfig.TrustAllServerCertificates = true;
#endif
        _wsConnector.Initialize(wsConnectorConfig);
        _wsConnector.Start();
    }

    private void _wsConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        _logger.LogInformation("Disconnected from the server");
    }

    private void _wsConnector_ReceiveError(object? sender, ReceiveErrorEventArgs e) {
        _logger.LogWarning(e.Exception, "Server data receive error");
    }

    private void _wsConnector_Connected(object? sender, ConnectedEventArgs e) {
        _logger.LogInformation("Connected to the server");
    }

    private void _wsConnector_SendDataError(object? sender, SendDataErrorEventArgs e) {
        _logger.LogWarning(e.Exception, "WebSocket send data error");
    }

    private void _wsConnector_DataReceived(object? sender, DataReceivedEventArgs e) {
        bool traceEnabled = _logger.IsEnabled(LogLevel.Trace);
        if (traceEnabled) {
            ExecuteIfTraceIsEnabled(() => {
                _logger.LogTrace("WebSocket data received. Bytes length: {0}, bytes: {1}",
                    e.Data.Length,
                    BitConverter.ToString(e.Data.ToArray())
                );
            });
        }
        try {
            string stringData = Encoding.UTF8.GetString(e.Data.ToArray());
            if (traceEnabled) {
                _logger.LogTrace("Received string '{0}'", stringData);
            }
            try {
                ProcessWebSocketConnectorReceivedData(stringData);
            } catch (Exception ex) {
                _logger.LogError(ex, "Can't process received data '{0}'", stringData);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Cannot process received data {0}", e.Data);
        }
    }

    private void ProcessWebSocketConnectorReceivedData(string stringData) {
        Message<object> msg = DeserializeMessage<object>(stringData);
        string msgType = msg.Header.Type;
        switch (msgType) {
            case MessageType.DeviceConfiguration:
                Message<DeviceConfigurationNotificationMessageBody> deviceConfigurationMsg = CreateTypedMessageFromGenericMessage<DeviceConfigurationNotificationMessageBody>(msg);
                ProcessDeviceConfigurationNotificationMessage(deviceConfigurationMsg);
                break;
            case MessageType.DeviceSetStatus:
                Message<DeviceSetStatusNotificationMessageBody> deviceSetStatusNotificationMsg = CreateTypedMessageFromGenericMessage<DeviceSetStatusNotificationMessageBody>(msg);
                ProcessDeviceSetStatusNotificationMessage(deviceSetStatusNotificationMsg);
                break;
        }
        //JsonSerializer.Deserialize<object>(deserialized.Body as JsonElement);
        //const bodyJsonElement = deserialized.Body as JsonElement;
    }

    private void ProcessDeviceConfigurationNotificationMessage(Message<DeviceConfigurationNotificationMessageBody> msg) {
        _deviceConfigMsg = msg;
    }

    private void ProcessDeviceSetStatusNotificationMessage(Message<DeviceSetStatusNotificationMessageBody> msg) {
        _deviceSetStatusMsg = msg;
        // TODO: send to local clients the new status
        var msgToSend = LocalClientStatusNotificationMessageHelper.CreateMessage();
        msgToSend.Body = new LocalClientStatusNotificationMessageBody {
            Started = msg.Body.Started,
            Amounts = msg.Body.Amounts,
        };
        string serialized = SerializeLocalClientNotificationMessage(msgToSend);
        SendToAllLocalClients(serialized);
    }

    private async void SendToAllLocalClients(string data) {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        ArraySegment<byte> arraySegment = new ArraySegment<byte>(bytes);
        foreach (var kvp in _state.WebSockets.Where(x => x.Key.State == WebSocketState.Open)) {
            var ws = kvp.Key;
            try {
                ExecuteIfTraceIsEnabled(() => {
                    _logger.LogTrace(new EventId(6, "Sending data to client WebSocket"), "Sending data to client WebSocket: {0}", data);
                });
                await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, _state.CancellationToken);
            } catch (Exception ex) {
                this._logger.LogError(ex, "Can't send data {0} to local client", data);
            }
        }
    }

    private Message<TBody> CreateTypedMessageFromGenericMessage<TBody>(Message<object> msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        Message<TBody> deserializedMsg = new Message<TBody>();
        deserializedMsg.Header = msg.Header;
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private Message<TBody> DeserializeMessage<TBody>(string jsonString) {
        var deserialized = JsonSerializer.Deserialize<Message<TBody>>(jsonString, _jsonSerializerOptions);
        return deserialized;
    }

    private TBody DeserializeBody<TBody>(string body) {
        var deserialized = JsonSerializer.Deserialize<TBody>(body, _jsonSerializerOptions);
        return deserialized;
    }

    private string SerializeMessage<TBody>(Message<TBody> msg) {
        var serializedString = JsonSerializer.Serialize(msg, _jsonSerializerOptions);
        return serializedString;
    }

    private string SerializeLocalClientNotificationMessage<TBody>(LocalClientNotificationMessage<TBody> msg) {
        var serializedString = JsonSerializer.Serialize(msg, _jsonSerializerOptions);
        return serializedString;
    }

    private void _wsConnector_ValidatingRemoteCertificate(object? sender, ValidatingRemoteCertificateArgs e) {
        _logger.LogInformation(
            "Validating remote server certificate{0}{1}{2}",
            Environment.NewLine,
            e.Certificate,
            Environment.NewLine,
            e.SslPolicyError
        );
    }

    private void _wsConnector_ConnectError(object? sender, ConnectErrorEventArgs e) {
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
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        // TODO: Bring this back
        //if (_deviceConfigMsg == null) {
        //    // Still did not received initial messages from server - probably this device 
        //    // is not part of the system / not active / no connection to the server
        //    return;
        //}
        // TODO: For testing only
        //#if DEBUG
        //        return;
        //#endif
        if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.Success) {
            // The proces seems started
            return;
        }

        // TODO: We could use environment variable
        // Get path using current path - the client app is in subfolder of current service executable path
        var clientAppProcessExecutableFullPath = GetClientExecutablePath();

        var sessions = ClientAppProcessController.GetSessions();
        LogClientAppProcessData(() => {
            if (_logger.IsEnabled(LogLevel.Trace)) {
                StringBuilder sb = new();
                foreach (var session in sessions) {
                    sb.AppendLine("Session id: " + session.SessionID);
                    sb.AppendLine("Session name: " + session.pWinStationName);
                    sb.AppendLine("Session state: " + session.State);
                    sb.AppendLine("-----------------");
                }
                _logger.LogTrace("Sessions: {sessions}", sb.ToString());
            }
        });
        ClientAppProcessController.WTS_SESSION_INFO? activeSession = sessions.FirstOrDefault(x => x.State == ClientAppProcessController.WTS_CONNECTSTATE_CLASS.WTSActive);
        if (activeSession is not null) {
            LogClientAppProcessData(() => {
                _logger.LogTrace("ActiveSession: {0}", activeSession.Value.SessionID);
            });
            // TODO: Check if the app already runs
            Process? clientAppProcess = GetProcessByExecutablePath(clientAppProcessExecutableFullPath, (int)activeSession.Value.SessionID);
            if (clientAppProcess == null) {
                LogClientAppProcessData(() => {
                    _logger.LogTrace("It seems that process with path {0} does not run in session {1}", clientAppProcessExecutableFullPath, activeSession.Value.SessionID);
                });
                if (_startProcessAsCurrentUserResult != null && _startProcessAsCurrentUserResult.ProcInfo.hProcess != 0) {
                    ClientAppProcessController.CloseProcInfoHandles(_startProcessAsCurrentUserResult.ProcInfo);
                }
                LogClientAppProcessData(() => {
                    _logger.LogTrace("Trying to start the process: {clientAppProcessExecutableFullPath}", clientAppProcessExecutableFullPath);
                });
                _startProcessAsCurrentUserResult = ClientAppProcessController.StartProcessAsCurrentUser(clientAppProcessExecutableFullPath, null, null, true, _logger);
                if (_startProcessAsCurrentUserResult.Success) {
                    LogClientAppProcessData(() => {
                        _logger.LogTrace("Process handle: {hProcess}", _startProcessAsCurrentUserResult.ProcInfo.hProcess);
                    });
                    LogClientAppProcessData(() => {
                        _logger.LogTrace("Process {clientAppProcessExecutableFullPath} started. PID: {pid}", clientAppProcessExecutableFullPath, _startProcessAsCurrentUserResult.ProcInfo.dwProcessId);
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
    }

    private void PingServer() {
        if (_state.CancellationToken.IsCancellationRequested) {
            return;
        }
        if (_deviceConfigMsg is null) {
            return;
        }
        if (_deviceConfigMsg.Body is null
            || _deviceConfigMsg.Body.PingInterval <= 0) {
            ExecuteIfTraceIsEnabled(() => {
                _logger.LogTrace("Can't ping server. Device configuration ping interval {0} is invalid", _deviceConfigMsg.Body?.PingInterval);
            });
            return;
        }

        try {
            TimeSpan diff = GetNow() - _state.LastServicePingDateTime;
            if (diff.TotalMilliseconds >= _deviceConfigMsg.Body.PingInterval) {
                _state.LastServicePingDateTime = DateTime.UtcNow;
                SendPingMessage();
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Can't ping the server");
        }
    }

    private void SendPingMessage() {
        Message<PingMessageBody> pingMsg = new() {
            Header = new MessageHeader { Type = MessageType.Ping },
            Body = new PingMessageBody(),
        };
        SendMessage(pingMsg);
    }

    private void SendMessage<TBody>(Message<TBody> msg) {
        string serialized = SerializeMessage(msg);
        ReadOnlyMemory<byte> buffer = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastDataSentAt = GetNow();
        _wsConnector.SendData(buffer);
    }

    private async Task<bool> SendLocalClientNotificationMessage<TBody>(LocalClientNotificationMessage<TBody> msg, WebSocket ws) {
        string serialized = SerializeLocalClientNotificationMessage(msg);
        ReadOnlyMemory<byte> bytes = new(Encoding.UTF8.GetBytes(serialized));
        _state.LastLocalClientDataSentAt = GetNow();
        try {
            ExecuteIfTraceIsEnabled(() => {
                _logger.LogTrace("Sending local client notification message: {0}", serialized);
            });
            await ws.SendAsync(bytes, WebSocketMessageType.Text, true, _state.CancellationToken);
            return true;
        } catch (Exception ex) {
            _logger.LogError(ex, "Can't send LocalClientNotificationMessage message");
            //SendDataError?.Invoke(this, new SendDataErrorEventArgs { Exception = ex });
            return false;
        }
        //_wsConnector.SendData(buffer);
    }

    public DateTimeOffset GetNow() {
        return DateTimeOffset.Now;
    }

    private void ExecuteIfTraceIsEnabled(Action action) {
        if (!_logger.IsEnabled(LogLevel.Trace)) {
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
    private Process? GetProcessByExecutablePath(string executablePath, int sessionId = -1) {
        IEnumerable<Process> processes;
        if (sessionId != -1) {
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
        public int LocalClientPingInterval { get; set; }
        public ConcurrentDictionary<WebSocket, ClientWebSocketState> WebSockets { get; set; }
    }
}
