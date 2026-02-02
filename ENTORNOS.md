# 🌍 Entornos de Subastas

## 📍 Entorno Local (Desarrollo)

### Frontend
- **URL**: http://localhost:4200
- **Estado**: ✅ Corriendo
- **Puerto**: 4200
- **Tecnología**: Angular 18 con servidor de desarrollo

### Backend API
- **URL**: http://localhost:56801
- **Estado**: ✅ Corriendo en ventana separada (verde)
- **Puerto**: 56801
- **Swagger**: http://localhost:56801/swagger
- **Health Check**: http://localhost:56801/health
- **Tecnología**: .NET 8 Web API

### Base de Datos
- **Servidor**: localhost (SQL Server)
- **Base de datos**: Subastas
- **Autenticación**: Windows Authentication

---

## ☁️ Entorno Azure (Producción)

### Backend API - App Service
- **Nombre**: SubastasWebApi20260202162157
- **URL**: https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net
- **Resource Group**: Curso
- **Health Check**: https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net/health
- **Swagger**: https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net/swagger
- **Tipo**: Azure App Service (Windows, .NET 8)
- **Estado**: ✅ ACTUALIZADO - Deployment exitoso (02/02/2026 19:00)

### Frontend - Static Web App (si existe)
- **Estado**: 🔍 Por verificar/configurar
- **Necesita**: Configuración de apiUrl apuntando a backend Azure

### Base de Datos - Azure SQL
- **Estado**: 🔍 Por verificar configuración
- **Connection String**: Configurado en App Service Settings

---

## 📦 Archivos de Deployment Listos

### Backend (Actualizado con normalización de roles)
```
C:\Users\JoseAntonioVallecill\source\repos\subastas\src\Subastas.WebApi\backend-deploy.zip
```

### Frontend (Compilado para producción)
```
C:\Users\JoseAntonioVallecill\source\repos\subastas\front\frontend-deploy.zip
```

---

## 🚀 Próximos Pasos para Azure

1. **Desplegar Backend Actualizado**:
   - Ve a: https://portal.azure.com
   - Busca: SubastasWebApi20260202162157
   - Deployment Center → Zip Deploy
   - Sube: backend-deploy.zip

2. **Verificar Backend**:
   - Abre: https://subastaswebapi20260202162157.azurewebsites.net/health
   - Prueba: https://subastaswebapi20260202162157.azurewebsites.net/swagger

3. **Desplegar Frontend** (si Static Web App existe):
   - Sube: frontend-deploy.zip
   - Actualiza apiUrl en configuración

---

## 🔧 Cambios Pendientes de Desplegar a Azure

✅ **Normalizaciones de roles** (trim + toLowerCase):
- AuthService.cs: Token genera con "Admin" o "Usuario" capitalizado
- UsuariosController.cs: Login compara rol normalizado
- PujasController.cs: Validación admin usa rol normalizado
- Program.cs: Política AdminPolicy case-insensitive
- AdminController.cs: Agregado [Authorize(Policy = "AdminPolicy")]

✅ **Mejoras Dashboard**:
- Chart.js integrado
- Interfaces TypeScript con camelCase
- Links de navegación corregidos

✅ **Fixes de Autenticación**:
- Admins bypass validación en login
- Política de autorización case-insensitive

---

## 📝 Notas

- **Total de entornos**: 2 (Local + Azure)
- **Git Remote**: Configurado pero repositorio no accesible
  - URL: https://github.com/javallecillo-hiberuscom/subastas.git
  - Estado: Repository not found (permisos o nombre incorrecto)
- **Commits locales**: Guardados y protegidos (2 commits recientes)
