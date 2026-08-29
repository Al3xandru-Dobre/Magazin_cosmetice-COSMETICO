using GlowUp.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.API.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public virtual async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<bool> ExistsAsync(int id) => await GetByIdAsync(id) is not null;

    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Remove(T entity) => _dbSet.Remove(entity);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
