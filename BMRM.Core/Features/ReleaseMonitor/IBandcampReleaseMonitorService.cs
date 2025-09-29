namespace BMRM.Core.Features.ReleaseMonitor;

public interface IBandcampReleaseMonitorService
{
    Task ParseAndSaveAsync();
}