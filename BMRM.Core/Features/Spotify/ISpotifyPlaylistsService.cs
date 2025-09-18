using BMRM.Core.Features.Spotify.SpotifyResponseModels;

namespace BMRM.Core.Features.Spotify;

public interface ISpotifyPlaylistsService
{
    Task<SpotifyPlaylistItemsResponse?> GetPlaylistTracksAsync(string playlistId);
    Task<PlaylistChangeResponse?> AddPlaylistTracksAsync(string playlistId, List<string> trackIds);
    Task<PlaylistChangeResponse?> DeletePlaylistTracksAsync(string playlistId, List<string> trackIds,string snapshotId);
}