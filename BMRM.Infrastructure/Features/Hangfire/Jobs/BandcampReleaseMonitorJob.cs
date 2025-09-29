using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;

namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class BandcampReleaseMonitorJob : IBandcampReleaseMonitorJob
{
    private readonly IBandcampReleaseMonitorService _bandcampReleaseMonitorService;
    private readonly IReleaseSpotifyLinkerService _releaseSpotifyLinkerService;

    public BandcampReleaseMonitorJob(IBandcampReleaseMonitorService bandcampReleaseMonitorService, IReleaseSpotifyLinkerService releaseSpotifyLinkerService)
    {
        _bandcampReleaseMonitorService = bandcampReleaseMonitorService;
        _releaseSpotifyLinkerService = releaseSpotifyLinkerService;
    }

    public async Task ExecuteJobAsync()
    {
        await _bandcampReleaseMonitorService.ParseAndSaveAsync();
     //   await _releaseSpotifyLinkerService.LinkReleasesToSpotifyAsync();
    }
}