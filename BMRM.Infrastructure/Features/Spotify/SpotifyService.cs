using System.Net.Http.Headers;
using System.Text.Json;
using BMRM.Core.Features.Spotify;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly ILogger<SpotifyService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly SpotifyTokenStore _spotifyTokenStore;
    private readonly ISpotifyTokenService _spotifyTokenService;

    public SpotifyService(HttpClient httpClient, ILogger<SpotifyService> logger, SpotifyTokenStore spotifyTokenStore,
        ISpotifyTokenService spotifyTokenService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _url = "https://api.spotify.com/v1/playlists/1TafmgIyZEYPlqxoDXEhAb/tracks";
        _spotifyTokenStore = spotifyTokenStore;
        _spotifyTokenService = spotifyTokenService;
    }

    public async Task<SpotifyResponse?> GetPlaylistReleasesAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_spotifyTokenStore.BearerToken))
            {
                await _spotifyTokenService.UpdateBearerTokenAsync();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _spotifyTokenStore.BearerToken);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
           

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<SpotifyResponse>(json, options);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download HTML from {Url}", _url);
            throw;
        }
    }
}