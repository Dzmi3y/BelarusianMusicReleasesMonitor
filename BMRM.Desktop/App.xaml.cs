using Prism.Ioc;
using BMRM.Desktop.Views;
using System.Windows;
using System.Windows.Forms;
using System.Drawing; 

namespace BMRM.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
        
        private NotifyIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
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