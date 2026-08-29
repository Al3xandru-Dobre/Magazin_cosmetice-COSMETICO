using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.Models.Entities;

/// <summary>
/// Entitatea centrala a catalogului.
/// Participa la 2 relatii One-to-Many (Category, Brand) si la 1 Many-to-Many (Ingredient).
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>URL relativ, ex: /images/serum.png (vezi Lab 5 - fisiere statice).</summary>
    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Soft delete: produsele comandate nu se sterg fizic, se dezactiveaza.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Token de concurenta optimista ([Timestamp] = rowversion in SQL Server).
    /// EF adauga WHERE RowVersion = @vechi la fiecare UPDATE: doi useri care
    /// cumpara simultan ultimul produs -> al doilea SaveChanges primeste
    /// DbUpdateConcurrencyException in loc sa lase stocul pe -1.
    /// Coloana e generata de baza de date; codul nu o seteaza niciodata.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ---- One-to-Many: Category are multe Product ----
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // ---- One-to-Many: Brand are multe Product ----
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }

    // ---- Many-to-Many: Product <-> Ingredient ----
    public List<Ingredient> Ingredients { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];
}

