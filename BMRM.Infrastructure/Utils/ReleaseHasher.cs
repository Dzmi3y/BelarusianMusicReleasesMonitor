using System.Security.Cryptography;
using System.Text;
using BMRM.Core.Enums;

namespace BMRM.Infrastructure.Utils;

public static class ReleaseHasher
{
    public static string GetId(string? artist, string? title, ReleaseType? type)
    {
        var normalized = $"{artist?.Trim().ToLowerInvariant()}|{title?.Trim().ToLowerInvariant()}|{type?.ToString()?.ToLowerInvariant()}";

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(normalized));

        return Convert.ToBase64String(hashBytes)
            .Replace('+', '-') 
            .Replace('/', '_') 
            .TrimEnd('=');  
    }
}
