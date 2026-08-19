using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs.Auth;

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
