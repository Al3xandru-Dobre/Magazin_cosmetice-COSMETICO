using Magazin_cosmetice_COSMETICO.DTOs.Common;
using Magazin_cosmetice_COSMETICO.DTOs.Products;

namespace Magazin_cosmetice_COSMETICO.Services;

/// <summary>
/// Contractul stratului de business.
/// Observa ca semnaturile lucreaza EXCLUSIV cu DTO-uri: entitatile
/// nu ies niciodata din spatele acestei bariere.
/// </summary>
public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters query);
    Task<ProductDetailDto> GetByIdAsync(int id);
    Task<ProductDetailDto> CreateAsync(CreateProductDto dto);
    Task<ProductDetailDto> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}

