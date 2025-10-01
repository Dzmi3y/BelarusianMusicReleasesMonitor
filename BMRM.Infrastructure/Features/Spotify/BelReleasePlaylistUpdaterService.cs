using BMRM.Core.Features.Spotify;
using BMRM.Core.Features.Spotify.SpotifyResponseModels;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BMRM.Infrastructure.Features.Spotify;

public class BelReleasePlaylistUpdaterService : IBelReleasePlaylistUpdaterService
{
    private readonly int _maxCount = 11;
    private readonly string _playlistId = "6fYIUA7NivMPlxvhDVFqXH";
    private readonly AppDbContext _db;
    private readonly ISpotifyPlaylistsService _spotifyPlaylistsService;

    public BelReleasePlaylistUpdaterService(AppDbContext db, ISpotifyPlaylistsService spotifyPlaylistsService)
    {
        _db = db;
        _spotifyPlaylistsService = spotifyPlaylistsService;
    }

    public async Task UpdateBelReleasePlaylistAsync()
    {
        try
        {
            var playlist = await _spotifyPlaylistsService.GetPlaylistTracksAsync(_playlistId);
            if (playlist == null)
                return;

            var existingTrackIds = playlist.Tracks.Items
                .Select(item => item.Track.Id)
                .ToHashSet();

            var candidateTrackIds = await _db.SpotifyTracks
                .Where(t => t.Playlists != null && !t.Playlists.Any(p => p.SpotifyPlaylistId == _playlistId))
                .Where(t => t.Playlists != null && !t.Playlists.Any(p => p.SpotifyPlaylistId == _playlistId))
                .Where(t => !existingTrackIds.Contains(t.Id))
                .OrderByDescending(t => t.Release.CreatedAt)
                .Select(t => t.Id)
                .Take(_maxCount)
                .ToListAsync();
            if (candidateTrackIds.Count == 0)
            {
                Log.Logger.Information("candidateTrackIds count is 0");
                return;
            }

            var totalAfterInsert = playlist.TotalTracks + candidateTrackIds.Count;
            await CleanupPlaylistAsync(totalAfterInsert, playlist);


            var result = await _spotifyPlaylistsService.AddPlaylistTracksAsync(_playlistId, candidateTrackIds);

            if (result != null)
            {
                var playlistTrackLinks = candidateTrackIds.Select(trackId => new PlaylistTrack
                {
                    Id = Guid.NewGuid(),
                    SpotifyPlaylistId = _playlistId,
                    SpotifyTrackId = trackId
                });

                await _db.BulkInsertAsync(playlistTrackLinks);
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error updating Belarusian release playlist");
            throw;
        }
    }

    private async Task CleanupPlaylistAsync(int totalAfterInsert, SpotifyPlaylistItemsResponse playlist)
    {
        if (totalAfterInsert > _maxCount)
        {
            var excessCount = totalAfterInsert - _maxCount;

            var trackIdsToRemove = playlist.Tracks.Items
                .TakeLast(excessCount)
                .Select(t => t.Track.Id)
                .ToList();

            await _spotifyPlaylistsService.DeletePlaylistTracksAsync(
                _playlistId,
                trackIdsToRemove,
                playlist.SnapshotId
            );

            var dbTracksToRemove = await _db.SpotifyTracks
                .Where(t => trackIdsToRemove.Contains(t.Id))
                .ToListAsync();
            
            await _db.BulkDeleteAsync(dbTracksToRemove);
            await _db.SaveChangesAsync();

            var orphanedReleases = await _db.Releases
                .Where(r => !r.Tracks.Any())
                .ToListAsync();

            if (orphanedReleases.Any())
            {
                await _db.BulkDeleteAsync(orphanedReleases);
                await _db.SaveChangesAsync();
            }
        }
    }
}