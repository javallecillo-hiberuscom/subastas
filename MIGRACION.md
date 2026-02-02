# 🚀 Guía de Migración - Estructura Antigua a Clean Architecture

## 📋 Resumen de Cambios

Este documento explica cómo migrar del código antiguo a la nueva arquitectura limpia.

## 🔄 Mapeo de Archivos

### Modelos → Domain/Entities

| **Archivo Antiguo** | **Nuevo Archivo** |
|---------------------|-------------------|
| `Models/Usuario.cs` | `src/Subastas.Domain/Entities/Usuario.cs` |
| `Models/Empresa.cs` | `src/Subastas.Domain/Entities/Empresa.cs` |
| `Models/Vehiculo.cs` | `src/Subastas.Domain/Entities/Vehiculo.cs` |
| `Models/Subastum.cs` | `src/Subastas.Domain/Entities/Subasta.cs` |
| `Models/Puja.cs` | `src/Subastas.Domain/Entities/Puja.cs` |
| `Models/Notificacion.cs` | `src/Subastas.Domain/Entities/Notificacion.cs` |

**Cambios importantes:**
- ✅ Nombres de propiedades cambiados a **PascalCase** (ej: `idUsuario` → `IdUsuario`)
- ✅ Nombres de navegación mejorados (ej: `idEmpresaNavigation` → `Empresa`)
- ✅ Namespace: `subastas.Models` → `Subastas.Domain.Entities`

### DTOs

| **Archivo Antiguo** | **Nuevo Archivo** |
|---------------------|-------------------|
| `Models/LoginRequest.cs` | `src/Subastas.Application/DTOs/Requests/LoginRequest.cs` |
| `DTOs/ActualizarPerfilRequest.cs` | `src/Subastas.Application/DTOs/Requests/ActualizarPerfilRequest.cs` |
| - | `src/Subastas.Application/DTOs/Requests/RegistroUsuarioRequest.cs` *(nuevo)* |
| - | `src/Subastas.Application/DTOs/Responses/*` *(todos nuevos)* |

**Cambios importantes:**
- ✅ Separación clara entre Requests y Responses
- ✅ Validaciones con DataAnnotations
- ✅ DTO genérico `ApiResponse<T>` para respuestas consistentes

### Data → Infrastructure/Data

| **Archivo Antiguo** | **Nuevo Archivo** |
|---------------------|-------------------|
| `Data/SubastaContext.cs` | `src/Subastas.Infrastructure/Data/SubastaContext.cs` |
| `Migrations/*` | `src/Subastas.Infrastructure/Data/Migrations/*` |

**Cambios importantes:**
- ✅ Namespace: `subastas.Data` → `Subastas.Infrastructure.Data`
- ✅ DbSets renombrados (ej: `Subasta` → `Subastas`)
- ✅ Configuración de navegación actualizada

### Services → Infrastructure/Services

| **Archivo Antiguo** | **Nuevo Archivo** |
|---------------------|-------------------|
| `Services/PasswordService.cs` | `src/Subastas.Infrastructure/Services/PasswordService.cs` |
| `Services/NotificacionAdminService.cs` | `src/Subastas.Infrastructure/Services/NotificacionAdminService.cs` |
| - | `src/Subastas.Infrastructure/Services/AuthService.cs` *(nuevo)* |

**Cambios importantes:**
- ✅ Implementan interfaces de `Application/Interfaces/Services`
- ✅ Namespace: `subastas.Services` → `Subastas.Infrastructure.Services`
- ✅ Inyección de dependencias obligatoria

### Controllers → WebApi/Controllers

| **Archivo Antiguo** | **Nuevo Archivo** |
|---------------------|-------------------|
| `Controllers/UsuariosController.cs` | `src/Subastas.WebApi/Controllers/UsuariosController.cs` |
| `Controllers/SubastasController.cs` | *(pendiente de migrar)* |
| `Controllers/VehiculosController.cs` | *(pendiente de migrar)* |
| `Controllers/PujasController.cs` | *(pendiente de migrar)* |

**Cambios importantes:**
- ✅ Usan repositorios en lugar de DbContext directamente
- ✅ Devuelven `ApiResponse<T>` consistente
- ✅ Logging integrado
- ✅ Manejo de errores mejorado

## 📝 Guía de Migración Paso a Paso

### Paso 1: Actualizar Referencias de Entidades

**Antes:**
```csharp
using subastas.Models;

var usuario = new Usuario
{
    idUsuario = 1,
    nombre = "Juan",
    email = "juan@email.com"
};
```

**Después:**
```csharp
using Subastas.Domain.Entities;

var usuario = new Usuario
{
    IdUsuario = 1,
    Nombre = "Juan",
    Email = "juan@email.com"
};
```

### Paso 2: Usar Repositorios en lugar de DbContext

**Antes:**
```csharp
public class UsuariosController : ControllerBase
{
    private readonly SubastaContext _context;
    
    public async Task<Usuario?> GetUsuario(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }
}
```

**Después:**
```csharp
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> GetUsuario(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound(ApiResponse<UsuarioResponse>.ErrorResult("Usuario no encontrado"));
            
        var response = MapToResponse(usuario);
        return Ok(ApiResponse<UsuarioResponse>.SuccessResult(response));
    }
}
```

### Paso 3: Usar DTOs de Respuesta

**Antes:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Usuario>> GetUsuario(int id)
{
    var usuario = await _context.Usuarios.FindAsync(id);
    return usuario; // ❌ Devuelve entidad directamente
}
```

**Después:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<UsuarioResponse>>> GetUsuario(int id)
{
    var usuario = await _usuarioRepository.GetByIdAsync(id);
    if (usuario == null)
        return NotFound(ApiResponse<UsuarioResponse>.ErrorResult("No encontrado"));
    
    var response = new UsuarioResponse
    {
        IdUsuario = usuario.IdUsuario,
        Nombre = usuario.Nombre,
        Email = usuario.Email
        // ... mapear propiedades
    };
    
    return Ok(ApiResponse<UsuarioResponse>.SuccessResult(response));
}
```

### Paso 4: Actualizar Inyección de Dependencias

**Antes (Program.cs):**
```csharp
builder.Services.AddScoped<PasswordService>();
```

**Después (Program.cs):**
```csharp
// Ya no se hace manualmente, se usa el método de extensión:
builder.Services.AddInfrastructure();
```

## 🔍 Cambios en Nombres de Propiedades

### Usuario
- `idUsuario` → `IdUsuario`
- `nombre` → `Nombre`
- `apellidos` → `Apellidos`
- `email` → `Email`
- `password` → `Password`
- `activo` → `Activo`
- `validado` → `Validado`
- `idEmpresa` → `IdEmpresa`
- `telefono` → `Telefono`
- `direccion` → `Direccion`
- `fotoPerfilBase64` → `FotoPerfilBase64`
- `documentoIAE` → `DocumentoIAE`

### Subasta
- `idSubasta` → `IdSubasta`
- `idVehiculo` → `IdVehiculo`
- `fechaInicio` → `FechaInicio`
- `fechaFin` → `FechaFin`
- `precioInicial` → `PrecioInicial`
- `incrementoMinimo` → `IncrementoMinimo`
- `precioActual` → `PrecioActual`
- `estado` → `Estado`

### Navegación
- `idEmpresaNavigation` → `Empresa`
- `idVehiculoNavigation` → `Vehiculo`
- `idUsuarioNavigation` → `Usuario`
- `idSubastaNavigation` → `Subasta`

## ⚙️ Configuración de Base de Datos

### Migración de Migraciones

Las migraciones existentes deben ser recreadas para la nueva estructura:

```bash
# 1. Eliminar carpeta Migrations antigua
# 2. Crear migración inicial en nuevo proyecto
cd src/Subastas.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Subastas.WebApi
dotnet ef database update --startup-project ../Subastas.WebApi
```

## 🧪 Testing

### Estructura de Tests

**Antes:**
```
Tests/
  ├── UsuariosControllerTests.cs
  └── VehiculosControllerTests.cs
```

**Después:**
```
test/
  └── Subastas.UnitTests/
      ├── Controllers/
      │   ├── UsuariosControllerTests.cs
      │   └── VehiculosControllerTests.cs
      └── Services/
          ├── PasswordServiceTests.cs
          └── AuthServiceTests.cs
```

## 📊 Ventajas de la Nueva Arquitectura

### ✅ Ventajas Técnicas
1. **Separación de Responsabilidades:** Cada capa tiene un propósito claro
2. **Testabilidad:** Fácil crear mocks de interfaces
3. **Mantenibilidad:** Código más organizado y fácil de navegar
4. **Escalabilidad:** Fácil añadir nuevas funcionalidades
5. **Reutilización:** DTOs y servicios reutilizables

### ✅ Ventajas de Negocio
1. **Menor deuda técnica**
2. **Desarrollo más rápido** a largo plazo
3. **Menos bugs** por mejor estructura
4. **Onboarding más fácil** para nuevos desarrolladores
5. **Preparado para microservicios** si fuera necesario

## 🎯 Checklist de Migración

- [x] Crear estructura de carpetas src/ y test/
- [x] Crear proyectos .csproj para cada capa
- [x] Migrar entidades a Domain
- [x] Crear DTOs en Application
- [x] Crear interfaces de repositorios y servicios
- [x] Implementar repositorios en Infrastructure
- [x] Implementar servicios en Infrastructure
- [x] Migrar DbContext a Infrastructure
- [x] Crear Program.cs en WebApi
- [x] Migrar UsuariosController
- [ ] Migrar SubastasController
- [ ] Migrar VehiculosController
- [ ] Migrar PujasController
- [ ] Migrar NotificacionesController
- [ ] Actualizar frontend Angular para usar nuevos endpoints
- [ ] Escribir pruebas unitarias
- [ ] Documentar API con Swagger

## 💡 Consejos

1. **Migrar gradualmente:** No es necesario migrar todo de una vez
2. **Mantener compatibilidad:** La API antigua puede coexistir temporalmente
3. **Usar logging:** Añadir logs durante la migración para detectar problemas
4. **Revisar configuración:** Verificar appsettings.json y variables de entorno
5. **Comunicar cambios:** Informar al equipo de frontend sobre cambios en la API

## 🆘 Solución de Problemas Comunes

### Error: "No se encuentra la tabla Usuario"
**Causa:** Las migraciones no se han aplicado  
**Solución:**
```bash
cd src/Subastas.WebApi
dotnet ef database update
```

### Error: "Cannot resolve IUsuarioRepository"
**Causa:** Falta registrar el servicio  
**Solución:** Asegurar que `builder.Services.AddInfrastructure()` está en Program.cs

### Error: "Unauthorized" en Swagger
**Causa:** Falta token JWT  
**Solución:** Hacer login primero y usar el botón "Authorize" en Swagger

---

**Nota:** Para cualquier duda, consultar [ARQUITECTURA.md](ARQUITECTURA.md)
