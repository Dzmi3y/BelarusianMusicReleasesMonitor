using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BMRM.Infrastructure.Features.ReleaseMonitor;

public class ReleaseMonitorJob : IReleaseMonitorJob
{
    private readonly ILogger<ReleaseMonitorJob> _logger;
    private readonly IReleaseTextParserService _parser;
    private readonly IHtmlDownloaderService _downloader;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public ReleaseMonitorJob(
        ILogger<ReleaseMonitorJob> logger,
        IReleaseTextParserService parser,
        IHtmlDownloaderService downloader,
        AppDbContext db,
        IConfiguration configuration)
    {
        _logger = logger;
        _parser = parser;
        _downloader = downloader;
        _db = db;
        _configuration = configuration;
    }


    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ReleaseMonitorJob started at: {time}", DateTimeOffset.Now);
        var random = new Random();
        var delayMinutes = random.Next(0, 360);
        _logger.LogInformation("ReleaseMonitorJob wait for: {time}", TimeSpan.FromMinutes(delayMinutes));
        await Task.Delay(TimeSpan.FromMinutes(delayMinutes), cancellationToken);

        await ParseAndSaveAsync(cancellationToken);
    }

    public async Task ParseAndSaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            var url = _configuration["ParsingPageUrl"];
            using var reader = await _downloader.GetHtmlStreamReaderAsync(url, cancellationToken);
            var newReleases = new List<Release>();

            var knownIds = new HashSet<string>(
                await _db.Releases.Select(r => r.Id).ToListAsync(cancellationToken)
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
                await _db.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Saved {Count} new releases", newReleases.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load or process HTML");
        }
        finally
        {
            _logger.LogInformation("ReleaseMonitorJob finished at: {time}", DateTimeOffset.Now);
        }
    }
}