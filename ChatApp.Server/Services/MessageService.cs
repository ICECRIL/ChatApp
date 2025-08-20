using ChatApp.Server.DB;
using ChatApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Services
{
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ChatDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message> CreateMessageAsync(string userName, string content)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException($"UserName and Content are required. " +
                                                $"Wrong data provided: {userName},{content}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == userName);
            if (user is null)
            {
                user = new User { Name = userName };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var message = new Message
            {
                Content = content,
                Sender = user,
                SenderId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(int count)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task DeleteMessageByIdAsync(Guid id)
        {
            var message = _context.Messages.FindAsync(id);
            _context.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMessageByIdAsync(Guid id, string content)
        {
            var message = await _context.Messages.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Message with ID: {id} not found");

            message.Content = content;
            await _context.SaveChangesAsync();
        }

        public async Task<Message> GetMessageByIdAsync(Guid id)
        {
            var message = await _context.Messages.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Message with ID: {id} not found");

            return message;
        }

        public async Task DeleteAllMessagesAsync()
        {
            await _context.Messages.ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
    }
}
