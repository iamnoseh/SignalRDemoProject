namespace Application.Chat.Dto;

public class ReactionDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Reaction { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
