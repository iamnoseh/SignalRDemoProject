namespace Application.Chat;

/// <summary>
/// Constants for SignalR event names to avoid magic strings
/// </summary>
public static class SignalREvents
{
    // Global chat events
    public const string ReceiveMessage = "ReceiveMessage";
    
    // Group chat events
    public const string ReceiveGroupMessage = "ReceiveGroupMessage";
    public const string SystemMessage = "SystemMessage";
    
    // Private chat events
    public const string ReceivePrivateMessage = "ReceivePrivateMessage";
    
    // User status events
    public const string UserOnline = "UserOnline";
    public const string UserOffline = "UserOffline";
    
    // Typing indicators
    public const string UserTyping = "UserTyping";
    public const string UserStoppedTyping = "UserStoppedTyping";
    
    // Message operations
    public const string MessageEdited = "MessageEdited";
    public const string MessageDeleted = "MessageDeleted";
    public const string MessageReaction = "MessageReaction";
    public const string MessageRead = "MessageRead";
}
