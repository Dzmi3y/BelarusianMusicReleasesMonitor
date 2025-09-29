using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.Hangfire;

public interface IJobManager
{
    void CreateOrUpdateJob(JobId jobId, string cron, bool enabled);
    void RemoveJob(JobId jobId);
    void RunJobNow(JobId jobId);
    IEnumerable<JobDefinition> GetAllJobs();
    List<JobLog> GetLastLogs(int count);
}
