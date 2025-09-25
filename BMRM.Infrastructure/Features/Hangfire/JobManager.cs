using System.Linq.Expressions;
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

    public JobManager(IRecurringJobManager recurring, IBackgroundJobClient client, IJobRepository repo, IJobDispatcherService jobDispatcherService)
    {
        _recurring = recurring;
        _client = client;
        _repo = repo;
        _jobDispatcherService = jobDispatcherService;
    }

    public void CreateOrUpdateJob(JobId jobId, string cron, bool enabled)
    {
        JobDefinition job = new JobDefinition()
        {
            JobId = jobId.GetDescription(),
            Cron = cron,
            Enabled = enabled
        };
;
        
        _recurring.AddOrUpdate(job.JobId, ()=> ExecuteJob(jobId), job.Cron);
        _repo.Save(job);
    }

    public void RemoveJob(JobId jobId)
    {
        _recurring.RemoveIfExists(jobId.GetDescription());
        _repo.Disable(jobId.GetDescription());
    }

    public void RunJobNow(JobId jobId)
    {
        _client.Enqueue(() => ExecuteJob(jobId));
    }

    public IEnumerable<JobDefinition> GetAllJobs() => _repo.GetAll();

    private async Task ExecuteJob(JobId jobId)
    {
        var job = _repo.Get(jobId.GetDescription());
        if (job == null || !job.Enabled) return;

        try
        {
            await _jobDispatcherService.DispatchAsync(jobId);
            _repo.LogRun(jobId.GetDescription(), DateTime.UtcNow, true);
        }
        catch (Exception ex)
        {
            _repo.LogRun(jobId.GetDescription(), DateTime.UtcNow, false, ex.Message);
        }
    }
}