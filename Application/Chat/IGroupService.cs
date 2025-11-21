using Application.Chat.Dto;
using Infrastructure.Responses;

namespace Application.Chat;

public interface IGroupService
{
    Task<Response<ChatGroupDto>> EnsureGroupExistsAsync(string ownerUserId, string groupName);
    Task<Response<bool>> JoinGroupAsync(string userId, string groupName);
    Task<Response<bool>> LeaveGroupAsync(string userId, string groupName);
    Task<bool> IsUserInGroupAsync(string userId, string groupName);
    Task<Response<List<ChatGroupDto>>> GetUserGroupsAsync(string userId);
}


