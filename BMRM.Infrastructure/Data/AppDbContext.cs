using BMRM.Core.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<SpotifyTrack> SpotifyTracks => Set<SpotifyTrack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Release>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<SpotifyTrack>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Release>()
            .HasMany(r => r.Tracks)
            .WithOne(t => t.Release)
            .HasForeignKey(t => t.ReleaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}