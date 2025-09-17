using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using BMRM.Core.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Storage.SQLite;
using BMRM.Infrastructure.Data;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Desktop.ViewModels;
using BMRM.Infrastructure.Features.ReleaseMonitor;
using BMRM.Desktop.Views;
using Prism.Events;
using Prism.Mvvm;
using Serilog;
using Serilog.Events;
using Application = System.Windows.Application;

namespace BMRM.Desktop
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
                .MinimumLevel.Override("Hangfire", LogEventLevel.Warning) 
                .Filter.ByExcluding(logEvent =>
                    logEvent.MessageTemplate.Text.Contains("Removing outdated records") ||
                    logEvent.MessageTemplate.Text.Contains("Server") ||
                    logEvent.MessageTemplate.Text.Contains("Executed DbCommand"))
                .WriteTo.Console()
                .WriteTo.File("logs/bmrm-log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7) 
                .CreateLogger();
            
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    
                    services.Configure<ReleasePatternConfig>(configuration.GetSection("ReleasePatterns"));
                    services.AddHttpClient<IHtmlDownloaderService, HtmlDownloaderService>();
                    
                    var hangfireConnection = configuration.GetConnectionString("Hangfire");
                    var hangfirePath = EnsureDirectoryExists(hangfireConnection);
                    services.AddHangfire(config =>
                        config.UseSQLiteStorage(hangfirePath));
                    services.AddHangfireServer();

                 
                    var dbConnection = configuration.GetConnectionString("Default");
                    EnsureDirectoryExists(dbConnection);
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(dbConnection, x => x.MigrationsAssembly("BMRM.Infrastructure")));

                   
                    services.AddSingleton<IReleaseMonitorJob, ReleaseMonitorJob>();
                    services.AddSingleton<IRecurringJobManager>(sp =>
                        new RecurringJobManager(sp.GetRequiredService<JobStorage>()));
                    services.AddSingleton<IRecurringJobService, RecurringJobService>();
                    
                    
                    services.AddSingleton<MainWindowViewModel>();
                
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();
            
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();


            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            MainWindow.Show();

            InitializeTrayIcon();
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

        private NotifyIcon _notifyIcon;

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
            };

            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, (s, args) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Shutdown();
            });
        }
    }
}
