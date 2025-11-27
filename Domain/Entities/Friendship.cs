using System;

namespace Domain.Entities;

public class Friendship
{
    public string User1Id { get; set; }
    public AppUser User1 { get; set; }

    public string User2Id { get; set; }
    public AppUser User2 { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
