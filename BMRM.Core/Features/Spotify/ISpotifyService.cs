namespace BMRM.Core.Features.Spotify;

public interface ISpotifyService
{
    Task<SpotifyResponse?> GetPlaylistReleasesAsync();
}