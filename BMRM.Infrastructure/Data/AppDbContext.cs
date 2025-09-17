using BMRM.Core.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Release> Releases => Set<Release>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Release>()
            .HasKey(r => r.Id);
    }

}