using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs.Auth;
using MovieApi.Models;
using MovieApi.Services;

namespace MovieApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMovieApiContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(IMovieApiContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(AuthRequestDto dto)
    {
        var username = dto.Username.Trim();

        var exists = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());

        if (exists)
        {
            return Conflict("Username already exists.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(await IssueAndPersistTokensAsync(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(AuthRequestDto dto)
    {
        var username = dto.Username.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(await IssueAndPersistTokensAsync(user));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto dto)
    {
        var refreshToken = dto.RefreshToken.Trim();

        if (refreshToken.Length == 0)
        {
            return BadRequest("Refresh token is required.");
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken is null || storedToken.Expiry < DateTime.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        // Rotate: invalidate the used refresh token and issue a fresh pair.
        _context.RefreshTokens.Remove(storedToken);
        await _context.SaveChangesAsync();

        return Ok(await IssueAndPersistTokensAsync(storedToken.User));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequestDto dto)
    {
        var refreshToken = dto.RefreshToken.Trim();

        if (refreshToken.Length > 0)
        {
            var tokenHash = _tokenService.HashRefreshToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (storedToken is not null)
            {
                _context.RefreshTokens.Remove(storedToken);
                await _context.SaveChangesAsync();
            }
        }

        return NoContent();
    }

    private AuthResponseDto IssueTokens(User user)
    {
        return new AuthResponseDto
        {
            AccessToken = _tokenService.CreateAccessToken(user),
            RefreshToken = _tokenService.CreateRefreshToken(),
        };
    }

    private async Task<AuthResponseDto> IssueAndPersistTokensAsync(User user)
    {
        var response = IssueTokens(user);

        _context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = _tokenService.HashRefreshToken(response.RefreshToken),
            Expiry = _tokenService.GetRefreshTokenExpiry(),
            UserId = user.Id,
        });

        await _context.SaveChangesAsync();

        return response;
    }
}
