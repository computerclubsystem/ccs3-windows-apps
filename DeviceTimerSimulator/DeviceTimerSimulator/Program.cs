using DeviceTimerSimulator.Messages;
using DeviceTimerSimulator.WebSocketConnector;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace DeviceTimerSimulator;

internal class App {
    private readonly string _cmdParamPrefix = "--";
    private WSConnector _wsConnector;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    public void Start(string[] args) {
        CommandLineParams cmdParams = ParseCommandLineParams(args);
        _wsConnector = new WSConnector();
        WSConnectorSettings wsConnectorSettings = new() {
            //ClientCertificateCertFileText = File.ReadAllText(cmdParams.ClientCertificateCertFilePath),
            //ClientCertificateKeyFileText = File.ReadAllText(cmdParams.ClientCertificateKeyFilePath),
            ServerCertificateThumbnail = cmdParams.ServerCertificateThumbnail,
            Uri = new Uri(cmdParams.Uri),
        };
        wsConnectorSettings.ClientCertificate = CreateClientCertificate(cmdParams.ClientCertificateCertFilePath, cmdParams.ClientCertificateKeyFilePath);
        Log("Server Uri: {0}", wsConnectorSettings.Uri);
        _wsConnector.Init(wsConnectorSettings);
        _wsConnector.SetConnectedAction(WSConnectorConnected);
        _wsConnector.SetDisconnectedAction(WSConnectorDisconnected);
        _wsConnector.SetExceptionAction(WSConnectorException);
        _wsConnector.SetDataReceivedAction(WSConnectorDataReceived);
        _wsConnector.Connect();
        Console.WriteLine("Type h <ENTER> for help");
        while (true) {
            string? line = Console.ReadLine();
            switch (line) {
                case "s":
                    StartSending();
                    break;
                case "h":
                    break;
                case "q":
                    break;
            }
        }
    }

    public void StartSending() {
        Task.Run(async () => {
            while (!_cancellationTokenSource.IsCancellationRequested) {
                var msg = new DeviceStatusDeviceMessage();
                msg.Body = new DeviceStatusDeviceMessageBody();
                msg.Body.CpuTemp = 40 + 30 * Random.Shared.NextSingle();
                msg.Body.CpuUsage = 50 * Random.Shared.NextSingle();
                msg.Body.CurrentTime = DateTimeOffset.Now;
                msg.Body.StorageFreeSpace = Random.Shared.Next(100000000, 1000000000);
                string json = SerializeInstance(msg);
                try {
                    await _wsConnector.SendText(json);
                } catch (Exception ex) {
                    Log("Cannot send text: {0}", GetAllExceptionMessagesString(ex));
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        });
    }

    public string SerializeInstance(object instance) {
        string result = JsonSerializer.Serialize(instance, _jsonSerializerOptions);
        return result;
    }

    public void WSConnectorDataReceived(byte[] data) {
        LogWSConnector("Data received");
    }

    public void WSConnectorConnected() {
        LogWSConnector("Connected");
    }

    public void WSConnectorDisconnected() {
        LogWSConnector("Disconnected");
    }

    public void WSConnectorException(Exception ex) {
        string msg = GetAllExceptionMessagesString(ex);
        LogWSConnector("Exception: {0}", msg);
    }

    public void LogWSConnector(string message, params object?[]? arg) {
        Log("WSConnector: " + message, arg);
    }

    public void Log(string message, params object?[]? arg) {
        Console.WriteLine(message, arg);
    }

    public X509Certificate2 CreateClientCertificate(string clientCertFilePath, string clientKeyFilePath) {
        X509Certificate2 cert = X509Certificate2.CreateFromPem(
            File.ReadAllText(clientCertFilePath).AsSpan(),
            File.ReadAllText(clientKeyFilePath).AsSpan()
        );
        // This is needed, otherwise the key of certificates loaded from arbitrary file location is considered
        // "ephemeral" and connect fails with exception
        // See https://stackoverflow.com/questions/72096812/loading-x509certificate2-from-pem-file-results-in-no-credentials-are-available/72101855#72101855
        // "The TLS layer on Windows requires that the private key be written to disk (in a particular way). The PEM-based certificate loading doesn't do that, only PFX-loading does."
        // "That is, export the cert+key to a PFX, then import it again immediately (to get the side effect of the key being (temporarily) written to disk in a way that SChannel can find it)."
        cert = new X509Certificate2(cert.Export(X509ContentType.Pfx));
        return cert;
    }

    public string[] GetAllExceptionMessagesArray(Exception ex) {
        List<string> messages = [ex.Message];
        Exception? innerEx = ex.InnerException;
        while (innerEx != null) {
            messages.Add(innerEx.Message);
            innerEx = innerEx.InnerException;
        }
        return messages.ToArray();
    }

    public string GetAllExceptionMessagesString(Exception ex, string? separator = null) {
        return string.Join(separator ?? ", ", GetAllExceptionMessagesArray(ex));
    }

    public CommandLineParams ParseCommandLineParams(string[] args) {
        CommandLineParams cmdParams = new();
        int argIndex = 0;
        while (argIndex < args.Length) {
            string arg = args[argIndex];
            if (CheckParamName(arg, CommandLineParamsName.Uri)) {
                cmdParams.Uri = args[++argIndex];
            } else if (CheckParamName(arg, CommandLineParamsName.ServerCertificateThumbnail)) {
                cmdParams.ServerCertificateThumbnail = args[++argIndex];
            } else if (CheckParamName(arg, CommandLineParamsName.ClientCertificateCertFilePath)) {
                cmdParams.ClientCertificateCertFilePath = args[++argIndex];
            } else if (CheckParamName(arg, CommandLineParamsName.ClientCertificateKeyFilePath)) {
                cmdParams.ClientCertificateKeyFilePath = args[++argIndex];
            } else {
                throw new ArgumentException($"Unknown argument {arg}");
            }
            argIndex++;
        }
        return cmdParams;
    }

    public bool CheckParamName(string param, string checkFor) {
        return _cmdParamPrefix + checkFor == param;
    }
}

internal class Program {
    static void Main(string[] args) {
        var app = new App();
        app.Start(args);
    }
}

internal class CommandLineParams {
    public string Uri { get; set; }
    // public string ClientCertificatePemFileText { get; set; }
    public string ClientCertificateCertFilePath { get; set; }
    public string ClientCertificateKeyFilePath { get; set; }
    public string ServerCertificateThumbnail { get; set; }
}

internal class CommandLineParamsName {
    public static readonly string Uri = "uri";
    public static readonly string ClientCertificateCertFilePath = "client-certificate-cert-file-path";
    public static readonly string ClientCertificateKeyFilePath = "client-certificate-key-file-path";
    public static readonly string ServerCertificateThumbnail = "server-certificate-thumbnail";
}