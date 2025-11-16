namespace RealTimeChatApp.DTOs
{
    public class RoomParticipantDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
