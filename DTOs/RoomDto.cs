namespace RealTimeChatApp.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxParticipants { get; set; }
    }
}
