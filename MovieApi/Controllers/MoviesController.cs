using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs.Detail;
using MovieApi.DTOs.Movie;
using MovieApi.DTOs.MovieDetail;
using MovieApi.Models;

namespace MovieApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly MovieApiContext _context;
        private readonly IMapper _mapper;
        public MoviesController(MovieApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovie(
            [FromQuery] string? genre = null,
            [FromQuery] int? year = null,
            [FromQuery] string? actor = null)
        {
            var query = _context.Movie
                .Include(m => m.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(m => m.Genre != null && m.Genre.Name.ToLower() == genre.ToLower());

            if (year.HasValue)
                query = query.Where(m => m.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(actor))
                query = query.Where(m => m.MovieActors
                    .Any(ma => ma.Actor != null && ma.Actor.Name.ToLower() == actor.ToLower()));

            var movies = await query
                .ProjectTo<MovieDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(movies);
        }

        // GET: api/MoviesWithDetail/5
        [HttpGet("WithDetail/{id}")]
        public async Task<ActionResult<MovieDetailDto>> GetMovieWithDetail(int id)
        {
            var movieDto = await _context.Movie
                .Where(m => m.Id == id)
                .ProjectTo<MovieDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (movieDto == null)
            {
                return NotFound();
            }

            return Ok(movieDto);
        }

        // GET: api/Movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetMovie(int id)
        {
            var movie = await _context.Movie.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // PUT: api/Movies/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int? id, MovieUpdateDto movieDto)
        {
            // Reminder: put from a postman perspective should need everything (also id)
            // but this controller changes how it connects to the db through DTO

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _mapper.Map(movieDto, movie);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id)) { return NotFound(); }
                else { throw; }
            }

            return NoContent();
        }

        // PATCH: api/Movies/5
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchMovie(int id, [FromBody] JsonPatchDocument<MovieUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            var movieToPatch = _mapper.Map<MovieUpdateDto>(movie);

            patchDoc.ApplyTo(movieToPatch, ModelState);

            if (!TryValidateModel(movieToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(movieToPatch, movie);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Movies
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie(MovieCreateDto movieCreateDto)
        {
            var movie = new Movie
            {
                Title = movieCreateDto.Title,
                Image = movieCreateDto.Image,
                Year = movieCreateDto.Year,
                Duration = movieCreateDto.Duration,
                GenreId = movieCreateDto.GenreId
            };

            if (movieCreateDto.Detail != null)
            {
                movie.Details = new MovieDetails
                {
                    Synopsis = movieCreateDto.Detail.Synopsis,
                    Director = movieCreateDto.Detail.Director,
                    Language = movieCreateDto.Detail.Language,
                    Budget = movieCreateDto.Detail.Budget
                };
            }

            _context.Movie.Add(movie);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovie", new { id = movie.Id }, movie);
        }

        // POST: api/Movies/2/actors/3
        [Authorize]
        [HttpPost("{movieId}/actors/{actorId}")]
        public async Task<IActionResult> AddActorToMovie(int movieId, int actorId, [FromBody] string role)
        {
            var movie = await _context.Movie.FindAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(actorId);
            if (actor == null)
            {
                return NotFound();
            }

            var alreadyLinked = await _context.MovieActors
                .AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (alreadyLinked)
            {
                return BadRequest("This actor is already assigned to this movie.");
            }

            var movieActor = new MovieActor
            {
                MovieId = movieId,
                ActorId = actorId,
                Role = role
            };

            _context.MovieActors.Add(movieActor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Movies/2/actors/3
        [Authorize]
        [HttpDelete("{movieId}/actors/{actorId}")]
        public async Task<IActionResult> DeleteActorToMovie(int movieId, int actorId)
        {
            var movieActor = await _context.MovieActors.FindAsync(movieId, actorId);
            if (movieActor == null)
            {
                return NotFound("This actor is not assigned to this movie.");
            }

            _context.MovieActors.Remove(movieActor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Movies/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int? id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int? id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
