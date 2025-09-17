namespace BMRM.Core.Features.Spotify;

public interface ISpotifyTokenService
{
    Task UpdateBearerTokenAsync();
}