namespace Messenger.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public ICollection<UserContact> Contacts { get; set; } = new List<UserContact>();
    public ICollection<ChatParticipant> Chats { get; set; } = new List<ChatParticipant>();
}