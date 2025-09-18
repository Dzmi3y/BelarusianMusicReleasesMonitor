using System.Net.Http.Headers;
using System.Text.Json;
using BMRM.Core.Features.Spotify;
using Microsoft.Extensions.Logging;

public abstract class SpotifyBaseService
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly SpotifyTokenStore _spotifyTokenStore;
    protected readonly ISpotifyTokenService _spotifyTokenService;

    protected SpotifyBaseService(HttpClient httpClient, ILogger logger, SpotifyTokenStore tokenStore, ISpotifyTokenService tokenService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _spotifyTokenStore = tokenStore;
        _spotifyTokenService = tokenService;
    }

    protected async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string url)
    {
        if (string.IsNullOrEmpty(_spotifyTokenStore.BearerToken))
        {
            await _spotifyTokenService.UpdateBearerTokenAsync();
        }

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _spotifyTokenStore.BearerToken);
        return request;
    }

    protected async Task<T?> SendRequestAsync<T>(HttpRequestMessage request)
    {
        try
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spotify API request failed");
            throw;
        }
    }
}