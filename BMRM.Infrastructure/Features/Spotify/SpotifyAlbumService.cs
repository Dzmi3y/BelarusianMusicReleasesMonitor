using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;


public class SpotifyAlbumService : SpotifyBaseService, ISpotifyAlbumService
{
    private const string UrlTemplate = "https://api.spotify.com/v1/albums/{}/tracks?limit=20&offset=0'";

    public SpotifyAlbumService(HttpClient httpClient, ILogger<SpotifySearchService> logger,
        SpotifyTokenStore tokenStore, ISpotifyTokenService tokenService)
        : base(httpClient, logger, tokenStore, tokenService) { }

    public async Task<SpotifyTrackResponse?> GetAlbumTracksAsync(string albumId)
    {
        string url = string.Format(UrlTemplate, albumId);

        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);
        return await SendRequestAsync<SpotifyTrackResponse>(request);
    }
}
