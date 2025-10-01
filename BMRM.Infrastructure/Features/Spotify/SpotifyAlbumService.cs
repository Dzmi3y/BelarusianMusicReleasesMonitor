using BMRM.Core.Features.Http;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Infrastructure.Data;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyAlbumService : SpotifyBaseService, ISpotifyAlbumService
{
    private const string UrlTemplate = "https://api.spotify.com/v1/albums/{0}/tracks?limit=20&offset=0";

    public SpotifyAlbumService(ICacheableHttpClient cacheableHttpClient, ISpotifySimpleTokenService simpleTokenService,
        ISpotifyCodeFlowTokenService spotifyCodeFlowTokenService, AppDbContext db)
        : base(cacheableHttpClient, simpleTokenService, spotifyCodeFlowTokenService, db)
    {
    }

    public async Task<SpotifyTrackResponse?> GetAlbumTracksAsync(string albumId)
    {
        string url = string.Format(UrlTemplate, albumId);

        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);
        return await SendRequestAsync<SpotifyTrackResponse>(request);
    }
}