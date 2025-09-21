using System.Text;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyPlaylistsService : SpotifyBaseService, ISpotifyPlaylistsService
{
    private const string GetPlaylistUrlTemplate =
        "https://api.spotify.com/v1/playlists/{0}?fields=id%2Csnapshot_id%2Ctracks%28total%2Citems%28track%28id%2Cname%2Cartists%28name%29%2Calbum%28name%2Calbum_type%2Ctotal_tracks%2Cid%2Crelease_date%29%29%29%29";

    private const string PlaylistTracksUrlTemplate = "https://api.spotify.com/v1/playlists/{0}/tracks";

    public SpotifyPlaylistsService(HttpClient httpClient, ISpotifySimpleTokenService simpleTokenService,
        ISpotifyCodeFlowTokenService spotifyCodeFlowTokenService, AppDbContext db)
        : base(httpClient, simpleTokenService, spotifyCodeFlowTokenService, db, true)
    {
    }

    public async Task<SpotifyPlaylistItemsResponse?> GetPlaylistTracksAsync(string playlistId)
    {
        var playlistApiUrl = string.Format(GetPlaylistUrlTemplate, playlistId);
        var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, playlistApiUrl);
        return await SendRequestAsync<SpotifyPlaylistItemsResponse>(request);
    }

    public async Task<PlaylistChangeResponse?> AddPlaylistTracksAsync(string playlistId, List<string> trackIds)
    {
        var playlistApiUrl = string.Format(PlaylistTracksUrlTemplate, playlistId);
        var uris = trackIds.Select(id => $"spotify:track:{id}").ToArray();

        var requestBody = new
        {
            uris = uris,
            position = 0
        };

        var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
        var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, playlistApiUrl);
        request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        return await SendRequestAsync<PlaylistChangeResponse>(request);
    }

    public async Task<PlaylistChangeResponse?> DeletePlaylistTracksAsync(string playlistId, List<string> trackIds,
        string snapshotId)
    {
        var playlistApiUrl = string.Format(PlaylistTracksUrlTemplate, playlistId);
        var tracks = trackIds.Select(id => new { uri = $"spotify:track:{id}" }).ToArray();

        var requestBody = new
        {
            tracks = tracks,
            snapshot_id = snapshotId
        };

        var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
        var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, playlistApiUrl);
        request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        return await SendRequestAsync<PlaylistChangeResponse>(request);
    }
}