using Messenger.Application.DTOs.ChatDTOs;
using Messenger.Application.Hubs;
using Messenger.Application.Interfaces;
using Messenger.Application.Exceptions;
using Messenger.Domain.Entities;
using Messenger.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(AppDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<ChatDTO> CreateChatAsync(List<Guid> participantIds, bool isGroup)
    {
        var users = await _context.Users
            .Where(u => participantIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count != participantIds.Count)
            throw new ValidationException("Один или несколько пользователей не существуют"); // 👈 кастомная ошибка

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            IsGroup = isGroup,
            Participants = users.Select(u => new ChatParticipant { UserId = u.Id }).ToList()
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
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
            throw new NotFoundException("Chat not found"); // 👈

        var sender = await _context.Users.FindAsync(senderId);
        if (sender == null)
            throw new NotFoundException("Sender not found"); // 👈

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

        var messageDto = new MessageDTO
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderUserName = sender.Username,
            Text = message.Text,
            SentAt = message.SentAt
        };

        await _hubContext.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", messageDto);

        return messageDto;
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
            Participants = c.Participants.Select(p => new UserDTO
            {
                Id = p.User.Id,
                Username = p.User.Username
            }).ToList(),
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
        if (message == null) throw new NotFoundException("Message not found");
        if (message.SenderId != userId) throw new ForbiddenException("You cannot delete someone else's message");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(message.ChatId.ToString())
            .SendAsync("MessageDeleted", message.Id);
    }

    public async Task<MessageDTO> EditMessageAsync(Guid messageId, Guid userId, string newText)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null) throw new NotFoundException("Message not found");
        if (message.SenderId != userId) throw new ForbiddenException("You cannot edit someone else's message");

        message.Text = newText;
        await _context.SaveChangesAsync();

        var dto = new MessageDTO
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderUserName = message.Sender.Username,
            Text = message.Text,
            SentAt = message.SentAt
        };

        await _hubContext.Clients.Group(message.ChatId.ToString())
            .SendAsync("MessageEdited", dto);

        return dto;
    }
}



