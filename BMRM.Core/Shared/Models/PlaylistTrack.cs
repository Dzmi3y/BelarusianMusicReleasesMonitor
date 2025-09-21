namespace BMRM.Core.Shared.Models;

public class PlaylistTrack
{
    public Guid Id { get; set; }
    public  string SpotifyPlaylistId { get; set; }
    public string SpotifyTrackId { get; set; }
    public virtual SpotifyTrack SpotifyTrack { get; set; }
}