using BMRM.Core.Enums;

namespace BMRM.Core.Models;

public class Release
{
    public Guid Id { get; set; }
    public string? Artist { get; set; }
    public string? Title { get; set; }
    public ReleaseType? Type { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Genres { get; set; }
    public string? City { get; set; }
}