using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.DTOs.Orders;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Mapping;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Services;

/// <summary>
/// Singura logica netriviala din proiect. Regulile:
/// 1. valideaza ca produsele exista si sunt active
/// 2. verifica stocul -> BusinessRuleException
/// 3. copiaza pretul curent in OrderItem.UnitPrice (snapshot)
/// 4. calculeaza TotalAmount PE SERVER, ignorand orice valoare din request
/// 5. scade stocul
/// 6. un singur SaveChangesAsync la final -> totul intr-o singura tranzactie
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orders, AppDbContext context, ILogger<OrderService> logger)
    {
        _orders = orders;
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto> CreateAsync(string userId, CreateOrderDto dto)
    {
        // Un singur query aduce toate produsele referite (fara N+1).
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var id in productIds)
            if (!products.ContainsKey(id))
                throw new NotFoundException("Produsul", id);

        foreach (var product in products.Values)
            if (!product.IsActive)
                throw new BusinessRuleException($"Produsul '{product.Name}' nu mai este disponibil.");

        // Cantitatile pentru acelasi produs se cumuleaza (ex: 2x + 3x = 5x).
        var quantities = dto.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var (productId, qty) in quantities)
        {
            var product = products[productId];
            if (qty > product.StockQuantity)
                throw new BusinessRuleException(
                    $"Stoc insuficient pentru '{product.Name}'. Disponibil: {product.StockQuantity}, cerut: {qty}.");
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = dto.ShippingAddress.Trim(),
            Status = OrderStatus.Pending,
            PlacedAt = DateTime.UtcNow
        };

        foreach (var (productId, qty) in quantities)
        {
            var product = products[productId];

            // SNAPSHOT: pretul de la momentul comenzii, nu cel curent.
            order.Items.Add(new OrderItem
            {
                ProductId = productId,
                Quantity = qty,
                UnitPrice = product.Price
            });

            product.StockQuantity -= qty;
        }

        // Totalul se calculeaza pe server; orice total trimis de client e ignorat.
        order.TotalAmount = order.Items.Sum(i => i.LineTotal);

        await _orders.AddAsync(order);
        // Un singur SaveChanges: comanda + liniile + scaderile de stoc pleaca
        // impreuna, ca o singura tranzactie. O eroare intre doua salvari separate
        // ar lasa baza intr-o stare inconsistenta (comanda da, stoc nescăzut).
        await _orders.SaveChangesAsync();

        _logger.LogInformation(
            "Comanda {OrderId} plasata de userul {UserId}. Total: {Total} lei, {Lines} linii.",
            order.Id, userId, order.TotalAmount, order.Items.Count);

        // Recitim cu Include-uri ca DTO-ul returnat sa contina si numele produselor.
        var created = await _orders.GetByIdWithItemsAsync(order.Id);
        return created!.ToDto();
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(string userId)
    {
        var orders = await _orders.GetByUserIdAsync(userId);
        return orders.Select(o => o.ToDto()).ToList();
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orders.GetAllWithItemsAsync();
        return orders.Select(o => o.ToDto()).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var order = await _orders.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException("Comanda", id);

        if (order.UserId != userId && !isAdmin)
            throw new ForbiddenException("Nu puteti vedea comanda altui utilizator.");

        return order.ToDto();
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            throw new BusinessRuleException(
                $"Status invalid '{status}'. Valori acceptate: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");

        var order = await _orders.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException("Comanda", id);

        if (order.Status == parsed)
            throw new BusinessRuleException($"Comanda are deja statusul '{parsed}'.");

        order.Status = parsed;
        await _orders.SaveChangesAsync();

        _logger.LogInformation("Status comanda {OrderId} schimbat in {Status}", id, parsed);

        return order.ToDto();
    }
}
