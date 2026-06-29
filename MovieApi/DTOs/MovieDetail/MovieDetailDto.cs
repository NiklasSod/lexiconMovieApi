using MovieApi.DTOs.Actor;
using MovieApi.DTOs.Detail;
using MovieApi.DTOs.Review;

namespace MovieApi.DTOs.MovieDetail
{
    public class MovieDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Duration { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;

        public DetailDto? Detail { get; set; }

        public List<ActorDto> Actors { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
