using Application.Chat;
using Application.Chat.Dto;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

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
        var userId = User.GetUserId();
        var userName = User.GetUserName();

        var result = await chatService.SaveMessageAsync(userId, userName, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<Response<PaginatedResponse<ChatMessageDto>>>> History([FromQuery] PaginationRequest pagination)
    {
        var result = await chatService.GetLastMessagesAsync(pagination);
        return StatusCode(result.StatusCode, result);
    }

    // Групповой чат
    [HttpPost("send-to-group")]
    public async Task<ActionResult<Response<ChatMessageDto>>> SendToGroup([FromBody] SendGroupMessageDto request)
    {
        var userId = User.GetUserId();
        var userName = User.GetUserName();

        var dto = new SendMessageDto { Message = request.Message };
        var result = await chatService.SaveGroupMessageAsync(userId, userName, request.GroupName, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history/group")]
    public async Task<ActionResult<Response<PaginatedResponse<ChatMessageDto>>>> GroupHistory([FromQuery] string groupName, [FromQuery] PaginationRequest pagination)
    {
        var result = await chatService.GetGroupHistoryAsync(groupName, pagination);
        return StatusCode(result.StatusCode, result);
    }

    // 1-1 чат
    [HttpPost("send-to-user")]
    public async Task<ActionResult<Response<ChatMessageDto>>> SendToUser([FromBody] SendPrivateMessageDto request)
    {
        var fromUserId = User.GetUserId();
        var fromUserName = User.GetUserName();

        var dto = new SendMessageDto { Message = request.Message };
        var result = await chatService.SavePrivateMessageAsync(fromUserId, fromUserName, request.ToUserId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history/private")]
    public async Task<ActionResult<Response<PaginatedResponse<ChatMessageDto>>>> PrivateHistory([FromQuery] string otherUserId, [FromQuery] PaginationRequest pagination)
    {
        var currentUserId = User.GetUserId();
        var result = await chatService.GetPrivateHistoryAsync(currentUserId, otherUserId, pagination);
        return StatusCode(result.StatusCode, result);
    }

    // Edit/Delete operations
    [HttpPut("edit/{messageId}")]
    public async Task<ActionResult<Response<ChatMessageDto>>> EditMessage(Guid messageId, [FromBody] SendMessageDto request)
    {
        var userId = User.GetUserId();
        var result = await chatService.EditMessageAsync(messageId, userId, request.Message);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult<Response<bool>>> DeleteMessage(Guid messageId)
    {
        var userId = User.GetUserId();
        var result = await chatService.DeleteMessageAsync(messageId, userId);
        return StatusCode(result.StatusCode, result);
    }
}
