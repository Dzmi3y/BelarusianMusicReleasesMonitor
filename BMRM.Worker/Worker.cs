using BMRM.Core.Interfaces;
using BMRM.Core.Models;
using BMRM.Infrastructure.Services;

namespace BMRM.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }


            //test
            using var scope = _scopeFactory.CreateScope();
            var parser = scope.ServiceProvider.GetRequiredService<IReleaseTextParserService>();
            var downloader = scope.ServiceProvider.GetRequiredService<IHtmlDownloaderService>();


            try
            {
                using var reader = await downloader.GetHtmlStreamReaderAsync("https://vk.com/wall-75669943?own=1", stoppingToken);
                
                var list = new List<Release>();
                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();

                    var res = parser.ParseSingleReleaseBlock(line);
                    if (res is not null)
                    {
                        list.Add(res);
                    }
                }
                // test
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load HTML");
            }

            logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}