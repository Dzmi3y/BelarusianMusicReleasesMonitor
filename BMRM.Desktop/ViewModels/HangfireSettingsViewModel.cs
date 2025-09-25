using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
    private readonly IBelReleasePlaylistUpdaterService  _belReleasePlaylistUpdaterService;

    public ICommand UpdateCommand { get; }
    public ICommand AddCommand { get; }
    public DelegateCommand<string> DeleteCommand { get; }

    public ObservableCollection<JobDefinition> Jobs { get; } = new();

    public ObservableCollection<string> ColumnHeaders { get; } = new()
    {
        "JobId", "Cron", "Enabled",
    };
    
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    public DelegateCommand<string> NavigateCommand { get; }

    public HangfireSettingsViewModel(IRegionManager regionManager, IJobManager jobManager,IBelReleasePlaylistUpdaterService  belReleasePlaylistUpdaterService)
    {
        NavigateCommand = new DelegateCommand<string>(view =>
            regionManager.RequestNavigate("MainRegion", view));

        _jobManager = jobManager;
        UpdateCommand = new DelegateCommand(UpdateJobsList);
        AddCommand = new DelegateCommand(AddJob);
        DeleteCommand = new DelegateCommand<string>(id =>
            DeleteJob(id));

        Jobs.AddRange(_jobManager.GetAllJobs());
        _belReleasePlaylistUpdaterService = belReleasePlaylistUpdaterService;
    }

    private void AddJob()
    {
        // _hangfireJobManager.AddOrUpdateJob<IBelReleasePlaylistUpdaterService>(id: "my-job-id",
        //     method: service => service.UpdateBelReleasePlaylistAsync(),
        //     cron: Cron.Minutely());
        
        UpdateJobsList();
    }
    

    private void DeleteJob(string id)
    {
        _jobManager.RemoveJob(JobId.UpdateSpotifyPlaylist);
        UpdateJobsList();
    }

    private void UpdateJobsList()
    {
        Jobs.Clear();
        Jobs.AddRange(_jobManager.GetAllJobs());
    }
}