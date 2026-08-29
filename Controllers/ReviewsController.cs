using System.Security.Claims;
using Magazin_cosmetice_COSMETICO.DTOs.Reviews;
using Magazin_cosmetice_COSMETICO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazin_cosmetice_COSMETICO.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service) => _service = service;

    /// <summary>GET /api/products/5/reviews — ruta absoluta, apartine tot acestui controller.</summary>
    [HttpGet("/api/products/{productId:int}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReviewDto>>> GetByProduct(int productId)
    {
        // Serviciul arunca NotFoundException daca produsul nu exista,
        // altfel am returna 200 cu lista vida pe o resursa inexistenta.
        return Ok(await _service.GetByProductIdAsync(productId));
    }

    /// <summary>POST /api/reviews — orice utilizator autentificat.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var created = await _service.CreateAsync(userId, dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>DELETE /api/reviews/5 — proprietarul recenziei sau Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(userId, User.IsInRole("Admin"), id);
        return NoContent();
    }
}
