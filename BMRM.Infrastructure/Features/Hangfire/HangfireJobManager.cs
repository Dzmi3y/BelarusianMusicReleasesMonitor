using System.Linq.Expressions;
using BMRM.Core.Features.Hangfire;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;

namespace BMRM.Infrastructure.Features.Hangfire;

public class HangfireJobManager : IHangfireJobManager
{
    private readonly IRecurringJobManager _manager;

    public HangfireJobManager(IRecurringJobManager manager)
    {
        _manager = manager;
    }

    public void AddOrUpdateJob<T>(string id, Expression<Action<T>> method, string cron) where T : class
    {
        _manager.AddOrUpdate<T>(id, method, cron, TimeZoneInfo.Local);
    }


    public void RemoveJob(string id) => _manager.RemoveIfExists(id);

    public void TriggerJob(string id) => RecurringJob.Trigger(id);
    public void ScheduleJob(Expression<Action> method, DateTime runAt)
    {
        BackgroundJob.Schedule(method, runAt - DateTime.Now);
    }


    public List<RecurringJobDto> GetJobs()
    {
        var connection = JobStorage.Current.GetConnection();
        return connection.GetRecurringJobs();
    }
}
