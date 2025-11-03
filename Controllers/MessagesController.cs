using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Data;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Extensions;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public MessagesController(ChatDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IResult> SendMessage(SendMessageDto dto)
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

            return Results.Ok(messages);
        }
    }
}
