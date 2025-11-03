using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public string? Content { get; set; } = null!;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Type { get; set; } = "text";

        public User? User { get; set; }
        public Room? Room { get; set; }

    }
}
