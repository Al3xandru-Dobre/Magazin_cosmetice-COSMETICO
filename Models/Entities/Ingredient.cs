using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.Models.Entities;

/// <summary>
/// Ingredient activ (retinol, niacinamida, acid hialuronic).
/// Partea a doua a relatiei Many-to-Many cu Product.
/// </summary>
public class Ingredient
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Marcheaza ingredientele problematice pentru piele sensibila.
    /// Permite un filtru real in catalog, nu doar o relatie decorativa.
    /// </summary>
    public bool IsAllergen { get; set; }

    // Skip navigation: EF Core 8 genereaza singur tabela de jonctiune
    // pe baza celor doua colectii (aici + Product.Ingredients).
    public List<Product> Products { get; set; } = [];
}

