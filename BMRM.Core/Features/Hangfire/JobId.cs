using System.ComponentModel;
using System.Reflection;

namespace BMRM.Core.Features.Hangfire;

public enum JobId
{
    [Description("update-spotify-playlist")]
    UpdateSpotifyPlaylist,
    [Description("vk-belmuz-parsing")]
    VkBelmuzParsing,
    [Description("bandcamp-belmuz-parsing")]
    BandcampBelmuzParsing,
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString();
    }
}
