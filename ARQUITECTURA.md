# 📐 Arquitectura del Proyecto Subastas

## 🎯 Visión General

Este proyecto implementa una **Clean Architecture (Arquitectura Limpia)** para el sistema de gestión de subastas de vehículos. La arquitectura está diseñada para maximizar la mantenibilidad, escalabilidad y testabilidad del código.

## 🏗️ Estructura del Proyecto

```
subastas/
├── src/
│   ├── Subastas.Domain/              # ⭐ Capa de Dominio
│   │   └── Entities/                 # Entidades del negocio
│   │       ├── Usuario.cs
│   │       ├── Empresa.cs
│   │       ├── Vehiculo.cs
│   │       ├── Subasta.cs
│   │       ├── Puja.cs
│   │       ├── Notificacion.cs
│   │       ├── NotificacionAdmin.cs
│   │       ├── ImagenVehiculo.cs
│   │       └── Pago.cs
│   │
│   ├── Subastas.Application/         # ⭐ Capa de Aplicación
│   │   ├── DTOs/
│   │   │   ├── Requests/             # DTOs de entrada
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── RegistroUsuarioRequest.cs
│   │   │   │   ├── ActualizarPerfilRequest.cs
│   │   │   │   ├── CrearSubastaRequest.cs
│   │   │   │   ├── CrearPujaRequest.cs
│   │   │   │   └── CrearVehiculoRequest.cs
│   │   │   └── Responses/            # DTOs de salida
│   │   │       ├── LoginResponse.cs
│   │   │       ├── UsuarioResponse.cs
│   │   │       ├── SubastaResponse.cs
│   │   │       ├── VehiculoResponse.cs
│   │   │       ├── PujaResponse.cs
│   │   │       └── ApiResponse.cs
│   │   └── Interfaces/
│   │       ├── Repositories/         # Contratos de repositorios
│   │       │   ├── IRepository.cs
│   │       │   ├── IUsuarioRepository.cs
│   │       │   ├── ISubastaRepository.cs
│   │       │   ├── IVehiculoRepository.cs
│   │       │   └── IPujaRepository.cs
│   │       └── Services/             # Contratos de servicios
│   │           ├── IAuthService.cs
│   │           ├── IPasswordService.cs
│   │           └── INotificacionAdminService.cs
│   │
│   ├── Subastas.Infrastructure/      # ⭐ Capa de Infraestructura
│   │   ├── Data/
│   │   │   ├── SubastaContext.cs     # DbContext de EF Core
│   │   │   └── Migrations/           # Migraciones de BD
│   │   ├── Repositories/             # Implementaciones de repositorios
│   │   │   ├── Repository.cs
│   │   │   ├── UsuarioRepository.cs
│   │   │   ├── SubastaRepository.cs
│   │   │   ├── VehiculoRepository.cs
│   │   │   └── PujaRepository.cs
│   │   ├── Services/                 # Implementaciones de servicios
│   │   │   ├── AuthService.cs
│   │   │   ├── PasswordService.cs
│   │   │   └── NotificacionAdminService.cs
│   │   └── Configuration/
│   │       └── DependencyInjection.cs # Configuración de DI
│   │
│   └── Subastas.WebApi/              # ⭐ Capa de Presentación
│       ├── Controllers/              # Controladores REST
       │   ├── UsuariosController.cs
       │   ├── SubastasController.cs
       │   ├── VehiculosController.cs
       │   ├── PujasController.cs
       │   ├── NotificacionesController.cs
       │   ├── NotificacionesAdminController.cs
       │   ├── DocumentosController.cs
       │   ├── EmpresasController.cs
       │   └── ImagenesVehiculoController.cs
│       ├── Extensions/               # Extensiones personalizadas
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Program.cs                # Punto de entrada
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── test/
│   └── Subastas.UnitTests/           # ⭐ Pruebas Unitarias
│       ├── Controllers/
│       └── Services/
│
├── Uploads/                          # Archivos subidos
└── img/                              # Imágenes de vehículos
```

## 📦 Capas de la Arquitectura

### 🔵 1. Subastas.Domain (Capa de Dominio)

**Responsabilidad:** Define las entidades de negocio y reglas de dominio.

**Características:**
- ✅ No tiene dependencias de otras capas
- ✅ Contiene las entidades del negocio
- ✅ Define la lógica de negocio core
- ✅ Es el corazón de la aplicación

**Entidades principales:**
- `Usuario`: Representa usuarios del sistema
- `Empresa`: Empresas participantes
- `Vehiculo`: Vehículos en subasta
- `Subasta`: Subastas activas/finalizadas
- `Puja`: Ofertas realizadas por usuarios
- `Notificacion`: Notificaciones a usuarios
- `NotificacionAdmin`: Notificaciones administrativas

### 🟢 2. Subastas.Application (Capa de Aplicación)

**Responsabilidad:** Define contratos (interfaces) y DTOs para la lógica de negocio.

**Características:**
- ✅ Define interfaces de repositorios y servicios
- ✅ Contiene DTOs de entrada (Requests) y salida (Responses)
- ✅ Depende solo de `Domain`
- ✅ Define casos de uso del sistema

**Componentes:**
- **DTOs/Requests:** Objetos para recibir datos de la API
- **DTOs/Responses:** Objetos para devolver datos desde la API
- **Interfaces/Repositories:** Contratos para acceso a datos
- **Interfaces/Services:** Contratos para servicios de negocio

### 🟡 3. Subastas.Infrastructure (Capa de Infraestructura)

**Responsabilidad:** Implementa acceso a datos y servicios externos.

**Características:**
- ✅ Implementa interfaces definidas en `Application`
- ✅ Contiene el DbContext de Entity Framework Core
- ✅ Implementa repositorios concretos
- ✅ Implementa servicios de infraestructura

**Componentes:**
- **Data/SubastaContext:** Contexto de Entity Framework Core
- **Repositories:** Implementaciones del patrón Repository
- **Services:** Servicios de autenticación, password, notificaciones
- **Configuration/DependencyInjection:** Configuración de inyección de dependencias

### 🔴 4. Subastas.WebApi (Capa de Presentación)

**Responsabilidad:** Expone la API REST y maneja HTTP.

**Características:**
- ✅ Controladores REST API
- ✅ Configuración de middleware
- ✅ Autenticación JWT
- ✅ Swagger/OpenAPI
- ✅ CORS para frontend Angular

**Componentes:**
- **Controllers:** Endpoints REST para todas las entidades del sistema
  - **UsuariosController:** Autenticación, registro, gestión de usuarios
  - **SubastasController:** CRUD de subastas, consulta por estado
  - **VehiculosController:** Gestión de vehículos con imágenes Base64
  - **PujasController:** Realización y consulta de pujas
  - **NotificacionesController:** Notificaciones a usuarios, emails
  - **NotificacionesAdminController:** Panel administrativo de notificaciones
  - **DocumentosController:** Subida/descarga de documentos IAE
  - **EmpresasController:** Gestión de empresas
  - **ImagenesVehiculoController:** Gestión de imágenes en Base64
- **Program.cs:** Configuración de la aplicación
- **Extensions:** Métodos de extensión personalizados
- **appsettings.json:** Configuración de la aplicación

## 🔄 Flujo de Dependencias

```
┌─────────────────────────────────────────┐
│         Subastas.WebApi                 │
│         (Presentación)                  │
└────────────────┬────────────────────────┘
                 │ depende de
                 ▼
┌─────────────────────────────────────────┐
│      Subastas.Infrastructure            │
│      (Datos y Servicios)                │
└────────────────┬────────────────────────┘
                 │ depende de
                 ▼
┌─────────────────────────────────────────┐
│      Subastas.Application               │
│      (Contratos y DTOs)                 │
└────────────────┬────────────────────────┘
                 │ depende de
                 ▼
┌─────────────────────────────────────────┐
│         Subastas.Domain                 │
│         (Entidades Core)                │
└─────────────────────────────────────────┘
```

**Regla de Oro:** Las dependencias fluyen **hacia adentro** (de fuera hacia el dominio), nunca al revés.

## 🛠️ Tecnologías Utilizadas

- **Framework:** .NET 8.0
- **ORM:** Entity Framework Core 8.0
- **Base de Datos:** SQL Server
- **Autenticación:** JWT (JSON Web Tokens)
- **Password Hashing:** BCrypt.NET
- **Documentación:** Swagger/OpenAPI
- **Testing:** xUnit, Moq
- **Frontend:** Angular (en carpeta `front/`)

## 🎯 Principios SOLID Aplicados

### ✅ Single Responsibility Principle (SRP)
Cada clase tiene una única responsabilidad:
- Controladores solo manejan HTTP
- Repositorios solo acceden a datos
- Servicios solo implementan lógica de negocio

### ✅ Open/Closed Principle (OCP)
El código está abierto para extensión pero cerrado para modificación:
- Interfaces permiten múltiples implementaciones
- Patrón Repository permite cambiar ORM sin afectar lógica

### ✅ Liskov Substitution Principle (LSP)
Las implementaciones pueden sustituir sus interfaces sin romper el sistema.

### ✅ Interface Segregation Principle (ISP)
Interfaces pequeñas y específicas:
- `IUsuarioRepository` tiene métodos específicos de usuarios
- `IPasswordService` solo maneja contraseñas

### ✅ Dependency Inversion Principle (DIP)
Las capas dependen de abstracciones (interfaces), no de implementaciones concretas.

## 📊 Patrones de Diseño Implementados

### 1. Repository Pattern
Abstrae el acceso a datos detrás de interfaces.

```csharp
IUsuarioRepository usuarioRepo = new UsuarioRepository(context);
var usuario = await usuarioRepo.GetByEmailAsync("email@example.com");
```

### 2. Dependency Injection (DI)
Inyección de dependencias en toda la aplicación.

```csharp
public UsuariosController(
    IUsuarioRepository usuarioRepository,
    IPasswordService passwordService)
{
    _usuarioRepository = usuarioRepository;
    _passwordService = passwordService;
}
```

### 3. DTO Pattern
Separación entre entidades de dominio y objetos de transferencia.

```csharp
// Request DTO
public class LoginRequest { ... }

// Response DTO
public class LoginResponse { ... }
```

### 4. Generic Repository
Repositorio genérico para operaciones CRUD básicas.

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    // ...
}
```

## 🔐 Seguridad

### Autenticación JWT
- Tokens firmados con HMACSHA256
- Expiración configurable
- Claims personalizados (userId, email, rol)

### Gestión de Contraseñas
- Hash con BCrypt (factor de trabajo ajustable)
- Salt automático por contraseña
- Verificación segura

### CORS
- Configurado para frontend Angular
- Origenes permitidos configurables
- Credenciales habilitadas

## 📝 Convenciones de Código

### Nomenclatura
- **PascalCase:** Clases, propiedades, métodos
- **camelCase:** Parámetros, variables locales
- **Prefijo I:** Interfaces (`IUsuarioRepository`)

### Comentarios XML
Todas las clases y métodos públicos documentados con XML comments.

```csharp
/// <summary>
/// Obtiene un usuario por su email.
/// </summary>
/// <param name="email">Email del usuario</param>
/// <returns>Usuario encontrado o null</returns>
Task<Usuario?> GetByEmailAsync(string email);
```

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
- .NET 8.0 SDK
- SQL Server (LocalDB o Server)
- Visual Studio 2022 o VS Code

### Configuración

1. **Configurar cadena de conexión** en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SubastaConnection": "Server=localhost;Database=SubastasDB;Trusted_Connection=True;"
  }
}
```

2. **Aplicar migraciones:**
```bash
cd src/Subastas.WebApi
dotnet ef database update
```

3. **Ejecutar la aplicación:**
```bash
dotnet run
```

4. **Acceder a Swagger:**
```
https://localhost:5001
```

## 🧪 Testing

### Ejecutar pruebas unitarias:
```bash
cd test/Subastas.UnitTests
dotnet test
```

## 📚 Próximos Pasos

- [x] ✅ Migrar todos los controladores a la nueva estructura
- [x] ✅ Implementar repositorios específicos
- [ ] Añadir pruebas de integración
- [ ] Implementar logging con Serilog
- [ ] Añadir caching con Redis
- [ ] Implementar patrones CQRS para operaciones complejas
- [ ] Añadir validaciones con FluentValidation

## 📋 Controladores Disponibles

### 🔑 UsuariosController
**Base URL:** `/api/Usuarios`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/registro` | Registra un nuevo usuario |
| POST | `/login` | Inicia sesión y devuelve JWT |
| GET | `/` | Obtiene todos los usuarios |
| GET | `/{id}` | Obtiene un usuario por ID |
| PUT | `/{id}` | Actualiza perfil de usuario |

### 🏎️ VehiculosController
**Base URL:** `/api/Vehiculos`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/` | Obtiene todos los vehículos |
| GET | `/{id}` | Obtiene un vehículo por ID |
| POST | `/` | Crea vehículo con imágenes |
| PUT | `/{id}` | Actualiza vehículo |
| DELETE | `/{id}` | Elimina vehículo |

### 🏆 SubastasController
**Base URL:** `/api/Subastas`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/?activas={bool}` | Obtiene subastas (activas/finalizadas) |
| GET | `/{id}` | Obtiene una subasta por ID |

### 💰 PujasController
**Base URL:** `/api/Pujas`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/activas` | Obtiene pujas activas |
| GET | `/usuario/{idUsuario}` | Obtiene pujas de un usuario |
| POST | `/` | Realiza una nueva puja |

### 🔔 NotificacionesController
**Base URL:** `/api/Notificaciones`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/enviar-email` | Envía email genérico |
| POST | `/procesar-finalizadas` | Procesa subastas finalizadas |
| GET | `/{idUsuario}` | Obtiene notificaciones del usuario |
| PUT | `/{id}/leida` | Marca notificación como leída |
| PUT | `/usuario/{idUsuario}/leidas` | Marca todas como leídas |

### 📊 NotificacionesAdminController
**Base URL:** `/api/NotificacionesAdmin`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/?soloNoLeidas={bool}&limite={int}` | Obtiene notificaciones admin |
| GET | `/contador-no-leidas` | Contador de no leídas |
| POST | `/` | Crea notificación admin |
| PUT | `/{id}/marcar-leida` | Marca como leída |
| PUT | `/marcar-todas-leidas` | Marca todas como leídas |
| DELETE | `/{id}` | Elimina notificación |
| DELETE | `/limpiar-leidas` | Elimina todas las leídas |

### 📄 DocumentosController
**Base URL:** `/api/Documentos`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/subir-iae/{idUsuario}` | Sube documento IAE en Base64 |
| GET | `/descargar-iae/{idUsuario}` | Descarga documento IAE |
| GET | `/verificar-iae/{idUsuario}` | Verifica si tiene documento |

### 🏢 EmpresasController
**Base URL:** `/api/Empresas`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/` | Obtiene todas las empresas |
| GET | `/{id}` | Obtiene una empresa por ID |
| POST | `/` | Crea nueva empresa |
| PUT | `/{id}` | Actualiza empresa |
| DELETE | `/{id}` | Elimina empresa |

### 🖼️ ImagenesVehiculoController
**Base URL:** `/api/ImagenesVehiculo`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/` | Sube imagen en Base64 |
| GET | `/vehiculo/{idVehiculo}` | Obtiene imágenes del vehículo |
| DELETE | `/{id}` | Elimina imagen |

## 🤝 Contribución

Este proyecto sigue las mejores prácticas de desarrollo empresarial. Para contribuir:

1. Mantener la separación de capas
2. Seguir principios SOLID
3. Documentar código con XML comments
4. Escribir pruebas unitarias
5. Usar async/await para operaciones I/O

---

**Versión:** 1.0.0  
**Última actualización:** Febrero 2026  
**Arquitectura:** Clean Architecture / Onion Architecture
