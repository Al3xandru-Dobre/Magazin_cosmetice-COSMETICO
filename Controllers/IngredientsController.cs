using Magazin_cosmetice_COSMETICO.DTOs.Products;
using Magazin_cosmetice_COSMETICO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazin_cosmetice_COSMETICO.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IngredientsController : ControllerBase
{
    private readonly IIngredientService _service;

    public IngredientsController(IIngredientService service) => _service = service;

    /// <summary>GET /api/ingredients — folosit de formularul de admin pentru produse.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<IngredientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IngredientDto>>> GetAll()
        => Ok(await _service.GetAllAsync());
}
