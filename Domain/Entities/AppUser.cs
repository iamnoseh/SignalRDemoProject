using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppUser : IdentityUser
{
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatGroup> OwnedGroups { get; set; } = new List<ChatGroup>();
    public ICollection<ChatGroupMember> GroupMemberships { get; set; } = new List<ChatGroupMember>();
}


