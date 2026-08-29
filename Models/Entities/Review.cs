using System.ComponentModel.DataAnnotations;
using Magazin_cosmetice_COSMETICO.Models.Identity;

namespace Magazin_cosmetice_COSMETICO.Models.Entities;

public class Review
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MinLength(10)]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}

