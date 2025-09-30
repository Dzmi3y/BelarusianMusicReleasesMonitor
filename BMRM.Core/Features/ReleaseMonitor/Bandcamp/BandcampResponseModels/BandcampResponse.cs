using System.Text.Json.Serialization;

namespace BMRM.Core.Features.ReleaseMonitor.Bandcamp.BandcampResponseModels;

public class BandcampResponse
{
    [JsonPropertyName("results")]
    public List<BandcampItem> Results { get; set; } = new();
}