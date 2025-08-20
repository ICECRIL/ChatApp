using ChatApp.Server.Models;

namespace ChatApp.Server.Services
{
    
        public interface IMessageService
        {
            Task<Message> CreateMessageAsync(string userName, string content);
            Task<IEnumerable<Message>> GetRecentMessagesAsync(int count);
            Task DeleteMessageByIdAsync(Guid messageId);
            Task UpdateMessageByIdAsync(Guid id, string content);
            Task<Message> GetMessageByIdAsync(Guid id);
            Task DeleteAllMessagesAsync();
        }
    
}
