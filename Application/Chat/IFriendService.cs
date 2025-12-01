using Application.Users.Dto;
using Domain.Entities;


namespace Application.Chat;

public interface IFriendService
{
    Task<bool> SendFriendRequestAsync(string senderId, string receiverId);
    Task<bool> AcceptFriendRequestAsync(string userId, Guid requestId);
    Task<bool> RejectFriendRequestAsync(string userId, Guid requestId);
    Task<List<UserDto>> GetFriendsAsync(string userId);
    Task<List<FriendRequest>> GetPendingRequestsAsync(string userId);
}
