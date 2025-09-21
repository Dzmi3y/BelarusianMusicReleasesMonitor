using System;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BMRM.Desktop.ViewModels
{
    public class NewReleasesViewModel : BindableBase
    {
        private AppDbContext _appDbContext;
        private string _title = "BMRM";
        private IReleaseMonitorJob _releaseMonitorJob;
        private readonly ISpotifyPlaylistsService _spotifyPlaylistsService;
        private readonly ISpotifySearchService _spotifySearchService;
        private readonly IReleaseSpotifyLinkerService _releaseSpotifyLinkerService;
        private readonly IBelReleasePlaylistUpdaterService _belReleasePlaylistUpdaterService;
        public ICommand UpdateCommand { get; }
        public ICommand LinkedReleasesCommand { get; }

        public ICommand UpdatePlaylistCommand { get; }

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

        public NewReleasesViewModel(AppDbContext appDbContext, IReleaseMonitorJob releaseMonitorJob , ISpotifyPlaylistsService spotifyPlaylistsService,
            ISpotifySearchService spotifySearchService, IReleaseSpotifyLinkerService releaseSpotifyLinkerService,
            IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService)
        {
            _appDbContext = appDbContext;
            _releaseMonitorJob = releaseMonitorJob;
            _spotifyPlaylistsService = spotifyPlaylistsService;
            _spotifySearchService = spotifySearchService;
            _releaseSpotifyLinkerService = releaseSpotifyLinkerService;
            _belReleasePlaylistUpdaterService = belReleasePlaylistUpdaterService;

            UpdateCommand = new DelegateCommand(() => _ = UpdateAsync());
            LinkedReleasesCommand = new DelegateCommand(() => _ = LinkedReleasesAsync());
            UpdatePlaylistCommand = new DelegateCommand(() => _ = UpdatePlaylistAsync());
            
            _ = InitAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Log.Logger.Error(t.Exception.Message);
            
                     Log.Logger.Error(t.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task InitAsync()
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

        private async Task LinkedReleasesAsync()
        {
            await _releaseSpotifyLinkerService.LinkReleasesToSpotifyAsync();
            await InitAsync();
        }

        private async Task UpdateAsync()
        {
            var cts = new CancellationTokenSource();
            Tracks.Clear();
            try
            {
                await _releaseMonitorJob.ParseAndSaveAsync(cts.Token);
                await InitAsync();
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Information("Operation cancelled by user");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error during update");
            }
        }

        private async Task UpdatePlaylistAsync()
        {
            await _belReleasePlaylistUpdaterService.UpdateBelReleasePlaylistAsync();
        }
    }
}