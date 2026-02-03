# 📝 Logging Configuration Guide

## Logging Actual (Application Insights + Console)

El proyecto ya está configurado con logging a través de `ILogger<T>` que se envía a:
- **Console** - Para desarrollo local
- **Application Insights** - Para producción en Azure

## Uso de Logging en el Código

```csharp
public class MiServicio
{
    private readonly ILogger<MiServicio> _logger;

    public MiServicio(ILogger<MiServicio> logger)
    {
        _logger = logger;
    }

    public void MiMetodo()
    {
        // Información
        _logger.LogInformation("Usuario {UserId} realizó acción {Action}", userId, action);

        // Advertencia
        _logger.LogWarning("Operación tardó {ElapsedMs}ms", elapsed);

        // Error
        _logger.LogError(ex, "Error procesando {EntityType} con ID {EntityId}", "Usuario", id);

        // Crítico
        _logger.LogCritical("Fallo crítico en {Component}", "Database");
    }
}
```

## Ver Logs en Azure

### 1. Logs en Tiempo Real (Stream)
```bash
# Habilitar logs
az webapp log config --name SubastasWebApi20260202162157 --resource-group Curso --application-logging filesystem --level information

# Ver logs en tiempo real
az webapp log tail --name SubastasWebApi20260202162157 --resource-group Curso
```

### 2. Application Insights

Los logs se envían automáticamente a Application Insights en producción. Para verlos:

1. Azure Portal → Resource Group "Curso"
2. Application Insights resource
3. Logs → Query:

```kusto
traces
| where timestamp > ago(1h)
| order by timestamp desc
| project timestamp, message, severityLevel, customDimensions
```

### 3. Logs Descargables
```bash
# Descargar últimos logs
az webapp log download --name SubastasWebApi20260202162157 --resource-group Curso --log-file logs.zip
```

## Logging Estructurado Recomendado

Para un logging más avanzado tipo Log4net, considerar **Serilog**:

### Instalación
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.ApplicationInsights
```

### Configuración en Program.cs
```csharp
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SubastasAPI")
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Usar Serilog
builder.Host.UseSerilog();
```

### Uso de Serilog
```csharp
_logger.Information("Usuario {UserId} inició sesión desde {IpAddress}", userId, ip);
_logger.Warning("Puja de {Amount:C} está cerca del límite", amount);
_logger.Error(ex, "Fallo al procesar puja {PujaId}", pujaId);
```

## Queries Útiles de Application Insights

### Errores de la última hora
```kusto
exceptions
| where timestamp > ago(1h)
| project timestamp, type, outerMessage, innermostMessage
| order by timestamp desc
```

### Requests más lentos
```kusto
requests
| where timestamp > ago(1h)
| where duration > 1000
| order by duration desc
| project timestamp, name, duration, resultCode
```

### Trace de un usuario específico
```kusto
traces
| where customDimensions.UserId == "2"
| order by timestamp desc
```

## Buenas Prácticas

1. **Usar niveles apropiados**:
   - `LogTrace`: Debugging muy detallado
   - `LogDebug`: Información de desarrollo
   - `LogInformation`: Eventos normales
   - `LogWarning`: Situaciones anormales pero recuperables
   - `LogError`: Errores que impiden una operación
   - `LogCritical`: Fallos del sistema

2. **Logging estructurado**:
   ```csharp
   // ❌ No hacer
   _logger.LogInformation($"Usuario {userId} creó subasta {subastaId}");
   
   // ✅ Hacer
   _logger.LogInformation("Usuario {UserId} creó subasta {SubastaId}", userId, subastaId);
   ```

3. **No logear información sensible**:
   ```csharp
   // ❌ No logear passwords, tokens, etc.
   _logger.LogInformation("Login: {Email} - {Password}", email, password);
   
   // ✅ Logear solo lo necesario
   _logger.LogInformation("Login exitoso para {Email}", email);
   ```
