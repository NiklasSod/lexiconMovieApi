using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MovieApi.DTOs.Review;
using MovieApi.Models;

namespace MovieApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly MovieApiContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(MovieApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/reviews/{movieId}
        [HttpGet("{movieId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForMovie(int movieId)
        {
            var movie = await _context.Movie.FindAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Where(r => r.MovieId == movieId)
                .Select(r => _mapper.Map<ReviewDto>(r))
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews/{movieId}
        [Authorize]
        [HttpPost("{movieId}")]
        public async Task<ActionResult<ReviewDto>> PostReviewForMovie(int movieId, ReviewCreateDto reviewDto)
        {
            var movie = await _context.Movie.FindAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var review = _mapper.Map<Review>(reviewDto);
            review.MovieId = movieId;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var returnDto = _mapper.Map<ReviewDto>(review);
            return CreatedAtAction("GetReviewsForMovie", new { movieId }, returnDto);
        }

        // DELETE: api/reviews/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int? id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
