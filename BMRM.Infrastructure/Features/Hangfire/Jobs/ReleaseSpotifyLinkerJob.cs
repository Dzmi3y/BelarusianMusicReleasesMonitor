using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.Spotify;

namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class ReleaseSpotifyLinkerJob : IReleaseSpotifyLinkerJob
{
    private readonly IReleaseSpotifyLinkerService _releaseSpotifyLinkerService;

    public ReleaseSpotifyLinkerJob(IReleaseSpotifyLinkerService releaseSpotifyLinkerService)
    {
        _releaseSpotifyLinkerService = releaseSpotifyLinkerService;
    }

    public async Task ExecuteJobAsync()
    {
        await _releaseSpotifyLinkerService.LinkReleasesToSpotifyAsync();
    }
}