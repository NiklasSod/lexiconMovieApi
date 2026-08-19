using MovieApi.Models;

namespace MovieApi.Services;

public interface ITokenService
{
    string CreateAccessToken(User user);

    string CreateRefreshToken();

    string HashRefreshToken(string refreshToken);

    DateTime GetRefreshTokenExpiry();
}
