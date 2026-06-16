using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Actor
{
    public class ActorCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
