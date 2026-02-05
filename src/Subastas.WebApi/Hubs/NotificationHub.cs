using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Subastas.WebApi.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        public async Task LeaveUserGroup(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        }

        // Methods to send notifications
        public async Task SendNotificationToUser(int userId, object notification)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAdmins(object notification)
        {
            await Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        }
    }
}