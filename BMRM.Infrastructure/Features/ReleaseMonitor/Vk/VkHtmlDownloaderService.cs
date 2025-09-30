using System.IO.Compression;
using System.Text;
using BMRM.Core.Features.Http;
using BMRM.Core.Features.ReleaseMonitor.Vk;
using Serilog;

namespace BMRM.Infrastructure.Features.ReleaseMonitor.Vk;

public class VkHtmlDownloaderService(ICacheableHttpClient cacheableHttpClient)
    : IVkHtmlDownloaderService
{
    private readonly ICacheableHttpClient _cacheableHttpClient = cacheableHttpClient;
    static VkHtmlDownloaderService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    public async Task<StreamReader> GetHtmlStreamReaderAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:118.0) Gecko/20100101 Firefox/118.0");

            var response = await _cacheableHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var charset = response.Content.Headers.ContentType?.CharSet;
            Encoding encoding;

            try
            {
                encoding = !string.IsNullOrWhiteSpace(charset)
                    ? Encoding.GetEncoding(charset)
                    : Encoding.GetEncoding("windows-1251");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to resolve encoding: {Charset}", charset);
                encoding = Encoding.GetEncoding("windows-1251");
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            var isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");
            var contentStream = isGzip
                ? new GZipStream(stream, CompressionMode.Decompress)
                : stream;

            return new StreamReader(contentStream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 8192, leaveOpen: false);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to download HTML from {Url}", url);
            throw;
        }
    }


}
