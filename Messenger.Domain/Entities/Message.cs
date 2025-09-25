using Messenger.Domain.Entities;

namespace Messenger.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public string Text { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}