using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;

namespace Ccs3ClientAppBootstrapWindowsService;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly HttpDownloader _httpDownloader;
    private readonly TimeSpan _serviceOperationTimeout = TimeSpan.FromSeconds(25);
    private readonly TimeSpan _processExitTimeout = TimeSpan.FromSeconds(20);

    public Worker(ILogger<Worker> logger, HttpDownloader httpDownloader, IHostApplicationLifetime hostApplicationLifetime) {
        _logger = logger;
        _httpDownloader = httpDownloader;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

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

        bool canLogInformation = _logger.IsEnabled(LogLevel.Information);
        bool canLogCritical = _logger.IsEnabled(LogLevel.Critical);
        bool canLogWarning = _logger.IsEnabled(LogLevel.Warning);

        if (canLogInformation) {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        string? staticFileServiceBaseUrl = Environment.GetEnvironmentVariable("CCS3_STATIC_FILES_SERVICE_BASE_URL");
        if (string.IsNullOrWhiteSpace(staticFileServiceBaseUrl)) {
            if (canLogCritical) {
                _logger.LogCritical("Cannot find the value for CCS3_STATIC_FILES_SERVICE_BASE_URL environment variable");
            }
            return;
        }
        bool baseUriCreated = Uri.TryCreate(staticFileServiceBaseUrl, UriKind.Absolute, out Uri? staticFileServiceBaseUri);
        if (!baseUriCreated) {
            if (canLogCritical) {
                _logger.LogCritical("Cannot create Uri from '{0}'", staticFileServiceBaseUrl);
            }
            return;
        }

        string appServiceName = "Ccs3ClientAppWindowsService";
        bool allLocalFilesShaMatch = false;
        string shaFileName = "ccs3-client-files-checksums.sha";
        Uri filesShaFileUri = new(staticFileServiceBaseUri!, shaFileName);
        string clientAppDirectory = "..\\ClientAppWindowsService";
        string ccs3ClientAppWindowsServiceFileName = "Ccs3ClientAppWindowsService.exe";
        string appServiceDownloadFileName = "Ccs3ClientAppWindowsService.zip";
        string localAppServiceDownloadFilePath = Path.Combine("downloads", appServiceDownloadFileName);
        string directoryToCreate = Path.GetDirectoryName(localAppServiceDownloadFilePath)!;
        Uri appServiceUri = new(staticFileServiceBaseUri!, appServiceDownloadFileName);
        while (!stoppingToken.IsCancellationRequested) {
            try {
                if (!allLocalFilesShaMatch) {
                    if (canLogInformation) {
                        _logger.LogInformation("Downloading client files");
                    }
                    // This will not be retried in case local files SHAs were already matched
                    if (canLogInformation) {
                        _logger.LogInformation("Downloading {0}", filesShaFileUri);
                    }
                    DownloadStringResult downloadShaStringResult = await _httpDownloader.DownloadStringAsync(filesShaFileUri, stoppingToken);
                    if (canLogInformation) {
                        _logger.LogInformation("Downloaded string '{0}'", downloadShaStringResult.Result);
                    }
                    if (downloadShaStringResult.Exception is not null) {
                        if (canLogWarning) {
                            _logger.LogWarning(downloadShaStringResult.Exception, "Error while downloading SHA file");
                        }
                    }
                    if (downloadShaStringResult.Success && !string.IsNullOrWhiteSpace(downloadShaStringResult.Result)) {
                        try {
                            Ccs3ClientAppWindowsServiceLocalFilesSha512MatchesResult localFilesSha512MatchesResult = Ccs3ClientAppWindowsServiceLocalFilesSha512Matches(downloadShaStringResult.Result, clientAppDirectory, ccs3ClientAppWindowsServiceFileName);
                            allLocalFilesShaMatch = localFilesSha512MatchesResult.AllMatch;
                            _logger.LogInformation("Local file SHA all match {0}", allLocalFilesShaMatch);
                            if (localFilesSha512MatchesResult.FilesInfo is not null) {
                                StringBuilder sb = new();
                                sb.AppendLine("File SHA results");
                                foreach (var item in localFilesSha512MatchesResult.FilesInfo) {
                                    sb.AppendLine(string.Format("File '{0}' , Calculated SHA {1} , Expected SHA {2}", item.Item1, item.Item2, item.Item3));
                                }
                                _logger.LogInformation(sb.ToString());
                            }
                        } catch (Exception ex) {
                            if (canLogWarning) {
                                _logger.LogWarning(ex, "Error on calculating local files hash");
                            }
                        }
                    }
                }

                if (allLocalFilesShaMatch) {
                    break;
                }

                if (!allLocalFilesShaMatch) {
                    if (canLogInformation) {
                        _logger.LogInformation("Download client app windows service {0} start", appServiceUri);
                        string localZipFileFullPath = Path.GetFullPath(localAppServiceDownloadFilePath);
                        string dirFullPath = Path.GetFullPath(directoryToCreate);
                        _logger.LogInformation("Local Zip file path: '{0}'. Destination folder: '{1}'", localZipFileFullPath, dirFullPath);
                    }
                    Directory.CreateDirectory(directoryToCreate);
                    DownloadAndSaveToFileResult downloadResult = await _httpDownloader.DownloadAndSaveToFileAsync(appServiceUri, localAppServiceDownloadFilePath, stoppingToken);
                    if (canLogInformation) {
                        _logger.LogInformation("Download client app windows service end");
                    }
                    if (downloadResult.Success) {
                        break;
                    } else {
                        if (canLogWarning) {
                            _logger.LogWarning(downloadResult.Exception, "Cannot download CCS3 client app windows service");
                        }
                    }
                }
            } catch (OperationCanceledException) {
                // Stop was requested
                return;
            } catch (Exception ex) {
                if (canLogCritical) {
                    _logger.LogCritical(ex, "{Message}", ex.ToString());
                }
                Environment.Exit(1);
            }
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        if (!allLocalFilesShaMatch) {
            // Download succeeded
            // Stop the service, extract the .zip file, register as service and start it
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    StopService(appServiceName, _serviceOperationTimeout);
                    ZipFile.ExtractToDirectory(localAppServiceDownloadFilePath, clientAppDirectory, true);
                    if (File.Exists(localAppServiceDownloadFilePath)) {
                        File.Delete(localAppServiceDownloadFilePath);
                    }
                    break;
                } catch (Exception ex) {
                    if (canLogWarning) {
                        _logger.LogWarning(ex, $"Can't stop service '{appServiceName}'");
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        // Register the client app service
        try {
            _logger.LogInformation("Stopping service '{0}'", appServiceName);
            try {
                StopService(appServiceName, _serviceOperationTimeout);
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Can't stop service '{0}'", appServiceName);
            }
            string fullCcs3ClientAppWindowsServiceExecutablePath = Path.GetFullPath(Path.Combine(clientAppDirectory, ccs3ClientAppWindowsServiceFileName));
            _logger.LogInformation("Unregistering service '{0}'", appServiceName);
            // First unregister the service
            UnregisterService(appServiceName);
            ProcessStartInfo psi = new() {
                FileName = "sc.exe",
                Arguments = string.Format("create \"{0}\" DisplayName= \"CCS3 Client App Windows Service\" start= auto obj= LocalSystem binpath= \"{1}\"", appServiceName, fullCcs3ClientAppWindowsServiceExecutablePath)
            };
            _logger.LogInformation("Starting '{0}' with arguments '{1}'", psi.FileName, psi.Arguments);
            using Process proc = Process.Start(psi)!;
            _logger.LogInformation("Waiting for '{0}' to exit", psi.FileName);
            proc.WaitForExit(_processExitTimeout);
            _logger.LogInformation("'{0}' exited with exit code", psi.FileName, proc.ExitCode);
            if (proc.ExitCode != 0) {
                if (canLogCritical) {
                    _logger.LogCritical("Can't register client app service. sc.exe exit code {0}", proc.ExitCode);
                }
            } else {
                ConfigureClientAppServiceRestartsOnFailure(appServiceName);
            }
            _logger.LogInformation("Starting service '{0}'", appServiceName);
            StartService(appServiceName, _serviceOperationTimeout);
            _logger.LogInformation("Starting service '{0}' completed", appServiceName);
        } catch (Exception ex) {
            if (canLogCritical) {
                _logger.LogCritical(ex, "Can't register or start client app service");
            }
        }
    }

    private Ccs3ClientAppWindowsServiceLocalFilesSha512MatchesResult Ccs3ClientAppWindowsServiceLocalFilesSha512Matches(
        string sh512aFileContent,
        string localCcs3ClientAppWindowsServiceDir,
        string ccs3ClientAppWindowsServiceFileName
    ) {
        Ccs3ClientAppWindowsServiceLocalFilesSha512MatchesResult result = new();
        result.FilesInfo = new Tuple<string, string, string>[0];
        string[] shaLines = sh512aFileContent.Split('\n');
        string[] filteredShaLines = shaLines.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        Dictionary<string, string> keyValuePairs = new();
        if (filteredShaLines.Length > 0) {
            foreach (string line in filteredShaLines) {
                string[] parts = line.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                string sha = parts[0];
                string filePath = parts[1];
                keyValuePairs.Add(filePath, sha);
            }
            // Find the prefix of sha lines paths that must be removed
            // The sha lines contain the path to files as they are found in the Linux container like "/usr/ccs3-published-files"
            var serviceExeLine = keyValuePairs.FirstOrDefault(x => Path.GetFileName(x.Key) == ccs3ClientAppWindowsServiceFileName);
            string serviceExeFilePath = serviceExeLine.Key;
            if (!string.IsNullOrWhiteSpace(serviceExeFilePath)) {
                // We have the root folder (when the files were at the time of their SHA claculation in the Linux container)
                // We must remove it from all lines
                string rootPath = Path.GetDirectoryName(serviceExeFilePath);
                Dictionary<string, string> localPathsWithSha = new();
                foreach (var item in keyValuePairs) {
                    string fullShaPathWithFileName = item.Key;
                    string shaPath = Path.GetDirectoryName(fullShaPathWithFileName);
                    string shaValue = item.Value;
                    string fileName = Path.GetFileName(fullShaPathWithFileName);
                    string strippedPath = shaPath.Replace(rootPath, string.Empty).TrimStart('\\');
                    string localPath = Path.Combine(localCcs3ClientAppWindowsServiceDir, strippedPath, fileName);
                    localPathsWithSha.Add(localPath, shaValue);
                }
                bool allMatch = true;
                List<Tuple<string, string, string>> filesInfoList = new();
                foreach (var item in localPathsWithSha) {
                    string localPath = item.Key;
                    string sha = item.Value;
                    using SHA512 sha512 = SHA512.Create();
                    string fullLocalPath = Path.GetFullPath(localPath);
                    using Stream fileStream = File.OpenRead(fullLocalPath);
                    byte[] calculatedHash = sha512.ComputeHash(fileStream);
                    string calculatedHashString = BitConverter.ToString(calculatedHash).ToLower().Replace("-", string.Empty);
                    allMatch = allMatch && (calculatedHashString == sha);
                    filesInfoList.Add(new Tuple<string, string, string>(fullLocalPath, calculatedHashString, sha));
                }
                result.FilesInfo = filesInfoList.ToArray();
                result.AllMatch = allMatch;
            }
        } else {
            result.AllMatch = false;
        }
        return result;
    }

    private void ConfigureClientAppServiceRestartsOnFailure(string appServiceName) {
        ProcessStartInfo psi = new() {
            FileName = "sc.exe",
            Arguments = string.Format("failure \"{0}\" reset= 600 actions= restart/0/restart/0/restart/0", appServiceName)
        };
        _logger.LogInformation("Configuring '{0}' to restart after failure", appServiceName);
        _logger.LogInformation("Starting '{0}' with arguments '{1}'", psi.FileName, psi.Arguments);
        using Process proc = Process.Start(psi)!;
        bool exitedNormally = proc.WaitForExit(_processExitTimeout);
        _logger.LogWarning("'{0}' exited with exit code {1}. Completed for less that 10 seconds?: {2}", psi.FileName, proc.ExitCode, exitedNormally);
    }

    private void StopService(string serviceName, TimeSpan timeout) {
        var existingService = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName);
        if (existingService != null) {
            _logger.LogInformation("Stopping service '{0}'", serviceName);
            if (existingService.Status != ServiceControllerStatus.Stopped && existingService.Status != ServiceControllerStatus.StopPending) {
                existingService.Stop(true);
            }
            existingService.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            _logger.LogInformation("Service '{0}' stopped", serviceName);
        } else {
        }
    }

    private void StartService(string serviceName, TimeSpan timeout) {
        var existingService = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName)!;
        if (existingService is not null) {
            _logger.LogInformation("Starting service '{0}'", serviceName);
            existingService.Start();
            existingService.WaitForStatus(ServiceControllerStatus.Running, timeout);
            _logger.LogInformation("Service '{0}' started", serviceName);
        } else {
            _logger.LogInformation("Can't start service '{0}' - not found", serviceName);
        }
    }

    private int UnregisterService(string serviceName) {
        ProcessStartInfo psi = new() {
            FileName = "sc.exe",
            Arguments = string.Format("delete \"{0}\"", serviceName)
        };
        _logger.LogInformation("Starting '{0}' with arguments '{1}'", psi.FileName, psi.Arguments);
        using Process proc = Process.Start(psi)!;
        proc.WaitForExit(_processExitTimeout);
        _logger.LogInformation("'{0}' exited with exit code {1}", psi.FileName, proc.ExitCode);
        return proc.ExitCode;
    }

    private class Ccs3ClientAppWindowsServiceLocalFilesSha512MatchesResult {
        public bool AllMatch { get; set; }
        public Tuple<string, string, string>[] FilesInfo { get; set; }
    }

    private class ShaCheckFileInfo {
        public string FilePath { get; set; }
        public string CalculatedSha512 { get; set; }
        public string ExpectedSha512 { get; set; }
    }
}
