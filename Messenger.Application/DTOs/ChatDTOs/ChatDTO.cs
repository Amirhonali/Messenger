using System;
namespace Messenger.Application.DTOs.ChatDTOs
{
    public class ChatDTO
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public List<UserDTO> Participants { get; set; } = new();
    }
}

