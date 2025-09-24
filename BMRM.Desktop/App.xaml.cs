using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Forms;
using BMRM.Core.Configuration;
using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Http;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Desktop.Utils;
using BMRM.Desktop.ViewModels;
using BMRM.Desktop.Views;
using BMRM.Infrastructure.Data;
using BMRM.Infrastructure.Features.Hangfire;
using BMRM.Infrastructure.Features.Http;
using BMRM.Infrastructure.Features.ReleaseMonitor;
using BMRM.Infrastructure.Features.Spotify;
using DryIoc;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prism.Container.DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Serilog;
using Serilog.Events;

namespace BMRM.Desktop
{
    public partial class App : PrismApplication
    {
        private NotifyIcon _notifyIcon;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("MainRegion", nameof(NewReleasesView));

            InitializeTrayIcon();
            
            var dryIocContainer = Container.GetContainer(); // Prism DryIoc
            GlobalConfiguration.Configuration.UseActivator(new DryIocJobActivator(dryIocContainer));
            
            var storage = Container.Resolve<JobStorage>();
            var options = new BackgroundJobServerOptions
            {
                ServerName = "BMRM-Worker",
                WorkerCount = 2 
            };
            var server = new BackgroundJobServer(options, storage);

            MainWindow.Closing += (s, args) =>
            {
                args.Cancel = true;
                MainWindow.Hide();
                _notifyIcon.ShowBalloonTip(1000, "Minimized",
                    "The application is running in the background", ToolTipIcon.Info);
            };
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NewReleasesView, NewReleasesViewModel>();
            containerRegistry.RegisterForNavigation<HangfireSettingsView, HangfireSettingsViewModel>();
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent =>
                    logEvent.MessageTemplate.Text.Contains("Removing outdated records") ||
                    logEvent.MessageTemplate.Text.Contains("Server") ||
                    logEvent.MessageTemplate.Text.Contains("Executed DbCommand"))
                .WriteTo.Console()
                .WriteTo.File("logs/bmrm-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            containerRegistry.RegisterInstance<IConfiguration>(config);
            

            var releasePatternConfig = config.GetSection("ReleasePatterns").Get<ReleasePatternConfig>();
            containerRegistry.RegisterInstance<IOptions<ReleasePatternConfig>>(
                Options.Create(releasePatternConfig));
            
            var hangfireConnection = config.GetConnectionString("Hangfire");
            var hangfirePath = EnsureDirectoryExists(hangfireConnection);
            GlobalConfiguration.Configuration.UseSQLiteStorage(hangfirePath);
            containerRegistry.RegisterInstance(JobStorage.Current);
            containerRegistry.RegisterSingleton<IHangfireJobManager, HangfireJobManager>();
            containerRegistry.RegisterSingleton<IRecurringJobManager>(() => new RecurringJobManager(JobStorage.Current));
            containerRegistry.RegisterSingleton<IRecurringJobService, RecurringJobService>();
            
            containerRegistry.RegisterInstance<ICacheableHttpClient>(
                new CacheableHttpClient(new HttpClient()));


            var dbConnection = config.GetConnectionString("Default");
            EnsureDirectoryExists(dbConnection);
            containerRegistry.Register<AppDbContext>(() =>
                new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(dbConnection, x => x.MigrationsAssembly("BMRM.Infrastructure"))
                    .UseLazyLoadingProxies()
                    .Options));

            containerRegistry.RegisterSingleton<IReleaseMonitorJob, ReleaseMonitorJob>();
            containerRegistry.RegisterSingleton<IReleaseTextParserService, ReleaseTextParserService>();
            containerRegistry.RegisterSingleton<IHtmlDownloaderService, HtmlDownloaderService>();
            containerRegistry.RegisterSingleton<ISpotifyPlaylistsService, SpotifyPlaylistsService>();
            containerRegistry.RegisterSingleton<ISpotifySimpleTokenService, SpotifySimpleTokenService>();
            containerRegistry.RegisterSingleton<ISpotifySearchService, SpotifySearchService>();
            containerRegistry.RegisterSingleton<ISpotifyAlbumService, SpotifyAlbumService>();
            containerRegistry.RegisterSingleton<IReleaseSpotifyLinkerService, ReleaseSpotifyLinkerService>();
            containerRegistry.RegisterSingleton<IBelReleasePlaylistUpdaterService, BelReleasePlaylistUpdaterService>();
            containerRegistry.RegisterSingleton<ISpotifyCodeFlowTokenService, SpotifyCodeFlowTokenService>();
        }

        private string EnsureDirectoryExists(string connectionString)
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            var dbPath = builder.DataSource;
            var directory = Path.GetDirectoryName(dbPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return dbPath;
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/favicon.ico"),
                Visible = true,
                Text = "Belarusian Music Releases Monitor",
                ContextMenuStrip = new ContextMenuStrip()
            };

            _notifyIcon.DoubleClick += (s, args) =>
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            };

            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, args) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Shutdown();
            });
        }
    }
}
