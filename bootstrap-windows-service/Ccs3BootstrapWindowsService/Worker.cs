using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text;

namespace Ccs3ClientAppBootstrapWindowsService;

public class Worker(
    ILogger<Worker> logger,
    HttpDownloader httpDownloader
//IHostApplicationLifetime hostApplicationLifetime
) : BackgroundService {
    //private readonly ILogger<Worker> _logger;
    //private readonly IHostApplicationLifetime _hostApplicationLifetime;

    //public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime) {
    //    _logger = logger;
    //    _hostApplicationLifetime = hostApplicationLifetime;
    //}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // The following steps must be performed
        // - Calculate the checksum of existing files
        // - Download checksum from the static files service
        // - If checksums does not match:
        //   - Download .zip file
        //   - Stop current service
        //   - Delete the old files
        //   - Extract the downloaded .zip file
        //   - Register extracted executable file as windows service
        //   - Start the app service
        //   - If checksums match
        //   - Start the app service

        bool canLogInformation = logger.IsEnabled(LogLevel.Information);
        bool canLogCritical = logger.IsEnabled(LogLevel.Critical);
        bool canLogWarning = logger.IsEnabled(LogLevel.Warning);

        if (canLogInformation) {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        string? staticFileServiceBaseUrl = Environment.GetEnvironmentVariable("CCS3_STATIC_FILES_SERVICE_BASE_URL");
        if (string.IsNullOrWhiteSpace(staticFileServiceBaseUrl)) {
            if (canLogCritical) {
                logger.LogCritical("Cannot find the value for CCS3_STATIC_FILES_SERVICE_BASE_URL environment variable");
            }
            return;
        }
        bool baseUriCreated = Uri.TryCreate(staticFileServiceBaseUrl, UriKind.Absolute, out Uri? staticFileServiceBaseUri);
        if (!baseUriCreated) {
            if (canLogCritical) {
                logger.LogCritical("Cannot create Uri from '{0}'", staticFileServiceBaseUrl);
            }
            return;
        }
        string appServiceDownloadFileName = "Ccs3ClientAppWindowsService.zip";
        string localAppServiceDownloadFilePath = Path.Combine("downloads", appServiceDownloadFileName);
        string directoryToCreate = Path.GetDirectoryName(localAppServiceDownloadFilePath)!;
        Uri appServiceUri = new(staticFileServiceBaseUri!, appServiceDownloadFileName);

        while (!stoppingToken.IsCancellationRequested) {
            if (canLogInformation) {
                logger.LogInformation("Download client app windows service {0} start", appServiceUri);
            }
            try {
                if (canLogInformation) {
                    string localZipFileFullPath = Path.GetFullPath(localAppServiceDownloadFilePath);
                    string dirFullPath = Path.GetFullPath(directoryToCreate);
                    logger.LogInformation("Local Zip file path: '{0}'. Destination folder: '{1}'", localZipFileFullPath, dirFullPath);
                }
                Directory.CreateDirectory(directoryToCreate);
                DownloadAndSaveToFileResult downloadResult = await httpDownloader.DownloadAndSaveToFileAsync(appServiceUri, localAppServiceDownloadFilePath, stoppingToken);
                if (canLogInformation) {
                    logger.LogInformation("Download client app windows service end");
                }
                if (downloadResult.Success) {
                    break;
                } else {
                    if (canLogWarning) {
                        logger.LogWarning(downloadResult.Exception, "Cannot download CCS3 client app windows service");
                    }
                }
            } catch (OperationCanceledException) {
                // Stop was requested
                return;
            } catch (Exception ex) {
                if (canLogCritical) {
                    logger.LogCritical(ex, "{Message}", ex.ToString());
                }
                Environment.Exit(1);
            }
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        // Download succeeded
        // Stop the service, extract the .zip file, register as service and start it
        string appServiceName = "Ccs3ClientAppWindowsService";
        string clientAppDirectory = "..\\ClientAppWindowsService";
        while (!stoppingToken.IsCancellationRequested) {
            try {
                StopService(appServiceName, TimeSpan.FromSeconds(10));
                ZipFile.ExtractToDirectory(localAppServiceDownloadFilePath, clientAppDirectory, true);
                if (File.Exists(localAppServiceDownloadFilePath)) {
                    File.Delete(localAppServiceDownloadFilePath);
                }
                break;
            } catch (Exception ex) {
                if (canLogWarning) {
                    logger.LogWarning(ex, $"Can't stop service '{appServiceName}'");
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        // Register the client app service
        try {
            string fullCcs3ClientAppWindowsServiceExecutablePath = Path.GetFullPath(Path.Combine(clientAppDirectory, "Ccs3ClientAppWindowsService.exe"));
            // First unregister the service
            UnregisterService(appServiceName);
            ProcessStartInfo psi = new() {
                FileName = "sc.exe",
                Arguments = string.Format("create \"{0}\" DisplayName= \"CCS3 Client App Windows Service\" start= auto obj= LocalSystem binpath= \"{1}\"", appServiceName, fullCcs3ClientAppWindowsServiceExecutablePath)
            };
            using Process proc = Process.Start(psi)!;
            proc.WaitForExit(TimeSpan.FromSeconds(5));
            if (proc.ExitCode != 0) {
                if (canLogCritical) {
                    logger.LogCritical("Can't register client app service. sc.exe exit code {0}", proc.ExitCode);
                }
            }
            StartService(appServiceName, TimeSpan.FromSeconds(10));
        } catch (Exception ex) {
            if (canLogCritical) {
                logger.LogCritical(ex, "Can't register or start client app service");
            }
        }
    }

    private void StopService(string serviceName, TimeSpan timeout) {
        var existingService = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName);
        if (existingService != null) {
            if (existingService.Status != ServiceControllerStatus.Stopped && existingService.Status != ServiceControllerStatus.StopPending) {
                existingService.Stop(true);
            }
            existingService.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }
    }

    private void StartService(string serviceName, TimeSpan timeout) {
        var existingService = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName)!;
        existingService.Start();
        existingService.WaitForStatus(ServiceControllerStatus.Running, timeout);
    }

    private int UnregisterService(string serviceName) {
        ProcessStartInfo psi = new() {
            FileName = "sc.exe",
            Arguments = string.Format("delete \"{0}\"", serviceName)
        };
        using Process proc = Process.Start(psi)!;
        proc.WaitForExit(TimeSpan.FromSeconds(5));
        return proc.ExitCode;
    }
}

