namespace BMRM.Core.Features.ReleaseMonitor;

public interface IVkReleaseMonitorService
{
    Task ParseAndSaveAsync();
}