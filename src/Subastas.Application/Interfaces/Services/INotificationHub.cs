using System.Threading.Tasks;

namespace Subastas.Application.Interfaces.Services
{
    public interface INotificationHub
    {
        Task SendNotificationToUser(int userId, object notification);
        Task SendNotificationToAdmins(object notification);
    }
}