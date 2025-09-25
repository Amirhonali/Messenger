using Messenger.Application.DTOs.ChatDTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;

    public ChatService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatDTO> CreateChatAsync(List<Guid> participantIds, bool isGroup)
    {
        // Проверка существования пользователей
        var users = await _context.Users
            .Where(u => participantIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count != participantIds.Count)
            throw new Exception("Один или несколько пользователей не существуют");

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            IsGroup = isGroup,
            Participants = users.Select(u => new ChatParticipant
            {
                UserId = u.Id
            }).ToList()
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        return new ChatDTO
        {
            Id = chat.Id,
            IsGroup = chat.IsGroup,
            Participants = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username
            }).ToList()
        };
    }

    public async Task<MessageDTO> SendMessageAsync(Guid chatId, Guid senderId, string text)
    {
        var chat = await _context.Chats
            .Include(c => c.Participants)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
            throw new Exception("Чат не найден");

        if (!chat.Participants.Any(p => p.UserId == senderId))
            throw new Exception("Пользователь не участвует в чате");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Text = text,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var sender = chat.Participants.First(p => p.UserId == senderId).User;

        return new MessageDTO
        {
            Id = message.Id,
            SenderId = senderId,
            SenderUserName = sender.Username,
            Text = message.Text,
            SentAt = message.SentAt
        };
    }

    public async Task<List<MessageDTO>> GetMessagesAsync(Guid chatId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
        .OrderBy(m => m.SentAt)
            .ToListAsync();

        return messages.Select(m => new MessageDTO
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderUserName = m.Sender.Username,
            Text = m.Text,
            SentAt = m.SentAt
        }).ToList();
    }

    public async Task<List<ChatDTO>> GetUserChatsWithLastMessageAsync(Guid userId)
    {
        var chats = await _context.ChatParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.Chat)
            .Include(c => c.Participants)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .ToListAsync();

        return chats.Select(c => new ChatDTO
        {
            Id = c.Id,
            IsGroup = c.IsGroup,
            Participants = c.Participants.Select(p => new UserDTO { Id = p.User.Id, Username = p.User.Username }).ToList(),
            LastMessage = c.Messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUserName = m.Sender.Username,
                Text = m.Text,
                SentAt = m.SentAt
            }).FirstOrDefault()
        }).ToList();
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) throw new Exception("Message not found");
        if (message.SenderId != userId) throw new Exception("Not authorized");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
    }

    public async Task<MessageDTO> EditMessageAsync(Guid messageId, Guid userId, string newText)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) throw new Exception("Message not found");
        if (message.SenderId != userId) throw new Exception("Not authorized");

        message.Text = newText;
        await _context.SaveChangesAsync();

        return new MessageDTO
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderUserName = message.Sender.Username,
            Text = message.Text,
            SentAt = message.SentAt
        };
    }
}