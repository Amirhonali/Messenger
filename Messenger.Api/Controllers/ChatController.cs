using Messenger.Application.Interfaces;
using Messenger.Application.Services;
using Messenger.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] List<Guid> participantIds, bool isGroup)
        {
            try
            {
                var chatDto = await _chatService.CreateChatAsync(participantIds, isGroup);
                return Ok(chatDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/message")]
        public async Task<IActionResult> SendMessage(Guid chatId, [FromQuery] Guid senderId, [FromBody] string text)
        {
            var message = await _chatService.SendMessageAsync(chatId, senderId, text);
            return Ok(message);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            var messages = await _chatService.GetMessagesAsync(chatId);
            return Ok(messages);
        }

        [HttpGet("user/{userId}/chats")]
        public async Task<IActionResult> GetUserChats(Guid userId)
        {
            var chats = await _chatService.GetUserChatsWithMessagesAsync(userId);
            return Ok(chats);
        }
    }
}