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
            var userId = Context.User.GetUserId();

            var isUserInRoom = await _context.RoomUsers
                .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            if (!isUserInRoom) { return;  }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}" );

            Console.WriteLine($"Пользователь {userId} присоединился к комнате {roomId}");

        }

        public async Task UserTyping(int roomId, bool isTyping) 
        {
            var userId = Context.User?.GetUserId();

            var isUserInRoom = await _context.RoomUsers
               .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            if (!isUserInRoom) return;

            var user = await _context.Users.FindAsync(userId);

            await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", user?.UserName ?? "Unknown", isTyping);

        }

        

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();

            if (userId.HasValue)
            {
                await UpdateUserOnlineStatus(userId.Value, true);
            }

            await base.OnConnectedAsync();

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.GetUserId();

            if (userId.HasValue)
            {
                await UpdateUserOnlineStatus(userId.Value, false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserOnlineStatus(int userId, bool isOnline)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.IsOnline = isOnline;
                user.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
       
    }
}
