using Xunit;
using Moq;
using Subastas.WebApi.Services;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Subastas.WebApi.Hubs;

namespace Subastas.UnitTests.Unit
{
    public class NotificationHubServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly NotificationHubService _service;

        public NotificationHubServiceTests()
        {
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            _service = new NotificationHubService(_hubContextMock.Object);
        }

        [Fact]
        public async Task SendNotificationToUser_CallsHub()
        {
            // Arrange
            var clientsMock = new Mock<IHubClients>();
            var groupMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.Group("user_1")).Returns(groupMock.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            // Act
            await _service.SendNotificationToUser(1, new { Message = "Test" });

            // Assert
            clientsMock.Verify(c => c.Group("user_1"), Times.Once);
        }
    }
}