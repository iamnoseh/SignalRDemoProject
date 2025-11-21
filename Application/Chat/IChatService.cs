using Application.Chat.Dto;
using Infrastructure.Responses;

namespace Application.Chat;

public interface IChatService
{
    // Глобальный / public чат
    Task<Response<ChatMessageDto>> SaveMessageAsync(string userId, string userName, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetLastMessagesAsync(int count);

    // Групповой чат
    Task<Response<ChatMessageDto>> SaveGroupMessageAsync(string userId, string userName, string groupName, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetGroupHistoryAsync(string groupName, int count);

    // 1-1 чат
    Task<Response<ChatMessageDto>> SavePrivateMessageAsync(string fromUserId, string fromUserName, string toUserId, SendMessageDto dto);
    Task<Response<List<ChatMessageDto>>> GetPrivateHistoryAsync(string currentUserId, string otherUserId, int count);
}

