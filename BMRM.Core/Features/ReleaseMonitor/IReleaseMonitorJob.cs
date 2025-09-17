namespace BMRM.Core.Features.ReleaseMonitor;

public interface IReleaseMonitorJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
    Task ParseAndSaveAsync(CancellationToken cancellationToken);
}