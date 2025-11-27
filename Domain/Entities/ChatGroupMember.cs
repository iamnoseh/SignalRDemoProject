namespace Domain.Entities;

public enum GroupMemberRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}

public class ChatGroupMember
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }
    public ChatGroup Group { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;
    
    public DateTime JoinedAt { get; set; }
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
}


