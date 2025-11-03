using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Models
{
    public class RoomUser
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string? Role { get; set; }

        public Room? Room { get; set; }
        public User? User { get; set; }
    }
}
