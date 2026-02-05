using Xunit;
using Moq;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Subastas.UnitTests.Integracion
{
    public class IntegracionRegistroUsuarioTests
    {
        [Fact]
        public async Task Registro_UsuarioYEliminaUsuario()
        {
            var usuarioServiceMock = new Mock<Subastas.Application.Interfaces.Services.IUsuarioService>();
            usuarioServiceMock.Setup(s => s.RegistrarAsync(It.IsAny<Subastas.Application.DTOs.Requests.RegistroUsuarioRequest>()))
                .ReturnsAsync(new Subastas.Application.DTOs.Responses.UsuarioResponse { IdUsuario = 99, Nombre = "Test", Email = "test.integracion@demo.com" });
            usuarioServiceMock.Setup(s => s.EliminarUsuarioAsync(99)).Returns(Task.CompletedTask);

            var logger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<Subastas.WebApi.Controllers.UsuariosController>>().Object;
            var controller = new Subastas.WebApi.Controllers.UsuariosController(usuarioServiceMock.Object, logger);

            var request = new Subastas.Application.DTOs.Requests.RegistroUsuarioRequest {
                Nombre = "Test",
                Apellidos = "Integracion",
                Email = "test.integracion@demo.com",
                Password = "Test1234"
            };
            var registroResult = await controller.Registro(request);
            Assert.IsType<OkObjectResult>(registroResult.Result);
            var usuarioCreado = ((registroResult.Result as OkObjectResult)?.Value as Subastas.Application.DTOs.Responses.ApiResponse<Subastas.Application.DTOs.Responses.UsuarioResponse>)?.Data;
            Assert.NotNull(usuarioCreado);
            Assert.Equal(99, usuarioCreado.IdUsuario);
            Assert.Equal("Test", usuarioCreado.Nombre);

            // Eliminar usuario
            var deleteResult = await controller.EliminarUsuario(usuarioCreado.IdUsuario);
            Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.ActionResult<Subastas.Application.DTOs.Responses.ApiResponse<object>>>(deleteResult);
        }

        private bool ValidarDni(string dni)
        {
            var letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dni, "^\\d{8}[A-Z]$")) return false;
            var numero = int.Parse(dni.Substring(0, 8));
            var letra = dni[8];
            return letra == letras[numero % 23];
        }
    }
}
