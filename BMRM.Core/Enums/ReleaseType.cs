using System.Text.Json.Serialization;

namespace BMRM.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReleaseType
{
    Album,
    EP,
    Single,
    Video,
    
}