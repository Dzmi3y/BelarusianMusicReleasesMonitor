namespace BMRM.Core.Features.Hangfire.Jobs;

public interface IJob
{
    Task ExecuteJobAsync();
}