using System.Collections.Concurrent;
using System.Net.Http.Json;
using BMRM.Core.Features.Http;

namespace BMRM.Infrastructure.Features.Http;

public class CacheableHttpClient : ICacheableHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public CacheableHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> SendAsync<T>(HttpRequestMessage request, HttpCompletionOption httpCompletionOption,
        TimeSpan ttl = default, Func<HttpResponseMessage, Task<T?>>? statusHandler = null)
    {
        ttl = ttl == default ? TimeSpan.FromSeconds(30) : ttl;
        var now = DateTime.UtcNow;
        var url = request.RequestUri.ToString();

        if (_cache.TryGetValue(url, out var entry) && now - entry.Timestamp < ttl)
            return (T)entry.Data;

        var response = await _httpClient.SendAsync(request, httpCompletionOption);
        
        var HandlerResult = statusHandler != null ? await statusHandler(response) : default;
        if (HandlerResult != null)
            return HandlerResult;
        
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<T>();
        _cache[url] = new CacheEntry(now, data!);

        return (T)data!;
    }
    
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption, TimeSpan ttl = default)
    {
        ttl = ttl == default ? TimeSpan.FromSeconds(30) : ttl;
        var now = DateTime.UtcNow;
        var url = request.RequestUri.ToString();

        if (_cache.TryGetValue(url, out var entry) && now - entry.Timestamp < ttl)
            return (HttpResponseMessage)entry.Data;

        var response = await _httpClient.SendAsync(request, httpCompletionOption);
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (contentType == "application/json")
        {
            _cache[url] = new CacheEntry(now, response);
        }

        return response;
    }


    private record CacheEntry(DateTime Timestamp, object Data);
}