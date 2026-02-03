using Microsoft.Extensions.DependencyInjection;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Application.Interfaces.Services;
using Subastas.Infrastructure.Repositories;
using Subastas.Infrastructure.Services;

namespace Subastas.Infrastructure.Configuration;

/// <summary>
/// Clase de extensión para configurar la inyección de dependencias de Infrastructure.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios y repositorios de la capa Infrastructure.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Registrar repositorios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISubastaRepository, SubastaRepository>();
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddScoped<IPujaRepository, PujaRepository>();

        // Registrar servicios
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INotificacionAdminService, NotificacionAdminService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        return services;
    }
}
