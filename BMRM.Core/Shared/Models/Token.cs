using BMRM.Core.Shared.Enums;

namespace BMRM.Core.Shared.Models;

public class Token
{
    public Guid Id { get; set; }
    public TokenType Type { get; set; }
    public string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}