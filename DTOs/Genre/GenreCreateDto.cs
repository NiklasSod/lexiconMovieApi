using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Genre
{
    public class GenreCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
