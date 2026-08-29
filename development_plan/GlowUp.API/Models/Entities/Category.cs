using System.ComponentModel.DataAnnotations;

namespace GlowUp.API.Models.Entities;

/// <summary>
/// Categorie de produse (Ingrijire ten, Machiaj, Par, Parfumuri).
/// Partea "one" din relatia One-to-Many cu Product.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Proprietate de navigare inversa. NU exista coloana pentru ea in baza de date;
    // EF Core o populeaza doar cand cerem explicit prin Include().
    public List<Product> Products { get; set; } = [];
}
