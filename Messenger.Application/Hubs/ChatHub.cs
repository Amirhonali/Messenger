using Microsoft.AspNetCore.SignalR;

namespace Messenger.Application.Hubs
{
    public class ChatHub : Hub
    {
        // Когда пользователь подключается
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", $"Добро пожаловать! Ваш ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Метод для отправки сообщения в чат
        public async Task SendMessage(Guid chatId, Guid senderId, string text)
        {
            // Шлём всем в группе (чат = группа SignalR)
            await Clients.Group(chatId.ToString())
                         .SendAsync("ReceiveMessage", chatId, senderId, text, DateTime.UtcNow);
        }

        // Присоединение к чату (SignalR группа)
        public async Task JoinChat(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            await Clients.Caller.SendAsync("JoinedChat", chatId);
        }

        // Отключение
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}