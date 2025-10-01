using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.ReleaseMonitor.Bandcamp;
using BMRM.Core.Features.ReleaseMonitor.Vk;
using BMRM.Core.Features.Spotify;
using BMRM.Infrastructure.Features.Hangfire.Jobs;
using Serilog;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobDispatcherService : IJobDispatcherService
{
    private readonly Dictionary<string, Func<Task>> _jobs;

    public JobDispatcherService(
        IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService,
        IVkReleaseMonitorService vkReleaseMonitorService,
        IBandcampReleaseMonitorService bandcampReleaseMonitorService,
        IReleaseSpotifyLinkerService releaseSpotifyLinkerService)
    {
        _jobs = RegisterJobs(
            belReleasePlaylistUpdaterService,
            vkReleaseMonitorService,
            bandcampReleaseMonitorService,
            releaseSpotifyLinkerService
        );
    }

    private Dictionary<string, Func<Task>> RegisterJobs(
        IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService,
        IVkReleaseMonitorService vkReleaseMonitorService,
        IBandcampReleaseMonitorService bandcampReleaseMonitorService,
        IReleaseSpotifyLinkerService releaseSpotifyLinkerService
    )
    {
        return new Dictionary<string, Func<Task>>
        {
            {
                JobIds.UpdateSpotifyPlaylist,
                new UpdateSpotifyPlaylistJob(belReleasePlaylistUpdaterService).ExecuteJobAsync
            },
            {
                JobIds.VkBelmuzParsing,
                new VkReleaseMonitorJob(vkReleaseMonitorService).ExecuteJobAsync
            },
            {
                JobIds.BandcampBelmuzParsing,
                new BandcampReleaseMonitorJob(bandcampReleaseMonitorService)
                    .ExecuteJobAsync
            },
            {
                JobIds.ReleaseSpotifyLinker,
                new ReleaseSpotifyLinkerJob(releaseSpotifyLinkerService).ExecuteJobAsync
            }
        };
    }

    public async Task DispatchAsync(string jobId)
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