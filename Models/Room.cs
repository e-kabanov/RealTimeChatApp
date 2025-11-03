using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Models
{
    public class Room
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; } 
        public string? Description { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxParticipants { get; set; }

        public User? CreatedBy { get; set; }
        public ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();


    }
}
