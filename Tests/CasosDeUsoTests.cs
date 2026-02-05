using Xunit;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

public class CasosDeUsoTests
{
    [Fact]
    public async Task RegistroUsuario_Valido_CreaUsuarioPendiente()
    {
        // Arrange
        var controller = new UsuariosController(null, null); // Mockear servicios reales
        var request = new RegistroUsuarioRequest {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            Password = "Test1234",
            Dni = "12345678Z"
        };
        // Act
        var result = await controller.Registro(request);
        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        // Aquí se puede comprobar el estado pendiente, etc.
    }

    [Fact]
    public async Task LoginUsuario_Valido_DevuelveJwt()
    {
        // Arrange
        var controller = new UsuariosController(null, null);
        var request = new LoginRequest {
            Email = "juan@test.com",
            Password = "Test1234"
        };
        // Act
        var result = await controller.Login(request);
        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        // Comprobar que el JWT está presente en la respuesta
    }

    // Más pruebas unitarias e integración para los casos de uso principales...
}
