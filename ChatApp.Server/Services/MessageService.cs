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
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName), "Username cannot be empty");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content), "Message cannot be empty");
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == userName);
                if (user is null)
                {
                    user = new User { Name = userName };
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new user: {UserName}", userName);
                }

                var message = new Message
                {
                    Content = content,
                    Sender = user,
                    SenderId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Message created by {UserName}" +
                                       " with ID {MessageId}", userName, message.Id);
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create message for user {UserName}", userName);
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(int count)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.Sender)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(count)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve recent messages");
                throw;
            }
        }

        public async Task DeleteMessageByIdAsync(Guid id)
        {
            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                    throw new KeyNotFoundException($"Message with ID {id} not found");

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message with ID {MessageId} was deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete message with ID {MessageId}", id);
                throw;
            }
        }

        public async Task UpdateMessageByIdAsync(Guid id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content), "Message content cannot be empty");

            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                    throw new KeyNotFoundException($"Message with ID {id} not found");

                message.Content = content;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message with ID {MessageId} was updated", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update message with ID {MessageId}", id);
                throw;
            }
        }

        public async Task<Message> GetMessageByIdAsync(Guid id)
        {
            try
            {
                var message = await _context.Messages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (message == null)
                    throw new KeyNotFoundException($"Message with ID {id} not found");

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get message with ID {MessageId}", id);
                throw;
            }
        }

        public async Task DeleteAllMessagesAsync()
        {
            try
            {
                int countOfDeletedMessages = await _context.Messages.ExecuteDeleteAsync();
                _logger.LogInformation("Deleted {Count} messages", countOfDeletedMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all messages");
                throw;
            }
        }
    }
}
