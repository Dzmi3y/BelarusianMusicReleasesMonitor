using BMRM.Core.Interfaces;

namespace BMRM.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IReleaseMonitorJob>();

        _logger.LogInformation("Worker triggered at: {time}", DateTimeOffset.Now);
        await job.ExecuteAsync(stoppingToken);
    }
}
