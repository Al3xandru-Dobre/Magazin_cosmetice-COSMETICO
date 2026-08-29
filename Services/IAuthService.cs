using Magazin_cosmetice_COSMETICO.DTOs.Auth;

namespace Magazin_cosmetice_COSMETICO.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
