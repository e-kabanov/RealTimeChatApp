using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RealTimeChatApp.Data;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Extensions;
using RealTimeChatApp.Hubs;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(ChatDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IResult> SendMessage(SendMessageDto dto) // добавлен signal r
        {
            var userId = User.GetUserId();

            if (dto == null)
            {
                return Results.BadRequest("Передана пустая модель");
            }

            var isUserInRoom = await _context.RoomUsers
                .AnyAsync(ru => ru.RoomId == dto.RoomId && ru.UserId == userId);

            if (!isUserInRoom)
                return Results.Forbid();

            var message = new Message
            {
                UserId = userId,
                RoomId = dto.RoomId,
                Content = dto.Content,
                TimeStamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // уведомление о новом сообщении

            await _hubContext.Clients.Group($"room_{dto.RoomId}").SendAsync("ReceiveMessage", new 
            {
                id = message.Id,
                userId = message.UserId,
                userName = message.User?.UserName ?? "Unknown",
                content = message.Content,
                timeStamp = message.TimeStamp,
                avatarUrl = message.User?.AvatarUrl

            });

            

            return Results.Ok(message);
        }

        [HttpGet("room/{roomId}")]
        public async Task<IResult> GetRoomMessages (int roomId)
        {
            var userId = User.GetUserId();

            if (roomId <= 0)
            {
                return Results.BadRequest("Неверный Id");
            }

            var isUserInRoom = await _context.RoomUsers
        .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            if (!isUserInRoom)
                return Results.Forbid();

            var messages = await _context.Messages
                 .Where(m => m.RoomId == roomId)
                 .Include(m => m.User)
                 .OrderBy(m => m.TimeStamp)
                 .Take(50)
                 .Select(m => new MessageDto
         {
             Id = m.Id,
             UserId = m.UserId,
             RoomId = m.RoomId,
             Content = m.Content ?? string.Empty,
             TimeStamp = m.TimeStamp,
             Type = m.Type,
             UserName = m.User!.UserName ?? "Unknown",
             AvatarUrl = m.User.AvatarUrl
         })
         .ToListAsync();

            if (messages.Any())
            {
                return Results.Ok(messages);
            }

            return Results.NotFound("Сообщения группы не найдены");
        }

        [HttpPut("{messageId}")]
        public async Task<IResult> EditMessage(int messageId, [FromBody] string content)  // добавлен signal r
        { 
            if (messageId <= 0 ||  content == null) { return Results.BadRequest("Сообщение пусто или передан неверный Id"); }


            var userId = User.GetUserId();

            var message = await _context.Messages.FindAsync(messageId);

            if (message == null) { return Results.NotFound("Сообщение не найдено."); }

            if (message.UserId != userId) { return Results.Forbid(); }

            message.Content = content;
            await _context.SaveChangesAsync();

            // уведомляем о редактированном сообщении
            await _hubContext.Clients.Group($"room_{message.RoomId}").SendAsync("MessageEdited", messageId, content);

            return Results.NoContent();
        }

        [HttpDelete("{messageId}")]
        public async Task<IResult> DeleteMessage (int messageId)
        {
            if (messageId <= 0) { return Results.BadRequest("Передан неверный Id"); }

            var userId = User.GetUserId();

            var message = await _context.Messages.FindAsync(messageId);

            if (message == null) { return Results.NotFound("Сообщение не найдено."); }

            var isRoomOwner = await _context.RoomUsers.AnyAsync(ru => ru.RoomId == message.RoomId && ru.UserId == userId && ru.Role == "owner");

            if (message.UserId != userId && !isRoomOwner) { return Results.Forbid(); }

            var roomId = message.RoomId;
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"room_{roomId}").SendAsync("MessageDeleted", messageId);

            return Results.NoContent();

        }

        
    }
}
