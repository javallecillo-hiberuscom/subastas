using Xunit;
using Moq;
using Subastas.Infrastructure.Services;
using Subastas.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Application.DTOs.Requests;

namespace Subastas.UnitTests.Unit
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<INotificacionAdminService> _notificacionAdminServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<UsuarioService>> _loggerMock;
        private readonly UsuarioService _usuarioService;

        public UsuarioServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _authServiceMock = new Mock<IAuthService>();
            _notificacionAdminServiceMock = new Mock<INotificacionAdminService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<UsuarioService>>();
            _usuarioService = new UsuarioService(
                _usuarioRepositoryMock.Object,
                _passwordServiceMock.Object,
                _authServiceMock.Object,
                _notificacionAdminServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ActualizarPerfilAsync_UpdatesUser_WhenValid()
        {
            // Arrange
            var user = new Usuario { IdUsuario = 1, Nombre = "Old" };
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _usuarioRepositoryMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            var request = new ActualizarPerfilRequest { IdUsuario = 1, Nombre = "New", Apellidos = "Last", Email = "email@example.com", Rol = "registrado", Activo = true };

            // Act
            var result = await _usuarioService.ActualizarPerfilAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result.Nombre);
        }
    }
}