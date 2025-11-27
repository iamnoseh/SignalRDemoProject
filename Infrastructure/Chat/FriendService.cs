using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Chat;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Chat;

public class FriendService : IFriendService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FriendService> _logger;

    public FriendService(AppDbContext context, ILogger<FriendService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
    {
        if (senderId == receiverId)
        {
            _logger.LogWarning("User {UserId} tried to add themselves as friend", senderId);
            return false;
        }

        // Check if already friends
        var existingFriendship = await _context.Friendships
            .AnyAsync(f => (f.User1Id == senderId && f.User2Id == receiverId) ||
                           (f.User1Id == receiverId && f.User2Id == senderId));

        if (existingFriendship)
        {
            _logger.LogInformation("Users {SenderId} and {ReceiverId} are already friends", senderId, receiverId);
            return false;
        }

        // Check if request already exists
        var existingRequest = await _context.FriendRequests
            .AnyAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId && r.Status == FriendRequestStatus.Pending);

        if (existingRequest)
        {
            _logger.LogInformation("Friend request from {SenderId} to {ReceiverId} already pending", senderId, receiverId);
            return false;
        }

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.FriendRequests.Add(request);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Friend request sent from {SenderId} to {ReceiverId}", senderId, receiverId);
        return true;
    }

    public async Task<bool> AcceptFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);

        if (request == null)
        {
            _logger.LogWarning("Pending friend request {RequestId} not found for user {UserId}", requestId, userId);
            return false;
        }

        request.Status = FriendRequestStatus.Accepted;

        // Create friendship (ensure consistent ordering for composite key if needed, or just add both ways/one way depending on logic. 
        // Here we store one record. Logic: User1Id < User2Id to ensure uniqueness if we wanted, but for now just storing as is or standardizing.)
        // Let's standardize: User1 is always "smaller" ID? Or just trust the DB constraint?
        // For simplicity, let's just add it. But wait, our key is {User1Id, User2Id}.
        // If we want bidirectional, we might need to check both.
        // Let's just store two records? No, that's redundant.
        // Let's store one record. We need to handle queries correctly (OR condition).
        
        var friendship = new Friendship
        {
            User1Id = request.SenderId,
            User2Id = request.ReceiverId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Friend request {RequestId} accepted. Friendship created between {User1} and {User2}", requestId, request.SenderId, request.ReceiverId);
        return true;
    }

    public async Task<bool> RejectFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);

        if (request == null)
        {
            _logger.LogWarning("Pending friend request {RequestId} not found for user {UserId}", requestId, userId);
            return false;
        }

        request.Status = FriendRequestStatus.Rejected;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Friend request {RequestId} rejected by user {UserId}", requestId, userId);
        return true;
    }

    public async Task<List<AppUser>> GetFriendsAsync(string userId)
    {
        var friendIds = await _context.Friendships
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync();

        var friends = await _context.Users
            .Where(u => friendIds.Contains(u.Id))
            .ToListAsync();

        return friends;
    }

    public async Task<List<FriendRequest>> GetPendingRequestsAsync(string userId)
    {
        return await _context.FriendRequests
            .Include(r => r.Sender)
            .Where(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
