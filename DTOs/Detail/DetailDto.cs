namespace MovieApi.DTOs.Detail
{
    public class DetailDto
    {
        public int Id { get; set; }
        public string Synopsis { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public decimal Budget { get; set; }
    }
}
