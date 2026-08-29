namespace Magazin_cosmetice_COSMETICO.Repositories;

/// <summary>
/// Contract generic de acces la date. Orice entitate primeste gratuit
/// operatiile de baza; repository-urile specifice adauga doar ce e particular.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>
    /// Commit-ul este separat de Add/Update/Remove intentionat.
    /// Asa serviciul poate face mai multe modificari si le salveaza
    /// intr-o singura tranzactie (ex: creare comanda + scadere stoc).
    /// </summary>
    Task<int> SaveChangesAsync();
}

