using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BMRM.Infrastructure.Features.ReleaseMonitor;

public class BandcampReleaseMonitorService : IBandcampReleaseMonitorService
{
    private readonly IVkReleaseTextParserService _parser;
    private readonly IVkHtmlDownloaderService _downloader;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public BandcampReleaseMonitorService(
        AppDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }
    
    public async Task ParseAndSaveAsync()
    {
        try
        {
           
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