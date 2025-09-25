using Messenger.Domain.Entities;

namespace Messenger.Domain.Entities;

public class ChatParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}