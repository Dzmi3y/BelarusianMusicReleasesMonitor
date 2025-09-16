using BMRM.Core.Configuration;
using BMRM.Core.Interfaces;
using BMRM.Infrastructure.Data;
using BMRM.Infrastructure.Services;
using BMRM.Worker;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/bmrm-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7) 
    .CreateLogger();


var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("Configuration/releasePatterns.json", optional: false, reloadOnChange: true);


Directory.CreateDirectory(builder.Configuration.GetConnectionString("Default")!);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"),
        x => x.MigrationsAssembly("BMRM.Infrastructure")));

builder.Services.AddHangfire(config =>
    config.UseSQLiteStorage(builder.Configuration.GetConnectionString("Hangfire")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IReleaseTextParserService, ReleaseTextParserService>();
builder.Services.AddScoped<IReleaseMonitorJob, ReleaseMonitorJob>();
builder.Services.AddHttpClient<IHtmlDownloaderService, HtmlDownloaderService>();
//builder.Services.AddHostedService<Worker>();
builder.Services.Configure<ReleasePatternConfig>(
    builder.Configuration.GetSection("ReleasePatterns"));
var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<IReleaseMonitorJob>(
        "release-monitor",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily);
}

host.Run();