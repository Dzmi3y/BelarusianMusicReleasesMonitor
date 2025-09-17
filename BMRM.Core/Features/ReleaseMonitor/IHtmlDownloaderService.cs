namespace BMRM.Core.Features.ReleaseMonitor;
public interface IHtmlDownloaderService
{
    Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default);
}