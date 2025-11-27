using System.Net;
using System.Security.Claims;
using Application.Auth;
using Application.Auth.Dto;
using Application.Responses;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Helpers;
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

    public async Task<Response<bool>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "User not found");
        }

        user.Nickname = dto.Nickname;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            return new Response<bool>(HttpStatusCode.BadRequest, message);
        }

        return new Response<bool>(true);
    }

    public async Task<Response<string>> UpdateProfilePictureAsync(string userId, string pictureUrl)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new Response<string>(HttpStatusCode.NotFound, "User not found");
        }

        user.ProfilePictureUrl = pictureUrl;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            return new Response<string>(HttpStatusCode.BadRequest, message);
        }

        return new Response<string>(pictureUrl);
    }

}


