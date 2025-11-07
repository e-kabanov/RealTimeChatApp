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

       // другие методы
    }
}
