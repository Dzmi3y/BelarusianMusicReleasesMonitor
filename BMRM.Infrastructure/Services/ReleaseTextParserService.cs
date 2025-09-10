using System.Text.RegularExpressions;
using BMRM.Core.Configuration;
using BMRM.Core.Enums;
using BMRM.Core.Interfaces;
using BMRM.Core.Models;
using Microsoft.Extensions.Options;

namespace BMRM.Infrastructure.Services;

public class ReleaseTextParserService(IOptions<ReleasePatternConfig> options) : IReleaseTextParserService
{
    private readonly ReleasePatternConfig _patterns = options.Value;
    private readonly string startWord = "Выканаўц[аы]";
    private readonly string endWord = "copy_history";

    public Release? ParseSingleReleaseBlock(string html)
    {
        var pattern = $"{startWord}.*?{endWord}";
        var match = Regex.Match(html, pattern, RegexOptions.Singleline);


        if (match.Success)
        {
            return ParseSingleRelease(match.Value);
        }
        else
        {
            return null;
        }
    }

    private Release? ParseSingleRelease(string input)
    {
        input = input.Replace("&lt;br&gt;", "\n")
            .Replace("&lt;", "\n")
            .Replace("&quot;", "\"");

        int endIndex = input.IndexOf(endWord, StringComparison.OrdinalIgnoreCase);
        if (endIndex > 0)
            input = input.Substring(0, endIndex);

        var release = new Release
        {
            Id = Guid.NewGuid(),

            Artist = ExtractValue(input, _patterns.Artist),
            Title = ExtractValue(input, _patterns.Title),
            ReleaseDate = ParseDate(ExtractValue(input, _patterns.ReleaseDate)),
            Genres = ExtractValue(input, _patterns.Genres),
            City = ExtractValue(input, _patterns.City),
        };

        release.Type = DetermineType(input);

        return release.Title != null ? release : null;
    }

    private string? ExtractValue(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Replace("\\/", "/");
        return DateTime.TryParseExact(raw, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date)
            ? date
            : null;
    }
    
    private ReleaseType? DetermineType(string input)
    {
        foreach (var kvp in _patterns.Types)
        {
            if (Regex.IsMatch(input, kvp.Value, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                if (Enum.TryParse<ReleaseType>(kvp.Key, ignoreCase: true, out var type))
                    return type;
            }
        }

        return null;
    }
}