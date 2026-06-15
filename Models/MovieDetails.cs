namespace MovieApi.Models
{
    public class MovieDetails
    {
        public int Id { get; set; }
        public string Synopsis { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Budget { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
    }
}
