using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Hangfire.Jobs;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Features.Spotify;
using BMRM.Infrastructure.Features.Hangfire.Jobs;
using Serilog;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobDispatcherService : IJobDispatcherService
{
    private readonly Dictionary<JobId, Func<Task>> _jobs;

    public JobDispatcherService(
        IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService,
        IVkReleaseMonitorService vkReleaseMonitorService,
        IBandcampReleaseMonitorService bandcampReleaseMonitorService)
    {
        _jobs = RegisterJobs(
            belReleasePlaylistUpdaterService,
            vkReleaseMonitorService,
            bandcampReleaseMonitorService
        );
    }

    private Dictionary<JobId, Func<Task>> RegisterJobs(
        IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService,
        IVkReleaseMonitorService vkReleaseMonitorService,
        IBandcampReleaseMonitorService bandcampReleaseMonitorService)
    {
        return new Dictionary<JobId, Func<Task>>
        {
            { JobId.UpdateSpotifyPlaylist, new UpdateSpotifyPlaylistJob(belReleasePlaylistUpdaterService).ExecuteJobAsync },
            { JobId.VkBelmuzParsing, new VkReleaseMonitorJob(vkReleaseMonitorService).ExecuteJobAsync },
            { JobId.BandcampBelmuzParsing, new BandcampReleaseMonitorJob(bandcampReleaseMonitorService).ExecuteJobAsync }
        };
    }

    public async Task DispatchAsync(JobId jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            Log.Information("Dispatching job: {JobId}", jobId);
            await job.Invoke();
        }
        else
        {
            Log.Warning("Job with ID '{JobId}' is not registered in dispatcher.", jobId);
            throw new ArgumentException($"Job with ID '{jobId}' is not registered.");
        }
    }
}