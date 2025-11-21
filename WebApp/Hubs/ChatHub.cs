using Application.Chat;
using Application.Chat.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApp.Hubs;

[Authorize]
public class ChatHub(IChatService chatService, IGroupService groupService) : Hub
{
    // Глобальный чат
    public async Task SendMessage(string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var userName = Context.User?.Identity?.Name ?? "Anonymous";

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SaveMessageAsync(userId, userName, dto);
        var saved = result.Data;

        if (saved != null)
        {
            await Clients.All.SendAsync("ReceiveMessage", saved.UserName, saved.Message, saved.CreatedAt);
        }
    }

    // Group
    public async Task JoinGroup(string groupName)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var ensure = await groupService.EnsureGroupExistsAsync(userId, groupName);
        if (ensure.StatusCode >= 400)
        {
            throw new HubException(ensure.Message ?? "Cannot join group");
        }

        var joinResult = await groupService.JoinGroupAsync(userId, groupName);
        if (joinResult.StatusCode >= 400 || !joinResult.Data)
        {
            throw new HubException(joinResult.Message ?? "Cannot join group");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName)
            .SendAsync("SystemMessage", $"{Context.User?.Identity?.Name} joined group {groupName}");
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName)
            .SendAsync("SystemMessage", $"{Context.User?.Identity?.Name} left group {groupName}");
    }

    public async Task SendGroupMessage(string groupName, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var userName = Context.User?.Identity?.Name ?? "Anonymous";

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SaveGroupMessageAsync(userId, userName, groupName, dto);
        var saved = result.Data;

        if (saved != null)
        {
            await Clients.Group(groupName)
                .SendAsync("ReceiveGroupMessage", saved.GroupName, saved.UserName, saved.Message, saved.CreatedAt);
        }
    }

    // 1-1 приватный чат
    public async Task SendPrivateMessage(string toUserId, string message)
    {
        var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var fromUserName = Context.User?.Identity?.Name ?? "Anonymous";

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SavePrivateMessageAsync(fromUserId, fromUserName, toUserId, dto);
        var saved = result.Data;

        if (saved != null)
        {
            
            await Clients.User(toUserId)
                .SendAsync("ReceivePrivateMessage", fromUserId, saved.UserName, saved.Message, saved.CreatedAt);

            await Clients.User(fromUserId)
                .SendAsync("ReceivePrivateMessage", toUserId, saved.UserName, saved.Message, saved.CreatedAt);
        }
    }
}

