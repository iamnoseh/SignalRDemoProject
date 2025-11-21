using Application.Chat;
using Application.Chat.Dto;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    // Глобальный чат
    [HttpPost("send")]
    public async Task<ActionResult<Response<ChatMessageDto>>> Send([FromBody] SendMessageDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var userName = User.Identity?.Name ?? "Anonymous";

        var result = await chatService.SaveMessageAsync(userId, userName, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<Response<List<ChatMessageDto>>>> History([FromQuery] int count = 50)
    {
        var result = await chatService.GetLastMessagesAsync(count);
        return StatusCode(result.StatusCode, result);
    }

    // Групповой чат
    [HttpPost("send-to-group")]
    public async Task<ActionResult<Response<ChatMessageDto>>> SendToGroup([FromBody] SendGroupMessageDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var userName = User.Identity?.Name ?? "Anonymous";

        var dto = new SendMessageDto { Message = request.Message };
        var result = await chatService.SaveGroupMessageAsync(userId, userName, request.GroupName, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history/group")]
    public async Task<ActionResult<Response<List<ChatMessageDto>>>> GroupHistory([FromQuery] string groupName, [FromQuery] int count = 50)
    {
        var result = await chatService.GetGroupHistoryAsync(groupName, count);
        return StatusCode(result.StatusCode, result);
    }

    // 1-1 чат
    [HttpPost("send-to-user")]
    public async Task<ActionResult<Response<ChatMessageDto>>> SendToUser([FromBody] SendPrivateMessageDto request)
    {
        var fromUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var fromUserName = User.Identity?.Name ?? "Anonymous";

        var dto = new SendMessageDto { Message = request.Message };
        var result = await chatService.SavePrivateMessageAsync(fromUserId, fromUserName, request.ToUserId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history/private")]
    public async Task<ActionResult<Response<List<ChatMessageDto>>>> PrivateHistory([FromQuery] string otherUserId, [FromQuery] int count = 50)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await chatService.GetPrivateHistoryAsync(currentUserId, otherUserId, count);
        return StatusCode(result.StatusCode, result);
    }
}
