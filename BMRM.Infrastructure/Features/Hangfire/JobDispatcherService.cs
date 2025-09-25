using BMRM.Core.Features.Hangfire;
using BMRM.Infrastructure.Features.Hangfire.Jobs;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobDispatcherService: IJobDispatcherService
{
    private readonly UpdateSpotifyPlaylistJob _UpdateSpotifyPlaylistJob;
    private readonly Dictionary<JobId, Func<Task>> _jobs;

    public JobDispatcherService()
    {
        _jobs = new Dictionary<JobId, Func<Task>>()
        {
            { JobId.UpdateSpotifyPlaylist, _UpdateSpotifyPlaylistJob.ExecuteJobAsync }
        };
    }

    public async Task DispatchAsync(JobId jobId)
    {
       await _jobs[jobId].Invoke();
    }
}