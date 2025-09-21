using BMRM.Core.Features.ReleaseMonitor;
using Serilog;

namespace BMRM.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IReleaseMonitorJob>();

        Log.Logger.Information("Worker triggered at: {time}", DateTimeOffset.Now);
        await job.ExecuteAsync(stoppingToken);
    }
}
