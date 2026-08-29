using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.DTOs.Brands;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Services;

public class BrandService : IBrandService
{
    private readonly IRepository<Brand> _brands;
    private readonly AppDbContext _context;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IRepository<Brand> brands, AppDbContext context, ILogger<BrandService> logger)
    {
        _brands = brands;
        _context = context;
        _logger = logger;
    }

    public async Task<List<BrandDto>> GetAllAsync()
    {
        return await _context.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(b.Id, b.Name, b.Country, b.Products.Count))
            .ToListAsync();
    }

    public async Task<BrandDto> GetByIdAsync(int id)
    {
        var dto = await _context.Brands
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BrandDto(b.Id, b.Name, b.Country, b.Products.Count))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Brand-ul", id);

        return dto;
    }

    public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
    {
        var name = dto.Name.Trim();
        if (await _context.Brands.AnyAsync(b => b.Name == name))
            throw new BusinessRuleException($"Exista deja un brand cu numele '{name}'.");

        var brand = new Brand { Name = name, Country = dto.Country?.Trim() };

        await _brands.AddAsync(brand);
        await _brands.SaveChangesAsync();

        _logger.LogInformation("Brand creat: {BrandId} - {Name}", brand.Id, brand.Name);

        return new BrandDto(brand.Id, brand.Name, brand.Country, 0);
    }

    public async Task<BrandDto> UpdateAsync(int id, UpdateBrandDto dto)
    {
        var brand = await _brands.GetByIdAsync(id)
            ?? throw new NotFoundException("Brand-ul", id);

        var name = dto.Name.Trim();
        if (await _context.Brands.AnyAsync(b => b.Name == name && b.Id != id))
            throw new BusinessRuleException($"Exista deja un brand cu numele '{name}'.");

        brand.Name = name;
        brand.Country = dto.Country?.Trim();

        _brands.Update(brand);
        await _brands.SaveChangesAsync();
        _logger.LogInformation("Brand actualizat: {BrandId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await _brands.GetByIdAsync(id)
            ?? throw new NotFoundException("Brand-ul", id);

        if (await _context.Products.AnyAsync(p => p.BrandId == id))
            throw new BusinessRuleException("Nu se poate sterge un brand care are produse. Mutati sau stergeti mai intai produsele.");

        _brands.Remove(brand);
        await _brands.SaveChangesAsync();
        _logger.LogInformation("Brand sters: {BrandId}", id);
    }
}
