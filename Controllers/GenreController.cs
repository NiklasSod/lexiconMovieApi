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
        private readonly MovieApiContext _context;
        public GenreController(MovieApiContext context)
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

        // delete with auth
    }
}