using System.ComponentModel.DataAnnotations;

namespace MovieApi.Models;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime Expiry { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;
}
