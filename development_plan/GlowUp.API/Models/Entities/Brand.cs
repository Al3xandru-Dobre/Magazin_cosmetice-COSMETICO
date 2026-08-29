using System.ComponentModel.DataAnnotations;

namespace GlowUp.API.Models.Entities;

public class Brand
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Country { get; set; }

    public List<Product> Products { get; set; } = [];
}
