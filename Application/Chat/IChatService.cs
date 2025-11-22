using Application.Chat.Dto;
using Infrastructure.Responses;

namespace Application.Chat;

public interface IChatService
{
    Task<Response<ChatMessageDto>> SaveMessageAsync(string userId, string userName, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetLastMessagesAsync(int count);
    
    Task<Response<ChatMessageDto>> SaveGroupMessageAsync(string userId, string userName, string groupName, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetGroupHistoryAsync(string groupName, int count);
    
    Task<Response<ChatMessageDto>> SavePrivateMessageAsync(string fromUserId, string fromUserName, string toUserId, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetPrivateHistoryAsync(string currentUserId, string otherUserId, int count);
}

