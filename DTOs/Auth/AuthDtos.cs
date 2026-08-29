using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Parola trebuie sa aiba minim 6 caractere.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    IList<string> Roles,
    DateTime ExpiresAt);
