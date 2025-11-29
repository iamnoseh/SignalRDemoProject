using System.Net;
using Application.Chat;
using Application.Chat.Dto;
using Application.Responses;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Chat;

public class ChatService(
    AppDbContext context, 
    UserManager<AppUser> userManager,
    ILogger<ChatService> logger) : IChatService
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
            Type = dto.Type,
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = false,
            ReceiverUserId = null,
            GroupName = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<PaginatedResponse<ChatMessageDto>>> GetLastMessagesAsync(PaginationRequest pagination)
    {
        var query = context.ChatMessages
            .Where(m => !m.IsPrivate && m.GroupName == null && !m.IsDeleted);

        var totalCount = await query.CountAsync();

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        var paginatedResponse = new PaginatedResponse<ChatMessageDto>(
            messages, totalCount, pagination.PageNumber, pagination.PageSize);

        return new Response<PaginatedResponse<ChatMessageDto>>(paginatedResponse);
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
            Type = dto.Type,
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = false,
            GroupName = normalizedName,
            ReceiverUserId = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<PaginatedResponse<ChatMessageDto>>> GetGroupHistoryAsync(string groupName, PaginationRequest pagination)
    {
        var query = context.ChatMessages
            .Where(m => !m.IsPrivate && m.GroupName == groupName && !m.IsDeleted);

        var totalCount = await query.CountAsync();

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        var paginatedResponse = new PaginatedResponse<ChatMessageDto>(
            messages, totalCount, pagination.PageNumber, pagination.PageSize);

        return new Response<PaginatedResponse<ChatMessageDto>>(paginatedResponse);
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

        // Validate target user exists
        var targetUser = await userManager.FindByIdAsync(toUserId);
        if (targetUser == null)
        {
            logger.LogWarning("User {FromUserId} attempted to send message to non-existent user {ToUserId}", 
                fromUserId, toUserId);
            return new Response<ChatMessageDto>(HttpStatusCode.NotFound, "Target user not found");
        }

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = fromUserId,
            UserName = fromUserName,
            Message = dto.Message,
            Type = dto.Type,
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = true,
            ReceiverUserId = toUserId,
            GroupName = null
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(entity));
    }

    public async Task<Response<PaginatedResponse<ChatMessageDto>>> GetPrivateHistoryAsync(string currentUserId, string otherUserId, PaginationRequest pagination)
    {
        var query = context.ChatMessages
            .Where(m => m.IsPrivate && !m.IsDeleted &&
                        ((m.UserId == currentUserId && m.ReceiverUserId == otherUserId) ||
                         (m.UserId == otherUserId && m.ReceiverUserId == currentUserId)));

        var totalCount = await query.CountAsync();

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();

        var paginatedResponse = new PaginatedResponse<ChatMessageDto>(
            messages, totalCount, pagination.PageNumber, pagination.PageSize);

        return new Response<PaginatedResponse<ChatMessageDto>>(paginatedResponse);
    }

    public async Task<Response<ChatMessageDto>> EditMessageAsync(Guid messageId, string userId, string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Message cannot be empty");
        }

        var message = await context.ChatMessages.FindAsync(messageId);
        if (message == null)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.NotFound, "Message not found");
        }

        if (message.UserId != userId)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.Forbidden, "You can only edit your own messages");
        }

        if (message.IsDeleted)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.BadRequest, "Cannot edit deleted message");
        }

        message.Message = newMessage;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new Response<ChatMessageDto>(MapToDto(message));
    }

    public async Task<Response<bool>> DeleteMessageAsync(Guid messageId, string userId)
    {
        var message = await context.ChatMessages.FindAsync(messageId);
        if (message == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "Message not found");
        }

        if (message.UserId != userId)
        {
            return new Response<bool>(HttpStatusCode.Forbidden,"You can only delete your own messages");
        }

        // Soft delete
        message.IsDeleted = true;

        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<ReactionDto>> ReactToMessageAsync(Guid messageId, string userId, string reaction)
    {
        var message = await context.ChatMessages.FindAsync(messageId);
        if (message == null)
        {
            return new Response<ReactionDto>(HttpStatusCode.NotFound, "Message not found");
        }

        if (string.IsNullOrWhiteSpace(reaction))
        {
            return new Response<ReactionDto>(HttpStatusCode.BadRequest, "Reaction cannot be empty");
        }

        // Check if user already reacted
        var existingReaction = await context.ChatReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (existingReaction != null)
        {
            // Update existing reaction
            existingReaction.Reaction = reaction;
            existingReaction.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new reaction
            existingReaction = new ChatReaction
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                UserId = userId,
                Reaction = reaction,
                CreatedAt = DateTime.UtcNow
            };
            context.ChatReactions.Add(existingReaction);
        }

        await context.SaveChangesAsync();

        // Get user name for DTO
        var user = await userManager.FindByIdAsync(userId);
        var reactionDto = new ReactionDto
        {
            Id = existingReaction.Id,
            UserId = userId,
            UserName = user?.UserName ?? "Unknown",
            Reaction = reaction,
            CreatedAt = existingReaction.CreatedAt
        };

        return new Response<ReactionDto>(reactionDto);
    }

    public async Task<Response<bool>> RemoveReactionAsync(Guid messageId, string userId)
    {
        var reaction = await context.ChatReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (reaction == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "Reaction not found");
        }

        context.ChatReactions.Remove(reaction);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<ChatMessageDto>> MarkMessageAsReadAsync(Guid messageId, string userId)
    {
        var message = await context.ChatMessages
            .Include(m => m.Reactions)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.NotFound, "Message not found");
        }

        // Only the receiver can mark as read
        if (!message.IsPrivate || message.ReceiverUserId != userId)
        {
            return new Response<ChatMessageDto>(HttpStatusCode.Forbidden, "Only the receiver can mark a private message as read");
        }

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        return new Response<ChatMessageDto>(MapToDto(message));
    }

    private static ChatMessageDto MapToDto(ChatMessage entity) =>
        new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            UserName = entity.UserName,
            Message = entity.Message,
            Type = entity.Type,
            FileUrl = entity.FileUrl,
            FileName = entity.FileName,
            CreatedAt = entity.CreatedAt,
            IsPrivate = entity.IsPrivate,
            ReceiverUserId = entity.ReceiverUserId,
            GroupName = entity.GroupName,
            IsEdited = entity.IsEdited,
            EditedAt = entity.EditedAt,
            IsDeleted = entity.IsDeleted,
            IsRead = entity.IsRead,
            ReadAt = entity.ReadAt,
            Reactions = entity.Reactions?.Select(r => new ReactionDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Unknown",
                Reaction = r.Reaction,
                CreatedAt = r.CreatedAt
            }).ToList() ?? new List<ReactionDto>()
        };
}
