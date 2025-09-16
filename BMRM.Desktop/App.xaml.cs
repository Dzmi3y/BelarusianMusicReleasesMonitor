using System;
using Prism.Ioc;
using BMRM.Desktop.Views;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using BMRM.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BMRM.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public IConfiguration Configuration { get; private set; }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterDB(containerRegistry);

            //containerRegistry.Register<IMusicRepository, MusicRepository>();
        }

        private void RegisterDB(IContainerRegistry containerRegistry)
        {
            var connectionString = Configuration.GetConnectionString("Default");
            
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var dbPath = builder.DataSource;

            var directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString, x => x.MigrationsAssembly("BMRM.Infrastructure"))
                .Options;

            var dbContext = new AppDbContext(options);


            dbContext.Database.Migrate();

            containerRegistry.RegisterInstance(dbContext);
        }

        private NotifyIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            string basePath = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
            base.OnStartup(e);
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/favicon.ico"),
                Visible = true,
                Text = "Belarusian Music Releases Monitor"
            };

            _notifyIcon.DoubleClick += (s, args) =>
            {
                Current.MainWindow.Show();
                Current.MainWindow.WindowState = WindowState.Normal;
            };

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, (s, args) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Current.Shutdown();
            });
        }
    }
}