using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Magazin_cosmetice_COSMETICO.DTOs.Auth;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Magazin_cosmetice_COSMETICO.Services;

/// <summary>
/// Genereaza tokenul JWT cu claim-urile NameIdentifier, Email si Role.
/// Cheia trebuie sa aiba minimum 32 de caractere (256 biti pentru HMAC-SHA256).
/// </summary>
public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResponseDto> CreateTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FullName ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            _config.GetValue("Jwt:ExpiresInMinutes", 120));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email ?? string.Empty,
            user.FullName ?? string.Empty,
            roles.ToList(),
            expires);
    }
}
