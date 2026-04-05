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
using RealTimeChatApp.Services;
using System.Security.Claims;

namespace RealTimeChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto dto) 
        {
            var userId = User.GetUserId();

            var result = await _messageService.SendMessage(dto, userId);

           return result.ToActionResult();
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoomMessages (int roomId)
        {
            var userId = User.GetUserId();
            var result = await _messageService.GetRoomMessages(roomId, userId);

            return result.ToActionResult();

            


        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId,  string content)  
        {
            var userId = User.GetUserId();
            var result = await _messageService.EditMessage(messageId,content, userId);

            return result.ToActionResult();

        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {

            var userId = User.GetUserId();
            var result = await _messageService.DeleteMessage(messageId, userId);

            return result.ToActionResult();
        }
    }
}
