using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class ChatGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string OwnerUserId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public ICollection<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
}


