using Magazin_cosmetice_COSMETICO.DTOs.Products;
using Magazin_cosmetice_COSMETICO.Mapping;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Services;

public class IngredientService : IIngredientService
{
    private readonly IRepository<Ingredient> _ingredients;

    public IngredientService(IRepository<Ingredient> ingredients)
    {
        _ingredients = ingredients;
    }

    public async Task<List<IngredientDto>> GetAllAsync()
    {
        var ingredients = await _ingredients.GetAllAsync();
        return ingredients
            .OrderBy(i => i.Name)
            .Select(i => i.ToDto())
            .ToList();
    }
}
