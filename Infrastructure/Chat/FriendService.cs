using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Chat;
using Application.Users.Dto;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Chat;

public class FriendService(
    AppDbContext context,
    ILogger<FriendService> logger,
    Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
    : IFriendService
{
    private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager = userManager;

    public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
    {
        if (senderId == receiverId)
        {
            logger.LogWarning("User {UserId} tried to add themselves as friend", senderId);
            return false;
        }

        var receiverExists = await context.Users.AnyAsync(u => u.Id == receiverId);
        if (!receiverExists)
        {
            logger.LogWarning("User {SenderId} tried to send friend request to non-existent user {ReceiverId}", senderId, receiverId);
            return false;
        }

        var existingFriendship = await context.Friendships
            .AnyAsync(f => (f.User1Id == senderId && f.User2Id == receiverId) ||
                           (f.User1Id == receiverId && f.User2Id == senderId));

        if (existingFriendship)
        {
            logger.LogInformation("Users {SenderId} and {ReceiverId} are already friends", senderId, receiverId);
            return false;
        }
        var existingRequest = await context.FriendRequests
            .AnyAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId && r.Status == FriendRequestStatus.Pending);

        if (existingRequest)
        {
            logger.LogInformation("Friend request from {SenderId} to {ReceiverId} already pending", senderId, receiverId);
            return false;
        }

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.FriendRequests.Add(request);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Friend request sent from {SenderId} to {ReceiverId}", senderId, receiverId);
        return true;
    }

    public async Task<bool> AcceptFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await context.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);

        if (request == null)
        {
            logger.LogWarning("Pending friend request {RequestId} not found for user {UserId}", requestId, userId);
            return false;
        }

        request.Status = FriendRequestStatus.Accepted;

        var friendship = new Friendship
        {
            User1Id = request.SenderId,
            User2Id = request.ReceiverId,
            CreatedAt = DateTime.UtcNow
        };

        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();

        logger.LogInformation("Friend request {RequestId} accepted. Friendship created between {User1} and {User2}", requestId, request.SenderId, request.ReceiverId);
        return true;
    }

    public async Task<bool> RejectFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await context.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);

        if (request == null)
        {
            logger.LogWarning("Pending friend request {RequestId} not found for user {UserId}", requestId, userId);
            return false;
        }

        request.Status = FriendRequestStatus.Rejected;
        await context.SaveChangesAsync();

        logger.LogInformation("Friend request {RequestId} rejected by user {UserId}", requestId, userId);
        return true;
    }

    public async Task<List<UserDto>> GetFriendsAsync(string userId)
    {
        var friendIds = await context.Friendships
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync();

        var friends = await context.Users
            .Where(u => friendIds.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nickname = u.Nickname,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .ToListAsync();

        return friends;
    }

    public async Task<List<FriendRequest>> GetPendingRequestsAsync(string userId)
    {
        return await context.FriendRequests
            .Include(r => r.Sender)
            .Where(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
