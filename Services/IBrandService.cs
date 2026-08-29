using Magazin_cosmetice_COSMETICO.DTOs.Brands;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface IBrandService
{
    Task<List<BrandDto>> GetAllAsync();
    Task<BrandDto> GetByIdAsync(int id);
    Task<BrandDto> CreateAsync(CreateBrandDto dto);
    Task<BrandDto> UpdateAsync(int id, UpdateBrandDto dto);
    Task DeleteAsync(int id);
}
