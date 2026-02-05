using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Subastas.WebApi.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Method to send notification to a specific user
        public async Task SendNotificationToUser(int userId, object notification)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        // Method to send notification to admins
        public async Task SendNotificationToAdmins(object notification)
        {
            await Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        }
    }
}