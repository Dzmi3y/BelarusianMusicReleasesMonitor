using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.ReleaseMonitor.Bandcamp;


namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class BandcampReleaseMonitorJob : IBandcampReleaseMonitorJob
{
    private readonly IBandcampReleaseMonitorService _bandcampReleaseMonitorService;

    public BandcampReleaseMonitorJob(IBandcampReleaseMonitorService bandcampReleaseMonitorService)
    {
        _bandcampReleaseMonitorService = bandcampReleaseMonitorService;
    }

    public async Task ExecuteJobAsync()
    {
        await _bandcampReleaseMonitorService.ParseAndSaveAsync();
    }
}