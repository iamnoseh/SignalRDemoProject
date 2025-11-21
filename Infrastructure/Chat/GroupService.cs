using System.Net;
using Application.Chat;
using Application.Chat.Dto;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Chat;

public class GroupService(AppDbContext context) : IGroupService
{
    public async Task<Response<ChatGroupDto>> EnsureGroupExistsAsync(string ownerUserId, string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return new Response<ChatGroupDto>(HttpStatusCode.BadRequest, "Group name is required");
        }

        var normalizedName = groupName.Trim();

        var existing = await context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == normalizedName);

        if (existing != null)
        {
            return new Response<ChatGroupDto>(new ChatGroupDto
            {
                Id = existing.Id,
                Name = existing.Name
            });
        }

        var group = new ChatGroup
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            OwnerUserId = ownerUserId,
            CreatedAt = DateTime.UtcNow
        };

        context.ChatGroups.Add(group);
        await context.SaveChangesAsync();

        return new Response<ChatGroupDto>(new ChatGroupDto
        {
            Id = group.Id,
            Name = group.Name
        });
    }

    public async Task<Response<bool>> JoinGroupAsync(string userId, string groupName)
    {
        var normalizedName = groupName.Trim();

        var group = await context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == normalizedName);

        if (group == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "Group not found");
        }

        var exists = await context.ChatGroupMembers
            .AnyAsync(m => m.GroupId == group.Id && m.UserId == userId);

        if (exists)
        {
            return new Response<bool>(true);
        }

        var member = new ChatGroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        context.ChatGroupMembers.Add(member);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<bool>> LeaveGroupAsync(string userId, string groupName)
    {
        var normalizedName = groupName.Trim();

        var group = await context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == normalizedName);

        if (group == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "Group not found");
        }

        var member = await context.ChatGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == group.Id && m.UserId == userId);

        if (member == null)
        {
            return new Response<bool>(HttpStatusCode.NotFound, "User is not in this group");
        }

        context.ChatGroupMembers.Remove(member);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<bool> IsUserInGroupAsync(string userId, string groupName)
    {
        var normalizedName = groupName.Trim();

        var group = await context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == normalizedName);

        if (group == null)
        {
            return false;
        }

        return await context.ChatGroupMembers
            .AnyAsync(m => m.GroupId == group.Id && m.UserId == userId);
    }

    public async Task<Response<List<ChatGroupDto>>> GetUserGroupsAsync(string userId)
    {
        var groups = await context.ChatGroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => new ChatGroupDto
            {
                Id = m.GroupId,
                Name = m.Group.Name
            })
            .Distinct()
            .ToListAsync();

        return new Response<List<ChatGroupDto>>(groups);
    }
}


