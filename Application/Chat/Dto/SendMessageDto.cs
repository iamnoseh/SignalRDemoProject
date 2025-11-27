namespace Application.Chat.Dto;

public class SendMessageDto
{
    public string Message { get; set; } = default!;
    public Domain.Enums.MessageType Type { get; set; } = Domain.Enums.MessageType.Text;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
}


