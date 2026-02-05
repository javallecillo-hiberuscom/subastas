using Xunit;
using Moq;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class IntegracionNotificacionTests
{
    [Fact]
    public async Task NotificacionAdmin_CreacionYLectura()
    {
        // Mock del servicio de notificaciones admin
        var notificacionAdminServiceMock = new Mock<INotificacionAdminService>();
        notificacionAdminServiceMock.Setup(s => s.CrearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(new NotificacionAdminResponse { IdNotificacion = 1, Titulo = "Test", Mensaje = "Mensaje de prueba", Tipo = "registro", Leida = 0 });
        notificacionAdminServiceMock.Setup(s => s.ObtenerTodasAsync())
            .ReturnsAsync(new[] { new NotificacionAdminResponse { IdNotificacion = 1, Titulo = "Test", Mensaje = "Mensaje de prueba", Tipo = "registro", Leida = 0 } });

        var controller = new NotificacionesAdminController(notificacionAdminServiceMock.Object);
        // Act: Crear notificación
        var crearResult = await controller.Crear("Test", "Mensaje de prueba", "registro", null);
        Assert.IsType<OkObjectResult>(crearResult.Result);
        // Act: Leer notificaciones
        var leerResult = await controller.GetAll();
        Assert.IsType<OkObjectResult>(leerResult.Result);
        var notificaciones = ((leerResult.Result as OkObjectResult)?.Value as ApiResponse<NotificacionAdminResponse[]>)?.Data;
        Assert.NotNull(notificaciones);
        Assert.Single(notificaciones);
    }
}
