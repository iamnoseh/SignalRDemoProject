using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatGroup> ChatGroups { get; set; }
    public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<ChatReaction> ChatReactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Relationship: ChatMessage -> User (sender)
            entity.HasOne(e => e.User)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: ChatMessage -> ReceiverUser (for private messages)
            entity.HasOne(e => e.ReceiverUser)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(e => e.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.GroupName);
            entity.HasIndex(e => e.ReceiverUserId);
            entity.HasIndex(e => new { e.UserId, e.ReceiverUserId });
        });

        // ChatGroup configuration
        modelBuilder.Entity<ChatGroup>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique constraint on Name
            entity.HasIndex(e => e.Name).IsUnique();

            // Relationship: ChatGroup -> Owner
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ChatGroupMember configuration
        modelBuilder.Entity<ChatGroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Composite unique index to prevent duplicate membership
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();

            // Relationship: ChatGroupMember -> Group
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: ChatGroupMember -> User
            entity.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FriendRequest configuration
        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Friendship configuration
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => new { e.User1Id, e.User2Id });

            entity.HasOne(e => e.User1)
                .WithMany()
                .HasForeignKey(e => e.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User2)
                .WithMany()
                .HasForeignKey(e => e.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

