using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.ActorRole
{
    public class ActorRoleUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;
    }
}
