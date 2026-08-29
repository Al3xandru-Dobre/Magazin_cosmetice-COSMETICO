namespace Magazin_cosmetice_COSMETICO.Models.Entities;

/// <summary>
/// Many-to-Many CU payload intre Order si Product.
/// Nu poate fi o tabela de jonctiune implicita, pentru ca poarta date proprii
/// (Quantity, UnitPrice). De aceea este entitate explicita.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// SNAPSHOT al pretului la momentul comenzii.
    /// Daca am citi Product.Price la afisarea unei comenzi vechi,
    /// totalul s-ar rescrie retroactiv la fiecare modificare de pret.
    /// </summary>
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}

