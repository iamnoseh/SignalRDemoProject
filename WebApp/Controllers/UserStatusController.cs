using Application.Chat;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserStatusController(IUserStatusService userStatusService) : ControllerBase
{
    [HttpGet("online")]
    public ActionResult<Response<List<string>>> GetOnlineUsers()
    {
        var onlineUsers = userStatusService.GetOnlineUsers();
        return Ok(new Response<List<string>>(onlineUsers));
    }

    [HttpGet("{userId}/status")]
    public ActionResult<Response<bool>> GetUserStatus(string userId)
    {
        var isOnline = userStatusService.IsOnline(userId);
        return Ok(new Response<bool>(isOnline));
    }
}
