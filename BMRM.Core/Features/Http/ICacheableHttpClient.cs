namespace BMRM.Core.Features.Http;

public interface ICacheableHttpClient
{
    Task<T> SendAsync<T>(HttpRequestMessage request, HttpCompletionOption httpCompletionOption,
        TimeSpan ttl = default, Func<HttpResponseMessage, Task<T?>>? statusHandler = null);

    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption,
        TimeSpan ttl = default);
}
