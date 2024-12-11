using System.Diagnostics;
using System.IO.Compression;
using System.ServiceProcess;

namespace Ccs3ClientAppBootstrapWindowsServiceInstaller;

internal class Program {
    private static readonly string _ccs3ClientAppBootstrapWindowsServiceExecutableFileName = "Ccs3ClientAppBootstrapWindowsService.exe";
    private static readonly string _ccs3ClientAppBootstrapWindowsServiceName = "Ccs3ClientAppBootstrapWindowsService";
    private static readonly string _ccs3ClientAppWindowsServiceName = "Ccs3ClientAppWindowsService";

    static int Main(string[] args) {
        if (!Environment.IsPrivilegedProcess) {
            ShowNotPrivilegedNotice();
        }

        if (args.Length == 0 || args[0] == "--help") {
            ShowUsage();
            return 0;
        }

        CommandLineParams cmdParams = ParseCommandLine(args);
        if (cmdParams.UnknownParameters is not null) {
            string unknownParameterNames = string.Join(" ; ", cmdParams.UnknownParameters);
            Console.WriteLine("Error: Unknown parameters: {0}", unknownParameterNames);
            return (int)ExitCode.UnknownParameterFound;
        }
        SetDefaultCommandLineParamValues(cmdParams);

        StopClientAppServices();

        string targetFolder = GetTargetFolder();
        CreateTargetFolder(targetFolder);

        ExtractContent(cmdParams.ContentFilePath, targetFolder);

        UnregisterService(_ccs3ClientAppBootstrapWindowsServiceName);
        string fullBootstrapServiceExecutableFilePath = Path.Combine(targetFolder, _ccs3ClientAppBootstrapWindowsServiceExecutableFileName);
        RegisterBootstrapExecutableAsService(fullBootstrapServiceExecutableFilePath);

        SetEnvironmentVariables(
            cmdParams.StaticFilesServiceBaseUrl,
            cmdParams.PcConnectorServiceBaseUrl,
            cmdParams.ClientAppWindowsServiceLocalBaseUrl
        );

        Console.WriteLine();
        Console.WriteLine("Installation finished. You can delete the installer files and restart.");
        return (int)ExitCode.Success;
    }

    private static void SetEnvironmentVariables(string? staticFilesServiceBaseUrl, string? pcConnectorServiceBaseUrl, string? clientAppServiceLocalBaseUrl) {
        if (!string.IsNullOrEmpty(staticFilesServiceBaseUrl)) {
            Console.WriteLine("Setting environment variable {0} to '{1}'", Ccs3EnvironmentVariableNames.CCS3_STATIC_FILES_SERVICE_BASE_URL, staticFilesServiceBaseUrl);
            Environment.SetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_STATIC_FILES_SERVICE_BASE_URL, staticFilesServiceBaseUrl, EnvironmentVariableTarget.Machine);
        }
        if (!string.IsNullOrEmpty(pcConnectorServiceBaseUrl)) {
            Console.WriteLine("Setting environment variable {0} to '{1}'", Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_BASE_URL, pcConnectorServiceBaseUrl);
            Environment.SetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_BASE_URL, pcConnectorServiceBaseUrl, EnvironmentVariableTarget.Machine);
        }
        if (!string.IsNullOrEmpty(clientAppServiceLocalBaseUrl)) {
            Console.WriteLine("Setting environment variable {0} to '{1}'", Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL, clientAppServiceLocalBaseUrl);
            Environment.SetEnvironmentVariable(Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL, clientAppServiceLocalBaseUrl, EnvironmentVariableTarget.Machine);
        }
    }

    private static string GetTargetFolder() {
        string programFilesPath = Environment.ExpandEnvironmentVariables("%ProgramFiles%");
        string bootstrapWindowsServicePath = Path.Combine(programFilesPath, "CCS3\\BootstrapWindowsService");
        return bootstrapWindowsServicePath;
    }
    private static void CreateTargetFolder(string targetFolder) {
        Console.WriteLine("Creating target folder '{0}'", targetFolder);
        Directory.CreateDirectory(targetFolder);
    }

    private static void ExtractContent(string zipFilePath, string targetFolder) {
        Console.WriteLine("Extracting content of '{0}' to target folder '{1}'", zipFilePath, targetFolder);
        ZipFile.ExtractToDirectory(zipFilePath, targetFolder, true);
    }

    private static void StopClientAppServices() {
        ServiceController[] services = ServiceController.GetServices();

        ServiceController? clientAppService = services.FirstOrDefault(x => x.ServiceName == _ccs3ClientAppWindowsServiceName);
        if (clientAppService != null) {
            StopService(clientAppService);
        }

        ServiceController? bootstrapService = services.FirstOrDefault(x => x.ServiceName == _ccs3ClientAppBootstrapWindowsServiceName);
        if (bootstrapService != null) {
            StopService(bootstrapService);
        }
    }

    private static void StopService(ServiceController serviceController) {
        try {
            Console.WriteLine("Stopping service {0}. Service status: {1}", serviceController.ServiceName, serviceController.Status);
            if (serviceController.Status == ServiceControllerStatus.Running
                || serviceController.Status == ServiceControllerStatus.StartPending) {
                serviceController.Stop(true);
            }
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
        } catch (Exception ex) {
            Console.WriteLine("Error while stopping service {0}. Error: {1}", serviceController.ServiceName, ex.Message);
        }
    }

    private static void UnregisterService(string serviceName) {
        ProcessStartInfo psi = new() {
            FileName = "sc.exe",
            Arguments = string.Format("delete \"{0}\"", serviceName)
        };
        Console.WriteLine("Unregistering '{0}' as service. Executing:", serviceName);
        Console.WriteLine("sc.exe {0}", psi.Arguments);
        using Process proc = Process.Start(psi)!;
        proc.WaitForExit(TimeSpan.FromSeconds(5));
    }

    private static void RegisterBootstrapExecutableAsService(string fullBootstrapExecutableFilePath) {
        ProcessStartInfo psi = new() {
            FileName = "sc.exe",
            Arguments = string.Format("create \"{0}\" DisplayName= \"CCS3 Client App Bootstrap Windows Service\" start= auto obj= LocalSystem binpath= \"{1}\"", _ccs3ClientAppBootstrapWindowsServiceName, fullBootstrapExecutableFilePath)
        };
        Console.WriteLine("Registering '{0}' as service. Executing:", fullBootstrapExecutableFilePath);
        Console.WriteLine("sc.exe {0}", psi.Arguments);
        using Process proc = Process.Start(psi)!;
        proc.WaitForExit(TimeSpan.FromSeconds(5));
        if (proc.ExitCode != 0) {
            Console.WriteLine("Can't register '{0}' as service. sc.exe exit code {1}", fullBootstrapExecutableFilePath, proc.ExitCode);
        }
    }


    private static void ShowNotPrivilegedNotice() {
        Console.WriteLine("-----");
        Console.WriteLine("The process is not privileged. Operations could fail.");
        Console.WriteLine("Start this application as administrator.");
        Console.WriteLine("-----");
        Console.WriteLine();
    }

    private static CommandLineParams ParseCommandLine(string[] args) {
        CommandLineParams cmdParams = new();
        List<string> unknownParams = new();
        for (int i = 0; i < args.Length; i++) {
            switch (args[i]) {
                case "--contentFilePath":
                    cmdParams.ContentFilePath = args[i + 1];
                    i++;
                    break;
                case "--static-files-service-base-url":
                    cmdParams.StaticFilesServiceBaseUrl = args[i + 1];
                    i++;
                    break;
                case "--pc-connector-service-base-url":
                    cmdParams.PcConnectorServiceBaseUrl = args[i + 1];
                    i++;
                    break;
                case "--client-app-windows-service-local-base-url":
                    cmdParams.ClientAppWindowsServiceLocalBaseUrl = args[i + 1];
                    i++;
                    break;
                default:
                    unknownParams.Add(args[i]);
                    break;
            }
        }
        if (unknownParams.Count > 0) {
            cmdParams.UnknownParameters = unknownParams.ToArray();
        }
        return cmdParams;
    }

    private static void SetDefaultCommandLineParamValues(CommandLineParams cmdParams) {
        if (string.IsNullOrEmpty(cmdParams.ContentFilePath)) {
            cmdParams.ContentFilePath = "Ccs3ClientAppBootstrapWindowsService.zip";
        }
    }

    private static void ShowUsage() {
        Console.WriteLine();
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine();
        Console.WriteLine("A file named Ccs3ClientAppBootstrapWindowsService.zip containing the bootstrap service files must exist in current folder.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine();
        Console.WriteLine("Ccs3ClientAppBootstrapWindowsServiceInstaller.exe [--static-files-service-base-url https://<name-or-ip-address>:<port>] [--pc-connector-service-base-url https://<name-or-ip-address>:<port>] [--client-app-windows-service-local-base-url https://localhost:<port>");
        Console.WriteLine();
        Console.WriteLine("- Optional parameter --static-files-service-base-url specifies the URL where the static files are served. It is used by the bootstrap service to download other client application files. The provided value must also contain the port and will be created as system environment variable named {0}", Ccs3EnvironmentVariableNames.CCS3_STATIC_FILES_SERVICE_BASE_URL);
        Console.WriteLine("- Optional parameter --pc-connector-service-base-url specifies the URL of the PC-Connector. It is used by the client application service to connect the computer to the CCS3 system. The provided value will be created as system environment variable named {0}", Ccs3EnvironmentVariableNames.CCS3_PC_CONNECTOR_SERVICE_BASE_URL);
        Console.WriteLine("- Optional parameter --client-app-windows-service-local-base-url specifies the URL on which the client app service will listen for connections from the client app. This should point to either https://localhost:<port> or https://127.0.0.1:<port>. It is used by the client application to connect to the client application Windows service. The provided value will be created as system environment variable named {0}. A trusted certificate with the provided host name or IP address as CN must exist in the certificate storage to avoid certificate errors in the client browser that shows session information to the customer. It is recommended to use either https://localhost:<port> or https://127.0.0.1:<port> to avoid exposing client app Windows service to other machines in the network", Ccs3EnvironmentVariableNames.CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL);

        Console.WriteLine();
        Console.WriteLine("Samples:");
        Console.WriteLine("- Installs only the CCS3 Client App Bootstrap Windows Service - use it only if you already have all the environment variables set or if they will be set manually:");
        Console.WriteLine("Ccs3ClientAppBootstrapWindowsServiceInstaller.exe");
        Console.WriteLine();
        Console.WriteLine("- Installs the CCS3 Client App Bootstrap Windows Service and also sets the environment variables using IP addresses:");
        Console.WriteLine("Ccs3ClientAppBootstrapWindowsServiceInstaller.exe --static-files-service-base-url https://192.168.1.20:65450 --pc-connector-service-base-url https://192.168.1.20:65451 --client-app-windows-service-local-base-url https://127.0.0.1:30000");
        Console.WriteLine();
        Console.WriteLine("- Installs the CCS3 Client App Bootstrap Windows Service and also sets the environment variables using host names:");
        Console.WriteLine("Ccs3ClientAppBootstrapWindowsServiceInstaller.exe --static-files-service-base-url https://ccs3-server-pc:65450 --pc-connector-service-base-url https://ccs3-server-pc:65451 --client-app-windows-service-local-base-url https://localhost:30000");
        Console.WriteLine();
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine();
    }

    private class CommandLineParams {
        public string ContentFilePath { get; set; }
        public string? StaticFilesServiceBaseUrl { get; set; }
        public string? PcConnectorServiceBaseUrl { get; set; }
        public string? ClientAppWindowsServiceLocalBaseUrl { get; set; }
        public string[] UnknownParameters { get; set; }
    }

    private static class Ccs3EnvironmentVariableNames {
        public static readonly string CCS3_STATIC_FILES_SERVICE_BASE_URL = "CCS3_STATIC_FILES_SERVICE_BASE_URL";
        public static readonly string CCS3_PC_CONNECTOR_SERVICE_BASE_URL = "CCS3_PC_CONNECTOR_SERVICE_BASE_URL";
        public static readonly string CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL = "CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL";
    }

    private enum ExitCode {
        Success = 0,
        UnknownParameterFound = 1,
    }
}
