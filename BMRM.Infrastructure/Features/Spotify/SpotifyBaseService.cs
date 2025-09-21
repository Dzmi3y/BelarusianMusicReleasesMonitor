using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Enums;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public abstract class SpotifyBaseService
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly ISpotifySimpleTokenService _spotifySimpleTokenService;
    protected readonly ISpotifyCodeFlowTokenService _spotifyCodeFlowTokenService;
    protected readonly bool _useAuthorizationCodeFlow;
    protected readonly TokenType _tokenType;
    protected readonly ISpotifyTokenService _tokenService;
    private readonly AppDbContext _db;

    protected SpotifyBaseService(HttpClient httpClient, ILogger logger,
        ISpotifySimpleTokenService simpleTokenService, ISpotifyCodeFlowTokenService spotifyCodeFlowTokenService,
        AppDbContext db, bool useAuthorizationCodeFlow = false)
    {
        _httpClient = httpClient;
        _logger = logger;
        _spotifySimpleTokenService = simpleTokenService;
        _spotifyCodeFlowTokenService = spotifyCodeFlowTokenService;
        _db = db;
        _useAuthorizationCodeFlow = useAuthorizationCodeFlow;

        _tokenType = useAuthorizationCodeFlow ? TokenType.CodeAuth : TokenType.Simple;
        _tokenService =
            useAuthorizationCodeFlow ? _spotifyCodeFlowTokenService : _spotifySimpleTokenService;
    }

    protected async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string url)
    {
        var token = await _db.Tokens.FirstOrDefaultAsync(t => t.Type == _tokenType) ??
                    await _tokenService.UpdateTokenAsync();

        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token?.AccessToken);
        return request;
    }

    protected async Task<T?> SendRequestAsync<T>(HttpRequestMessage request)
    {
        try
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            { 
                _logger.LogWarning("Access token expired. Attempting to refresh...");
                var newToken =  await _tokenService.UpdateTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            }


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