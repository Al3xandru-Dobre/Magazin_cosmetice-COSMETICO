using Magazin_cosmetice_COSMETICO.DTOs.Products;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface IIngredientService
{
    Task<List<IngredientDto>> GetAllAsync();
}
