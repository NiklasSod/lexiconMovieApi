using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs.ActorRole;
using MovieApi.Models;

[Route("api/movies/{movieId}/cast")]
[ApiController]
public class ActorRoleController : ControllerBase
{
    private readonly MovieApiContext _context;
    private readonly IMapper _mapper;

    public ActorRoleController(MovieApiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/movies/2/cast
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActorRoleDto>>> GetCast(int movieId)
    {
        var movie = await _context.Movie.FindAsync(movieId);
        if (movie == null)
            return NotFound();

        var movieActors = await _context.MovieActors
            .Where(ma => ma.MovieId == movieId)
            .Include(ma => ma.Actor)
            .ToListAsync();

        var cast = _mapper.Map<IEnumerable<ActorRoleDto>>(movieActors);
        return Ok(cast);
    }

    // GET: api/movies/2/cast/3
    [HttpGet("{actorId}")]
    public async Task<ActionResult<ActorRoleDto>> GetCastMember(int movieId, int actorId)
    {
        var movieActor = await _context.MovieActors
            .Include(ma => ma.Actor)
            .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

        if (movieActor == null)
            return NotFound();

        var actorRoleDto = _mapper.Map<ActorRoleDto>(movieActor);
        return Ok(actorRoleDto);
    }

    // POST: api/movies/2/cast/3
    [Authorize]
    [HttpPost("{actorId}")]
    public async Task<IActionResult> AddCastMember(int movieId, int actorId, [FromBody] ActorRoleCreateDto roleDto)
    {
        var movie = await _context.Movie.FindAsync(movieId);
        if (movie == null)
            return NotFound("Movie not found.");

        var actor = await _context.Actors.FindAsync(actorId);
        if (actor == null)
            return NotFound("Actor not found.");

        var exists = await _context.MovieActors
            .AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

        if (exists)
            return Conflict("Actor is already in the cast for this movie.");

        var movieActor = _mapper.Map<MovieActor>(roleDto);
        movieActor.MovieId = movieId;
        movieActor.ActorId = actorId;

        _context.MovieActors.Add(movieActor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/movies/2/cast/3
    [Authorize]
    [HttpPut("{actorId}")]
    public async Task<IActionResult> UpdateCastMember(int movieId, int actorId, [FromBody] ActorRoleUpdateDto roleDto)
    {
        var movieActor = await _context.MovieActors
            .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

        if (movieActor == null)
            return NotFound();

        _mapper.Map(roleDto, movieActor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/movies/2/cast/3
    [Authorize]
    [HttpDelete("{actorId}")]
    public async Task<IActionResult> RemoveCastMember(int movieId, int actorId)
    {
        var movieActor = await _context.MovieActors
            .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

        if (movieActor == null)
            return NotFound();

        _context.MovieActors.Remove(movieActor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
