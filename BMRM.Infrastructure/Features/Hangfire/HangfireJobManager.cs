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

    public void AddOrUpdateJob(string id, Expression<Action> method, string cron)
    {
        _manager.AddOrUpdate(id, Job.FromExpression(method), cron);
    }

    public void RemoveJob(string id) => _manager.RemoveIfExists(id);

    public void TriggerJob(string id) => RecurringJob.Trigger(id);

    public List<RecurringJobDto> GetJobs()
    {
        var connection = JobStorage.Current.GetConnection();
        return connection.GetRecurringJobs();
    }
}
