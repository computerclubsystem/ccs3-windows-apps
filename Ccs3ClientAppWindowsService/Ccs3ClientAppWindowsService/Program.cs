using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Ccs3ClientAppWindowsService;

public class Program {
    private static ILogger _logger;
    private static readonly string _serviceName = "Ccs3ClientAppWindowsService";

    public static void Main(string[] args) {
        var (loggerFactory, logger) = CreateLogger();
        _logger = logger;
        var builder = CreateAppBuilder(args);
        var app = builder.Build();
        Directory.SetCurrentDirectory(builder.Environment.ContentRootPath);
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.Run();
    }

    private static WebApplicationBuilder CreateAppBuilder(string[] args) {
        WebApplicationOptions webApplicationOptions = new() {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "static-web-files"),
        };
        //var builder = Host.CreateApplicationBuilder(settings);
        var builder = WebApplication.CreateBuilder(webApplicationOptions);
        builder.Services.AddWindowsService(options => {
            options.ServiceName = _serviceName;
        });
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
            _logger.LogInformation("Trying to listen to '{0}:{1}' ('{2}')", preferredIPAddress.ToString(), listenUri.Port, listenUri.ToString());
            serverOptions.Listen(preferredIPAddress, listenUri.Port, listenOptions => {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                listenOptions.UseHttps(httpsConfig => {
                    httpsConfig.ServerCertificate = GetLocalServiceCertificate();
                    httpsConfig.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;
                });
            });
        });

        builder.Services.AddHostedService<Worker>();
        return builder;
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
            _logger.LogCritical("Cannot find value for the enivonment variable {0}", Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return (null, localBaseUrlEnvironmentVariableValue);
        }
        Uri.TryCreate(localBaseUrlEnvironmentVariableValue, UriKind.Absolute, out Uri? localBaseUri);
        if (localBaseUri is null) {
            _logger.LogCritical("The value '{0}' for environment variable {1} is invalid URL", localBaseUrlEnvironmentVariableValue, Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return (null, localBaseUrlEnvironmentVariableValue);
        }
        return (localBaseUri, localBaseUrlEnvironmentVariableValue);
    }

    private static X509Certificate2? GetLocalServiceCertificate() {
        (Uri? listenUri, string? envVarValue) = GetLocalBaseUri();
        if (listenUri is null) {
            _logger.LogCritical("The value '{0}' for the environment variable '{1}' is missing or invalid URL", envVarValue, Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return null;
        }
        // TODO: This must provided as environment variable
        //       or provide the issuer certificate thumbprint and search for it instead of using Certificates.Find(X509FindType.FindByIssuerName)
        string caCertCN = "CCS3 Doom Certificate Authority";
        using X509Store x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        var certsIssuedByCcs3RootCa = x509Store.Certificates.Find(X509FindType.FindByIssuerName, caCertCN, false);
        _logger.LogInformation("Certificates issued by '{0}':{1}{2}", caCertCN, Environment.NewLine, string.Join(Environment.NewLine, certsIssuedByCcs3RootCa.Select(x => x.Subject + " (thumbprint " + x.Thumbprint + ")")));
        var pcCerts = certsIssuedByCcs3RootCa.Find(X509FindType.FindBySubjectName, listenUri.Host, false);
        _logger.LogInformation("Certificates with subject '{0}':{1}{2}", listenUri.Host, Environment.NewLine, string.Join(Environment.NewLine, pcCerts.Select(x => x.Subject + " (thumbprint " + x.Thumbprint + ")")));
        if (pcCerts.Count == 0) {
            _logger.LogCritical("Cannot find certificate issued by '{0}' with subject name '{1}'", caCertCN, listenUri.Host);
        }
        var cert = pcCerts.FirstOrDefault();
        return cert;
    }

    private static string? GetLocalBaseUrlEnvironmentVariableValue() {
        return Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);
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
        public static readonly string CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL = "CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL";
    }
}