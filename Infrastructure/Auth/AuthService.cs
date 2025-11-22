using System.Net;
using System.Security.Claims;
using Application.Auth;
using Application.Auth.Dto;
using Domain.Entities;
using Infrastructure.Helpers;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

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

        var token = JwtTokenHelper.GenerateJwtToken(user, configuration);

        var authResult = new AuthResultDto
        {
            UserName = user.UserName ?? string.Empty,
            Token = token
        };

        return new Response<AuthResultDto>(authResult);
    }

}


