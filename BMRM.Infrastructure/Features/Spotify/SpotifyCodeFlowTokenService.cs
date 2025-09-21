using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Core.Shared.Enums;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.Spotify;

public class SpotifyCodeFlowTokenService : ISpotifyCodeFlowTokenService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri = "http://localhost/";
    private readonly ILogger<SpotifyCodeFlowTokenService> _logger;
    private readonly AppDbContext _db;

    private readonly string[] _scopes = new[]
    {
        "playlist-modify-public",
        "playlist-modify-private"
    };

    public SpotifyCodeFlowTokenService(ILogger<SpotifyCodeFlowTokenService> logger, AppDbContext db)
    {
        _clientId = GetRequiredEnv("spotify_client_id");
        _clientSecret = GetRequiredEnv("spotify_client_secret");
        _logger = logger;
        _db = db;
    }

    public async Task<Token?> UpdateTokenAsync()
    {
        var tokenResponse = await RefreshTokenAsync();
        if (tokenResponse != null)
        {
            var token = _db.Tokens.FirstOrDefault(t => t.Type == TokenType.CodeAuth);
            if (token != null)
            {
                token.AccessToken = tokenResponse.AccessToken;
                token.RefreshToken = tokenResponse.RefreshToken;
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                token.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                token = new Token()
                {
                    Id = Guid.NewGuid(),
                    Type = TokenType.CodeAuth,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Tokens.Add(token);
            }

            await _db.SaveChangesAsync();
            return token;
        }

        return null;
    }

    private async Task<SpotifyCodeFlowTokenResponse?> AuthorizeAsync()
    {
        try
        {
            var authUrl = $"https://accounts.spotify.com/authorize?response_type=code" +
                          $"&client_id={_clientId}" +
                          $"&scope={Uri.EscapeDataString(string.Join(" ", _scopes))}" +
                          $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";

            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

            using var listener = new HttpListener();
            listener.Prefixes.Add(_redirectUri);
            listener.Start();

            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];

            var responseText = "Authorization successful. You may close this window.";
            var buffer = Encoding.UTF8.GetBytes(responseText);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            listener.Stop();

            return await ExchangeCodeForTokenAsync(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging token");
            return null;
        }
    }

    private async Task<SpotifyCodeFlowTokenResponse?> RefreshTokenAsync()
    {
        var token = _db.Tokens.FirstOrDefault(t => t.Type == TokenType.CodeAuth);
        if (string.IsNullOrEmpty(token?.RefreshToken))
        {
            return await AuthorizeAsync();
        }

        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsync("https://accounts.spotify.com/api/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = token.RefreshToken,
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret
                }));

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SpotifyCodeFlowTokenResponse>(json, options)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return await AuthorizeAsync();
        }
    }

    private async Task<SpotifyCodeFlowTokenResponse?> ExchangeCodeForTokenAsync(string code)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync("https://accounts.spotify.com/api/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _redirectUri,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            }));

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<SpotifyCodeFlowTokenResponse>(json, options)!;
    }

    private static string GetRequiredEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Environment variable '{key}' is not set");
    }
}