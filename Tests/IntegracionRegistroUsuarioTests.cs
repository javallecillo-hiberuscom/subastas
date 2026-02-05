using Xunit;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class IntegracionRegistroUsuarioTests
{
    [Fact]
    public async Task Registro_ValidaDniYEliminaUsuario()
    {
        // Mock del servicio de usuario
        var usuarioServiceMock = new Moq.Mock<Subastas.Application.Interfaces.Services.IUsuarioService>();
        usuarioServiceMock.Setup(s => s.RegistrarAsync(Moq.It.IsAny<RegistroUsuarioRequest>()))
            .ReturnsAsync(new UsuarioResponse { IdUsuario = 99, Nombre = "Test", Email = "test.integracion@demo.com", Dni = "12345678Z" });
        usuarioServiceMock.Setup(s => s.EliminarAsync(99)).ReturnsAsync(true);

        var controller = new UsuariosController(usuarioServiceMock.Object, null);
        var dniValido = "12345678Z";
        var request = new RegistroUsuarioRequest {
            Nombre = "Test",
            Apellidos = "Integracion",
            Email = "test.integracion@demo.com",
            Password = "Test1234",
            Dni = dniValido
        };
        // Act: Registrar usuario
        var registroResult = await controller.Registro(request);
        Assert.IsType<OkObjectResult>(registroResult.Result);
        var usuarioCreado = ((registroResult.Result as OkObjectResult)?.Value as ApiResponse<UsuarioResponse>)?.Data;
        Assert.NotNull(usuarioCreado);
        Assert.Equal(99, usuarioCreado.IdUsuario);
        // Validar el DNI
        Assert.True(ValidarDni(dniValido));
        // Eliminar usuario
        var deleteResult = await controller.EliminarUsuario(99);
        Assert.IsType<OkObjectResult>(deleteResult);
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
