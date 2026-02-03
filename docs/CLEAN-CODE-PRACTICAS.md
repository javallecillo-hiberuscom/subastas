# Clean Code y Buenas Prácticas - Sistema de Subastas

## 📐 Principios SOLID Aplicados

### 1. Single Responsibility Principle (SRP)
Cada clase tiene una única responsabilidad bien definida:

**✅ Ejemplo Correcto:**
```csharp
// ✓ UsuarioRepository solo maneja persistencia de Usuario
public class UsuarioRepository : IUsuarioRepository
{
    public async Task<Usuario?> GetByIdAsync(int id) { }
    public async Task<Usuario?> GetByEmailAsync(string email) { }
    public async Task AddAsync(Usuario usuario) { }
}

// ✓ UsuarioService solo maneja lógica de negocio de Usuario
public class UsuarioService : IUsuarioService
{
    public async Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request) { }
    public async Task<LoginResponse> LoginAsync(LoginRequest request) { }
}
```

### 2. Open/Closed Principle (OCP)
El código está abierto para extensión pero cerrado para modificación:

**✅ Uso de Interfaces:**
```csharp
// Interfaz genérica que permite nuevas implementaciones
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

// Fácil extender para repositorios específicos
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
}
```

### 3. Liskov Substitution Principle (LSP)
Las clases derivadas son sustituibles por sus clases base:

**✅ Repository Pattern:**
```csharp
// Clase base genérica
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly SubastaContext _context;
    // Implementación genérica
}

// Especialización que cumple el contrato
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    // Añade funcionalidad específica sin romper el contrato base
}
```

### 4. Interface Segregation Principle (ISP)
Interfaces específicas y cohesivas en lugar de una interfaz "gorda":

**✅ Interfaces Segregadas:**
```csharp
// ✓ Interfaces específicas por funcionalidad
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IAuthService
{
    string GenerateToken(Usuario usuario);
    ClaimsPrincipal ValidateToken(string token);
}

// ✗ Evitamos interfaces monolíticas
public interface ISecurityService // ❌ Demasiado amplio
{
    string HashPassword(string password);
    string GenerateToken(Usuario usuario);
    void SendEmail(string to, string subject);
    bool ValidateDocument(string path);
}
```

### 5. Dependency Inversion Principle (DIP)
Las capas superiores dependen de abstracciones, no de implementaciones:

**✅ Inyección de Dependencias:**
```csharp
// ✓ Controller depende de abstracción (IUsuarioService)
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService; // Abstracción
    
    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService; // Inyectado por DI container
    }
}

// Configuración en Program.cs
services.AddScoped<IUsuarioService, UsuarioService>();
```

---

## 🏗️ Arquitectura Limpia (Clean Architecture)

### Separación en Capas

```
┌─────────────────────────────────────┐
│   PRESENTATION (WebApi)             │  ← Controladores HTTP
│   Depends on: Application           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   APPLICATION (Services, DTOs)      │  ← Lógica de Aplicación
│   Depends on: Domain                │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   DOMAIN (Entities, Interfaces)     │  ← Núcleo de Negocio
│   Depends on: Nothing (!)           │  ← Sin dependencias externas
└───────────────────────────────────────┘
               ▲
┌──────────────┴──────────────────────┐
│   INFRASTRUCTURE (EF, Repos)        │  ← Detalles de Implementación
│   Depends on: Domain, Application   │
└─────────────────────────────────────┘
```

### Reglas de Dependencia

**✅ Permitido:**
- WebApi → Application → Domain
- Infrastructure → Domain
- Infrastructure → Application

**❌ Prohibido:**
- Domain → cualquier otra capa
- Application → Infrastructure
- Application → WebApi

---

## 🎯 Patrones de Diseño Implementados

### 1. Repository Pattern
**Propósito:** Abstraer el acceso a datos

```csharp
// Interfaz en Domain/Application
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
}

// Implementación en Infrastructure
public class UsuarioRepository : IUsuarioRepository
{
    private readonly SubastaContext _context;
    
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }
}
```

### 2. Dependency Injection (DI)
**Propósito:** Inversión de control para bajo acoplamiento

```csharp
// Registro en Infrastructure/Configuration/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        return services;
    }
}
```

### 3. DTO Pattern
**Propósito:** Separar modelos de dominio de modelos de transferencia

```csharp
// Entidad de dominio (no se expone directamente)
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Password { get; set; } // ¡Nunca se devuelve!
    // ...
}

// DTO de respuesta (lo que ve el cliente)
public class UsuarioResponse
{
    public int IdUsuario { get; set; }
    public string Email { get; set; }
    // Sin Password ✓
}
```

### 4. Service Layer Pattern
**Propósito:** Encapsular lógica de negocio

```csharp
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAuthService _authService;
    
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 1. Validación
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (usuario == null) throw new Exception("Usuario no encontrado");
        
        // 2. Lógica de negocio
        if (!_passwordService.VerifyPassword(request.Password, usuario.Password))
            throw new Exception("Contraseña incorrecta");
        
        // 3. Generación de respuesta
        var token = _authService.GenerateToken(usuario);
        return new LoginResponse { Token = token };
    }
}
```

---

## 📝 Convenciones de Código

### Nomenclatura

**Backend (.NET):**
- **PascalCase** para: clases, propiedades, métodos públicos
- **camelCase** para: variables locales, parámetros
- **_camelCase** para: campos privados

```csharp
public class UsuarioService // PascalCase
{
    private readonly IUsuarioRepository _usuarioRepository; // _camelCase
    
    public async Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request) // PascalCase
    {
        var usuario = new Usuario(); // camelCase local
        return new UsuarioResponse();
    }
}
```

**Frontend (Angular/TypeScript):**
- **PascalCase** para: clases, interfaces, tipos
- **camelCase** para: variables, funciones, propiedades
- **UPPER_SNAKE_CASE** para: constantes

```typescript
export interface UsuarioResponse { } // PascalCase
export class UsuarioService { } // PascalCase

const API_BASE_URL = 'https://...'; // UPPER_SNAKE_CASE

getCurrentUser() { // camelCase
    const userId = 123; // camelCase
}
```

### Comentarios y Documentación

**✅ Usar XML Documentation en C#:**
```csharp
/// <summary>
/// Valida un usuario cambiando su estado a validado.
/// </summary>
/// <param name="id">ID del usuario a validar.</param>
/// <returns>Usuario actualizado.</returns>
/// <exception cref="NotFoundException">Si el usuario no existe.</exception>
public async Task<UsuarioResponse> ValidarUsuarioAsync(int id)
{
    // Comentarios inline solo para lógica compleja
}
```

**✅ JSDoc en TypeScript:**
```typescript
/**
 * Realiza una puja en una subasta activa
 * @param pujaRequest - Datos de la puja
 * @returns Observable con la respuesta del servidor
 */
realizarPuja(pujaRequest: PujaRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/pujas`, pujaRequest);
}
```

---

## ✅ Buenas Prácticas Aplicadas

### 1. Manejo de Errores Consistente

**Backend:**
```csharp
// Respuesta consistente con ApiResponse<T>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

// Uso en controllers
return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
    usuario, "Usuario creado correctamente"));
```

**Frontend:**
```typescript
// Manejo de errores en servicios
realizarPuja(request: PujaRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/pujas`, request)
        .pipe(
            catchError(error => {
                this.toast.error(error.error.message || 'Error al realizar puja');
                return throwError(() => error);
            })
        );
}
```

### 2. Validación en Múltiples Capas

**Frontend (UX):**
```typescript
// Validación reactiva con Angular Forms
const form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)])
});
```

**Backend (Seguridad):**
```csharp
// Validación de reglas de negocio
if (usuario.Rol == "Administrador")
    return BadRequest("Los administradores no pueden pujar");

if (!usuario.Validado)
    return BadRequest("Cuenta no validada");
```

### 3. Seguridad

**✅ Hash de Contraseñas:**
```csharp
public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password); // Nunca texto plano
    }
}
```

**✅ Autenticación JWT:**
```csharp
[Authorize(Policy = "AdminPolicy")] // Proteger endpoints
public async Task<ActionResult> ValidarUsuario(int id)
{
    // Solo administradores pueden acceder
}
```

**✅ CORS Configurado:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://...")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### 4. Código DRY (Don't Repeat Yourself)

**✅ Helpers Reutilizables:**
```typescript
// api-url.helper.ts
export function getApiUrl(endpoint: string): string {
    const base = environment.production 
        ? 'https://api.production.com'
        : 'http://localhost:56801';
    return `${base}${endpoint}`;
}

// Uso en múltiples servicios
this.http.get(getApiUrl('/api/usuarios')); // ✓ Sin repetir lógica
```

### 5. Inmutabilidad con Signals (Angular 18)

**✅ Reactive State Management:**
```typescript
export class DetalleVehiculoComponent {
    // Signals para estado reactivo e inmutable
    vehiculo = signal<Vehiculo | null>(null);
    subasta = signal<Subasta | null>(null);
    pujas = signal<Puja[]>([]);
    
    // Computed values (auto-actualizados)
    precioMinimo = computed(() => {
        const sub = this.subasta();
        return sub ? sub.precioActual + sub.incrementoMinimo : 0;
    });
    
    // Actualización inmutable
    agregarPuja(nuevaPuja: Puja) {
        this.pujas.update(pujas => [...pujas, nuevaPuja]); // Nuevo array
    }
}
```

### 6. Async/Await Consistente

**✅ Backend C#:**
```csharp
public async Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request)
{
    var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
    if (usuario != null) throw new Exception("Email ya registrado");
    
    // ...
    await _usuarioRepository.AddAsync(nuevoUsuario);
    await _usuarioRepository.SaveChangesAsync();
    
    return MapToResponse(nuevoUsuario);
}
```

**✅ Frontend TypeScript:**
```typescript
async cargarDatos() {
    try {
        const usuarios = await firstValueFrom(this.usuarioService.getUsuarios());
        this.usuarios.set(usuarios);
    } catch (error) {
        this.toast.error('Error al cargar datos');
    }
}
```

---

## 🔍 Code Smells Evitados

### ❌ God Classes
**Problema:** Clases con demasiadas responsabilidades
**Solución:** Separar en services específicos (UsuarioService, PujaService, etc.)

### ❌ Magic Numbers
**Problema:** Números sin contexto en el código
**Solución:** Constantes con nombres descriptivos
```csharp
// ❌ Mal
if (usuario.Rol == 1) { }

// ✅ Bien
if (usuario.Rol == "Administrador") { }
```

### ❌ Long Methods
**Problema:** Métodos de más de 30-40 líneas
**Solución:** Extraer submétodos privados

### ❌ Hardcoded Values
**Problema:** URLs, secrets en código
**Solución:** appsettings.json, environment.ts
```typescript
// ❌ Mal
const url = 'http://localhost:56801/api/usuarios';

// ✅ Bien
const url = getApiUrl('/api/usuarios');
```

---

## 📊 Métricas de Calidad

✅ **Cobertura de Tests:** Pendiente implementar (objetivo: >70%)
✅ **Complejidad Ciclomática:** Mantenida baja con métodos pequeños
✅ **Acoplamiento:** Bajo gracias a interfaces y DI
✅ **Cohesión:** Alta con SRP en todas las clases
✅ **Documentación:** XML docs en backend, JSDoc en frontend
✅ **Convenciones:** Consistentes en todo el proyecto

---

*Documento actualizado: 3 de febrero de 2026*
