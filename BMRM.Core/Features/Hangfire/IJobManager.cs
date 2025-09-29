using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.Hangfire;

public interface IJobManager
{
    void CreateOrUpdateJob(string jobId, string cron, bool enabled);
    void RemoveJob(string jobId);
    void RunJobNow(string jobId);
    IEnumerable<JobDefinition> GetAllJobs();
    List<JobLog> GetLastLogs(int count);
}
