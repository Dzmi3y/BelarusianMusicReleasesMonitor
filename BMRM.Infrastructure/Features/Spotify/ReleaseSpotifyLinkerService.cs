using BMRM.Core.Features.Spotify;
using BMRM.Core.Shared.Enums;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;


namespace BMRM.Infrastructure.Features.Spotify;

public class ReleaseSpotifyLinkerService:IReleaseSpotifyLinkerService
{
    private readonly AppDbContext _db;
    private readonly ISpotifyAlbumService _spotifyAlbumService;
    private readonly ISpotifySearchService _spotifySearchService;

    public ReleaseSpotifyLinkerService(AppDbContext db, ISpotifyAlbumService spotifyAlbumService,
        ISpotifySearchService spotifySearchService)
    {
        _db = db;
        _spotifyAlbumService = spotifyAlbumService;
        _spotifySearchService = spotifySearchService;
    }

    public async Task LinkReleasesToSpotifyAsync()
    {
        try
        {
            var newReleases =
                _db.Releases
                    .Where(r => r.ProcessingStatus == ProcessingStatus.New)
                    .ToList();
            foreach (var release in newReleases)
            {
                var spotifyResponse = await _spotifySearchService.FindReleaseAsync(release.Artist, release.Title);
                var spotifyAlbumId = spotifyResponse?.Albums.Items.FirstOrDefault()?.Id;

                if (string.IsNullOrEmpty(spotifyAlbumId))
                {
                    release.ProcessingStatus = ProcessingStatus.NotFoundOnSpotify;
                    continue;
                }

                release.SpotifyAlbumId = spotifyAlbumId;
                release.ProcessingStatus = ProcessingStatus.FoundOnSpotify;

                var albumResponce = await _spotifyAlbumService.GetAlbumTracksAsync(spotifyAlbumId);
                var tracks = albumResponce?.Items.Select(t => new SpotifyTrack()
                {
                    Id = t.Id,
                    Name = t.Name,
                    ReleaseId = release.Id
                }).ToList();

                if (tracks != null)
                    await _db.BulkInsertAsync(tracks);
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
             Log.Logger.Error(ex, "Error while linking releases");
        }
    }
}