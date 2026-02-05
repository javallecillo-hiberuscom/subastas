using Microsoft.AspNetCore.SignalR;
using Subastas.Application.Interfaces.Services;
using Subastas.WebApi.Hubs;
using System.Threading.Tasks;

namespace Subastas.WebApi.Services
{
    public class NotificationHubService : INotificationHub
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUser(int userId, object notification)
        {
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAdmins(object notification)
        {
            await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        }
    }
}