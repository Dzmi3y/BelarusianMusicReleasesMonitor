using System.Text.Json;
using BMRM.Core.Features.Spotify;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyTokenService: ISpotifyTokenService
{
    private const string TokenUrl = "https://accounts.spotify.com/api/token";

    private readonly ILogger<SpotifyTokenService> _logger;
    private readonly HttpClient _httpClient;
    private readonly SpotifyTokenStore _spotifyTokenStore;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public SpotifyTokenService(HttpClient httpClient, ILogger<SpotifyTokenService> logger, SpotifyTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _logger = logger;
        _spotifyTokenStore = tokenStore;

        _clientId = GetRequiredEnv("spotify_client_id");
        _clientSecret = GetRequiredEnv("spotify_client_secret");
    }

    public async Task UpdateBearerTokenAsync()
    {
        var request = BuildTokenRequest();

        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var token = ExtractAccessToken(json);

            _spotifyTokenStore.BearerToken = token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving Spotify token");
            throw;
        }
    }

    private HttpRequestMessage BuildTokenRequest()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _clientId },
            { "client_secret", _clientSecret }
        };

        return new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new FormUrlEncodedContent(formData)
        };
    }

    private static string ExtractAccessToken(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()
               ?? throw new InvalidOperationException("access_token not found in response");
    }

    private static string GetRequiredEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Environment variable '{key}' is not set");
    }
}
