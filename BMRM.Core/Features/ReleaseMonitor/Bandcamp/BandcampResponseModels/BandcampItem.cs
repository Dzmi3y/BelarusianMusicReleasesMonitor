using System.Text.Json.Serialization;

namespace BMRM.Core.Features.ReleaseMonitor.Bandcamp.BandcampResponseModels;

public class BandcampItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("band_name")]
    public string BandName { get; set; }

    [JsonPropertyName("band_location")]
    public string BandLocation { get; set; }

    [JsonPropertyName("band_genre_id")]
    public int BandGenreId { get; set; }

    [JsonPropertyName("release_date")]
    public string ReleaseDateRaw { get; set; }

    [JsonIgnore]
    public DateTime? ReleaseDate => DateTime.TryParse(ReleaseDateRaw, out var dt) ? dt : null;
}