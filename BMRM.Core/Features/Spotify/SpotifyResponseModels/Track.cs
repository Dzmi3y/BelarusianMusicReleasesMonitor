namespace BMRM.Core.Features.Spotify.SpotifyResponseModels;

public class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Artist> Artists { get; set; }
    public Album Album { get; set; }
}