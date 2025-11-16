namespace RealTimeChatApp.DTOs
{
    public class RoomInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxParticipants { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
