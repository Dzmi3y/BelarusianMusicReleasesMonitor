namespace BMRM.Infrastructure.Utils;

public static class BandcampGenreMapper
{
    private static readonly Dictionary<int, string> _genres = new()
    {
        { 1, "Acoustic" },
        { 2, "Alternative" },
        { 3, "Ambient" },
        { 4, "Blues" },
        { 10, "Electronic" },
        { 11, "Experimental" },
        { 12, "Folk" },
        { 13, "Funk" },
        { 14, "Hip-Hop/Rap" },
        { 15, "Jazz" },
        { 18, "Metal" },
        { 19, "Pop" },
        { 20, "Punk" },
        { 23, "Rock" },
        { 24, "Soundtrack" }
    };

    public static string GetGenreById(int id)
    {
        return _genres.TryGetValue(id, out var genre) ? genre : "Unknown";
    }
}
