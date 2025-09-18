using BMRM.Core.Features.Spotify.SpotifyResponseModels;

namespace BMRM.Core.Features.Spotify;

public interface ISpotifySearchService
{
    Task<SpotifyAlbumSearchResponse?> FindReleaseAsync(string artistName, string albumName);
}