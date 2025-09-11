using BMRM.Core.Interfaces;
using BMRM.Core.Models;
using BMRM.Infrastructure.Data;
using BMRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Worker;

public class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly string _url = configuration.GetValue<string>("ParsingPageUrl");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BMRM.Worker running at: {time}", DateTimeOffset.Now);

        using var scope = _scopeFactory.CreateScope();
        var parser = scope.ServiceProvider.GetRequiredService<IReleaseTextParserService>();
        var downloader = scope.ServiceProvider.GetRequiredService<IHtmlDownloaderService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

        try
        {
            using var reader = await downloader.GetHtmlStreamReaderAsync(_url, stoppingToken);
            var newReleases = new List<Release>();

            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                var release = parser.ParseSingleReleaseBlock(line);

                if (release is null) continue;

                bool exists = await db.Releases.AnyAsync(r => r.Id == release.Id, stoppingToken);
                if (!exists)
                {
                    newReleases.Add(release);
                }
            }

            if (newReleases.Count > 0)
            {
                db.Releases.AddRange(newReleases);
                await db.SaveChangesAsync(stoppingToken);
            }

            logger.LogInformation("Saved {Count} new releases", newReleases.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load or process HTML");
        }
        finally
        {
            logger.LogInformation("BMRM.Worker stopped at: {time}", DateTimeOffset.Now);
            lifetime.StopApplication();
        }
    }
}