using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppUser : IdentityUser
{
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatGroup> OwnedGroups { get; set; } = new List<ChatGroup>();
    public ICollection<ChatGroupMember> GroupMemberships { get; set; } = new List<ChatGroupMember>();
    public ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    
    public string? Nickname { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}


