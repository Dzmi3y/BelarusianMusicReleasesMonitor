using System.Text.Json.Serialization;

namespace BMRM.Core.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReleaseType
{
    Album,
    EP,
    Single,
    Video,
    
}