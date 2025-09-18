using System.Text.Json.Serialization;

namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class PlaylistChangeResponse
{
    [JsonPropertyName("snapshot_id")]
    public string SnapshotId { get; set; }
}