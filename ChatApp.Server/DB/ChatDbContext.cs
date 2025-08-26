using ChatApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.DB
{
    public class ChatDbContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        { }
    }
}
