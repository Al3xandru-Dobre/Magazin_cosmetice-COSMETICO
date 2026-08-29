using Magazin_cosmetice_COSMETICO.Models.Entities;

namespace Magazin_cosmetice_COSMETICO.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    /// <summary>Toate recenziile unui produs, cu utilizatorul incarcat, cele mai recente primele.</summary>
    Task<List<Review>> GetByProductIdAsync(int productId);

    /// <summary>True daca userul a lasat deja recenzie la produsul dat.</summary>
    Task<bool> ExistsForUserAsync(int productId, string userId);
}
