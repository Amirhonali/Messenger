using System;
namespace Messenger.Application.DTOs.ChatDTOs
{
    public class CreateChatRequest
    {
        public List<Guid> ParticipantIds { get; set; } = new();
        public bool IsGroup { get; set; }
    }
}

