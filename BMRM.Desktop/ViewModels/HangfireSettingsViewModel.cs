using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using Hangfire;
using Hangfire.Storage;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Serilog;

namespace BMRM.Desktop.ViewModels;

public class HangfireSettingsViewModel : BindableBase
{
    private string _title = "HangfireSettings";

    private readonly IJobManager _jobManager;
    private readonly IBelReleasePlaylistUpdaterService _belReleasePlaylistUpdaterService;

    public ICommand UpdateCommand { get; }
    public ICommand AddCommand { get; }
    public DelegateCommand<string> DeleteCommand { get; }

    public ObservableCollection<JobDefinition> Jobs { get; } = new();
    public ObservableCollection<JobLog> Logs { get; } = new();

    public ObservableCollection<string> JobColumnHeaders { get; } = new()
    {
        "JobId", "Cron", "Enabled",
    };

    public ObservableCollection<string> LogColumnHeaders { get; } = new()
    {
        "Id", "JobId", "Timestamp", "Success", "ErrorMessage"
    };
    
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    public DelegateCommand<string> NavigateCommand { get; }

    private DispatcherTimer _timer;

    public HangfireSettingsViewModel(IRegionManager regionManager, IJobManager jobManager,
        IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService)
    {
        NavigateCommand = new DelegateCommand<string>(view =>
            regionManager.RequestNavigate("MainRegion", view));

        _jobManager = jobManager;
        UpdateCommand = new DelegateCommand(UpdateAll);
        AddCommand = new DelegateCommand(AddJob);
        DeleteCommand = new DelegateCommand<string>(id =>
            DeleteJob(id));

        _belReleasePlaylistUpdaterService = belReleasePlaylistUpdaterService;

        UpdateAll();
        SetUpTimer();
    }

    private void SetUpTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _timer.Tick += (s, e) => UpdateAll();
        _timer.Start();
    }

    private void UpdateLogs()
    {
        Logs.Clear();
        Logs.AddRange(_jobManager.GetLastLogs(10));
    }

    private void UpdateJobsList()
    {
        Jobs.Clear();
        Jobs.AddRange(_jobManager.GetAllJobs());
    }

    private void UpdateAll()
    {
        UpdateJobsList();
        UpdateLogs();
    }

    private void AddJob()
    {
        _jobManager.CreateOrUpdateJob(JobId.UpdateSpotifyPlaylist, Cron.Minutely(), true);

        UpdateJobsList();
    }


    private void DeleteJob(string id)
    {
        _jobManager.RemoveJob(JobId.UpdateSpotifyPlaylist);
        UpdateJobsList();
    }
}