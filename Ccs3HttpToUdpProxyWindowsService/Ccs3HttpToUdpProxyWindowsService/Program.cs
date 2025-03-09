using Ccs3HttpToUdpProxyWindowsService.Entities;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Ccs3HttpToUdpProxyWindowsService;

public class Program {
    private static ILogger _logger;
    private static readonly string _serviceName = "Ccs3HttpToUdpWindowsService";
    private static X509Certificate2? _localCert;
    private static WebApplicationBuilder _builder;
    private static WebApplication _app;
    private static Dictionary<string, string> _requiredHeaders = new Dictionary<string, string>();
    private static bool _sendingUdpPackets = false;
    private static string? _allowedIpAddress = null;
    enum ProcessExitCode {
        LocalServiceCertificateNotFound = 1
    }

    public static int Main(string[] args) {
        var builder = CreateAppBuilder(args);
        _builder = builder;
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        var (loggerFactory, logger) = CreateLogger();
        _logger = logger;
        _localCert = GetLocalServiceCertificate();
        if (_localCert == null) {
            var exitCode = (int)ProcessExitCode.LocalServiceCertificateNotFound;
            Environment.ExitCode = exitCode;
            return exitCode;
        }
        var app = builder.Build();
        _app = app;
        Worker worker = (Worker)app.Services.GetServices<IHostedService>().First(x => x.GetType() == typeof(Worker));
        worker.SetAppStoppingCancellationToken(app.Lifetime.ApplicationStopping);
        Directory.SetCurrentDirectory(builder.Environment.ContentRootPath);
        string? requiredHeadersEnvValue = GetRequiredRequestHeadersVariableValue();
        if (!string.IsNullOrWhiteSpace(requiredHeadersEnvValue)) {
            string[] headerItems = requiredHeadersEnvValue.Split(';');
            foreach (string headerItem in headerItems) {
                string[] headerParts = headerItem.Split(":");
                _requiredHeaders.TryAdd(headerParts[0], headerParts[1]);
            }
        }
        _allowedIpAddress = GetAllowedSourceIpAddressVariableValue();
        app.MapPost("/send-packets", (HttpContext ctx, SendPacketsRequest payload) => {
            var remoteIpAddress = ctx.Connection.RemoteIpAddress?.ToString();
            if (!ValidateSourceIpAddress(_allowedIpAddress, remoteIpAddress)) {
                _logger.LogWarning($"Specified remote IP address {remoteIpAddress} is not allowed");
                ctx.Response.StatusCode = 403;
                return;
            }
            if (!ValidateRequiredHeaders(ctx.Request.Headers, _requiredHeaders)) {
                _logger.LogWarning($"Required header is not provided or its value does not match");
                ctx.Response.StatusCode = 400;
                return;
            }
            SendUdpPackets(payload);
        });
        app.Run();
        return 0;
    }

    private static async void SendUdpPackets(SendPacketsRequest payload) {
        if (_sendingUdpPackets) {
            return;
        }
        _sendingUdpPackets = true;
        using UdpClient udpClient = new UdpClient();

        for (int i = 0; i < payload.PacketsData.Length; i++) {
            var packetData = payload.PacketsData[i];
            try {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(packetData.DestinationIpAddress), packetData.DestinationPort);
                byte[] packetBytes = Convert.FromHexString(packetData.PacketHexString);
                await udpClient.SendAsync(packetBytes, packetBytes.Length, endpoint);
                if (payload.DelayBetweenPacketsMilliseconds > 0 && i < payload.PacketsData.Length) {
                    await Task.Delay(payload.DelayBetweenPacketsMilliseconds);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Can't send packet to {packetData.DestinationIpAddress}:{packetData.DestinationPort}");
            }
        }
        _sendingUdpPackets = false;
    }

    private static bool ValidateSourceIpAddress(string? allowedIpAddress, string? requestIpAddress) {
        if (string.IsNullOrWhiteSpace(allowedIpAddress)) {
            return true;
        }
        return allowedIpAddress == requestIpAddress;

    }

    private static bool ValidateRequiredHeaders(IHeaderDictionary requestHeaders, Dictionary<string, string> requiredHeaders) {
        if (_requiredHeaders.Count == 0) {
            return true;
        }
        foreach (KeyValuePair<string, string> kvp in requiredHeaders) {
            requestHeaders.TryGetValue(kvp.Key, out StringValues headerValues);
            string joinedValues = string.Join(",", headerValues.ToArray());
            if (kvp.Value != joinedValues) {
                return false;
            }
        }
        return true;
    }

    private static X509Certificate2? GetLocalServiceCertificate() {
        (Uri? listenUri, string? envVarValue) = GetLocalBaseUri();
        if (listenUri is null) {
            _logger.LogCritical("The value '{0}' of the environment variable '{1}' is missing or invalid URL", envVarValue, Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return null;
        }
        string? certificateThumbprint = GetCertificateThumbprintEnvironmentVariableValue();
        if (certificateThumbprint is null) {
            _logger.LogCritical("The value of the environment variable '{0}' is empty", Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_CERTIFICATE_THUMBPRINT);
            return null;
        }
        using X509Store trustedRootStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        var cert = trustedRootStore.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false).FirstOrDefault();
        if (cert is null) {
            _logger.LogCritical("Personal certificate with thumbprint '{0}' not found", certificateThumbprint);
            return null;
        }

        _logger.LogInformation("Personal certificate found {0} ({1})", cert.Subject, cert.Thumbprint);
        return cert;
    }

    private static WebApplicationBuilder CreateAppBuilder(string[] args) {
        WebApplicationOptions webApplicationOptions = new() {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        };
        //var builder = Host.CreateApplicationBuilder(settings);
        var builder = WebApplication.CreateBuilder(webApplicationOptions);
        builder.Services.AddWindowsService(options => {
            options.ServiceName = _serviceName;
        });
        ConfigureKestrel(builder);
        builder.Services.AddHostedService<Worker>();
        return builder;
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder) {
        builder.WebHost.ConfigureKestrel((context, serverOptions) => {
            (Uri? listenUri, string? envVarValue) = GetLocalBaseUri();
            if (listenUri is null) {
                _logger.LogCritical("Cannot listen at '{0}'", envVarValue);
                return;
            }

            IPAddress[] resolvedIPAddresses;
            try {
                resolvedIPAddresses = System.Net.Dns.GetHostAddresses(listenUri.Host);
            } catch (Exception ex) {
                _logger.LogCritical("Cannot resolve host '{0}'. Error: {1}", listenUri.Host, ex.ToString());
                return;
            }
            if (resolvedIPAddresses.Length == 0) {
                _logger.LogCritical("No IP addresses were resolved for host name '{0}'", listenUri.Host);
                return;
            }
            IPAddress preferredIPAddress = GetPreferredIPAddressForListen(resolvedIPAddresses);
            _logger.LogInformation("Trying to listen on '{0}' ('{1}:{2}')", listenUri.ToString(), preferredIPAddress.ToString(), listenUri.Port);
            serverOptions.Listen(preferredIPAddress, listenUri.Port, listenOptions => {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                listenOptions.UseHttps(httpsConfig => {
                    httpsConfig.ServerCertificate = _localCert;
                    // httpsConfig.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;
                });
            });
        });
    }

    private static IPAddress GetPreferredIPAddressForListen(IPAddress[] ipAddresses) {
        IPAddress? preferredIPAddress = ipAddresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        if (preferredIPAddress is null) {
            preferredIPAddress = ipAddresses.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        }
        if (preferredIPAddress is null) {
            preferredIPAddress = ipAddresses[0];
        }
        return preferredIPAddress;
    }

    private static (Uri?, string? envVariableValue) GetLocalBaseUri() {
        string? localBaseUrlEnvironmentVariableValue = GetLocalBaseUrlEnvironmentVariableValue();
        if (localBaseUrlEnvironmentVariableValue is null) {
            _logger.LogCritical("Cannot find value for the enivonment variable {0}", Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return (null, localBaseUrlEnvironmentVariableValue);
        }
        Uri.TryCreate(localBaseUrlEnvironmentVariableValue, UriKind.Absolute, out Uri? localBaseUri);
        if (localBaseUri is null) {
            _logger.LogCritical("The value '{0}' for environment variable {1} is invalid URL", localBaseUrlEnvironmentVariableValue, Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return (null, localBaseUrlEnvironmentVariableValue);
        }
        return (localBaseUri, localBaseUrlEnvironmentVariableValue);
    }

    private static string? GetCertificateThumbprintEnvironmentVariableValue() {
        return Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_CERTIFICATE_THUMBPRINT, EnvironmentVariableTarget.Machine);
    }

    private static string? GetLocalBaseUrlEnvironmentVariableValue() {
        return Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL, EnvironmentVariableTarget.Machine);
    }

    private static string? GetRequiredRequestHeadersVariableValue() {
        return Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_REQUIRED_REQUEST_HEADERS);
    }

    private static string? GetAllowedSourceIpAddressVariableValue() {
        return Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_ALLOWED_SOURCE_IP_ADDRESS);
    }

    private static (ILoggerFactory, ILogger) CreateLogger() {
        var configuration = new ConfigurationBuilder()
          .AddEnvironmentVariables()
          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
          .Build();
        var loggerFactory = LoggerFactory.Create(loggingBuilder => {
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings { SourceName = _serviceName });
            loggingBuilder.AddDebug();
        });
        var logger = loggerFactory.CreateLogger<Program>();
        return (loggerFactory, logger);
    }

    private static class Ccs3EnvironmentVariableNames {
        public static readonly string CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL = "CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_LOCAL_BASE_URL";
        public static readonly string CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_CERTIFICATE_THUMBPRINT = "CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_CERTIFICATE_THUMBPRINT";
        public static readonly string CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_REQUIRED_REQUEST_HEADERS = "CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_REQUIRED_REQUEST_HEADERS";
        public static readonly string CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_ALLOWED_SOURCE_IP_ADDRESS = "CCS3_HTTP_TO_UDP_WINDOWS_SERVICE_ALLOWED_SOURCE_IP_ADDRESS";
    }
}