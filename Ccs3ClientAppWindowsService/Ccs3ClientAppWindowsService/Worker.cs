using Ccs3ClientAppWindowsService.Messages;
using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
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
    Message<DeviceConfigurationMessageBody> _deviceConfigMsg;
    System.Threading.Timer _timer;
    private WorkerState _state = new WorkerState();

    public Worker(
        ILogger<Worker> logger
    ) {
        _logger = logger;
        _timer = new Timer(TimerCallbackFunction, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        PrepareState();
        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Worker running at: {time}", GetNow());
        }
        _jsonSerializerOptions = CreateJsonSerializerOptions();
        _disableClientAppProcessStartLogs = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CAWS_DEBUG_DISABLE_CLIENT_APP_PROCESS_START_LOGS) == "true";
        StartWebSocketConnector(stoppingToken);
        while (!stoppingToken.IsCancellationRequested) {
            StartClientAppIfNotStarted();
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
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
        _wsConnector.Initialize(wsConnectorConfig);
        _wsConnector.Start();
    }

    private void _wsConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        _logger.LogWarning("Disconnected from the server");
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
            _logger.LogTrace("WebSocket data received. Bytes length: {0}, bytes: {1}",
                e.Data.Length,
                BitConverter.ToString(e.Data.ToArray())
            );
        }
        try {
            string stringData = Encoding.UTF8.GetString(e.Data.ToArray());
            if (traceEnabled) {
                _logger.LogTrace("Received string '{0}'", stringData);
            }
            try {
                this.ProcessReceivedMessage(stringData);
            } catch (Exception ex) {
                _logger.LogError(ex, "Can't process received data '{0}'", stringData);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Cannot process received data {0}", e.Data);
        }
    }

    private void ProcessReceivedMessage(string stringData) {
        Message<object> msg = DeserializeMessage<object>(stringData);
        string msgType = msg.Header.Type;
        switch (msgType) {
            case MessageType.DeviceConfiguration:
                Message<DeviceConfigurationMessageBody> deviceConfigurationMsg = CreateTypedMessageFromGenericMessage<DeviceConfigurationMessageBody>(msg);
                ProcessDeviceConfigurationMessage(deviceConfigurationMsg);
                break;
        }
        //JsonSerializer.Deserialize<object>(deserialized.Body as JsonElement);
        //const bodyJsonElement = deserialized.Body as JsonElement;
    }

    private void ProcessDeviceConfigurationMessage(Message<DeviceConfigurationMessageBody> msg) {
        _deviceConfigMsg = msg;
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

    private void StartClientAppIfNotStarted() {
        // TODO: For testing only
        return;
        // TODO: Get path using current path - the client app is in subfolder of current service executable path
        var clientAppProcessExecutableFullPath = Path.GetFullPath(Path.Combine(".", "ClientApp\\Ccs3ClientApp.exe"));

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
        ClientAppProcessController.WTS_SESSION_INFO? activeSession = sessions.FirstOrDefault(x => x.State == ClientAppProcessController.WTS_CONNECTSTATE_CLASS.WTSActive);
        if (activeSession is not null) {
            // TODO: Check if the app already runs
            Process? clientAppProcess = GetProcessByExecutablePath((int)activeSession.Value.SessionID, clientAppProcessExecutableFullPath);
            if (clientAppProcess == null) {
                if (_startProcessAsCurrentUserResult?.ProcInfo != null) {
                    ClientAppProcessController.CloseProcInfoHandles(_startProcessAsCurrentUserResult.ProcInfo);
                }
                LogClientAppProcessData(() => {
                    _logger.LogInformation("Trying to start the process: {clientAppProcessExecutableFullPath}", clientAppProcessExecutableFullPath);
                });
                _startProcessAsCurrentUserResult = ClientAppProcessController.StartProcessAsCurrentUser(clientAppProcessExecutableFullPath);
                LogClientAppProcessData(() => {
                    _logger.LogInformation("Process handle: {hProcess}", _startProcessAsCurrentUserResult.ProcInfo.hProcess);
                });
                if (_startProcessAsCurrentUserResult.Success) {
                    LogClientAppProcessData(() => {
                        _logger.LogInformation("Process {clientAppProcessExecutableFullPath} started. PID: {pid}", clientAppProcessExecutableFullPath, _startProcessAsCurrentUserResult.ProcInfo.dwProcessId);
                    });
                    // TODO: Should we call WaitForInputIdle to know when the process has finished initialization ?
                    Process pc = Process.GetProcessById((int)_startProcessAsCurrentUserResult.ProcInfo.dwProcessId);
                    // TODO: Provide cancellation token
                    WaitForProcessToExit(_startProcessAsCurrentUserResult.ProcInfo.hProcess).ContinueWith(task => {
                        LogClientAppProcessData(() => {
                            _logger.LogInformation("Client app process has exited");
                        });
                    });
                    LogClientAppProcessData(() => {
                        _logger.LogInformation("Waiting for the ClientApp process to exit");
                    });
                } else {
                    LogClientAppProcessData(() => {
                        _logger.LogWarning("Can't start the process {clientAppProcessExecutableFullPath}. WinAPI errors: {lastErrors}", clientAppProcessExecutableFullPath, _startProcessAsCurrentUserResult.LastErrors);
                    });
                }
            }
        }
    }

    private void TimerCallbackFunction(object? state) {
        PingServer();
    }

    private void PingServer() {
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
    }

    private void LogClientAppProcessData(Action logAction) {
        if (_disableClientAppProcessStartLogs) {
            return;
        }
        logAction();
    }
    private Process? GetProcessByExecutablePath(int sessionId, string executablePath) {
        var processes = Process.GetProcesses().Where(x => x.SessionId == sessionId);
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

    // TODO: Accept CancellationToken
    private Task WaitForProcessToExit(nint processHandle) {
        var task = Task.Run(() => {
            EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
            ewh.SafeWaitHandle = new SafeWaitHandle(_startProcessAsCurrentUserResult!.ProcInfo.hProcess, true);
            bool waitOneResult = ewh.WaitOne();
        });
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

    private class WorkerState {
        public DateTimeOffset LastServicePingDateTime { get; set; }
        public DateTimeOffset? LastDataSentAt { get; set; }
    }
}
