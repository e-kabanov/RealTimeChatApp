using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Data;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Hubs;
using RealTimeChatApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Services
{
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessageService> _logger;

        public MessageService (ChatDbContext context, IHubContext<ChatHub> hubContext, ILogger<MessageService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<OperationResult<object>> DeleteMessage(int messageId, int userId)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null) { return OperationResult<object>.NotFound("Сообщение не найдено"); }

            var isRoomOwner = await _context.RoomUsers.AnyAsync(ru => ru.RoomId == message.RoomId && ru.UserId == userId && ru.Role == "owner");

            if (message.UserId != userId && !isRoomOwner) { return OperationResult<object>.Forbidden(); }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            var roomId = message.RoomId;
            await _hubContext.Clients.Group($"room_{roomId}").SendAsync("MessageDeleted", messageId);

            return OperationResult<object>.NoContent();

        }

        public async Task<OperationResult<object>> EditMessage(int messageId, string content, int userId)
        {
            if (string.IsNullOrWhiteSpace(content)) { return OperationResult<object>.BadRequest("Текст сообщения не может быть пустым"); }

            var message = await _context.Messages.FindAsync(messageId);

            if (message == null) { return OperationResult<object>.NotFound("Сообщение на найдено"); }

            if (message.UserId != userId) { return OperationResult<object>.Forbidden(); }

            message.Content = content;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"room_{message.RoomId}").SendAsync("MessageEdited", messageId, content);

            return OperationResult<object>.NoContent();

        }

        public async Task<OperationResult<List<MessageDto>>> GetRoomMessages(int roomId, int userId)
        {
            var isUserInRoom = await _context.RoomUsers
               .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

            var userInRoom = await HasRoomAccess(roomId, userId);

            if (!userInRoom) { return OperationResult<List<MessageDto>>.Forbidden(); }

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
                }).ToListAsync();

            return OperationResult<List<MessageDto>>.Ok(messages);
        }

        public async Task<OperationResult<MessageResponseDto>> SendMessage(SendMessageDto dto, int senderId)
        {   

            if (string.IsNullOrWhiteSpace(dto.Content)) { return OperationResult<MessageResponseDto>.BadRequest("Сообщение не может быть пустым"); }

            var userInRoom = await HasRoomAccess(dto.RoomId, senderId);

            if (!userInRoom) { return OperationResult<MessageResponseDto>.Forbidden(); }
                
            var message = new Message
            {
                UserId = senderId,
                RoomId = dto.RoomId,
                Content = dto.Content,
                TimeStamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(senderId);

            await _hubContext.Clients.Group($"room_{dto.RoomId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                userId = message.UserId,
                userName = user.UserName ?? "Unknown",
                content = message.Content,
                timeStamp = message.TimeStamp,
                roomId = message.RoomId,
                avatarUrl = user.AvatarUrl

            });

            var response = new MessageResponseDto
            {
                Id = message.Id,
                UserId = senderId,
                RoomId = message.RoomId,
                Content = message.Content,
                TimeStamp = message.TimeStamp,
                UserName = user.UserName,
                AvatarUrl = user.AvatarUrl

            };

            return OperationResult<MessageResponseDto>.Created(response);

        }

        private async Task<bool> HasRoomAccess(int roomId, int userId)
        {
           return await _context.RoomUsers.AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);
        }
    }
}
