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
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovie()
        {
            var movies = await _context.Movie.ToListAsync();
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
        public async Task<IActionResult> PutMovie(int? id, Movie movie)
        {
            if (id != movie.Id)
            {
                return BadRequest();
            }

            _context.Entry(movie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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
                Year = movieCreateDto.Year,
                Duration = movieCreateDto.Duration,
                GenreId = movieCreateDto.GenreId
            };

            if (movieCreateDto.Detail != null)
            {
                movieCreateDto.Detail = new DetailUpdateDto
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