using BMRM.Core.Features.Spotify.SpotifyResponseModels;

namespace BMRM.Core.Features.Spotify;

public interface ISpotifyPlaylistsService
{
    Task<SpotifyPlaylistResponse?> GetPlaylistReleasesAsync();
}