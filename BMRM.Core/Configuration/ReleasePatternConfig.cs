namespace BMRM.Core.Configuration;

public class ReleasePatternConfig
{
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ReleaseDate { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Dictionary<string, string> Types { get; set; } = new();
}
