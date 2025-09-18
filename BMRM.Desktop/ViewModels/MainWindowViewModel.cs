using System;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using BMRM.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Desktop.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private AppDbContext _appDbContext;
        private string _title = "BMRM";
        private IReleaseMonitorJob _releaseMonitorJob;
        private readonly ISpotifyPlaylistsService _spotifyPlaylistsService;
        private readonly ISpotifySearchService  _spotifySearchService;
        public ICommand UpdateCommand {get;}
        
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

        public MainWindowViewModel(AppDbContext appDbContext, ILogger<MainWindowViewModel> logger,
            IReleaseMonitorJob  releaseMonitorJob,ISpotifyPlaylistsService  spotifyPlaylistsService, 
            ISpotifySearchService  spotifySearchService)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _releaseMonitorJob =  releaseMonitorJob;
            _spotifyPlaylistsService = spotifyPlaylistsService;
            _spotifySearchService = spotifySearchService;
            UpdateCommand = new DelegateCommand( () => _ = UpdateAsync());
            
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
             Tracks.Clear();
             Tracks.AddRange(trackList);
        }
        private async Task UpdateAsync()
        {
            
            var cts = new CancellationTokenSource();
            Tracks.Clear();
            try
            {
                 

               ///var res = await _spotifySearchService.FindReleaseAsync("Tehosekoitin", "Freak out");

                 var res  =await _spotifyPlaylistsService.GetPlaylistTracksAsync("1TafmgIyZEYPlqxoDXEhAb");
                // foreach (var release in res.Items)
                // {
                //     Tracks.Add(new Release(){Artist = release.Track.Artists.FirstOrDefault()?.Name, Title = release.Track.Name, Id = ReleaseHasher.GetId(release.Track.Artists.FirstOrDefault()?.Name,release.Track.Name)});
                // }

                // await _releaseMonitorJob.ParseAndSaveAsync(cts.Token);
                //await InitAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Операция отменена пользователем");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении");
            }

            
        }

    }
}