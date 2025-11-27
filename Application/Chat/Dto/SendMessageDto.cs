namespace Application.Chat.Dto;

public class SendMessageDto
{
    public string Message { get; set; } = default!;
    public Domain.Entities.MessageType Type { get; set; } = Domain.Entities.MessageType.Text;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
}


