using Magazin_cosmetice_COSMETICO.DTOs.Auth;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Magazin_cosmetice_COSMETICO.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (existing is not null)
            throw new BusinessRuleException($"Exista deja un cont cu emailul '{dto.Email}'.");

        var user = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FullName = dto.FullName.Trim(),
            RegisteredAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return await _tokenService.CreateTokenAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        // Mesaj generic intentionat: specificarea carei campuri e gresit
        // ar ajuta un atacator sa confirme ce adrese de email exista.
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new BusinessRuleException("Email sau parola incorecta.");

        return await _tokenService.CreateTokenAsync(user);
    }
}
