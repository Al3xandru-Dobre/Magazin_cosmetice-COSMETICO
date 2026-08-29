using Magazin_cosmetice_COSMETICO.DTOs.Products;
using Magazin_cosmetice_COSMETICO.Models.Entities;

namespace Magazin_cosmetice_COSMETICO.Mapping;

/// <summary>
/// Mapare Entitate -> DTO scrisa ca EXTENSION METHODS (exact mecanismul din Lab 1:
/// clasa static, metoda static, primul parametru precedat de 'this').
///
/// De ce manual si nu AutoMapper? Pentru un proiect de aceasta marime,
/// maparea manuala e explicita, verificata de compilator si nu ascunde
/// query-uri N+1 in spatele conventiilor.
/// </summary>
public static class ProductMappings
{
    public static ProductDto ToDto(this Product p) => new(
        p.Id,
        p.Name,
        p.Price,
        p.StockQuantity,
        p.ImagePath,
        p.Category?.Name ?? "-",
        p.Brand?.Name ?? "-",
        p.Reviews.Count > 0 ? Math.Round(p.Reviews.Average(r => r.Rating), 2) : 0,
        p.Reviews.Count);

    public static ProductDetailDto ToDetailDto(this Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.StockQuantity,
        p.ImagePath,
        p.CreatedAt,
        p.CategoryId,
        p.Category?.Name ?? "-",
        p.BrandId,
        p.Brand?.Name ?? "-",
        p.Ingredients.Select(i => i.ToDto()).ToList(),
        p.Reviews.Count > 0 ? Math.Round(p.Reviews.Average(r => r.Rating), 2) : 0,
        p.Reviews.Count);

    public static IngredientDto ToDto(this Ingredient i) => new(i.Id, i.Name, i.IsAllergen);

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products)
        => products.Select(p => p.ToDto()).ToList();

    /// <summary>DTO -> entitate noua (fara Id: il genereaza baza de date).</summary>
    public static Product ToEntity(this CreateProductDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Description = dto.Description.Trim(),
        Price = dto.Price,
        StockQuantity = dto.StockQuantity,
        CategoryId = dto.CategoryId,
        BrandId = dto.BrandId,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    /// <summary>
    /// Update pe o entitate DEJA urmarita de EF. Nu cream obiect nou:
    /// change tracker-ul detecteaza modificarile si genereaza UPDATE-ul.
    /// </summary>
    public static void ApplyUpdate(this Product p, UpdateProductDto dto)
    {
        p.Name = dto.Name.Trim();
        p.Description = dto.Description.Trim();
        p.Price = dto.Price;
        p.StockQuantity = dto.StockQuantity;
        p.CategoryId = dto.CategoryId;
        p.BrandId = dto.BrandId;
        p.IsActive = dto.IsActive;
    }
}

