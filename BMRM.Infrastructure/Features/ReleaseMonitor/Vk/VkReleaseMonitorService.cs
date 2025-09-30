using BMRM.Core.Features.ReleaseMonitor.Vk;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BMRM.Infrastructure.Features.ReleaseMonitor.Vk;

public class VkReleaseMonitorService : IVkReleaseMonitorService
{
    private readonly IVkReleaseTextParserService _parser;
    private readonly IVkHtmlDownloaderService _downloader;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public VkReleaseMonitorService(
        IVkReleaseTextParserService parser,
        IVkHtmlDownloaderService downloader,
        AppDbContext db,
        IConfiguration configuration)
    {
        _parser = parser;
        _downloader = downloader;
        _db = db;
        _configuration = configuration;
    }
    
    public async Task ParseAndSaveAsync()
    {
        try
        {
            var url = _configuration["ParsingPageUrl"];
            using var reader = await _downloader.GetHtmlStreamReaderAsync(url);
            var newReleases = new List<Release>();

            var knownIds = new HashSet<string>(
                await _db.Releases.Select(r => r.Id).ToListAsync()
            );

            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                var release = _parser.ParseSingleReleaseBlock(line);

                if (release is null) continue;

                if (knownIds.Add(release.Id))
                {
                    newReleases.Add(release);
                }

                if (newReleases.Count > 3) //////// test
                    break;
            }

            if (newReleases.Count > 0)
            {
                _db.Releases.AddRange(newReleases);
                await _db.SaveChangesAsync();
            }

            Log.Logger.Information("Saved {Count} new releases", newReleases.Count);
        }
        catch (Exception ex)
        {
             Log.Logger.Error(ex, "Failed to load or process HTML");
        }
        finally
        {
            Log.Logger.Information("ReleaseMonitorJob finished at: {time}", DateTimeOffset.Now);
        }
    }
}