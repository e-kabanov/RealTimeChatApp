using Microsoft.AspNetCore.Mvc;

namespace RealTimeChatApp.DTOs
{
    public class CreateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxParticipants { get; set; }
    }
}
