using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Reviews;

public record ReviewDto(
    int Id,
    int ProductId,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    string UserName);

public class CreateReviewDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Produsul este obligatoriu.")]
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Ratingul trebuie sa fie intre 1 si 5.")]
    public int Rating { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "Comentariul trebuie sa aiba minim 10 caractere.")]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
