using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Products;

/// <summary>
/// Forma "slim" pentru lista de catalog. Nu contine ingrediente sau recenzii:
/// pentru 12 produse pe pagina, acele date ar tripla payload-ul degeaba.
/// </summary>
public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity,
    string? ImagePath,
    string CategoryName,
    string BrandName,
    double AverageRating,
    int ReviewCount);

/// <summary>Forma completa, pentru pagina de detaliu.</summary>
public record ProductDetailDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string? ImagePath,
    DateTime CreatedAt,
    int CategoryId,
    string CategoryName,
    int BrandId,
    string BrandName,
    List<IngredientDto> Ingredients,
    double AverageRating,
    int ReviewCount);

public record IngredientDto(int Id, string Name, bool IsAllergen);

/// <summary>
/// DTO de intrare. Observa: NU are Id, NU are CreatedAt, NU are AverageRating.
/// Asta face over-posting-ul imposibil prin constructie - un client
/// nu poate injecta valori in campuri pe care serverul le controleaza.
/// Validarile [Required]/[Range] sunt aplicate automat de [ApiController] (Lab 3).
/// </summary>
public class CreateProductDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage = "Pretul trebuie sa fie intre 0.01 si 100000.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Categoria este obligatorie.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Brand-ul este obligatoriu.")]
    public int BrandId { get; set; }

    public List<int> IngredientIds { get; set; } = [];
}

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Parametrii de filtrare/paginare din query string.
/// Grupati intr-o clasa in loc de 6 parametri separati in semnatura metodei:
/// model binding-ul ii populeaza automat din ?page=1&amp;search=serum.
/// </summary>
public class ProductQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 12;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        // Plafon server-side: fara el, un client poate cere ?pageSize=1000000
        // si transforma endpoint-ul intr-un vector de DoS.
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>name | price_asc | price_desc | newest</summary>
    public string? SortBy { get; set; }
}

