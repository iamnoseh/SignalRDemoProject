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
        // Check if nickname exists
        var existingUser = await userManager.FindByNameAsync(dto.Nickname);
        if (existingUser != null)
        {
            return new Response<string>(HttpStatusCode.BadRequest, "Nickname is already taken.");
        }

        // Check if phone number exists (if needed, though Identity doesn't enforce unique phone by default unless configured)
        // We can do a manual check
        if (userManager.Users.Any(u => u.PhoneNumber == dto.PhoneNumber))
        {
             return new Response<string>(HttpStatusCode.BadRequest, "Phone number is already registered.");
        }

        var user = new AppUser
        {
            UserName = dto.Nickname, // Use Nickname as UserName for Identity
            Nickname = dto.Nickname,
            PhoneNumber = dto.PhoneNumber,
            Email = $"{dto.Nickname}@placeholder.com" // Identity might require Email, set a placeholder or make it optional in Identity config. 
                                                      // For now, let's set a placeholder to avoid validation errors if Email is required.
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
        // Try to find by UserName (Nickname)
        var user = await userManager.FindByNameAsync(dto.Identifier);
        
        // If not found, try to find by PhoneNumber
        if (user == null)
        {
            user = userManager.Users.FirstOrDefault(u => u.PhoneNumber == dto.Identifier);
        }

        if (user == null)
        {
            return new Response<AuthResultDto>(HttpStatusCode.Unauthorized, "Invalid credentials");
        }

        var validPassword = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
        {
            return new Response<AuthResultDto>(HttpStatusCode.Unauthorized, "Invalid credentials");
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

        if (!string.IsNullOrEmpty(dto.Nickname)) user.Nickname = dto.Nickname;
        if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Gender)) user.Gender = dto.Gender;
        if (!string.IsNullOrEmpty(dto.Address)) user.Address = dto.Address;
        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;

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


