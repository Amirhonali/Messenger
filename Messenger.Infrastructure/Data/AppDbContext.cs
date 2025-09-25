using System;
using Microsoft.EntityFrameworkCore;
using Messenger.Domain.Entities;

namespace Messenger.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserContact> UserContacts { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Связь контактов (многие-ко-многим через UserContact)
        modelBuilder.Entity<UserContact>()
            .HasOne(uc => uc.Owner)
            .WithMany(u => u.Contacts)
            .HasForeignKey(uc => uc.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserContact>()
            .HasOne(uc => uc.Contact)
            .WithMany()
            .HasForeignKey(uc => uc.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь участников чата
        modelBuilder.Entity<ChatParticipant>()
            .HasOne(cp => cp.User)
            .WithMany(u => u.Chats)
            .HasForeignKey(cp => cp.UserId);

        modelBuilder.Entity<ChatParticipant>()
            .HasOne(cp => cp.Chat)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChatId);

        // Сообщения
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId);
    }
}