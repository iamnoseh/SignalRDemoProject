namespace Application.Chat.Dto;

public class SendGroupMessageDto : SendMessageDto
{
    public string GroupName { get; set; } = default!;
}


