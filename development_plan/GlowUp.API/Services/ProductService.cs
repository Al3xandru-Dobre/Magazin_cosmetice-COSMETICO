using GlowUp.API.Data;
using GlowUp.API.DTOs.Common;
using GlowUp.API.DTOs.Products;
using GlowUp.API.Exceptions;
using GlowUp.API.Mapping;
using GlowUp.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.API.Services;

/// <summary>
/// Aici traieste logica de business. Regula: serviciul NU stie nimic
/// despre HTTP (fara IActionResult, fara status codes), iar controllerul
/// NU stie nimic despre EF Core. Fiecare strat vorbeste doar cu vecinul.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly AppDbContext _context; // pentru validari FK simple
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository products,
        AppDbContext context,
        ILogger<ProductService> logger)
    {
        _products = products;
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters query)
    {
        var (items, total) = await _products.GetPagedAsync(query);

        return new PagedResult<ProductDto>
        {
            Items = items.ToDtoList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }

    public async Task<ProductDetailDto> GetByIdAsync(int id)
    {
        var product = await _products.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Produsul", id);

        return product.ToDetailDto();
    }

    public async Task<ProductDetailDto> CreateAsync(CreateProductDto dto)
    {
        // Validarile de FORMAT (lungime, interval) au fost deja facute
        // automat de [ApiController] pe baza DataAnnotations din DTO.
        // Aici raman doar validarile care au nevoie de baza de date.
        await EnsureRelationsExistAsync(dto.CategoryId, dto.BrandId);

        if (await _context.Products.AnyAsync(p => p.Name == dto.Name.Trim()))
            throw new BusinessRuleException($"Exista deja un produs cu numele '{dto.Name}'.");

        var product = dto.ToEntity();

        if (dto.IngredientIds.Count > 0)
        {
            var ingredients = await _products.GetIngredientsByIdsAsync(dto.IngredientIds);

            // Verificam ca TOATE id-urile trimise exista. Daca am sari peste,
            // un id gresit ar fi ignorat silentios si clientul ar crede ca a mers.
            if (ingredients.Count != dto.IngredientIds.Distinct().Count())
                throw new BusinessRuleException("Unul sau mai multe ingrediente nu exista.");

            product.Ingredients = ingredients;
        }

        await _products.AddAsync(product);
        await _products.SaveChangesAsync();

        _logger.LogInformation("Produs creat: {ProductId} - {Name}", product.Id, product.Name);

        // Recitim cu toate relatiile, ca DTO-ul returnat sa fie complet
        // (CategoryName, BrandName nu sunt populate pe entitatea abia inserata).
        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductDetailDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        // Include(Ingredients) + tracking activ: EF are nevoie sa vada
        // colectia veche ca sa calculeze ce randuri sterge/adauga in jonctiune.
        var product = await _context.Products
            .Include(p => p.Ingredients)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Produsul", id);

        await EnsureRelationsExistAsync(dto.CategoryId, dto.BrandId);

        product.ApplyUpdate(dto);

        var ingredients = await _products.GetIngredientsByIdsAsync(dto.IngredientIds);
        if (ingredients.Count != dto.IngredientIds.Distinct().Count())
            throw new BusinessRuleException("Unul sau mai multe ingrediente nu exista.");

        // Inlocuim colectia; change tracker-ul genereaza DELETE + INSERT
        // doar pentru diferente, nu pentru toate randurile.
        product.Ingredients = ingredients;

        await _products.SaveChangesAsync();
        _logger.LogInformation("Produs actualizat: {ProductId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new NotFoundException("Produsul", id);

        // SOFT DELETE. Un produs care apare in comenzi vechi nu poate
        // fi sters fizic fara sa distruga istoricul. Il scoatem din catalog.
        var isInOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);

        if (isInOrders)
        {
            product.IsActive = false;
            _products.Update(product);
            _logger.LogInformation("Produs dezactivat (apare in comenzi): {ProductId}", id);
        }
        else
        {
            _products.Remove(product);
            _logger.LogInformation("Produs sters definitiv: {ProductId}", id);
        }

        await _products.SaveChangesAsync();
    }

    private async Task EnsureRelationsExistAsync(int categoryId, int brandId)
    {
        if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
            throw new NotFoundException("Categoria", categoryId);

        if (!await _context.Brands.AnyAsync(b => b.Id == brandId))
            throw new NotFoundException("Brand-ul", brandId);
    }
}
