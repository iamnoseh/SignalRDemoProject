using Application.Responses;
using Application.Users;
using Application.Users.Dto;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Users;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<Response<List<UserDto>>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new Response<List<UserDto>>(new List<UserDto>()) { Message = "Query cannot be empty." };
        }

        var users = await context.Users
            .Where(u => u.UserName.ToLower().Contains(query.ToLower()) || 
                        (u.Nickname != null && u.Nickname.ToLower().Contains(query.ToLower())) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(query.ToLower())))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nickname = u.Nickname,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .ToListAsync();

        return new Response<List<UserDto>>(users) { Message = "Users retrieved successfully." };
    }

    public async Task<Response<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nickname = u.Nickname,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .ToListAsync();

        return new Response<List<UserDto>>(users) { Message = "All users retrieved successfully." };
    }
}
