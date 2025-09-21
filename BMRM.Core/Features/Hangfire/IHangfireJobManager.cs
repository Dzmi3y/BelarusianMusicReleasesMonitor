using System.Linq.Expressions;
using Hangfire.Storage;

namespace BMRM.Core.Features.Hangfire;

public interface IHangfireJobManager
{
    void AddOrUpdateJob(string id, Expression<Action> method, string cron);
    void RemoveJob(string id);
    void TriggerJob(string id);
    List<RecurringJobDto> GetJobs();
}
