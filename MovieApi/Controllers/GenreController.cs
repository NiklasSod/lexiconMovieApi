using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs.Genre;
using MovieApi.Models;

namespace MovieApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IMovieApiContext _context;
        public GenreController(IMovieApiContext context)
        {
            _context = context;
        }

        // Post: api/Genre
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Genre>> CreateGenre([FromBody] GenreCreateDto genreDto)
        {
            var exists = await _context.Genres
                .AnyAsync(g => g.Name.ToLower() == genreDto.Name.ToLower());

            if (exists)
            {
                return BadRequest("Genre already exists.");
            }

            var genre = new Genre {
                Name = genreDto.Name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenreById), new { id = genre.Id }, genre);
        }

        // GET api/Genre/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenreById(int id)
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return NotFound();
            }

            return genre;
        }

        // DELETE api/Genre/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenreById(int id)
        {
            var genre = _context.Genres.FirstOrDefault(g => g.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}