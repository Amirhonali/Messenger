using System;
namespace Messenger.Application.DTOs.ChatDTOs
{
    public class ChatWithMessagesDTO
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public List<UserDTO> Participants { get; set; } = new();
        public List<MessageDTO> LastMessages { get; set; } = new();
    }
}

