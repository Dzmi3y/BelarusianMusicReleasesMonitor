using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.Hangfire;

public interface IJobRepository
{
    JobDefinition? Get(string jobId);
    IEnumerable<JobDefinition> GetAll();
    void Save(JobDefinition job);
    void Disable(string jobId);
    JobLog? GetLastLog(string jobId);
    void LogRun(string jobId, DateTime timestamp, bool success, string? error = null);
    List<JobLog> GetLastLogs(int count);
}
