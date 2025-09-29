namespace BMRM.Core.Features.ReleaseMonitor;
public interface IVkHtmlDownloaderService
{
    Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default);
}