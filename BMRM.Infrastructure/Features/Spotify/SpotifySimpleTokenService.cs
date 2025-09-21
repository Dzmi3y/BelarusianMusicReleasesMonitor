using System.Text.Json;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Core.Shared.Enums;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifySimpleTokenService : ISpotifySimpleTokenService
{
    private const string TokenUrl = "https://accounts.spotify.com/api/token";

    private readonly ILogger<SpotifySimpleTokenService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly AppDbContext _db;

    public SpotifySimpleTokenService(HttpClient httpClient, ILogger<SpotifySimpleTokenService> logger, AppDbContext db)
    {
        _httpClient = httpClient;
        _logger = logger;

        _clientId = GetRequiredEnv("spotify_client_id");
        _clientSecret = GetRequiredEnv("spotify_client_secret");
        _db = db;
    }

    public async Task<Token?> UpdateTokenAsync()
    {
        var request = BuildTokenRequest();

        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<SpotifySimpleTokenResponse>(json);

            if (tokenResponse != null)
            {
                var dbToken = _db.Tokens.FirstOrDefault(t => t.Type == TokenType.Simple);
                if (dbToken != null)
                {
                    dbToken.AccessToken = tokenResponse.AccessToken;
                    dbToken.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    dbToken.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    dbToken = new Token()
                    {
                        Id = Guid.NewGuid(),
                        Type = TokenType.Simple,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = null,
                        ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                        UpdatedAt = DateTime.UtcNow
                    };
                    _db.Tokens.Add(dbToken);
                }

                await _db.SaveChangesAsync();
                return dbToken;
            }
            return null;
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

    private static string GetRequiredEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Environment variable '{key}' is not set");
    }
}