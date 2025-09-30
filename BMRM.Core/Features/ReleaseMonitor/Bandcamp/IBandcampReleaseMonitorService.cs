namespace BMRM.Core.Features.ReleaseMonitor.Bandcamp;

public interface IBandcampReleaseMonitorService
{
    Task ParseAndSaveAsync();
}