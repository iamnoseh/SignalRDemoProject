using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Application.Auth;
using Application.Auth.Dto;
using Domain.Entities;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

public class AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
    : IAuthService
{
    public async Task<Response<string>> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            return new Response<string>(HttpStatusCode.BadRequest, message);
        }
        return new Response<string>("User registration succeeded");
    }

    public async Task<Response<AuthResultDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return new Response<AuthResultDto>(HttpStatusCode.Unauthorized, "Username or password is incorrect");
        }

        var validPassword = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
        {
            return new Response<AuthResultDto>(HttpStatusCode.Unauthorized, "Username or password is incorrect");
        }

        var token = GenerateJwtToken(user);

        var authResult = new AuthResultDto
        {
            UserName = user.UserName ?? string.Empty,
            Token = token
        };

        return new Response<AuthResultDto>(authResult);
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var minutes) ? minutes : 60;

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
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


