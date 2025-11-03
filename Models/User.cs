using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string? UserName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeen {  get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; }  
        public string? PasswordHash { get; set; }

        public ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Room> CreatedRooms { get; set; } = new List<Room>();
    }
}
