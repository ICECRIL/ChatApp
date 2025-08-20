using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.DB
{
    public class ChatDbContext : DbContext
    {

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }
    }
}
