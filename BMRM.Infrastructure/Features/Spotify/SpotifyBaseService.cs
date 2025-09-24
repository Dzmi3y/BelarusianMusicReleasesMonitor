using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BMRM.Core.Features.Http;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Enums;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

public abstract class SpotifyBaseService
{
    protected readonly ICacheableHttpClient _cacheableHttpClient;
    protected readonly ISpotifySimpleTokenService _spotifySimpleTokenService;
    protected readonly ISpotifyCodeFlowTokenService _spotifyCodeFlowTokenService;
    protected readonly bool _useAuthorizationCodeFlow;
    protected readonly TokenType _tokenType;
    protected readonly ISpotifyTokenService _tokenService;
    private readonly AppDbContext _db;

    protected SpotifyBaseService(ICacheableHttpClient cacheableHttpClient,
        ISpotifySimpleTokenService simpleTokenService, ISpotifyCodeFlowTokenService spotifyCodeFlowTokenService,
        AppDbContext db, bool useAuthorizationCodeFlow = false)
    {
        _cacheableHttpClient = cacheableHttpClient;
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
            Func<HttpResponseMessage, Task<T?>> statusHandler = async (response) =>
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await Task.Delay(TimeSpan.FromSeconds(4));
                    var newToken = await _tokenService.UpdateTokenAsync();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                    return await _cacheableHttpClient.SendAsync<T>(request, HttpCompletionOption.ResponseHeadersRead);
                }

                return default;
            };

            return await _cacheableHttpClient.SendAsync<T>(request, HttpCompletionOption.ResponseHeadersRead,
                default, statusHandler);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Spotify API request failed");
            throw;
        }
    }
}