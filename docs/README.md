# 📚 Índice de Documentación - Sistema de Subastas

## 🎯 Guías Rápidas

### Para Desarrolladores Nuevos
1. **[README.md](../README.md)** - Inicio rápido y estructura del proyecto
2. **[MANUAL-DESPLIEGUE.md](MANUAL-DESPLIEGUE.md)** ⭐ **NUEVO** - Tutorial completo paso a paso
3. **[DIAGRAMAS.html](DIAGRAMAS.html)** - Visualización interactiva de arquitectura (abrir en navegador)
4. **[CASOS-DE-USO.md](CASOS-DE-USO.md)** - Funcionalidades por tipo de usuario

### Para Arquitectura y Diseño
1. **[ARQUITECTURA-DESPLIEGUE-AZURE.md](ARQUITECTURA-DESPLIEGUE-AZURE.md)** - Arquitectura completa y deployment
2. **[ANALISIS-OPTIMIZACION-BD.md](ANALISIS-OPTIMIZACION-BD.md)** ⭐ **NUEVO** - Análisis y mejoras de BD
3. **[CLEAN-CODE-PRACTICAS.md](CLEAN-CODE-PRACTICAS.md)** - Principios SOLID y buenas prácticas
4. **[DIAGRAMAS.html](DIAGRAMAS.html)** - Diagramas de arquitectura, BD, flujos

### Para Deployment y DevOps
1. **[MANUAL-DESPLIEGUE.md](MANUAL-DESPLIEGUE.md)** ⭐ **NUEVO** - Guía completa de deployment
2. **[ARQUITECTURA-DESPLIEGUE-AZURE.md](ARQUITECTURA-DESPLIEGUE-AZURE.md)** - Configuración Azure
3. **[../deployment-scripts/](../deployment-scripts/)** - Scripts PowerShell para deploy
4. **[../database-scripts/](../database-scripts/)** - Scripts SQL incluyendo optimizaciones

---

## 📖 Documentos Disponibles

### 1. ⭐ MANUAL-DESPLIEGUE.md **NUEVO**
**Contenido:**
- ✅ Requisitos previos (software, cuentas, herramientas)
- ✅ Configuración entorno local paso a paso
- ✅ Configuración de Azure SQL Database
- ✅ Despliegue Backend (.NET 8 → Azure App Service)
- ✅ Despliegue Frontend (Angular → Static Web App)
- ✅ Solución de problemas comunes (troubleshooting)
- ✅ Comandos útiles (dotnet, npm, git, azure cli)
- ✅ Checklist completo de despliegue

**Secciones:**
- 📋 Requisitos (Node.js, .NET, Git, Azure CLI)
- 🖥️ Setup Local (backend + frontend + base de datos)
- ☁️ Despliegue Azure (paso a paso con scripts)
- 🔧 Troubleshooting (10+ problemas comunes resueltos)
- 📚 Comandos de referencia rápida

**Ideal para:**
- ✅ Primera instalación del proyecto
- ✅ Configurar entorno de desarrollo local
- ✅ Deployments a Azure desde cero
- ✅ Resolver problemas comunes (CORS, DB, JWT)
- ✅ Desarrolladores nuevos en el equipo

---

### 2. ⭐ ANALISIS-OPTIMIZACION-BD.md **NUEVO**
**Contenido:**
- ✅ Análisis del esquema actual (9 tablas base)
- ✅ Verificación: ¿La tabla Usuario sobra? **NO, es fundamental**
- ✅ Identificación de problemas (datos hardcodeados, falta normalización)
- ✅ Propuesta: 8 tablas maestras/catálogos nuevas
- ✅ Mejoras a tabla Pago (campos adicionales para transacciones)
- ✅ Sistema de auditoría y trazabilidad
- ✅ 12+ índices de performance
- ✅ Script de migración completo incluido

**Tablas Maestras Propuestas:**
- EstadoVehiculo (7 estados)
- EstadoSubasta (6 estados)
- Rol (4 roles con niveles)
- TipoNotificacion (10 tipos con plantillas)
- MetodoPago (4 métodos)
- EstadoPago (7 estados)
- MarcaVehiculo (18 marcas)
- TipoVehiculo (10 tipos)
- ConfiguracionSistema (parámetros dinámicos)

**Mejoras de Performance:**
- 12 índices estratégicos
- Normalización de direcciones
- Sistema de documentos

**Ideal para:**
- ✅ Entender el modelo de datos
- ✅ Escalabilidad y optimización
- ✅ Migración a tablas maestras
- ✅ Auditoría y compliance
- ✅ DBAs y arquitectos de datos

---

### 3. ARQUITECTURA-DESPLIEGUE-AZURE.md
**Contenido:**
- ✅ Arquitectura de 3 capas (Frontend, Backend, Base de Datos)
- ✅ Configuración de Azure SQL Database
- ✅ Despliegue en Azure App Service (Backend)
- ✅ Despliegue en Azure Static Web Apps (Frontend)
- ✅ Flujos de comunicación completos
- ✅ Variables de configuración
- ✅ URLs de producción y desarrollo
- ✅ Credenciales de acceso

**Ideal para:**
- Entender la arquitectura global del sistema
- Realizar deployments a Azure
- Configurar entornos de desarrollo/producción

---

### 3. ARQUITECTURA-DESPLIEGUE-AZURE.md
**Contenido:**
- ✅ Arquitectura de 3 capas (Frontend, Backend, Base de Datos)
- ✅ Configuración de Azure SQL Database
- ✅ Despliegue en Azure App Service (Backend)
- ✅ Despliegue en Azure Static Web Apps (Frontend)
- ✅ Flujos de comunicación completos
- ✅ Variables de configuración
- ✅ URLs de producción y desarrollo
- ✅ Credenciales de acceso

**Ideal para:**
- Entender la arquitectura global del sistema
- Realizar deployments a Azure
- Configurar entornos de desarrollo/producción

---

### 4. CASOS-DE-USO.md
**Contenido:**
- ✅ Actores del sistema (Usuario Registrado, Validado, Administrador)
- ✅ 12 casos de uso detallados con flujos principales y alternativos
- ✅ Precondiciones y postcondiciones
- ✅ Flujos de trabajo completos
- ✅ Reglas de negocio (10 RN documentadas)

**Casos de Uso Incluidos:**
- CU-01: Registro de Usuario
- CU-02: Subir Documento IAE
- CU-03: Ver Subastas
- CU-04: Realizar Puja
- CU-05: Ver Mis Pujas
- CU-06: Actualizar Perfil
- CU-07: Validar Usuario (Admin)
- CU-08: Gestionar Vehículos (Admin)
- CU-09: Crear Subasta (Admin)
- CU-10: Gestionar Empresas (Admin)
- CU-11: Ver Dashboard Administrativo
- CU-12: Gestionar Notificaciones

**Ideal para:**
- Entender funcionalidades del sistema
- Testeo de casos de uso
- Capacitación de nuevos usuarios
- Especificación de requisitos

### 4. CASOS-DE-USO.md
**Contenido:**
- ✅ Actores del sistema (Usuario Registrado, Validado, Administrador)
- ✅ 12 casos de uso detallados con flujos principales y alternativos
- ✅ Precondiciones y postcondiciones
- ✅ Flujos de trabajo completos
- ✅ Reglas de negocio (10 RN documentadas)

**Casos de Uso Incluidos:**
- CU-01: Registro de Usuario
- CU-02: Subir Documento IAE
- CU-03: Ver Subastas
- CU-04: Realizar Puja
- CU-05: Ver Mis Pujas
- CU-06: Actualizar Perfil
- CU-07: Validar Usuario (Admin)
- CU-08: Gestionar Vehículos (Admin)
- CU-09: Crear Subasta (Admin)
- CU-10: Gestionar Empresas (Admin)
- CU-11: Ver Dashboard Administrativo
- CU-12: Gestionar Notificaciones

**Ideal para:**
- Entender funcionalidades del sistema
- Testeo de casos de uso
- Capacitación de nuevos usuarios
- Especificación de requisitos

---

### 5. DIAGRAMAS.html
**Contenido:** (Visualización interactiva HTML)
- ✅ Diagrama de Arquitectura Clean Architecture (3 capas)
- ✅ Diagrama de Base de Datos (Entidades y relaciones)
- ✅ Diagramas de Flujo (Autenticación, Pujas)
- ✅ Diagrama de Casos de Uso por Actor
- ✅ Diagrama de Despliegue en Azure

**Características:**
- 🎨 **Interfaz interactiva** con tabs
- 🖼️ **Gráficos visuales** de arquitectura
- 📊 **Modelo de datos** completo con PK/FK
- 🔄 **Flujos paso a paso** con explicaciones
- ☁️ **Arquitectura de deployment** en Azure

**Ideal para:**
- Presentaciones y demos
- Onboarding de equipo
- Explicar arquitectura a stakeholders
- Videos de capacitación

### 5. DIAGRAMAS.html
**Contenido:** (Visualización interactiva HTML)
- ✅ Diagrama de Arquitectura Clean Architecture (3 capas)
- ✅ Diagrama de Base de Datos (Entidades y relaciones)
- ✅ Diagramas de Flujo (Autenticación, Pujas)
- ✅ Diagrama de Casos de Uso por Actor
- ✅ Diagrama de Despliegue en Azure

**Características:**
- 🎨 **Interfaz interactiva** con tabs
- 🖼️ **Gráficos visuales** de arquitectura
- 📊 **Modelo de datos** completo con PK/FK
- 🔄 **Flujos paso a paso** con explicaciones
- ☁️ **Arquitectura de deployment** en Azure

**Ideal para:**
- Presentaciones y demos
- Onboarding de equipo
- Explicar arquitectura a stakeholders
- Videos de capacitación

---

### 6. CLEAN-CODE-PRACTICAS.md
**Contenido:**
- ✅ Principios SOLID aplicados (con ejemplos de código)
- ✅ Arquitectura Clean Architecture explicada
- ✅ Patrones de diseño (Repository, DI, DTO, Service Layer)
- ✅ Convenciones de nomenclatura (.NET y TypeScript)
- ✅ Buenas prácticas de seguridad
- ✅ Manejo de errores consistente
- ✅ Code smells evitados

**Principios Cubiertos:**
- Single Responsibility Principle (SRP)
- Open/Closed Principle (OCP)
- Liskov Substitution Principle (LSP)
- Interface Segregation Principle (ISP)
- Dependency Inversion Principle (DIP)

**Ideal para:**
- Code reviews
- Capacitación en clean code
- Establecer estándares de equipo
- Refactoring guiado

---

## 🗂️ Otros Recursos

### Scripts de Base de Datos
**Ubicación:** `../database-scripts/`

- `migracion-tablas-maestras.sql` ⭐ **NUEVO** - Script completo de optimización BD
- `crear-tabla-notificaciones-admin.sql` - Crear tabla de notificaciones admin
- `fix-fk-empresa.sql` - Corregir foreign keys de empresas
- `insertar-vehiculos-subastas.sql` - Datos de ejemplo (5 vehículos con subastas)
- `verificar-actualizar-admin.sql` - Crear usuario administrador

### Scripts de Deployment
**Ubicación:** `../deployment-scripts/`

- `deploy-frontend.ps1` - Deploy frontend a Azure Static Web Apps
- `deploy-backend.ps1` - Deploy backend a Azure App Service
- `deploy-backend-completo.ps1` - Deploy completo con dependencias
- `deploy-to-azure.ps1` - Deploy full stack

---

## 🚀 Inicio Rápido por Rol

### 🎨 Frontend Developer
1. Leer **README.md** (estructura del proyecto)
2. Ver **DIAGRAMAS.html** → Tab "Arquitectura" → Sección "Frontend Angular"
3. Consultar **CLEAN-CODE-PRACTICAS.md** → Convenciones TypeScript

### 🔧 Backend Developer
1. Leer **README.md** (estructura del proyecto)
2. Ver **DIAGRAMAS.html** → Tab "Arquitectura" → Sección "Clean Architecture"
3. Revisar **CLEAN-CODE-PRACTICAS.md** → Principios SOLID
4. Consultar **ARQUITECTURA-DESPLIEGUE-AZURE.md** → Estructura del Backend

### 🗄️ Database Administrator
1. Ver **DIAGRAMAS.html** → Tab "Base de Datos"
2. Ejecutar scripts en **../database-scripts/**
3. Consultar **ARQUITECTURA-DESPLIEGUE-AZURE.md** → Azure SQL Database

### ☁️ DevOps Engineer
1. Leer **ARQUITECTURA-DESPLIEGUE-AZURE.md** completo
2. Revisar **../deployment-scripts/**
3. Ver **DIAGRAMAS.html** → Tab "Despliegue"

### 📋 Product Owner / Business Analyst
1. Leer **CASOS-DE-USO.md** completo
2. Ver **DIAGRAMAS.html** → Tab "Casos de Uso"
3. Revisar reglas de negocio en **CASOS-DE-USO.md** (sección final)

### 🎓 Nuevo en el Equipo
1. **README.md** (visión general)
2. **DIAGRAMAS.html** (explorar todos los tabs)
3. **CASOS-DE-USO.md** (entender funcionalidades)
4. **ARQUITECTURA-DESPLIEGUE-AZURE.md** (arquitectura técnica)

---

## 📞 Información de Contacto

**URLs Importantes:**
- Frontend Producción: https://blue-flower-00b3c6b03.1.azurestaticapps.net
- Backend API: https://subastaswebapi20260202162157.azurewebsites.net
- Azure Portal: https://portal.azure.com

**Repositorio:**
- Ubicación: `c:\Users\JoseAntonioVallecill\source\repos\subastas`

---

*Índice actualizado: 3 de febrero de 2026*
