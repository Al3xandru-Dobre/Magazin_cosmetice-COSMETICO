using GlowUp.API.Models.Identity;

namespace GlowUp.API.Models.Entities;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }

    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Total calculat pe server la plasarea comenzii, NU trimis de client.
    /// Daca l-am accepta din request, un client ar putea comanda cu total = 0.
    /// </summary>
    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    // One-to-Many: User are multe Order. Cheia Identity este string (GUID), nu int.
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
