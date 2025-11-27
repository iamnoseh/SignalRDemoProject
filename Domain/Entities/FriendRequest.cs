using System;

namespace Domain.Entities;

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected
}

public class FriendRequest
{
    public Guid Id { get; set; }

    public string SenderId { get; set; }
    public AppUser Sender { get; set; }

    public string ReceiverId { get; set; }
    public AppUser Receiver { get; set; }

    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
