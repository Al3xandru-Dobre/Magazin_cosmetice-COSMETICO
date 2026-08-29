using System.ComponentModel.DataAnnotations;

namespace Magazin_cosmetice_COSMETICO.DTOs.Orders;

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ProductId invalid.")]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Cantitatea trebuie sa fie intre 1 si 100.")]
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required]
    [MinLength(10, ErrorMessage = "Adresa de livrare trebuie sa aiba minim 10 caractere.")]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Comanda trebuie sa contina cel putin un produs.")]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderDto(
    int Id,
    DateTime PlacedAt,
    string Status,
    decimal TotalAmount,
    string ShippingAddress,
    List<OrderItemDto> Items);

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
