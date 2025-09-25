using Messenger.Application.DTOs.ChatDTOs;
using Messenger.Domain.Entities;

namespace Messenger.Application.Interfaces;

public interface IChatService
{
    Task<ChatDTO> CreateChatAsync(List<Guid> participantIds, bool isGroup = false);
    Task<Message> SendMessageAsync(Guid chatId, Guid senderId, string text);
    Task<List<Message>> GetMessagesAsync(Guid chatId);
    Task<List<ChatWithMessagesDTO>> GetUserChatsWithMessagesAsync(Guid userId);
}