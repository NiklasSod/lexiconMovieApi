namespace MovieApi.DTOs.Movie
{
    public class MovieDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int Year { get; init; }
        public int Duration { get; init; }
        public int GenreId { get; init; }
        public string GenreName { get; init; } = string.Empty;
    }
}
