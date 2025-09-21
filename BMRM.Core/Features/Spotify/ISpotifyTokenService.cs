using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.Spotify;

public interface ISpotifyTokenService
{
    Task<Token?> UpdateTokenAsync();
}