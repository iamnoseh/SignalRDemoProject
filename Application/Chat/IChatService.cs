using Application.Chat.Dto;
using Application.Responses;

namespace Application.Chat;

public interface IChatService
{
    Task<Response<ChatMessageDto>> SaveMessageAsync(string userId, string userName, SendMessageDto dto);
    Task<Response<PaginatedResponse<ChatMessageDto>>> GetLastMessagesAsync(PaginationRequest pagination);
    
    Task<Response<ChatMessageDto>> SaveGroupMessageAsync(string userId, string userName, string groupName, SendMessageDto dto);
    Task<Response<PaginatedResponse<ChatMessageDto>>> GetGroupHistoryAsync(string groupName, PaginationRequest pagination);
    
    Task<Response<ChatMessageDto>> SavePrivateMessageAsync(string fromUserId, string fromUserName, string toUserId, SendMessageDto dto);
    Task<Response<PaginatedResponse<ChatMessageDto>>> GetPrivateHistoryAsync(string currentUserId, string otherUserId, PaginationRequest pagination);
    
    // Edit/Delete operations
    Task<Response<ChatMessageDto>> EditMessageAsync(Guid messageId, string userId, string newMessage);
    Task<Response<bool>> DeleteMessageAsync(Guid messageId, string userId);
}
