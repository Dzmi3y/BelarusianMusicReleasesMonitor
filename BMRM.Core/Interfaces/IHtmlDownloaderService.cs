namespace BMRM.Core.Interfaces;
public interface IHtmlDownloaderService
{
    Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default);
}