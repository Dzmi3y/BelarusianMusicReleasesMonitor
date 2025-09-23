using System.Linq.Expressions;
using Hangfire.Storage;

namespace BMRM.Core.Features.Hangfire;

public interface IHangfireJobManager
{
    void AddOrUpdateJob<T>(string id, Expression<Action<T>> method, string cron) where T : class;
    void RemoveJob(string id);
    void TriggerJob(string id);
    public void ScheduleJob(Expression<Action> method, DateTime runAt);
    List<RecurringJobDto> GetJobs();
}
