using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Data;
using RealTimeChatApp.Extensions;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }


        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }

        public async Task SendMessage(int roomId, string content)
        {
            var userId = Context.User?.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return;

            // Сохраняем в БД
            var message = new Message
            {
                RoomId = roomId,
                UserId = userId.Value,
                Content = content,
                TimeStamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Отправляем всем в комнате
            await Clients.Group($"room_{roomId}").SendAsync("ReceiveMessage",
                user.UserName ?? "Unknown",
                content,
                DateTime.UtcNow);
        }
    }
}
