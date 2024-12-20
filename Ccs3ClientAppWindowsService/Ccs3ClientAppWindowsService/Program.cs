using Ccs3ClientAppWindowsService.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Ccs3ClientAppWindowsService;

public class Program {
    private static ILogger _logger;
    private static readonly string _serviceName = "Ccs3ClientAppWindowsService";
    private static readonly CertificateHelper _certificateHelper = new();

    public static void Main(string[] args) {
        var builder = CreateAppBuilder(args);
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        var (loggerFactory, logger) = CreateLogger();
        _logger = logger;
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
        builder.Services.AddSingleton<CertificateHelper>(_certificateHelper);
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
            _logger.LogInformation("Trying to listen to '{0}:{1}' ('{2}')", preferredIPAddress.ToString(), listenUri.Port, listenUri.ToString());
            serverOptions.Listen(preferredIPAddress, listenUri.Port, listenOptions => {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                listenOptions.UseHttps(httpsConfig => {
                    httpsConfig.ServerCertificate = GetLocalServiceCertificate();
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
            _logger.LogCritical("The value '{0}' of the environment variable '{1}' is missing or invalid URL", envVarValue, Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);
            return null;
        }
        string? caCertThumbprint = Environment.GetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT);
        if (caCertThumbprint is null) {
            _logger.LogCritical("The value of the environment variable '{0}' is missing. It must contain the value of the CA certificate thumbprint", Ccs3EnvironmentVariableNames.CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT);
            return null;
        }
        string localCertificateCNToSearchFor = listenUri.Host;
        var certResult = _certificateHelper.GetPersonalCertificateIssuedByTrustedRootCA(caCertThumbprint, localCertificateCNToSearchFor);
        if (!certResult.IssuerFound) {
            _logger.LogCritical("Trusted root certificate with thumbprint '{0}' was not found", caCertThumbprint);
            return null;
        }
        if (certResult.PersonalCertificate == null) {
            _logger.LogCritical("Local service certificate with subject '{0}' issued by certificate with thumbprint {1} was not found", localCertificateCNToSearchFor, caCertThumbprint);
            return null;
        }
        return certResult.PersonalCertificate;
        //var clientCert 
        //using X509Store trustedRootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        //var caCert = trustedRootStore.Certificates.Find(X509FindType.FindByThumbprint, caCertThumbprint, false).FirstOrDefault();
        //if (caCert is null) {
        //    _logger.LogCritical("The CA certificate thumbprint {0} specified in the environment variable {1} is not found. Make sure CA certificate (.crt file) is installed in Certificates - Local Computer - Trusted Root Certification Authorities", caCertThumbprint, Ccs3EnvironmentVariableNames.CCS3_CERTIFICATE_AUTHORITY_CERTIFICATE_THUMBPRINT);
        //    return null;
        //}

        //string? caCertNameString = caCert.SubjectName.ToString();
        //using X509Store personalStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly);
        //string clientCertSimpleNameToSearchFor = Environment.MachineName;
        //var clientCert = personalStore.Certificates.FirstOrDefault(cert => {
        //    if (cert.Issuer != caCert.Subject) {
        //        return false;
        //    }
        //    string certSimpleName = cert.GetNameInfo(X509NameType.SimpleName, false);
        //    return string.Equals(certSimpleName, clientCertSimpleNameToSearchFor, StringComparison.OrdinalIgnoreCase);
        //});
        //return clientCert;
        //x509Store.Certificates.Where(x => x.IssuerName.)
        //var certsIssuedByCcs3RootCa = caCert.Find(X509FindType.findbyis, caCertCN, false);
        //_logger.LogInformation("Certificates issued by '{0}':{1}{2}", caCert.Subject, Environment.NewLine, string.Join(Environment.NewLine, certsIssuedByCcs3RootCa.Select(x => x.Subject + " (thumbprint " + x.Thumbprint + ")")));
        //var pcCerts = certsIssuedByCcs3RootCa.Find(X509FindType.FindBySubjectName, listenUri.Host, false);
        //_logger.LogInformation("Certificates with subject '{0}':{1}{2}", listenUri.Host, Environment.NewLine, string.Join(Environment.NewLine, pcCerts.Select(x => x.Subject + " (thumbprint " + x.Thumbprint + ")")));
        //if (pcCerts.Count == 0) {
        //    _logger.LogCritical("Cannot find certificate issued by '{0}' with subject name '{1}'", caCertCN, listenUri.Host);
        //}
        //var cert = pcCerts.FirstOrDefault();
        //return cert;
    }

    /// <summary>
    /// Seaches certificate issued by the trusted CA with the name of the local machine to use it to connect to servers
    /// </summary>
    /// <returns>Client certificate for authenticating against servers or null if not found</returns>

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
        public static readonly string CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT = "CCS3_CERTIFICATE_AUTHORITY_ISSUER_CERTIFICATE_THUMBPRINT";
    }
}