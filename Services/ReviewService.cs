using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.DTOs.Reviews;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Mapping;
using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository reviews, AppDbContext context, ILogger<ReviewService> logger)
    {
        _reviews = reviews;
        _context = context;
        _logger = logger;
    }

    public async Task<List<ReviewDto>> GetByProductIdAsync(int productId)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == productId))
            throw new NotFoundException("Produsul", productId);

        var reviews = await _reviews.GetByProductIdAsync(productId);
        return reviews.Select(r => r.ToDto()).ToList();
    }

    public async Task<ReviewDto> CreateAsync(string userId, CreateReviewDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive)
            ?? throw new NotFoundException("Produsul", dto.ProductId);

        // Indexul unic (ProductId, UserId) blocheaza duplicatele si la nivel
        // de baza de date; verificarea aici da un mesaj frumos in loc de eroare SQL.
        if (await _reviews.ExistsForUserAsync(dto.ProductId, userId))
            throw new BusinessRuleException("Ai lasat deja o recenzie pentru acest produs.");

        var user = await _context.Users.FindAsync(userId)
            ?? throw new ForbiddenException("Utilizator invalid.");

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _reviews.AddAsync(review);
        await _reviews.SaveChangesAsync();

        _logger.LogInformation(
            "Recenzie creata: produs {ProductId}, user {UserId}, rating {Rating}/5",
            review.ProductId, userId, review.Rating);

        return new ReviewDto(
            review.Id,
            review.ProductId,
            review.Rating,
            review.Comment,
            review.CreatedAt,
            user.FullName ?? user.UserName ?? "-");
    }

    public async Task DeleteAsync(string userId, bool isAdmin, int reviewId)
    {
        var review = await _reviews.GetByIdAsync(reviewId)
            ?? throw new NotFoundException("Recenzia", reviewId);

        // Un user isi poate sterge doar propria recenzie; adminul pe toate.
        if (review.UserId != userId && !isAdmin)
            throw new ForbiddenException("Nu va apartine aceasta recenzie.");

        _reviews.Remove(review);
        await _reviews.SaveChangesAsync();

        _logger.LogInformation("Recenzie stearsa: {ReviewId} (de catre {Actor}, admin={IsAdmin})",
            reviewId, userId, isAdmin);
    }
}
