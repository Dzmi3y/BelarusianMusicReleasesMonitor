namespace BMRM.Core.Features.Spotify;

public interface IReleaseSpotifyLinkerService
{
    Task LinkReleasesToSpotifyAsync();
}