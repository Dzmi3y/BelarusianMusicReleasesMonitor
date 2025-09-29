using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace BMRM.Desktop.ViewModels;

public class HangfireSettingsViewModel : BindableBase
{
    private string _title = "Job Settings";

    private readonly IJobManager _jobManager;
    public ICommand UpdateCommand { get; }
    public DelegateCommand<string> JobToggleCommand { get; }
    public DelegateCommand<string> RunJobOnceCommand { get; }
    public ObservableCollection<JobDefinition> Jobs { get; } = new();
    public ObservableCollection<JobLog> Logs { get; } = new();

    public ObservableCollection<string> JobColumnHeaders { get; } = new()
    {
        "JobId", "Cron", "", ""
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

    public HangfireSettingsViewModel(IRegionManager regionManager, IJobManager jobManager)
    {
        NavigateCommand = new DelegateCommand<string>(view =>
            regionManager.RequestNavigate("MainRegion", view));

        _jobManager = jobManager;
        UpdateCommand = new DelegateCommand(UpdateAll);
        RunJobOnceCommand = new DelegateCommand<string>(id =>
            RunJobOnce(id));
        JobToggleCommand = new DelegateCommand<string>(id =>
            JobToggle(id));
        
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

    private void RunJobOnce(string id)
    {
        _jobManager.RunJobNow(id);
        UpdateAll();
    }

    private void JobToggle(string id)
    {
        if (Jobs.FirstOrDefault(j => j.JobId == id) is { Enabled: true })
        {
            //_jobManager.RemoveJob(id);
        }
        else
        {
            //_jobManager.CreateOrUpdateJob(id, Cron.Daily(), true);
        }

        UpdateAll();
    }
}