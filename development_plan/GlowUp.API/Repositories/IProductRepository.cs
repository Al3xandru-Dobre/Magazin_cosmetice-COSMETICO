using GlowUp.API.DTOs.Products;
using GlowUp.API.Models.Entities;

namespace GlowUp.API.Repositories;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>Produs cu Category, Brand, Ingredients si Reviews incarcate.</summary>
    Task<Product?> GetByIdWithDetailsAsync(int id);

    /// <summary>Lista paginata + filtrata. Returneaza si numarul total pentru metadate.</summary>
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query);

    Task<List<Ingredient>> GetIngredientsByIdsAsync(List<int> ids);
}
