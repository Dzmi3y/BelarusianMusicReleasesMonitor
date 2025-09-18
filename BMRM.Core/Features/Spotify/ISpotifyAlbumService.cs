using BMRM.Core.Features.Spotify.SpotifyResponseModels;

namespace BMRM.Core.Features.Spotify;

public interface ISpotifyAlbumService
{
    Task<SpotifyTrackResponse?> GetAlbumTracksAsync(string albumId);
}