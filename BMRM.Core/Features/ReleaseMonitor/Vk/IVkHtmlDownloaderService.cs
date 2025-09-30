namespace BMRM.Core.Features.ReleaseMonitor.Vk;
public interface IVkHtmlDownloaderService
{
    Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default);
}