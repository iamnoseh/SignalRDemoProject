namespace Domain.Entities;

public class ChatGroupMember
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }
    public ChatGroup Group { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public DateTime JoinedAt { get; set; }
}


