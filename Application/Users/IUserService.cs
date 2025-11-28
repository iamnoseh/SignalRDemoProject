using Application.Responses;
using Application.Users.Dto;

namespace Application.Users;

public interface IUserService
{
    Task<Response<List<UserDto>>> SearchUsersAsync(string query);
}
