using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.DTOs.Categories;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categories;
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IRepository<Category> categories, AppDbContext context, ILogger<CategoryService> logger)
    {
        _categories = categories;
        _context = context;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        // ProductCount se traduce intr-un subquery COUNT in SQL, nu se incarca produsele in memorie.
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Products.Count))
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var dto = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Products.Count))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Categoria", id);

        return dto;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name == name))
            throw new BusinessRuleException($"Exista deja o categorie cu numele '{name}'.");

        var category = new Category { Name = name, Description = dto.Description?.Trim() };

        await _categories.AddAsync(category);
        await _categories.SaveChangesAsync();

        _logger.LogInformation("Categorie creata: {CategoryId} - {Name}", category.Id, category.Name);

        return new CategoryDto(category.Id, category.Name, category.Description, 0);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Categoria", id);

        var name = dto.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != id))
            throw new BusinessRuleException($"Exista deja o categorie cu numele '{name}'.");

        category.Name = name;
        category.Description = dto.Description?.Trim();

        _categories.Update(category);
        await _categories.SaveChangesAsync();
        _logger.LogInformation("Categorie actualizata: {CategoryId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Categoria", id);

        // Restrict: nu stergem o categorie care are produse, altfel am
        // distruge intreg catalogul din ea (cascade la nivel de FK).
        if (await _context.Products.AnyAsync(p => p.CategoryId == id))
            throw new BusinessRuleException("Nu se poate sterge o categorie care are produse. Mutati sau stergeti mai intai produsele.");

        _categories.Remove(category);
        await _categories.SaveChangesAsync();
        _logger.LogInformation("Categorie stearsa: {CategoryId}", id);
    }
}
