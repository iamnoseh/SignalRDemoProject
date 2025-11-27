namespace Application.Chat;

public interface IUserStatusService
{
    void SetOnline(string userId, string connectionId);
    void SetOffline(string userId, string connectionId);
    bool IsOnline(string userId);
    List<string> GetOnlineUsers();
    int GetConnectionCount(string userId);
}
