namespace BMRM.Core.Shared.Models;

public class PlaylistTrack
{
    public string Id { get; set; }

    public string SpotifyTrackId { get; set; }
    public virtual SpotifyTrack SpotifyTrack { get; set; }
}