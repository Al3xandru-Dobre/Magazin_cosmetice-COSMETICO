using Magazin_cosmetice_COSMETICO.DTOs.Common;
using Magazin_cosmetice_COSMETICO.DTOs.Products;
using Magazin_cosmetice_COSMETICO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazin_cosmetice_COSMETICO.Controllers;

/// <summary>
/// Rol unic: traducere HTTP.
/// Primeste request -> cheama serviciul -> alege status code-ul.
/// Zero LINQ, zero try/catch, zero reguli de business.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    /// <summary>GET /api/products?page=1&amp;pageSize=12&amp;search=serum&amp;categoryId=2</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] ProductQueryParameters query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
    }

    /// <summary>GET /api/products/5</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        // Fara if (product == null) return NotFound().
        // Serviciul arunca NotFoundException, middleware-ul o traduce in 404.
        var product = await _service.GetByIdAsync(id);
        return Ok(product);
    }

    /// <summary>POST /api/products - ADMIN ONLY (cerinta: min. 2 endpoint-uri Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] CreateProductDto dto)
    {
        var created = await _service.CreateAsync(dto);

        // 201 + header Location cu URL-ul noii resurse (Lab 2).
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/products/5 - ADMIN ONLY</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>DELETE /api/products/5 - ADMIN ONLY</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent(); // 204: succes fara body
    }
}

