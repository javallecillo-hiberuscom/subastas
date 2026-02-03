# Arquitectura y Despliegue de la Aplicación de Subastas

## 📋 Resumen Ejecutivo

La aplicación de subastas está desplegada en Microsoft Azure utilizando una arquitectura de tres capas:

1. **Frontend (SPA)** - Azure Static Web Apps
2. **Backend (API REST)** - Azure App Service
3. **Base de Datos** - Azure SQL Database

---

## 🗄️ Base de Datos - Azure SQL Database

### Ubicación y Configuración

- **Servidor**: `subastasbidserver.database.windows.net`
- **Puerto**: `1433` (puerto estándar de SQL Server)
- **Base de Datos**: `Subastas`
- **Usuario Admin**: `subastasbidadmin`
- **Región**: Canada Central
- **Tipo**: Azure SQL Database (PaaS)

### Cadena de Conexión

```
Server=tcp:subastasbidserver.database.windows.net,1433;
Initial Catalog=Subastas;
Persist Security Info=False;
User ID=subastasbidadmin;
Password=Pepon2025!!;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

### Estructura de la Base de Datos

La base de datos contiene las siguientes tablas principales:

- **Usuario** - Datos de usuarios del sistema
- **Empresa** - Empresas registradas
- **Vehiculo** - Catálogo de vehículos
- **ImagenVehiculo** - Imágenes de los vehículos
- **Subasta** - Subastas activas/finalizadas
- **Puja** - Pujas realizadas por usuarios
- **Notificacion** - Notificaciones para usuarios
- **NotificacionAdmin** - Notificaciones para administradores
- **Pago** - Registro de pagos

### Acceso a la Base de Datos

**Desde SQL Server Management Studio (SSMS):**
1. Servidor: `subastasbidserver.database.windows.net`
2. Autenticación: SQL Server Authentication
3. Usuario: `subastasbidadmin`
4. Contraseña: `Pepon2025!!`

**Desde Azure Portal:**
1. Ir a "SQL databases" → "Subastas"
2. Click en "Query editor"
3. Iniciar sesión con las credenciales

**Desde PowerShell (sqlcmd):**
```powershell
sqlcmd -S tcp:subastasbidserver.database.windows.net,1433 `
       -d Subastas `
       -U subastasbidadmin `
       -P "Pepon2025!!" `
       -Q "SELECT * FROM Usuario"
```

---

## 🔧 Backend - Azure App Service (.NET 8 Web API)

### Ubicación y Configuración

- **URL**: `https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net`
- **Servicio**: Azure App Service (Web App)
- **Runtime**: .NET 8.0
- **Sistema Operativo**: Windows
- **Región**: Canada Central
- **Plan de App Service**: Free/Shared (F1) o Basic (B1)

### Estructura del Backend

**Arquitectura Clean Architecture:**

```
src/Subastas.WebApi/          → Capa de Presentación (API Controllers)
src/Subastas.Application/     → Capa de Aplicación (DTOs, Services)
src/Subastas.Domain/          → Capa de Dominio (Entidades, Interfaces)
src/Subastas.Infrastructure/  → Capa de Infraestructura (EF Core, Repositories)
```

**Principales Endpoints:**

- `GET /api/Subastas` - Obtener subastas activas
- `GET /api/Vehiculos` - Listar vehículos
- `POST /api/Pujas` - Realizar una puja
- `POST /api/Usuarios/login` - Autenticación
- `GET /api/NotificacionesAdmin` - Notificaciones de administrador
- `PUT /api/Usuarios/{id}/validar` - Validar usuario

### Autenticación y Seguridad

- **Tipo**: JWT (JSON Web Tokens)
- **Política Admin**: Solo usuarios con rol "Administrador" pueden acceder a endpoints admin
- **CORS**: Configurado para permitir peticiones desde el frontend
- **HTTPS**: Todas las comunicaciones están cifradas

### Variables de Configuración (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:subastasbidserver.database.windows.net,1433;..."
  },
  "JwtSettings": {
    "SecretKey": "clave-secreta-muy-larga-y-segura...",
    "Issuer": "SubastasAPI",
    "Audience": "SubastasFrontend",
    "ExpirationMinutes": 1440
  }
}
```

### Despliegue del Backend

**Método 1: Desde Visual Studio**
1. Click derecho en proyecto `Subastas.WebApi`
2. "Publish..." → Seleccionar perfil de Azure
3. Click en "Publish"

**Método 2: Desde PowerShell**
```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas
.\deploy-backend.ps1
```

**Verificar que funciona:**
```
https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net/api/Subastas
```

---

## 🌐 Frontend - Azure Static Web Apps (Angular 18)

### Ubicación y Configuración

- **URL Producción**: `https://blue-flower-00b3c6b03.1.azurestaticapps.net`
- **Servicio**: Azure Static Web Apps
- **Framework**: Angular 18 (Standalone Components)
- **Región**: East US 2
- **Modo de Compilación**: Producción (optimizado)

### Estructura del Frontend

```
front/
├── src/
│   ├── app/
│   │   ├── admin/                  → Componentes de administración
│   │   │   ├── vehiculos/          → Gestión de vehículos
│   │   │   ├── empresas/           → Gestión de empresas
│   │   │   ├── usuarios/           → Gestión de usuarios
│   │   │   ├── pujas/              → Gestión de pujas
│   │   │   ├── notificaciones-admin/ → Notificaciones admin
│   │   │   └── dashboard-admin/    → Dashboard administrativo
│   │   ├── layout/                 → Layout principal con header/sidebar
│   │   ├── login/                  → Página de login
│   │   ├── registro/               → Página de registro
│   │   ├── dashboard/              → Dashboard de usuario
│   │   ├── lista-pujas/            → Lista de subastas
│   │   ├── detalle-vehiculo/       → Detalle de vehículo y pujas
│   │   ├── mis-pujas/              → Pujas del usuario
│   │   ├── perfil/                 → Perfil de usuario
│   │   ├── subir-iae/              → Subida de documento IAE
│   │   ├── services/               → Servicios (Auth, Notifications, Toast)
│   │   ├── guards/                 → Guards de autenticación
│   │   └── models/                 → Interfaces y tipos TypeScript
│   ├── environments/               → Configuración por entorno
│   └── assets/                     → Recursos estáticos
├── proxy.conf.json                 → Configuración de proxy para desarrollo
└── angular.json                    → Configuración de Angular
```

### Configuración de Entornos

**Desarrollo Local (localhost):**
- Frontend: `http://localhost:4200`
- Backend: `http://localhost:56801`
- Proxy configurado en `proxy.conf.json`

**Producción (Azure):**
- Frontend: `https://blue-flower-00b3c6b03.1.azurestaticapps.net`
- Backend: `https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net`

### Características Principales

1. **Autenticación JWT**: Token almacenado en localStorage
2. **Signals de Angular**: Estado reactivo (notificaciones, usuario actual)
3. **Standalone Components**: No usa NgModules
4. **Lazy Loading**: Carga diferida de rutas
5. **Guards**: Protección de rutas (authGuard, adminGuard)
6. **Notificaciones en Tiempo Real**: Polling cada 3 segundos
7. **Responsive Design**: CSS Grid y Flexbox

### Despliegue del Frontend

**Método Automático (recomendado):**
```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas
.\deploy-frontend.ps1
```

Este script:
1. Compila Angular en modo producción
2. Genera archivos optimizados en `dist/front/browser`
3. Despliega a Azure Static Web Apps usando SWA CLI
4. Tiempo de propagación: 2-3 minutos

**Verificar despliegue:**
1. Esperar 2-3 minutos
2. Abrir: `https://blue-flower-00b3c6b03.1.azurestaticapps.net`
3. Limpiar caché del navegador (Ctrl+Shift+R)

---

## 🔄 Flujo de Comunicación Completo

### Ejemplo: Usuario Realiza una Puja

```
1. FRONTEND (Angular)
   ├─ Usuario hace clic en "Realizar Puja"
   ├─ detalle-vehiculo.component.ts ejecuta realizarPuja()
   └─ HttpClient envía POST a /api/Pujas
        ↓
2. NETWORK (HTTPS)
   ├─ Request sale del navegador
   └─ Llega a Azure App Service
        ↓
3. BACKEND (.NET API)
   ├─ PujasController.PostPuja() recibe la petición
   ├─ Valida JWT token (usuario autenticado)
   ├─ Valida que usuario esté validado
   ├─ Valida que la puja sea correcta
   └─ Guarda en base de datos usando Entity Framework
        ↓
4. BASE DE DATOS (Azure SQL)
   ├─ INSERT en tabla Puja
   ├─ UPDATE en tabla Subasta (nuevo precio actual)
   └─ Confirma transacción
        ↓
5. BACKEND (Respuesta)
   ├─ Devuelve HTTP 204 No Content (éxito)
   └─ O error 400 con mensaje descriptivo
        ↓
6. FRONTEND (Procesamiento)
   ├─ Recibe respuesta
   ├─ Muestra toast de confirmación
   ├─ Recarga la subasta (actualiza precio)
   └─ Actualiza notificaciones
```

---

## 🚀 Desarrollo Local vs Producción

### Desarrollo Local

**Iniciar Backend:**
```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas\src\Subastas.WebApi
dotnet run
```
Backend en: `http://localhost:56801`

**Iniciar Frontend:**
```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas\front
npm start
```
Frontend en: `http://localhost:4200`

**Ventajas:**
- Cambios instantáneos (hot reload)
- Debugging completo
- No consume recursos de Azure
- Logs detallados en consola

### Producción en Azure

**Ventajas:**
- Accesible desde internet
- Escalabilidad automática
- Backups automáticos de BD
- HTTPS automático
- CDN global (Static Web Apps)

**Desventajas:**
- Requiere despliegue (2-3 minutos)
- Debugging más complejo
- Costos por uso

---

## 📊 Monitoreo y Logs

### Azure Portal

**Ver logs del Backend:**
1. Azure Portal → App Services → `subastaswebapi20260202162157...`
2. "Log stream" para ver logs en tiempo real
3. "Application Insights" para métricas detalladas

**Ver logs del Frontend:**
1. Azure Portal → Static Web Apps → `blue-flower-00b3c6b03`
2. "Functions" → "Monitor" para ver logs de funciones
3. Consola del navegador (F12) para errores del cliente

### Herramientas de Debugging

**Backend:**
- Visual Studio Debugger (local)
- Azure Application Insights (producción)
- Postman/Thunder Client (probar endpoints)

**Frontend:**
- Chrome DevTools (F12)
- Angular DevTools extension
- Network tab para ver peticiones HTTP

**Base de Datos:**
- SQL Server Management Studio
- Azure Portal Query Editor
- sqlcmd desde PowerShell

---

## 🔐 Seguridad y Credenciales

### Información Sensible (NO compartir públicamente)

**Base de Datos:**
- Usuario: `subastasbidadmin`
- Password: `Pepon2025!!`

**JWT Secret Key:**
- Almacenada en `appsettings.json` del backend
- Nunca expuesta en el frontend

**Azure Deployment Token:**
- Almacenado en `front/deployment-token.txt`
- Usado por SWA CLI para desplegar

### Usuarios de Prueba

**Administrador:**
- Email: `lucia@admin.com`
- Rol: Administrador
- Permisos: Todos (CRUD completo, validar usuarios, etc.)

**Usuario Validado:**
- Email: Cualquier usuario con `Validado = 1`
- Permisos: Ver subastas, realizar pujas, subir IAE

---

## 📝 Comandos Útiles de Referencia

### Despliegues
```powershell
# Desplegar Backend
.\deploy-backend.ps1

# Desplegar Frontend
.\deploy-frontend.ps1

# Desplegar ambos
.\deploy-to-azure.ps1
```

### Base de Datos
```powershell
# Conectar a SQL
sqlcmd -S tcp:subastasbidserver.database.windows.net,1433 `
       -d Subastas -U subastasbidadmin -P "Pepon2025!!"

# Ver usuarios
sqlcmd ... -Q "SELECT * FROM Usuario"

# Ver notificaciones admin
sqlcmd ... -Q "SELECT * FROM NotificacionAdmin WHERE Leida = 0"
```

### Desarrollo Local
```powershell
# Backend
cd src\Subastas.WebApi
dotnet run

# Frontend
cd front
npm start

# Compilar Frontend para producción
npm run build -- --configuration production
```

---

## 🎯 Puntos Clave para el Video

1. **Arquitectura de 3 capas separadas físicamente** en Azure
2. **Base de datos centralizada** que ambos entornos (dev/prod) usan
3. **Backend API RESTful** que maneja toda la lógica de negocio
4. **Frontend SPA** que solo maneja la presentación
5. **Autenticación JWT** para seguridad entre capas
6. **Despliegue independiente** de cada capa
7. **Desarrollo local** para rapidez vs **Producción Azure** para accesibilidad
8. **Notificaciones en tiempo real** mediante polling
9. **Clean Architecture** en el backend para mantenibilidad
10. **Signals de Angular** para reactividad en el frontend

---

## 📞 Recursos Adicionales

### URLs Importantes

- **Aplicación en Producción**: https://blue-flower-00b3c6b03.1.azurestaticapps.net
- **API Backend**: https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net
- **Azure Portal**: https://portal.azure.com

### Documentación

Ver carpeta `docs/` para documentación adicional técnica.

---

*Documento creado: 3 de febrero de 2026*
*Versión: 1.0*
