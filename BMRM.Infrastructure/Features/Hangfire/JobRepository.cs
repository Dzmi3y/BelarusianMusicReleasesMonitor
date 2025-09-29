using BMRM.Core.Features.Hangfire;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Infrastructure.Features.Hangfire;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _db;

    public JobRepository(AppDbContext db) => _db = db;

    public JobDefinition? Get(string jobId) =>
        _db.JobDefinitions.FirstOrDefault(j => j.JobId == jobId);

    public IEnumerable<JobDefinition> GetAll() =>
        _db.JobDefinitions.ToList();

    public void Save(JobDefinition job)
    {
        var existing = Get(job.JobId);
        if (existing == null)
            _db.JobDefinitions.Add(job);
        else
        {
            existing.Cron = job.Cron;
            existing.Enabled = job.Enabled;
        }
        _db.SaveChanges();
    }

    public void Disable(string jobId)
    {
        var job = Get(jobId);
        if (job != null)
        {
            job.Enabled = false;
            _db.SaveChanges();
        }
    }

    public JobLog? GetLastLog(string jobId)
    {
        var log = _db.JobLogs
            .AsNoTracking()
            .Where(j => j.JobId == jobId)
            .OrderByDescending(j => j.Timestamp)
            .FirstOrDefault();
        return log;
    }
    
    public List<JobLog> GetLastLogs(int count)
    {
        var log = _db.JobLogs
            .AsNoTracking()
            .OrderByDescending(j => j.Timestamp)
            .Take(count).ToList();
        return log;
    }

    public void LogRun(string jobId, DateTime timestamp, bool success, string? error = null)
    {
        _db.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Timestamp = timestamp,
            Success = success,
            ErrorMessage = error
        });
        _db.SaveChanges();
    }
}
