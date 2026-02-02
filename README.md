# 📖 README - Sistema de Subastas (Clean Architecture)

## 🎯 Descripción del Proyecto

Sistema de gestión de subastas de vehículos desarrollado con **Clean Architecture** para Desguaces Borox. Permite a usuarios autenticados participar en subastas, realizar pujas y gestionar vehículos.

## 🏗️ Arquitectura

Este proyecto implementa **Clean Architecture** (Arquitectura Limpia) con las siguientes capas:

```
📦 Subastas
├── 🔵 Domain          (Entidades de negocio)
├── 🟢 Application     (Contratos y DTOs)
├── 🟡 Infrastructure  (Implementaciones)
└── 🔴 WebApi          (API REST)
```

Para más detalles, consulta [ARQUITECTURA.md](ARQUITECTURA.md)

## 🚀 Tecnologías

- **.NET 8.0**
- **Entity Framework Core 8.0**
- **SQL Server**
- **JWT Authentication**
- **BCrypt** para hash de contraseñas
- **Swagger/OpenAPI**
- **xUnit** para testing
- **Angular 19** (frontend en `front/`)

## 📋 Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express o superior)
- [Node.js](https://nodejs.org/) v18+ (para el frontend)
- Visual Studio 2022 / VS Code / Rider

## ⚙️ Configuración

### 1. Clonar el repositorio

```bash
git clone <repository-url>
cd subastas
```

### 2. Configurar Base de Datos

Edita `src/Subastas.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SubastaConnection": "Server=localhost;Database=SubastasDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Aplicar Migraciones

```bash
cd src/Subastas.WebApi
dotnet ef database update
```

### 4. Configurar JWT

Edita la clave secreta en `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "SubastasAPI",
    "Audience": "SubastasClient",
    "ExpirationMinutes": "60"
  }
}
```

## 🎮 Ejecución

### Backend (API)

```bash
cd src/Subastas.WebApi
dotnet run
```

La API estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001` (raíz)

### Frontend (Angular)

```bash
cd front/front
npm install
ng serve
```

El frontend estará en `http://localhost:4200`

## 📚 Documentación API

### Swagger UI

Una vez ejecutando la API, abre tu navegador en:
```
https://localhost:5001
```

### Endpoints Principales

#### Autenticación

**POST** `/api/Usuarios/registro`
```json
{
  "nombre": "Juan",
  "apellidos": "Pérez",
  "email": "juan@example.com",
  "password": "Password123!",
  "telefono": "123456789",
  "direccion": "Calle Principal 123"
}
```

**POST** `/api/Usuarios/login`
```json
{
  "email": "juan@example.com",
  "password": "Password123!"
}
```

#### Usuarios

**GET** `/api/Usuarios` - Obtener todos los usuarios (Admin)  
**GET** `/api/Usuarios/{id}` - Obtener usuario por ID  
**PUT** `/api/Usuarios/{id}` - Actualizar perfil  

#### Subastas

**GET** `/api/Subastas` - Listar subastas  
**GET** `/api/Subastas/{id}` - Obtener subasta  
**POST** `/api/Subastas` - Crear subasta (Admin)  

#### Pujas

**GET** `/api/Pujas/subasta/{idSubasta}` - Pujas de una subasta  
**POST** `/api/Pujas` - Realizar puja  

## 🧪 Testing

### Ejecutar Tests Unitarios

```bash
cd test/Subastas.UnitTests
dotnet test
```

### Cobertura de Tests

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📁 Estructura del Proyecto

```
subastas/
├── src/
│   ├── Subastas.Domain/              # Entidades
│   ├── Subastas.Application/         # DTOs e Interfaces
│   ├── Subastas.Infrastructure/      # Repositorios y Servicios
│   └── Subastas.WebApi/              # API REST
├── test/
│   └── Subastas.UnitTests/           # Pruebas
├── front/                            # Frontend Angular
├── img/                              # Imágenes de vehículos
├── Uploads/                          # Archivos subidos
├── ARQUITECTURA.md                   # Documentación de arquitectura
├── MIGRACION.md                      # Guía de migración
└── SubastasCleanArchitecture.sln    # Solución de Visual Studio
```

## 🔐 Seguridad

### Autenticación
- JWT con HMACSHA256
- Tokens con expiración configurable
- Claims: userId, email, rol

### Contraseñas
- Hash con BCrypt
- Salt automático
- Factor de trabajo ajustable

### CORS
- Configurado para `localhost:4200` (Angular)
- Credenciales habilitadas
- Headers personalizados permitidos

## 🐛 Solución de Problemas

### "Could not find file or assembly"
```bash
dotnet clean
dotnet restore
dotnet build
```

### "Cannot connect to SQL Server"
Verifica:
1. SQL Server está corriendo
2. Cadena de conexión correcta
3. Usuario tiene permisos

### "Unauthorized" en Swagger
1. Hacer POST a `/api/Usuarios/login`
2. Copiar el token
3. Clic en "Authorize" en Swagger
4. Pegar: `Bearer <tu-token>`

## 📝 Convenciones de Código

- **Idioma:** Español (nombres de clases, propiedades)
- **Nomenclatura:** PascalCase para públicos, camelCase para privados
- **Comentarios:** XML comments en inglés/español
- **Async/Await:** Obligatorio para I/O operations
- **DTOs:** Request/Response separados
- **Repository Pattern:** Para acceso a datos

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/NuevaFuncionalidad`)
3. Commit cambios (`git commit -m 'Añadir nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/NuevaFuncionalidad`)
5. Abre un Pull Request

### Guía de Estilo
- Seguir principios SOLID
- Mantener separación de capas
- Escribir tests unitarios
- Documentar código público

## 📜 Licencia

Este proyecto es privado y de uso exclusivo para Desguaces Borox.

## 👥 Equipo

- **Desarrollador:** José Antonio Valle
- **Cliente:** Desguaces Borox

## 📞 Soporte

Para reportar bugs o solicitar funcionalidades:
- Email: dev@subastas.com
- Issues: [GitHub Issues]

## 📅 Roadmap

### Versión 1.1 (Q1 2026)
- [ ] Migrar todos los controladores
- [ ] Implementar CQRS
- [ ] Añadir caching con Redis
- [ ] Mejorar logging con Serilog

### Versión 2.0 (Q2 2026)
- [ ] Microservicios
- [ ] Event Sourcing
- [ ] SignalR para pujas en tiempo real
- [ ] Notificaciones push

## 🎓 Recursos de Aprendizaje

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api/)

---

**Versión:** 1.0.0  
**Última actualización:** Febrero 2026  
**Estado:** ✅ En desarrollo activo
