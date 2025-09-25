using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.Spotify;

namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class UpdateSpotifyPlaylistJob:IUpdateSpotifyPlaylistJob
{
    private readonly IBelReleasePlaylistUpdaterService _belReleasePlaylistUpdaterService;

    public UpdateSpotifyPlaylistJob(IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService)
    {
        _belReleasePlaylistUpdaterService = belReleasePlaylistUpdaterService;
    }

    public async Task ExecuteJobAsync()
    {
        await _belReleasePlaylistUpdaterService.UpdateBelReleasePlaylistAsync();
    }
}