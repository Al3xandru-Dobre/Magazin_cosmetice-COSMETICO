using Magazin_cosmetice_COSMETICO.DTOs.Reviews;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface IReviewService
{
    Task<List<ReviewDto>> GetByProductIdAsync(int productId);
    Task<ReviewDto> CreateAsync(string userId, CreateReviewDto dto);
    Task DeleteAsync(string userId, bool isAdmin, int reviewId);
}
