using System.Text.Json.Serialization;

namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class Album
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("album_type")]
    public string AlbumType { get; set; }

    [JsonPropertyName("total_tracks")]
    public int TotalTracks { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    public List<Artist> Artists { get; set; }
    
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }
}