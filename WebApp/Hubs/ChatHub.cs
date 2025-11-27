using Application.Chat;
using Application.Chat.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebApp.Extensions;

namespace WebApp.Hubs;

[Authorize]
public class ChatHub(
    IChatService chatService, 
    IGroupService groupService,
    IUserStatusService userStatusService) : Hub
{
    // Connection lifecycle
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

        userStatusService.SetOnline(userId, Context.ConnectionId);

        // Broadcast user online status
        await Clients.All.SendAsync(SignalREvents.UserOnline, userId, userName);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

        userStatusService.SetOffline(userId, Context.ConnectionId);

        // Only broadcast offline if user has no more connections
        if (!userStatusService.IsOnline(userId))
        {
            await Clients.All.SendAsync(SignalREvents.UserOffline, userId, userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Глобальный чат
    public async Task SendMessage(string message)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SaveMessageAsync(userId, userName, dto);
        var saved = result.Data;

        if (saved != null)
        {
            await Clients.All.SendAsync(SignalREvents.ReceiveMessage, saved.UserName, saved.Message, saved.CreatedAt);
        }
    }

    // Group
    public async Task JoinGroup(string groupName)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

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
            .SendAsync(SignalREvents.SystemMessage, $"{userName} joined group {groupName}");
    }

    public async Task LeaveGroup(string groupName)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

        // FIX: Ҳам аз SignalR group ва ҳам аз database баромадан
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        var leaveResult = await groupService.LeaveGroupAsync(userId, groupName);
        if (leaveResult.StatusCode < 400)
        {
            await Clients.Group(groupName)
                .SendAsync(SignalREvents.SystemMessage, $"{userName} left group {groupName}");
        }
    }

    public async Task SendGroupMessage(string groupName, string message)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User.GetUserName();

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SaveGroupMessageAsync(userId, userName, groupName, dto);
        var saved = result.Data;

        if (saved != null)
        {
            await Clients.Group(groupName)
                .SendAsync(SignalREvents.ReceiveGroupMessage, saved.GroupName, saved.UserName, saved.Message, saved.CreatedAt);
        }
    }

    // 1-1
    public async Task SendPrivateMessage(string toUserId, string message)
    {
        var fromUserId = Context.User!.GetUserId();
        var fromUserName = Context.User.GetUserName();

        var dto = new SendMessageDto { Message = message };
        var result = await chatService.SavePrivateMessageAsync(fromUserId, fromUserName, toUserId, dto);
        var saved = result.Data;

        if (saved != null)
        {
            // FIX: Барои қабулкунанда номи фиристанда
            await Clients.User(toUserId)
                .SendAsync(SignalREvents.ReceivePrivateMessage, fromUserId, fromUserName, saved.Message, saved.CreatedAt);

            // Барои фиристанда тасдиқи иршод
            await Clients.User(fromUserId)
                .SendAsync(SignalREvents.ReceivePrivateMessage, toUserId, fromUserName, saved.Message, saved.CreatedAt);
        }
    }

    // Typing indicators
    public async Task UserTyping(string groupName)
    {
        var userName = Context.User!.GetUserName();
        await Clients.OthersInGroup(groupName)
            .SendAsync(SignalREvents.UserTyping, groupName, userName);
    }

    public async Task UserStoppedTyping(string groupName)
    {
        var userName = Context.User!.GetUserName();
        await Clients.OthersInGroup(groupName)
            .SendAsync(SignalREvents.UserStoppedTyping, groupName, userName);
    }

    public async Task UserTypingPrivate(string toUserId)
    {
        var fromUserId = Context.User!.GetUserId();
        var fromUserName = Context.User.GetUserName();
        await Clients.User(toUserId)
            .SendAsync(SignalREvents.UserTyping, fromUserId, fromUserName);
    }

    public async Task UserStoppedTypingPrivate(string toUserId)
    {
        var fromUserId = Context.User!.GetUserId();
        var fromUserName = Context.User.GetUserName();
        await Clients.User(toUserId)
            .SendAsync(SignalREvents.UserStoppedTyping, fromUserId, fromUserName);
    }

    // Message operations
    public async Task EditMessage(Guid messageId, string newMessage)
    {
        var userId = Context.User!.GetUserId();
        var result = await chatService.EditMessageAsync(messageId, userId, newMessage);

        if (result.Data != null)
        {
            var edited = result.Data;
            
            // Broadcast edit to relevant clients
            if (edited.IsPrivate && edited.ReceiverUserId != null)
            {
                await Clients.Users(userId, edited.ReceiverUserId)
                    .SendAsync(SignalREvents.MessageEdited, edited.Id, edited.Message, edited.CreatedAt);
            }
            else if (edited.GroupName != null)
            {
                await Clients.Group(edited.GroupName)
                    .SendAsync(SignalREvents.MessageEdited, edited.Id, edited.Message, edited.CreatedAt);
            }
            else
            {
                await Clients.All.SendAsync(SignalREvents.MessageEdited, edited.Id, edited.Message, edited.CreatedAt);
            }
        }
    }

    public async Task DeleteMessage(Guid messageId)
    {
        var userId = Context.User!.GetUserId();
        var result = await chatService.DeleteMessageAsync(messageId, userId);

        if (result.Data)
        {
            // For simplicity, broadcast to all - in production, should target specific users/groups
            await Clients.All.SendAsync(SignalREvents.MessageDeleted, messageId);
        }
    }
}
