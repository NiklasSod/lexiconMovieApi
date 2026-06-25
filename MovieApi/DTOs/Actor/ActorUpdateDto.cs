using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Actor
{
    public class ActorUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
