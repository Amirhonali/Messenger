using Messenger.Application.DTOs.ChatDTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Application.Services
{
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
            {
                throw new Exception("Один или несколько пользователей не существуют");
            }

            // Создание чата
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

            // Преобразование в DTO
            var chatDto = new ChatDTO
            {
                Id = chat.Id,
                IsGroup = chat.IsGroup,
                Participants = users.Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username
                }).ToList()
            };

            return chatDto;
        }

        public async Task<Message> SendMessageAsync(Guid chatId, Guid senderId, string text)
        {
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
            return message;
        }

        public async Task<List<Message>> GetMessagesAsync(Guid chatId)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
            .ToListAsync();
        }

        public async Task<List<ChatWithMessagesDTO>> GetUserChatsWithMessagesAsync(Guid userId)
        {
            var chats = await _context.Chats
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages) // Подключаем сообщения
                .ToListAsync();

            return chats.Select(c => new ChatWithMessagesDTO
            {
                Id = c.Id,
                IsGroup = c.IsGroup,
                Participants = c.Participants.Select(p => new UserDTO
                {
                    Id = p.User.Id,
                    Username = p.User.Username
                }).ToList(),
                LastMessages = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Take(10) // Возвращаем последние 10 сообщений
                    .Select(m => new MessageDTO
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        Text = m.Text,
                        SentAt = m.SentAt
                    }).ToList()
            }).ToList();
        }
    }
}