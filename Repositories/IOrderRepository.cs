using Magazin_cosmetice_COSMETICO.Models.Entities;

namespace Magazin_cosmetice_COSMETICO.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Comanda cu liniile si produsele incarcate (tracking activ, pentru update).</summary>
    Task<Order?> GetByIdWithItemsAsync(int id);

    /// <summary>Comenzile unui utilizator, cele mai recente primele.</summary>
    Task<List<Order>> GetByUserIdAsync(string userId);

    /// <summary>Toate comenzile (admin), cele mai recente primele.</summary>
    Task<List<Order>> GetAllWithItemsAsync();
}
