using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Data;
using RealTimeChatApp.Extensions;
using RealTimeChatApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace RealTimeChatApp.Hubs
{
    [Authorize]
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

            if (!isUserInRoom) { return; }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");

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

            await NotifyUserStatusChange(userId, user.UserName, user.IsOnline);
        }

        private async Task NotifyUserStatusChange(int userId, string userName, bool isOnline)
        {
            await Clients.All.SendAsync("UserOnlineStatusChanged", userId, isOnline, userName);

            if (isOnline)
            {
                await Clients.All.SendAsync("UserWentOnline", userId, userName);
            }
            else
            {
                await Clients.All.SendAsync("UserWentOffline", userId, userName);
            }

        }
    }
}
