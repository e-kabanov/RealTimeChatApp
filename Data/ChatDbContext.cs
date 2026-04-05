using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext (DbContextOptions<ChatDbContext> options) : base(options)
        { 

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Сохраняем сообщения при удалении пользователя
            modelBuilder.Entity<Message>()
              .HasOne(m => m.User)              
              .WithMany(u => u.Messages)        
              .HasForeignKey(m => m.UserId)
              .OnDelete(DeleteBehavior.NoAction);


            // Удаляем сообщения при удалении комнаты
            modelBuilder.Entity<Message>()
              .HasOne(m => m.Room)              
              .WithMany(r => r.Messages)        
              .HasForeignKey(m => m.RoomId)
              .OnDelete(DeleteBehavior.Cascade);

            // Удаляем связь при удалении пользователя
            modelBuilder.Entity<RoomUser>()
              .HasOne(ru => ru.User)
              .WithMany(u => u.RoomUsers)
              .HasForeignKey(ru => ru.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            // Удаляем связь при удалении комнаты
            modelBuilder.Entity<RoomUser>()
              .HasOne(ru => ru.Room)
              .WithMany(r => r.RoomUsers)
              .HasForeignKey(ru => ru.RoomId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Room>()
              .HasOne(r => r.CreatedBy)
              .WithMany(u => u.CreatedRooms)
              .HasForeignKey(r => r.CreatedById)
              .OnDelete(DeleteBehavior.NoAction);


            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<RoomUser> RoomUsers { get; set; }
        
    }
}
