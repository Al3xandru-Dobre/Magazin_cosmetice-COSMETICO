using GlowUp.API.Data;
using GlowUp.API.DTOs.Products;
using GlowUp.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.API.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Ingredients)
            .Include(p => p.Reviews)
            // AsSplitQuery: cu 4 Include-uri, un singur JOIN ar produce
            // explozie carteziana (nr_ingrediente x nr_recenzii randuri duplicate).
            // EF trimite in schimb cateva query-uri separate si le compune in memorie.
            .AsSplitQuery()
            // Read-only: fara change tracking, EF nu mai retine snapshot-uri
            // pentru comparatie la SaveChanges. Query mai rapid, memorie mai putina.
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters q)
    {
        // IQueryable, NU IEnumerable. Diferenta e esentiala:
        // pe IQueryable, filtrele se traduc in WHERE-uri SQL si se executa
        // in baza de date. Pe IEnumerable, s-ar incarca TOATE produsele in
        // memorie si abia apoi s-ar filtra in C#. (Lab 3 - deferred execution)
        IQueryable<Product> query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Description.Contains(term));
        }

        if (q.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == q.CategoryId.Value);

        if (q.BrandId.HasValue)
            query = query.Where(p => p.BrandId == q.BrandId.Value);

        if (q.MinPrice.HasValue)
            query = query.Where(p => p.Price >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= q.MaxPrice.Value);

        // COUNT(*) se executa DUPA filtre, dar INAINTE de Skip/Take.
        // Altfel numarul total de pagini ar fi gresit.
        var totalCount = await query.CountAsync();

        query = q.SortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest"     => query.OrderByDescending(p => p.CreatedAt),
            _            => query.OrderBy(p => p.Name)
        };

        // Skip/Take se traduc in OFFSET/FETCH NEXT in SQL Server (Lab 5).
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Ingredient>> GetIngredientsByIdsAsync(List<int> ids)
    {
        // Fara AsNoTracking aici: aceste entitati vor fi atasate unui Product
        // si trebuie sa fie urmarite, ca EF sa scrie randurile in ProductIngredients.
        return await _context.Ingredients
            .Where(i => ids.Contains(i.Id))
            .ToListAsync();
    }
}
