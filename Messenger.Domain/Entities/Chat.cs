namespace Messenger.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsGroup { get; set; }

    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}