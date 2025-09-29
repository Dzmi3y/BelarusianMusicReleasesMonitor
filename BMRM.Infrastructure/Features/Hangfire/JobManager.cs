using BMRM.Core.Features.Hangfire;
using BMRM.Core.Shared.Models;
using Hangfire;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobManager : IJobManager
{
    private readonly IRecurringJobManager _recurring;
    private readonly IBackgroundJobClient _client;
    private readonly IJobRepository _repo;
    private readonly IJobDispatcherService _jobDispatcherService;

    public JobManager(IRecurringJobManager recurring, IBackgroundJobClient client, IJobRepository repo,
        IJobDispatcherService jobDispatcherService)
    {
        _recurring = recurring;
        _client = client;
        _repo = repo;
        _jobDispatcherService = jobDispatcherService;
    }

    public void CreateOrUpdateJob(string jobId, string cron, bool enabled)
    {
        JobDefinition job = new JobDefinition()
        {
            JobId = jobId,
            Cron = cron,
            Enabled = enabled
        };
        ;

        _recurring.AddOrUpdate(job.JobId, () => ExecuteJob(jobId, false), job.Cron);
        _repo.Save(job);
    }

    public void RemoveJob(string jobId)
    {
        _recurring.RemoveIfExists(jobId);
        _repo.Disable(jobId);
    }

    public void RunJobNow(string jobId)
    {
        _client.Enqueue(() => ExecuteJob(jobId, true));
    }

    public IEnumerable<JobDefinition> GetAllJobs() => _repo.GetAll();

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteJob(string jobId, bool IsRunOnce = false)
    {
        var job = _repo.Get(jobId);

        if (job is null)
            return;

        if (!job.Enabled && !IsRunOnce)
            return;


        var lastLog = _repo.GetLastLog(jobId);
        if (lastLog != null)
        {
            var timeSinceLastLog = DateTime.UtcNow - lastLog.Timestamp;
            if (timeSinceLastLog < TimeSpan.FromSeconds(20))
            {
                _repo.LogRun(jobId, DateTime.UtcNow, false, "Rejected");
                return;
            }
        }

        try
        {
            if (!IsRunOnce)
            {
                await DelayAsync(jobId, 10);
            }

            await _jobDispatcherService.DispatchAsync(jobId);
            _repo.LogRun(jobId, DateTime.UtcNow, true);
        }
        catch (Exception ex)
        {
            _repo.LogRun(jobId, DateTime.UtcNow, false, ex.Message);
        }
    }

    public List<JobLog> GetLastLogs(int count)
    {
        return _repo.GetLastLogs(count);
    }

    private async Task DelayAsync(string jobId, int delay)
    {
        var random = new Random();
        var delayMinutes = random.Next(0, delay);
        var info = $"{jobId} wait for: {TimeSpan.FromMinutes(delayMinutes)}";
        _repo.LogRun(jobId, DateTime.UtcNow, false, info);
        await Task.Delay(TimeSpan.FromMinutes(delayMinutes));
    }
}