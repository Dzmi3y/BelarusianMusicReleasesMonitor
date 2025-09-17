namespace BMRM.Core.Features.ReleaseMonitor;

public interface IRecurringJobService
{
    void AddOrUpdateReleaseMonitor();

    void RemoveReleaseMonitor();
}