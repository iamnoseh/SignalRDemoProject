using System.Collections.Concurrent;
using Application.Chat;

namespace Infrastructure.Chat;

public class UserStatusService : IUserStatusService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _onlineUsers = new();
    private readonly object _lock = new();

    public void SetOnline(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_onlineUsers.ContainsKey(userId))
            {
                _onlineUsers[userId] = new HashSet<string>();
            }
            _onlineUsers[userId].Add(connectionId);
        }
    }

    public void SetOffline(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                }
            }
        }
    }

    public bool IsOnline(string userId)
    {
        return _onlineUsers.ContainsKey(userId) && _onlineUsers[userId].Count > 0;
    }

    public List<string> GetOnlineUsers()
    {
        return _onlineUsers.Keys.ToList();
    }

    public int GetConnectionCount(string userId)
    {
        return _onlineUsers.TryGetValue(userId, out var connections) ? connections.Count : 0;
    }
}
