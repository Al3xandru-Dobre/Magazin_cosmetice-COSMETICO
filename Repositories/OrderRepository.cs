using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetAllWithItemsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();
    }
}
