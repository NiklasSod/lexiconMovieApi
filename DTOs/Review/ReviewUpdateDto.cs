using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Review
{
    public class ReviewUpdateDto
    {
        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
    }
}
