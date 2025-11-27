namespace Application.Chat.Dto;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public bool IsPrivate { get; set; }
    public string? ReceiverUserId { get; set; }
    public string? GroupName { get; set; }
    
    public Domain.Entities.MessageType Type { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
}


