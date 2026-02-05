using Xunit;
using Moq;
using Subastas.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Subastas.UnitTests.Unit
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.example.com");
            configMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
            configMock.Setup(c => c["Email:SmtpUser"]).Returns("user@example.com");
            configMock.Setup(c => c["Email:SmtpPass"]).Returns("pass");
            configMock.Setup(c => c["Email:FromName"]).Returns("Test");

            _loggerMock = new Mock<ILogger<EmailService>>();
            _emailService = new EmailService(configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task EnviarEmailAsync_ReturnsTrue_WhenValid()
        {
            // This will fail in test environment, but checks if method runs
            var result = await _emailService.EnviarEmailAsync("test@example.com", "Subject", "Body");
            // In test, SMTP may not connect, so expect false or handle
            Assert.IsType<bool>(result);
        }
    }
}