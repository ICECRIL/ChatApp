using ChatApp.Server.DTOs;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }
        public async Task SendMessage(string userName, string content)
        {
            var message = await _messageService.CreateMessageAsync(userName, content);

            var messageDto = new MessageDto
            {
                Content = content,
                UserName = userName,
                CreatedAt = message.CreatedAt
            };

            await Clients.All.SendAsync("ReceiveMessage", messageDto);
        }

        public async Task GetHistory()
        {
            var messages = await _messageService.GetRecentMessagesAsync(50);
            var dto = messages.Select(m => new MessageDto
            {
                UserName = m.Sender.Name,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            });

            await Clients.Caller.SendAsync("ReceiveHistory", dto);
        }
    }
}
