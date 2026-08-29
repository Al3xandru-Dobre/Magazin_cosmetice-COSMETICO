using Magazin_cosmetice_COSMETICO.DTOs.Auth;
using Magazin_cosmetice_COSMETICO.Models.Identity;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface ITokenService
{
    Task<AuthResponseDto> CreateTokenAsync(ApplicationUser user);
}
