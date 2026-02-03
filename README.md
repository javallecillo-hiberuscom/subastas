# Sistema de Subastas - Clean Architecture

## 📁 Estructura del Proyecto

```
subastas/
├── src/                        # 🔧 BACKEND - .NET 8 Clean Architecture
│   ├── Subastas.WebApi/         # API REST
│   ├── Subastas.Application/    # Lógica de aplicación  
│   ├── Subastas.Domain/         # Entidades y lógica de negocio
│   └── Subastas.Infrastructure/ # Acceso a datos y servicios externos
│
├── front/                      # 🎨 FRONTEND - Angular 18
│   ├── src/app/                 # Componentes y servicios
│   └── src/environments/        # Configuración de entornos
│
├── test/                       # ✅ Tests unitarios
│
├── database-scripts/           # 🗄️ Scripts SQL
│   ├── crear-tabla-notificaciones-admin.sql
│   ├── migracion-tablas-maestras.sql    # ⭐ NUEVO: Optimizaciones BD
│   ├── fix-fk-empresa.sql
│   ├── insertar-vehiculos-subastas.sql
│   └── verificar-actualizar-admin.sql
│
├── deployment-scripts/         # 🚀 Scripts de despliegue Azure
│   ├── deploy-frontend.ps1      # Deploy a Azure Static Web Apps
│   ├── deploy-backend.ps1       # Deploy a Azure App Service
│   ├── deploy-backend-completo.ps1
│   └── deploy-to-azure.ps1
│
└── docs/                       # 📚 Documentación técnica completa
    ├── README.md                     # Índice de documentación
    ├── MANUAL-DESPLIEGUE.md          # ⭐ NUEVO: Tutorial completo
    ├── ANALISIS-OPTIMIZACION-BD.md   # ⭐ NUEVO: Análisis BD
    ├── ARQUITECTURA-DESPLIEGUE-AZURE.md
    ├── CASOS-DE-USO.md
    ├── DIAGRAMAS.html
    ├── CLEAN-CODE-PRACTICAS.md
    └── ...más documentos
```

## 🚀 Inicio Rápido

### Backend (.NET 8)
```powershell
cd src/Subastas.WebApi
dotnet run
```
El backend arrancará en: http://localhost:56801

### Frontend (Angular 18)
```powershell
cd front
npm start
```
El frontend arrancará en: http://localhost:4200

## 📚 Documentación

Ver **[Índice Completo de Documentación](docs/README.md)** para guías detalladas por rol.

**Documentos Principales:**
- **[Arquitectura y Despliegue en Azure](docs/ARQUITECTURA-DESPLIEGUE-AZURE.md)** - Guía completa de arquitectura y deployment
- **[Casos de Uso](docs/CASOS-DE-USO.md)** - Documentación detallada de funcionalidades por actor (12 CU)
- **[Diagramas Interactivos](docs/DIAGRAMAS.html)** - Visualización HTML de arquitectura, BD, flujos y casos de uso
- **[Clean Code y Buenas Prácticas](docs/CLEAN-CODE-PRACTICAS.md)** - Principios SOLID, patrones y convenciones

## 🔗 URLs de Producción

- **Frontend**: https://blue-flower-00b3c6b03.1.azurestaticapps.net
- **Backend**: https://subastaswebapi20260202162157.azurewebsites.net
- **Database**: subastasbidserver.database.windows.net

## 🛠️ Tecnologías

- **Frontend**: Angular 18, TypeScript, Bootstrap
- **Backend**: .NET 8, Entity Framework Core, Clean Architecture
- **Database**: Azure SQL Database
- **Deployment**: Azure Static Web Apps, Azure App Service
