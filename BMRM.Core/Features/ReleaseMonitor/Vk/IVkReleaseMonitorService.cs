namespace BMRM.Core.Features.ReleaseMonitor.Vk;

public interface IVkReleaseMonitorService
{
    Task ParseAndSaveAsync();
}