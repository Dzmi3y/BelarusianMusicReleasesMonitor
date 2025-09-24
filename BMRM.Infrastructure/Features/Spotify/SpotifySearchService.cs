using BMRM.Core.Features.Http;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifySearchService : SpotifyBaseService, ISpotifySearchService
{
    private const string UrlTemplate = "https://api.spotify.com/v1/search?q={0}&type=album&limit=1&offset=0";

    public SpotifySearchService(ICacheableHttpClient cacheableHttpClient, ISpotifySimpleTokenService simpleTokenService,
        ISpotifyCodeFlowTokenService spotifyCodeFlowTokenService, AppDbContext db)
        : base(cacheableHttpClient, simpleTokenService, spotifyCodeFlowTokenService, db)
    {
    }

    public async Task<SpotifyAlbumSearchResponse?> FindReleaseAsync(string artistName, string albumName)
    {
        string query = Uri.EscapeDataString($"album:{albumName} artist:{artistName}");
        string url = string.Format(UrlTemplate, query);

        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);
        return await SendRequestAsync<SpotifyAlbumSearchResponse>(request);
    }
}