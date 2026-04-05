using Microsoft.AspNetCore.Mvc;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Services
{
    public interface IMessageService
    {
        Task<OperationResult<MessageResponseDto>> SendMessage(SendMessageDto dto, int senderId);
        Task<OperationResult<List<MessageDto>>> GetRoomMessages(int roomId, int userId);
        Task<OperationResult<object>> EditMessage(int messageId, string content, int userId);
        Task<OperationResult<object>> DeleteMessage(int messageId, int userId);
    }
}
