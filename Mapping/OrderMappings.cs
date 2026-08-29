using Magazin_cosmetice_COSMETICO.DTOs.Orders;
using Magazin_cosmetice_COSMETICO.Models.Entities;

namespace Magazin_cosmetice_COSMETICO.Mapping;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order o) => new(
        o.Id,
        o.PlacedAt,
        o.Status.ToString(),
        o.TotalAmount,
        o.ShippingAddress,
        o.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product?.Name ?? "-",
            i.Quantity,
            i.UnitPrice,
            i.LineTotal)).ToList());
}
