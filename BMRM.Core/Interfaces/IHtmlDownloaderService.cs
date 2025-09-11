public interface IHtmlDownloaderService
{
    Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default);
}