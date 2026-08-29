using Magazin_cosmetice_COSMETICO.DTOs.Auth;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Magazin_cosmetice_COSMETICO.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
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

        _logger.LogInformation("Cont nou inregistrat: {Email} (rol User)", user.Email);

        return await _tokenService.CreateTokenAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        // Mesaj generic intentionat: specificarea carei campuri e gresit
        // ar ajuta un atacator sa confirme ce adrese de email exista.
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Autentificare esuata pentru {Email}", dto.Email.Trim());
            throw new BusinessRuleException("Email sau parola incorecta.");
        }

        _logger.LogInformation("Autentificare reusita: {Email}", user.Email);

        return await _tokenService.CreateTokenAsync(user);
    }
}
