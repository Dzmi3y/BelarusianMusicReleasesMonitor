using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyPlaylistsService : SpotifyBaseService, ISpotifyPlaylistsService
{
    private const string PlaylistUrl = "https://api.spotify.com/v1/playlists/1TafmgIyZEYPlqxoDXEhAb/tracks";

    public SpotifyPlaylistsService(HttpClient httpClient, ILogger<SpotifyPlaylistsService> logger,
        SpotifyTokenStore tokenStore, ISpotifyTokenService tokenService)
        : base(httpClient, logger, tokenStore, tokenService) { }

    public async Task<SpotifyPlaylistResponse?> GetPlaylistReleasesAsync()
    {
        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, PlaylistUrl);
        return await SendRequestAsync<SpotifyPlaylistResponse>(request);
    }
}