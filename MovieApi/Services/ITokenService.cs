using MovieApi.Models;

namespace MovieApi.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
