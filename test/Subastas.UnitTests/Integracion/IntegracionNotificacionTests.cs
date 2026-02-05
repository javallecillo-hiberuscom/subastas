using Xunit;
using Microsoft.EntityFrameworkCore;
using Subastas.WebApi.Controllers;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;
using Moq;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Subastas.UnitTests.Integracion
{
    public class IntegracionNotificacionTests
    {
        [Fact]
        public async Task NotificacionAdmin_CreacionYLectura()
        {
            // Usar un contexto en memoria para pruebas
            var options = new DbContextOptionsBuilder<SubastaContext>()
                .UseInMemoryDatabase(databaseName: "Test_NotificacionesAdmin")
                .Options;
            var context = new SubastaContext(options);
            var logger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<NotificacionesAdminController>>().Object;
            var controller = new NotificacionesAdminController(context, logger);

            // Crear notificación
            var notificacion = new NotificacionAdmin
            {
                Titulo = "Test",
                Mensaje = "Mensaje de prueba",
                Tipo = "registro",
                IdUsuario = null
            };
            var crearResult = await controller.CrearNotificacion(notificacion);
            Assert.IsType<CreatedAtActionResult>(crearResult.Result);

            // Leer notificaciones
            var leerResult = await controller.GetNotificaciones();
            Assert.IsType<OkObjectResult>(leerResult.Result);
            var lista = (leerResult.Result as OkObjectResult)?.Value as System.Collections.Generic.List<NotificacionAdmin>;
            Assert.NotNull(lista);
            Assert.Single(lista);
            Assert.Equal("Test", lista[0].Titulo);
        }
    }
}
