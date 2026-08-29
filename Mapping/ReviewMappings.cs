using Magazin_cosmetice_COSMETICO.DTOs.Reviews;
using Magazin_cosmetice_COSMETICO.Models.Entities;

namespace Magazin_cosmetice_COSMETICO.Mapping;

public static class ReviewMappings
{
    public static ReviewDto ToDto(this Review r) => new(
        r.Id,
        r.ProductId,
        r.Rating,
        r.Comment,
        r.CreatedAt,
        r.User?.FullName ?? r.User?.UserName ?? "-");
}
