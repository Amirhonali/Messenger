using Messenger.Application.Interfaces;
using Messenger.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Messenger.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // теперь все методы требуют авторизации
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
        public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] string text)
        {
            // Получаем userId из JWT токена
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID in token");

            var message = await _chatService.SendMessageAsync(chatId, userId, text);
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
            // userId берём из токена
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID in token");

            var chats = await _chatService.GetUserChatsWithLastMessageAsync(userId);
            return Ok(chats);
        }

        [Authorize]
        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            await _chatService.DeleteMessageAsync(messageId, userId);
            return NoContent();
        }

        [Authorize]
        [HttpPut("message/{messageId}")]
        public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] string newText)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            var message = await _chatService.EditMessageAsync(messageId, userId, newText);
            return Ok(message);
        }
    }
}