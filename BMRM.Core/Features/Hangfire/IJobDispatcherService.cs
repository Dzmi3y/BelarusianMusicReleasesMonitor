namespace BMRM.Core.Features.Hangfire;

public interface IJobDispatcherService
{
    Task DispatchAsync(JobId jobId);
}