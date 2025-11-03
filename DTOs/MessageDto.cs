namespace RealTimeChatApp.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public string Type { get; set; } = "text";
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
