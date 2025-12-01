using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Helpers;

public static class JwtTokenHelper
{
    public static string GenerateJwtToken(AppUser user, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var minutes) ? minutes : 60;

        var authClaims = new List<Claim>
        {
            // Standard claims required by ASP.NET Core Identity
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            
            // Custom claims for frontend
            new("id", user.Id),
            new("userName", user.UserName ?? string.Empty),
            new("nickname", user.Nickname ?? string.Empty),
            new("fullName", user.FullName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("email", user.Email ?? string.Empty),
            new("profilePictureUrl", user.ProfilePictureUrl ?? string.Empty)
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
