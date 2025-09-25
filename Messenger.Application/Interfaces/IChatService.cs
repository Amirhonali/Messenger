using Messenger.Application.DTOs.ChatDTOs;
using Messenger.Domain.Entities;

namespace Messenger.Application.Interfaces;

public interface IChatService
{
    Task<ChatDTO> CreateChatAsync(List<Guid> participantIds, bool isGroup);
    Task<MessageDTO> SendMessageAsync(Guid chatId, Guid senderId, string text);
    Task<List<MessageDTO>> GetMessagesAsync(Guid chatId);
    Task<List<ChatDTO>> GetUserChatsWithLastMessageAsync(Guid userId);
    Task DeleteMessageAsync(Guid messageId, Guid userId);
    Task<MessageDTO> EditMessageAsync(Guid messageId, Guid userId, string newText);
}