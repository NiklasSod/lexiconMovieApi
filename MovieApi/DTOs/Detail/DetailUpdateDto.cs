namespace MovieApi.DTOs.Detail
{
    public class DetailUpdateDto
    {
        public string Synopsis { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public decimal Budget { get; set; }
    }
}
