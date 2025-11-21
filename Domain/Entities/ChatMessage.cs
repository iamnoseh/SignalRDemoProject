using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(CreatedAt))]
public class ChatMessage
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    // Барои 1-1 чат ва групповой чат
    public bool IsPrivate { get; set; }
    public string? ReceiverUserId { get; set; }
    public string? GroupName { get; set; }
}


