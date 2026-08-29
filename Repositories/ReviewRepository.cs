using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<List<Review>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsForUserAsync(int productId, string userId)
    {
        return await _dbSet.AnyAsync(r => r.ProductId == productId && r.UserId == userId);
    }
}
