using System.ComponentModel.DataAnnotations;

namespace MovieApi.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Duration { get; set; }

        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        public MovieDetails? Details { get; set; }

        public List<Review> Reviews { get; set; } = new();

        public List<MovieActor> MovieActors { get; set; } = new();
    }
}
