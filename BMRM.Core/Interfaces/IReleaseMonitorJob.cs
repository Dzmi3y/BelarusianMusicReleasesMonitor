namespace BMRM.Core.Interfaces;

public interface IReleaseMonitorJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}