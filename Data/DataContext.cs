using DatinApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatinApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Like>()
                        .HasKey(k => new { k.LikerId, k.LikeeId });

            modelBuilder.Entity<Like>()
                        .HasOne(l => l.Likee)
                        .WithMany(l => l.Likers)
                        .HasForeignKey(l => l.LikeeId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                        .HasOne(l => l.Liker)
                        .WithMany(l => l.Likees)
                        .HasForeignKey(l => l.LikerId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                        .HasOne(m => m.Sender)
                        .WithMany(m => m.MessagesSent)
                        .HasForeignKey(m => m.SenderId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                        .HasOne(m => m.Recipient)
                        .WithMany(m => m.MessagesReceived)
                        .HasForeignKey(m => m.RecipientId)
                        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
