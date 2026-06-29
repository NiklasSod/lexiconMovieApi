using MovieApi.DTOs.Detail;
using MovieApi.DTOs.MovieDetail;
using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Movie
{
    public class MovieCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        [Range(1888, 2100)]
        public int Year { get; set; }

        [Range(1, 1000)]
        public int Duration { get; set; }

        [Range(1, int.MaxValue)]
        public int GenreId { get; set; }

        public DetailUpdateDto? Detail { get; set; }
    }
}
