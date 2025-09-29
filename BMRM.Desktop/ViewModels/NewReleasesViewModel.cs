using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Serilog;

namespace BMRM.Desktop.ViewModels
{
    public class NewReleasesViewModel : BindableBase
    {
        private AppDbContext _appDbContext;
        private string _title = "BMRM";
        public ICommand UpdateCommand { get; }
        public DelegateCommand<string> NavigateCommand { get; }

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

        private DispatcherTimer _timer;


        public NewReleasesViewModel(AppDbContext appDbContext, IRegionManager regionManager)
        {
            _appDbContext = appDbContext;

            UpdateCommand = new DelegateCommand(() => _ = UpdateAsync());
            NavigateCommand = new DelegateCommand<string>(view =>
                regionManager.RequestNavigate("MainRegion", view));

            RunUpdateWithLogging();
        }

        private void RunUpdateWithLogging()
        {
            _ = UpdateAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Log.Logger.Error(t.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task UpdateAsync()
        {
            try
            {
                var trackList = await _appDbContext.Releases.AsNoTracking().ToListAsync();
                Tracks.Clear();
                Tracks.AddRange(trackList);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
            }
        }
    }
}