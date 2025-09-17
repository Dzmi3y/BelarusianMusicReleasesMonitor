using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Desktop.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private AppDbContext _appDbContext;
        private string _title = "BMRM";
        public ICommand AddTrackCommand => new DelegateCommand(OnAddTrack);
        
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private int _count = 0;

        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        public ObservableCollection<Release> Tracks { get; } = new();

        public MainWindowViewModel(AppDbContext appDbContext, ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _ = InitAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    _logger.LogError(t.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task InitAsync()
        {
             var trackList = await _appDbContext.Releases.AsNoTracking().ToListAsync();
             Tracks.AddRange(trackList);
        }

        private void OnAddTrack()
        {
        }
    }
}