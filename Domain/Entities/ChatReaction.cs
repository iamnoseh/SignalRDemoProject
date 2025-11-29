using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ChatReaction
{
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = default!;
    
    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;
    
    [MaxLength(10)]
    public string Reaction { get; set; } = default!; // Emoji
    
    public DateTime CreatedAt { get; set; }
}
