using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Brands;

public record BrandDto(int Id, string Name, string? Country, int ProductCount);

public class CreateBrandDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Country { get; set; }
}

public class UpdateBrandDto : CreateBrandDto { }
