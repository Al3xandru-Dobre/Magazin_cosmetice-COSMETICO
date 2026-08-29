using Magazin_cosmetice_COSMETICO.DTOs.Orders;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(string userId, CreateOrderDto dto);
    Task<List<OrderDto>> GetMyOrdersAsync(string userId);
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<OrderDto> UpdateStatusAsync(int id, string status);
}
