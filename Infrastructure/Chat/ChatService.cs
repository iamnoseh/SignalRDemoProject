using System.Net;
using Application.Chat;
using Application.Chat.Dto;
using Domain.Entities;
using Infrastructure.Responses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Chat;

public class ChatService(AppDbContext context) : IChatService
{
    public async Task<Response<ChatMessageDto>> SaveMessageAsync(string userId, string userName, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Message cannot be empty");
        }

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = false,
            ReceiverUserId = null,
            GroupName = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<List<ChatMessageDto>>> GetLastMessagesAsync(int count)
    {
        count = count <= 0 ? 50 : count;

        var messages = await context.ChatMessages
            .Where(m => !m.IsPrivate && m.GroupName == null)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new Response<List<ChatMessageDto>>(messages);
    }

    public async Task<Response<ChatMessageDto>> SaveGroupMessageAsync(string userId, string userName, string groupName, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Group name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Message cannot be empty");
        }

        var normalizedName = groupName.Trim();

        var group = await context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == normalizedName);

        if (group == null)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.NotFound, "Group not found");
        }

        var isMember = await context.ChatGroupMembers
            .AnyAsync(m => m.GroupId == group.Id && m.UserId == userId);

        if (!isMember)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.Forbidden, "User is not a member of this group");
        }

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = false,
            GroupName = normalizedName,
            ReceiverUserId = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<List<ChatMessageDto>>> GetGroupHistoryAsync(string groupName, int count)
    {
        count = count <= 0 ? 50 : count;

        var messages = await context.ChatMessages
            .Where(m => !m.IsPrivate && m.GroupName == groupName)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new Response<List<ChatMessageDto>>(messages);
    }

    public async Task<Response<ChatMessageDto>> SavePrivateMessageAsync(string fromUserId, string fromUserName, string toUserId, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(toUserId))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Target user is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Message cannot be empty");
        }

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = fromUserId,
            UserName = fromUserName,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = true,
            ReceiverUserId = toUserId,
            GroupName = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<List<ChatMessageDto>>> GetPrivateHistoryAsync(string currentUserId, string otherUserId, int count)
    {
        count = count <= 0 ? 50 : count;

        var messages = await context.ChatMessages
            .Where(m => m.IsPrivate &&
                        ((m.UserId == currentUserId && m.ReceiverUserId == otherUserId) ||
                         (m.UserId == otherUserId && m.ReceiverUserId == currentUserId)))
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new Response<List<ChatMessageDto>>(messages);
    }

    private static ChatMessageDto MapToDto(ChatMessage entity) =>
        new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            UserName = entity.UserName,
            Message = entity.Message,
            CreatedAt = entity.CreatedAt,
            IsPrivate = entity.IsPrivate,
            ReceiverUserId = entity.ReceiverUserId,
            GroupName = entity.GroupName
        };
}

