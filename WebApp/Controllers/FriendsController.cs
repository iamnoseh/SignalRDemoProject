using Application.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpPost("request/{receiverId}")]
    public async Task<IActionResult> SendFriendRequest(string receiverId)
    {
        var senderId = User.GetUserId();
        var result = await _friendService.SendFriendRequestAsync(senderId, receiverId);

        if (!result)
        {
            return BadRequest("Could not send friend request. Either already friends, request pending, or invalid user.");
        }

        return Ok("Friend request sent.");
    }

    [HttpPost("accept/{requestId}")]
    public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
    {
        var userId = User.GetUserId();
        var result = await _friendService.AcceptFriendRequestAsync(userId, requestId);

        if (!result)
        {
            return BadRequest("Could not accept friend request.");
        }

        return Ok("Friend request accepted.");
    }

    [HttpPost("reject/{requestId}")]
    public async Task<IActionResult> RejectFriendRequest(Guid requestId)
    {
        var userId = User.GetUserId();
        var result = await _friendService.RejectFriendRequestAsync(userId, requestId);

        if (!result)
        {
            return BadRequest("Could not reject friend request.");
        }

        return Ok("Friend request rejected.");
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.GetUserId();
        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = User.GetUserId();
        var requests = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }
}
