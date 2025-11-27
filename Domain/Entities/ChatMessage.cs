using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(CreatedAt))]
[Index(nameof(GroupName))]
[Index(nameof(ReceiverUserId))]
public class ChatMessage
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Message { get; set; } = default!;
    public MessageType Type { get; set; } = MessageType.Text;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPrivate { get; set; }
    public string? ReceiverUserId { get; set; }
    public string? GroupName { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public AppUser User { get; set; } = default!;
    public AppUser? ReceiverUser { get; set; }
}


