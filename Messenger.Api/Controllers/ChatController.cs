using Messenger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Messenger.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Все методы требуют авторизации
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // 🔹 Вспомогательный метод для получения userId из JWT
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not found in token");

            return Guid.Parse(userIdClaim);
        }

        // 🔹 Модель для отправки сообщения
        public class SendMessageRequest
        {
            public string Text { get; set; } = string.Empty;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] List<Guid> participantIds, [FromQuery] bool isGroup)
        {
            var chatDto = await _chatService.CreateChatAsync(participantIds, isGroup);
            return Ok(chatDto);
        }

        [HttpPost("{chatId}/message")]
        public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest request)
        {
            var userId = GetUserId();
            var message = await _chatService.SendMessageAsync(chatId, userId, request.Text);
            return Ok(message);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            var messages = await _chatService.GetMessagesAsync(chatId);
            return Ok(messages);
        }

        [HttpGet("user/chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = GetUserId();
            var chats = await _chatService.GetUserChatsWithLastMessageAsync(userId);
            return Ok(chats);
        }

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = GetUserId();
            await _chatService.DeleteMessageAsync(messageId, userId);
            return NoContent();
        }

        [HttpPut("message/{messageId}")]
        public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] string newText)
        {
            var userId = GetUserId();
            var message = await _chatService.EditMessageAsync(messageId, userId, newText);
            return Ok(message);
        }
    }
}