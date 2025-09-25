using Messenger.Domain.Entities;

namespace Messenger.Domain.Entities;

public class UserContact
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid ContactId { get; set; }
    public User Contact { get; set; } = null!;
}