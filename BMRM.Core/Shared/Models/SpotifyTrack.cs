namespace BMRM.Core.Shared.Models;

public class SpotifyTrack
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string ReleaseId { get; set; }
    public virtual Release Release { get; set; }
}