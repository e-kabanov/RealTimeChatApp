namespace RealTimeChatApp.DTOs
{
    public class SendMessageDto
    {
        public int RoomId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
