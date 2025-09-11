using BMRM.Core.Configuration;
using BMRM.Core.Interfaces;
using BMRM.Infrastructure.Data;
using BMRM.Infrastructure.Services;
using BMRM.Worker;
using Microsoft.EntityFrameworkCore;


var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("Configuration/releasePatterns.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"),
        x => x.MigrationsAssembly("BMRM.Infrastructure")));
builder.Services.AddScoped<IReleaseTextParserService, ReleaseTextParserService>();
builder.Services.AddHttpClient<IHtmlDownloaderService, HtmlDownloaderService>();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<ReleasePatternConfig>(
    builder.Configuration.GetSection("ReleasePatterns"));


var host = builder.Build();
host.Run();