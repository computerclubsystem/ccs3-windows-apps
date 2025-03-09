namespace Ccs3HttpToUdpProxyWindowsService;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly WorkerState _state = new WorkerState();

    public Worker(ILogger<Worker> logger) {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            if (_state.AppCancellationToken.IsCancellationRequested) {
                return;
            }
            try {
                await Task.Delay(1000, stoppingToken);
            } catch (Exception ex) {
            }
        }
    }

    public void SetAppStoppingCancellationToken(CancellationToken token) {
        _state.AppCancellationToken = token;
    }

    private class WorkerState {
        public CancellationToken AppCancellationToken { get; set; }
    }
}
