using System.Text.RegularExpressions;
using BMRM.Core.Configuration;
using BMRM.Core.Features.ReleaseMonitor;
using BMRM.Core.Shared.Enums;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Utils;
using Microsoft.Extensions.Options;

namespace BMRM.Infrastructure.Features.ReleaseMonitor;

public class ReleaseTextParserService(IOptions<ReleasePatternConfig> options) : IReleaseTextParserService
{
    private readonly ReleasePatternConfig _patterns = options.Value;

    private static readonly string StartWord = "Выканаўц[аы]";
    private static readonly string EndWord = "copy_history";

    public Release? ParseSingleReleaseBlock(string html)
    {
        var match = Regex.Match(html, $"{StartWord}.*?{EndWord}", RegexOptions.Singleline);
        return match.Success ? ParseSingleRelease(match.Value) : null;
    }

    private Release? ParseSingleRelease(string input)
    {
        input = DecodeHtml(input);

        int endIndex = input.IndexOf(EndWord, StringComparison.OrdinalIgnoreCase);
        if (endIndex > 0)
            input = input[..endIndex];

        var artist = ExtractValue(input, _patterns.Artist);
        var title = ExtractValue(input, _patterns.Title);

        if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
            return null;

        var type = DetermineType(input);

        var release = new Release
        {
            Artist = artist,
            Title = title,
            ProcessingStatus = ProcessingStatus.New,
            ReleaseDate = ParseDate(ExtractValue(input, _patterns.ReleaseDate)),
            Genres = ExtractValue(input, _patterns.Genres),
            City = ExtractValue(input, _patterns.City),
            Type = type,
            CreatedAt = DateTime.Now,
            Id = ReleaseHasher.GetId(artist, title)
        };

        return release;
    }

    private static string DecodeHtml(string input) =>
        input.Replace("&lt;br&gt;", "\n")
             .Replace("&lt;", "\n")
             .Replace("&quot;", "\"");

    private static string? ExtractValue(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        raw = raw.Replace("\\/", "/");
        return DateTime.TryParseExact(raw, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private ReleaseType? DetermineType(string input)
    {
        foreach (var (key, pattern) in _patterns.Types)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline) &&
                Enum.TryParse<ReleaseType>(key, true, out var type))
            {
                return type;
            }
        }

        return null;
    }
}