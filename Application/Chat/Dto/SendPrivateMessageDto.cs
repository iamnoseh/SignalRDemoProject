namespace Application.Chat.Dto;

public class SendPrivateMessageDto : SendMessageDto
{
    public string ToUserId { get; set; } = default!;
}


