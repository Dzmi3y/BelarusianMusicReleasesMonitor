public class SpotifyResponse
{
    public List<Item> Items { get; set; }
}

public class Item
{
    public Track Track { get; set; }
}

public class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Artist> Artists { get; set; }
    public Album Album { get; set; }
}

public class Artist
{
    public string Name { get; set; }
}

public class Album
{
    public string ReleaseDate { get; set; }
}