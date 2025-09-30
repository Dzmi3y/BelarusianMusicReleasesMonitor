using Newtonsoft.Json;

namespace BMRM.Core.Features.ReleaseMonitor.Bandcamp;

public class BandcampSearchRequest
{
    [JsonProperty("category_id")]
    public int CategoryId { get; set; }

    [JsonProperty("tag_norm_names")]
    public List<string> TagNormNames { get; set; }

    [JsonProperty("geoname_id")]
    public int GeonameId { get; set; }

    [JsonProperty("slice")]
    public string Slice { get; set; }

    [JsonProperty("time_facet_id")]
    public int? TimeFacetId { get; set; }

    [JsonProperty("cursor")]
    public string Cursor { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("include_result_types")]
    public List<string> IncludeResultTypes { get; set; }
}
