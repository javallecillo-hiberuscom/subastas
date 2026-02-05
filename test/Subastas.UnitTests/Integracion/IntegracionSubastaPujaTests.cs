using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Subastas.WebApi.Controllers;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Subastas.UnitTests.Integracion
{
    public class IntegracionSubastaPujaTests : IDisposable
    {
        private readonly Subastas.Infrastructure.Data.SubastaContext _context;
        private readonly Mock<Subastas.Application.Interfaces.Services.IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<Subastas.WebApi.Controllers.PujasController>> _loggerMock;
        private readonly Subastas.WebApi.Controllers.PujasController _controller;

        public IntegracionSubastaPujaTests()
        {
            var options = new DbContextOptionsBuilder<Subastas.Infrastructure.Data.SubastaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Subastas.Infrastructure.Data.SubastaContext(options);
            _emailServiceMock = new Mock<Subastas.Application.Interfaces.Services.IEmailService>();
            _loggerMock = new Mock<ILogger<Subastas.WebApi.Controllers.PujasController>>();
            _controller = new Subastas.WebApi.Controllers.PujasController(_context, _emailServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CrearPuja_ValidaReglasYActualizaPrecio()
        {
            // Arrange
            var vehiculo = new Subastas.Domain.Entities.Vehiculo 
            { 
                Marca = "Toyota", 
                Modelo = "Corolla", 
                Anio = 2020,
                Estado = "registrado",
                Matricula = "1234ABC",
                TipoCarroceria = "Sedan",
                TipoMotor = "Gasolina",
                Color = "Blanco",
                Kilometraje = 50000,
                Potencia = 150,
                Descripcion = "Vehículo de prueba",
                NumeroPuertas = 4,
                FechaMatriculacion = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1).Date),
                FechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow.Date)
            };
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            var subasta = new Subastas.Domain.Entities.Subasta
            {
                IdVehiculo = vehiculo.IdVehiculo,
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow.AddDays(1),
                PrecioInicial = 10000,
                IncrementoMinimo = 500,
                Estado = "activa"
            };
            _context.Subastas.Add(subasta);
            await _context.SaveChangesAsync();

            var usuario = new Subastas.Domain.Entities.Usuario
            {
                Nombre = "Juan",
                Apellidos = "Pérez",
                Email = "juan@example.com",
                Password = "hashed",
                Rol = "registrado",
                Activo = 1,
                Validado = 1,
                DocumentoIAE = "doc123"
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var request = new Subastas.WebApi.Controllers.PujaRequest
            {
                IdSubasta = subasta.IdSubasta,
                IdUsuario = usuario.IdUsuario,
                Cantidad = 10500,
                FechaPuja = DateTime.UtcNow
            };

            // Act
            var result = await _controller.PostPuja(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedSubasta = await _context.Subastas.FindAsync(subasta.IdSubasta);
            Assert.Equal(10500, updatedSubasta.PrecioActual);
            var puja = await _context.Pujas.FirstOrDefaultAsync(p => p.IdSubasta == subasta.IdSubasta);
            Assert.NotNull(puja);
            Assert.Equal(10500, puja.Cantidad);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
