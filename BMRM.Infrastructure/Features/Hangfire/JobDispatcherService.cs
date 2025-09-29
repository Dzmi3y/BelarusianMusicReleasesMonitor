using BMRM.Core.Features.Hangfire;
using BMRM.Core.Features.Spotify;
using BMRM.Infrastructure.Features.Hangfire.Jobs;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobDispatcherService: IJobDispatcherService
{
    private readonly UpdateSpotifyPlaylistJob _updateSpotifyPlaylistJob;
    private readonly Dictionary<JobId, Func<Task>> _jobs;

    public JobDispatcherService(IBelReleasePlaylistUpdaterService belReleasePlaylistUpdaterService)
    {
         _updateSpotifyPlaylistJob = new UpdateSpotifyPlaylistJob(belReleasePlaylistUpdaterService);
         
        _jobs = new Dictionary<JobId, Func<Task>>()
        {
            { JobId.UpdateSpotifyPlaylist, _updateSpotifyPlaylistJob.ExecuteJobAsync }
        };
    }

    public async Task DispatchAsync(JobId jobId)
    {
       await _jobs[jobId].Invoke();
    }
}