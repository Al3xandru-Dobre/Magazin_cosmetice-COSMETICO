using GlowUp.API.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace GlowUp.API.Models.Identity;

/// <summary>
/// Extindem IdentityUser in loc sa scriem o clasa User proprie.
/// Mostenim gratuit: hash de parola, email confirmation, lockout,
/// two-factor, security stamp - toate testate si sigure.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public List<Order> Orders { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
