using System.Text.Json.Serialization;

namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class PlaylistTrackCollection
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
}