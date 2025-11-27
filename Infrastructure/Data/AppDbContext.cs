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
    }
}

