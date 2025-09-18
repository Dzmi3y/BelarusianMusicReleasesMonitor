using System.Text.Json.Serialization;

namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class Artist
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}