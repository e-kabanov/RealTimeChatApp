using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Data;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Models;
using RealTimeChatApp.Extensions;
using Microsoft.OpenApi.Validations;

namespace RealTimeChatApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly ChatDbContext _context;
        public RoomsController(ChatDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IResult> GetUserRooms()
        {
            var userId = User.GetUserId();

            if (userId <= 0)
            {
                return Results.BadRequest("Неверный Id");
            }

            var rooms = await _context.Rooms
        .Where(r => r.RoomUsers.Any(ru => ru.UserId == userId))
        .Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description,
            CreatedById = r.CreatedById,
            CreatedAt = r.CreatedAt,
            IsPrivate = r.IsPrivate,
            MaxParticipants = r.MaxParticipants
        })
        .ToListAsync();

            return Results.Ok(rooms);
        }

        [HttpPost]
        public async Task<IResult> CreateRoom(CreateRoomDto dto)
        {
            var userId = User.GetUserId();

            if (dto == null)
            {
                return Results.BadRequest("Передана пустая модель.");
            }

            var room = new Room
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                IsPrivate = dto.IsPrivate,
                MaxParticipants = dto.MaxParticipants,

            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var roomUser = new RoomUser
            {
                RoomId = room.Id,
                UserId = userId,
                Role = "owner",
                JoinedAt = DateTime.UtcNow

            };

            _context.RoomUsers.Add(roomUser);
            await _context.SaveChangesAsync();

            return Results.Ok();

        }

        [HttpPost("{roomId}/users/{userId}")]
        public async Task<IResult> AddUserToRoom (int roomId, int userId)
        {
            var currentUser = User.GetUserId();

            var isOwner = await _context.RoomUsers
                .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == currentUser && ru.Role == "owner");

            if (!isOwner)
            {
                return Results.Forbid();
            }

            var roomUser = new RoomUser
            {
                RoomId = roomId,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            _context.RoomUsers.Add(roomUser);
            await _context.SaveChangesAsync();

            return Results.Ok();

        }

        [HttpGet("{roomId}")]
        public async Task<IResult> GetRoomUsers (int roomId)
        {
            if (roomId <= 0)
            {
                return Results.BadRequest("Неверный Id комнаты");
            }

            var userId = User.GetUserId();

            var isUserInRoom = await _context.RoomUsers
                .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            if (!isUserInRoom)
            {
                return Results.Forbid();
            }

            var users = await _context.RoomUsers
                .Where(ru => ru.RoomId == roomId)
                .Include(ru => ru.User)
                .Select(ru => new
                {
                    UserId = ru.UserId,
                    Username = ru.User.UserName,
                    Role = ru.Role,
                    JoinedAt = ru.JoinedAt,
                    isOnline = ru.User.IsOnline

                }).ToListAsync();

            if (users.Count > 0)
            {
                return Results.Ok(users);
            }

            return Results.NotFound("Пользователи комнаты не найдены");
        }

        [HttpDelete("{roomId}/users/{userIdToRemove}")]
        public async Task<IResult> RemoveUserFromRoom (int roomId, int userIdToRemove)
        {
            if (roomId <= 0 || userIdToRemove <= 0)
            {
                return Results.NotFound();
            }

            var currentUser = User.GetUserId();

            var isOwner = await _context.RoomUsers.AnyAsync(ru => ru.RoomId == roomId && ru.UserId == currentUser && ru.Role == "owner");

            if (!isOwner) { return Results.Forbid(); }

            var roomUser = await _context.RoomUsers.FirstOrDefaultAsync(ru => ru.RoomId == roomId && ru.UserId == userIdToRemove);

            if (roomUser == null) { Results.NotFound("Пользователь не найден в комнате."); }

            _context.RoomUsers.Remove(roomUser);
            await _context.SaveChangesAsync();

            return Results.NoContent();
        }

        [HttpDelete("leave/{roomId}")]  // ТУТ ВОПРОС!
        public async Task<IResult> LeaveRoom (int roomId)
        {
            if (roomId <= 0) { return Results.BadRequest("Передан неверный Id"); }

            var userId = User.GetUserId();

            var roomUser = await _context.RoomUsers.FirstOrDefaultAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            if (roomUser == null) { return Results.NotFound("Вы не состоите в этой комнате"); }

            if (roomUser.Role == "owner")
            {
                int roomUserId = roomUser.Id;

                var newOwner = await _context.RoomUsers
                    .Where(ru => ru.RoomId == roomId && ru.UserId != roomUserId  ) //userId
                    .OrderBy(ru => ru.JoinedAt)
                    .FirstOrDefaultAsync();

                if (newOwner != null)
                {
                    // Тут сомнение!
                   
                    newOwner.Role = "owner";
                    _context.RoomUsers.Remove(roomUser);
                    await _context.SaveChangesAsync();
                    
                }

                else
                {
                    var room = await _context.Rooms.FindAsync(roomId);
                    if (room != null)
                    {
                        _context.Rooms.Remove(room);
                        // удалить пользователя
                        // _context.RoomUsers.Remove(roomUser)

                        await _context.SaveChangesAsync();
                        return Results.NoContent();
                    }
                }
            }

            _context.RoomUsers.Remove(roomUser);
            await _context.SaveChangesAsync();

            return Results.NoContent();

            
        }

        [HttpDelete("{roomId}")]
        public async Task<IResult> DeleteRoom (int roomId)
        {
            if (roomId <= 0) { return Results.BadRequest("Передан неверный Id"); }

            var userId = User.GetUserId();

            var isOwner = await _context.RoomUsers
                .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId && ru.Role == "owner");

            if (!isOwner) { return Results.Forbid(); }

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) { Results.NotFound("Комната не найдена."); }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Results.NoContent();

        }


    }
}
