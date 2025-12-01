using Application.Responses;
using Application.Users;
using Application.Users.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<Response<List<UserDto>>>> SearchUsers([FromQuery] string query)
    {
        var result = await userService.SearchUsersAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<UserDto>>>> GetAllUsers()
    {
        var result = await userService.GetAllUsersAsync();
        return StatusCode(result.StatusCode, result);
    }
}
