using System.Diagnostics;
using System.Text;

namespace Ccs3ClientAppWindowsService;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger) {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
        while (!stoppingToken.IsCancellationRequested) {
            StartClientAppIfNotStarted();
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private void StartClientAppIfNotStarted() {
        // TODO: Get path using current path - the client app is in subfolder of current service executable path
        var clientAppProcessExecutableFullPath = "C:\\Program Files\\CCS3\\ClientAppWindowsService\\ClientApp\\Ccs3ClientApp.exe";

        var sessions = ClientAppProcessController.GetSessions();
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
        ClientAppProcessController.WTS_SESSION_INFO? activeSession = sessions.FirstOrDefault(x => x.State == ClientAppProcessController.WTS_CONNECTSTATE_CLASS.WTSActive);
        if (activeSession is not null) {
            // TODO: Check if the app already runs
            Process? clientAppProcess = GetProcessByExecutablePath((int)activeSession.Value.SessionID, clientAppProcessExecutableFullPath);
            if (clientAppProcess == null) {
                _logger.LogInformation("Trying to start the process: {clientAppProcessExecutableFullPath}", clientAppProcessExecutableFullPath);
                bool startProcessResult = ClientAppProcessController.StartProcessAsCurrentUser(clientAppProcessExecutableFullPath);
                _logger.LogInformation("Start process result: {startProcessResult}", startProcessResult);
            }
        }
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
}
