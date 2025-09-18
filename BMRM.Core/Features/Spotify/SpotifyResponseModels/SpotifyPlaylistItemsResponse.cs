using System.Text.Json.Serialization;

namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class SpotifyPlaylistItemsResponse
{
    [JsonPropertyName("id")]
    public string PlaylistId { get; set; }

    [JsonPropertyName("snapshot_id")]
    public string SnapshotId { get; set; }

    [JsonPropertyName("tracks")]
    public PlaylistTrackCollection Tracks { get; set; }
    
    public int TotalTracks => Tracks?.Total ?? 0;
}