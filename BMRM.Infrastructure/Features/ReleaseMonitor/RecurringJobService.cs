using BMRM.Core.Features.ReleaseMonitor;
using Hangfire;

namespace BMRM.Infrastructure.Features.ReleaseMonitor;

public class RecurringJobService:IRecurringJobService
{
    private readonly IRecurringJobManager _jobManager;

    public RecurringJobService(IRecurringJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public void AddOrUpdateReleaseMonitor()
    {
        _jobManager.AddOrUpdate<IReleaseMonitorJob>(
            "release-monitor",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily);
    }

    public void RemoveReleaseMonitor()
    {
        _jobManager.RemoveIfExists("release-monitor");
    }
}