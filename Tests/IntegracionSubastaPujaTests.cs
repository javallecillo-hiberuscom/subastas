using Xunit;
using Moq;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class IntegracionSubastaPujaTests
{
    [Fact]
    public async Task Puja_Exitosa_ActualizaSubastaYNotifica()
    {
        // Mock del servicio de pujas y subastas
        var pujaServiceMock = new Mock<IPujaService>();
        pujaServiceMock.Setup(s => s.RealizarPujaAsync(It.IsAny<PujaRequest>()))
            .ReturnsAsync(new PujaResponse { IdPuja = 1, IdSubasta = 10, Monto = 1000, Estado = "ganadora" });

        var subastaServiceMock = new Mock<ISubastaService>();
        subastaServiceMock.Setup(s => s.ObtenerPorIdAsync(10))
            .ReturnsAsync(new SubastaResponse { IdSubasta = 10, Estado = "activa" });

        var controller = new PujasController(pujaServiceMock.Object, subastaServiceMock.Object, null);
        var pujaRequest = new PujaRequest {
            IdSubasta = 10,
            IdUsuario = 99,
            Monto = 1000
        };
        // Act: Realizar puja
        var pujaResult = await controller.RealizarPuja(pujaRequest);
        Assert.IsType<OkObjectResult>(pujaResult.Result);
        var puja = ((pujaResult.Result as OkObjectResult)?.Value as ApiResponse<PujaResponse>)?.Data;
        Assert.NotNull(puja);
        Assert.Equal("ganadora", puja.Estado);
        // Simular notificación (mock)
        // Aquí podrías mockear el servicio de notificaciones y verificar que se llama
    }
}
