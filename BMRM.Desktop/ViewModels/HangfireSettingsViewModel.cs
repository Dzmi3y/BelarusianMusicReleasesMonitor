using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Spotify;
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
    
    private readonly IHangfireJobManager _hangfireJobManager;
    private readonly IBelReleasePlaylistUpdaterService  _belReleasePlaylistUpdaterService;

    public ICommand UpdateCommand { get; }
    public ICommand AddCommand { get; }
    public DelegateCommand<string> DeleteCommand { get; }

    public ObservableCollection<RecurringJobDto> Jobs { get; } = new();

    public ObservableCollection<string> ColumnHeaders { get; } = new()
    {
        "Id", "CreatedAt", "LastExecution", "NextExecution", "Queue", " Removed", " RetryAttempt", " Cron"
    };


    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    public DelegateCommand<string> NavigateCommand { get; }

    public HangfireSettingsViewModel(IRegionManager regionManager, IHangfireJobManager hangfireJobManager,IBelReleasePlaylistUpdaterService  belReleasePlaylistUpdaterService)
    {
        NavigateCommand = new DelegateCommand<string>(view =>
            regionManager.RequestNavigate("MainRegion", view));

        _hangfireJobManager = hangfireJobManager;
        UpdateCommand = new DelegateCommand(UpdateJobsList);
        AddCommand = new DelegateCommand(AddJob);
        DeleteCommand = new DelegateCommand<string>(id =>
            DeleteJob(id));

        Jobs.AddRange(_hangfireJobManager.GetJobs());
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
        _hangfireJobManager.RemoveJob(id);
        UpdateJobsList();
    }

    private void UpdateJobsList()
    {
        Jobs.Clear();
        Jobs.AddRange(_hangfireJobManager.GetJobs());
    }
}