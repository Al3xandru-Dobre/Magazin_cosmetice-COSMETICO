using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Categories;

public record CategoryDto(int Id, string Name, string? Description, int ProductCount);

public class CreateCategoryDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto { }
