using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.ReleaseMonitor;

namespace BMRM.Infrastructure.Features.Hangfire.Jobs;

public class VkReleaseMonitorJob:IVkReleaseMonitorJob
{
    private readonly IVkReleaseMonitorService _vkReleaseMonitorService;
    public VkReleaseMonitorJob(IVkReleaseMonitorService vkReleaseMonitorService)
    {
        _vkReleaseMonitorService = vkReleaseMonitorService;
    }
        
    public async Task ExecuteJobAsync()
    {
        await _vkReleaseMonitorService.ParseAndSaveAsync();
    }
}
