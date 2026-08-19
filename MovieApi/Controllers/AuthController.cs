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

        return Ok(new AuthResponseDto { Token = _tokenService.CreateToken(user) });
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

        return Ok(new AuthResponseDto { Token = _tokenService.CreateToken(user) });
    }
}
