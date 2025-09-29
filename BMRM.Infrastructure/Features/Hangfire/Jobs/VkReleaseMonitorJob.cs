using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;

namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class VkReleaseMonitorJob : IVkReleaseMonitorJob
{
    private readonly IVkReleaseMonitorService _vkReleaseMonitorService;
    private readonly IReleaseSpotifyLinkerService _releaseSpotifyLinkerService;

    public VkReleaseMonitorJob(IVkReleaseMonitorService vkReleaseMonitorService,IReleaseSpotifyLinkerService  releaseSpotifyLinkerService)
    {
        _vkReleaseMonitorService = vkReleaseMonitorService;
        _releaseSpotifyLinkerService = releaseSpotifyLinkerService;
    }

    public async Task ExecuteJobAsync()
    {
        await _vkReleaseMonitorService.ParseAndSaveAsync();
        await _releaseSpotifyLinkerService.LinkReleasesToSpotifyAsync();
    }
}