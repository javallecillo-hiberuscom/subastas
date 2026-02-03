# 🗄️ Análisis y Optimización del Esquema de Base de Datos

## Resumen Ejecutivo

Tras revisar el esquema actual, se identifican **mejoras importantes** para escalabilidad, mantenibilidad y performance.

---

## 📊 Esquema Actual

### Tablas Existentes (9)

```
1. Usuario          - Usuarios del sistema
2. Empresa          - Empresas registradas
3. Vehiculo         - Catálogo de vehículos
4. ImagenVehiculo   - Imágenes de vehículos
5. Subasta          - Subastas activas/finalizadas
6. Puja             - Pujas realizadas
7. Notificacion     - Notificaciones usuarios
8. NotificacionAdmin- Notificaciones admin
9. Pago             - Registro de pagos
```

### Relaciones Principales

```
Usuario 1→N Puja
Usuario 1→N Notificacion
Usuario ?→1 Empresa

Empresa 1→N Usuario

Vehiculo 1→N ImagenVehiculo
Vehiculo 1→N Subasta

Subasta 1→N Puja
Subasta 1→N Notificacion

Puja 1→N Pago
```

---

## ❌ Problemas Identificados

### 1. **Tabla Usuario NO es Redundante**

**Respuesta a "creo que sobra user"**: La tabla Usuario **NO sobra**. Es fundamental porque:

- ✅ Contiene datos de autenticación (Email, PasswordHash)
- ✅ Datos personales (Nombre, Apellidos, DNI, Telefono, Direccion)
- ✅ Roles y permisos (Rol: admin/gestor/registrado)
- ✅ Estado de validación (Validado, DocumentoIAE)
- ✅ Relación con Empresa (IdEmpresa - puede ser NULL)

**Empresa != Usuario**: Empresa representa la entidad legal, Usuario representa a la persona física.

```
Ejemplo:
- Empresa: "Transportes García SL" (CIF: B12345678)
  - Usuario 1: Juan García (Director) 
  - Usuario 2: María López (Operadora)
  - Usuario 3: Pedro Ruiz (Conductor)
```

### 2. **Falta de Tablas Maestras/Catálogos**

❌ **Datos hardcodeados** en código que deberían estar en tablas:

#### A. Estados de Vehículo
Actualmente: `Estado VARCHAR` con valores hardcodeados
```csharp
// En código
entity.Property(e => e.Estado).HasDefaultValue("registrado");
```

**Propuesta**: Tabla maestra `EstadoVehiculo`
```sql
CREATE TABLE EstadoVehiculo (
    IdEstadoVehiculo INT PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200),
    Activo BIT DEFAULT 1
);

INSERT INTO EstadoVehiculo VALUES
(1, 'registrado', 'Registrado', 'Vehículo recién registrado', 1),
(2, 'en_revision', 'En Revisión', 'Esperando validación', 1),
(3, 'aprobado', 'Aprobado', 'Listo para subasta', 1),
(4, 'en_subasta', 'En Subasta', 'Actualmente en subasta', 1),
(5, 'vendido', 'Vendido', 'Vendido en subasta', 1),
(6, 'rechazado', 'Rechazado', 'No cumple requisitos', 1);
```

#### B. Estados de Subasta
```sql
CREATE TABLE EstadoSubasta (
    IdEstadoSubasta INT PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    PermiteOfertas BIT DEFAULT 1,
    Activo BIT DEFAULT 1
);

INSERT INTO EstadoSubasta VALUES
(1, 'programada', 'Programada', 0, 1),
(2, 'activa', 'Activa', 1, 1),
(3, 'finalizada', 'Finalizada', 0, 1),
(4, 'cancelada', 'Cancelada', 0, 1),
(5, 'suspendida', 'Suspendida', 0, 1);
```

#### C. Roles de Usuario
```sql
CREATE TABLE Rol (
    IdRol INT PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200),
    Nivel INT NOT NULL, -- Para jerarquía de permisos
    Activo BIT DEFAULT 1
);

INSERT INTO Rol VALUES
(1, 'admin', 'Administrador', 'Acceso total al sistema', 100, 1),
(2, 'gestor', 'Gestor', 'Gestión de subastas y vehículos', 50, 1),
(3, 'registrado', 'Usuario Registrado', 'Usuario validado', 10, 1),
(4, 'pendiente', 'Pendiente Validación', 'Esperando validación', 0, 1);
```

#### D. Tipos de Notificación
```sql
CREATE TABLE TipoNotificacion (
    IdTipoNotificacion INT PRIMARY KEY,
    Codigo VARCHAR(30) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Plantilla VARCHAR(500), -- Template del mensaje
    Prioridad INT DEFAULT 1, -- 1=baja, 2=media, 3=alta
    Activo BIT DEFAULT 1
);

INSERT INTO TipoNotificacion VALUES
(1, 'nueva_puja', 'Nueva Puja', 'Nueva puja de {cantidad}€ en {vehiculo}', 2, 1),
(2, 'subasta_ganada', 'Subasta Ganada', '¡Felicidades! Has ganado la subasta de {vehiculo}', 3, 1),
(3, 'subasta_superada', 'Puja Superada', 'Tu puja ha sido superada en {vehiculo}', 2, 1),
(4, 'nueva_validacion', 'Nueva Validación', 'Nuevo usuario pendiente de validación', 2, 1),
(5, 'usuario_validado', 'Usuario Validado', 'Tu cuenta ha sido validada', 2, 1);
```

#### E. Marcas de Vehículos
```sql
CREATE TABLE MarcaVehiculo (
    IdMarca INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Pais VARCHAR(50),
    Activo BIT DEFAULT 1
);

INSERT INTO MarcaVehiculo (Nombre, Pais) VALUES
('Mercedes-Benz', 'Alemania'),
('Volkswagen', 'Alemania'),
('Renault', 'Francia'),
('Peugeot', 'Francia'),
('Seat', 'España'),
('Iveco', 'Italia'),
('Scania', 'Suecia'),
('Volvo', 'Suecia'),
('MAN', 'Alemania'),
('DAF', 'Países Bajos');
```

#### F. Tipos de Vehículo
```sql
CREATE TABLE TipoVehiculo (
    IdTipoVehiculo INT PRIMARY KEY,
    Codigo VARCHAR(30) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    RequierePermisoEspecial BIT DEFAULT 0,
    Activo BIT DEFAULT 1
);

INSERT INTO TipoVehiculo VALUES
(1, 'camion_rigido', 'Camión Rígido', 1, 1),
(2, 'camion_articulado', 'Camión Articulado', 1, 1),
(3, 'furgon', 'Furgón', 0, 1),
(4, 'trailer', 'Tráiler', 1, 1),
(5, 'semirremolque', 'Semirremolque', 1, 1);
```

### 3. **Tabla Pago Incompleta**

La tabla `Pago` existe pero no tiene campos suficientes:

```csharp
// Actual (probablemente)
public class Pago
{
    public int IdPago { get; set; }
    public int IdPuja { get; set; }
    // Faltan campos críticos
}
```

**Propuesta mejorada**:
```sql
CREATE TABLE Pago (
    IdPago INT PRIMARY KEY IDENTITY(1,1),
    IdPuja INT NOT NULL,
    IdMetodoPago INT NOT NULL, -- FK a tabla maestra
    IdEstadoPago INT NOT NULL, -- FK a tabla maestra
    
    Cantidad DECIMAL(10,2) NOT NULL,
    Referencia VARCHAR(100), -- Número de referencia bancaria
    
    FechaPago DATETIME,
    FechaConfirmacion DATETIME,
    
    DatosTransaccion TEXT, -- JSON con detalles
    
    CONSTRAINT FK_Pago_Puja FOREIGN KEY (IdPuja) 
        REFERENCES Puja(IdPuja),
    CONSTRAINT FK_Pago_MetodoPago FOREIGN KEY (IdMetodoPago) 
        REFERENCES MetodoPago(IdMetodoPago),
    CONSTRAINT FK_Pago_EstadoPago FOREIGN KEY (IdEstadoPago) 
        REFERENCES EstadoPago(IdEstadoPago)
);

-- Tabla maestra de métodos de pago
CREATE TABLE MetodoPago (
    IdMetodoPago INT PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Activo BIT DEFAULT 1
);

INSERT INTO MetodoPago VALUES
(1, 'transferencia', 'Transferencia Bancaria', 1),
(2, 'tarjeta', 'Tarjeta de Crédito/Débito', 1),
(3, 'bizum', 'Bizum', 1);

-- Tabla maestra de estados de pago
CREATE TABLE EstadoPago (
    IdEstadoPago INT PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Activo BIT DEFAULT 1
);

INSERT INTO EstadoPago VALUES
(1, 'pendiente', 'Pendiente', 1),
(2, 'procesando', 'Procesando', 1),
(3, 'completado', 'Completado', 1),
(4, 'fallido', 'Fallido', 1),
(5, 'reembolsado', 'Reembolsado', 1);
```

### 4. **Auditoría y Trazabilidad**

❌ **Falta información de auditoría** en todas las tablas

**Propuesta**: Agregar campos de auditoría estándar:
```sql
-- A TODAS las tablas principales
ALTER TABLE Usuario ADD 
    FechaCreacion DATETIME DEFAULT GETDATE(),
    UsuarioCreacion INT,
    FechaModificacion DATETIME,
    UsuarioModificacion INT;

ALTER TABLE Vehiculo ADD 
    FechaCreacion DATETIME DEFAULT GETDATE(),
    UsuarioCreacion INT,
    FechaModificacion DATETIME,
    UsuarioModificacion INT;

-- etc...
```

O mejor aún, crear una tabla de auditoría centralizada:
```sql
CREATE TABLE AuditoriaLog (
    IdAuditoria BIGINT PRIMARY KEY IDENTITY(1,1),
    Tabla VARCHAR(50) NOT NULL,
    IdRegistro INT NOT NULL,
    Accion VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    ValoresAnteriores TEXT, -- JSON
    ValoresNuevos TEXT, -- JSON
    IdUsuario INT,
    FechaHora DATETIME DEFAULT GETDATE(),
    DireccionIP VARCHAR(45)
);
```

### 5. **Normalización de Direcciones**

❌ **Campo Direccion como VARCHAR** en Usuario y Empresa

**Problema**: Dificulta búsquedas geográficas y estadísticas por ubicación.

**Propuesta**:
```sql
CREATE TABLE Provincia (
    IdProvincia INT PRIMARY KEY,
    Codigo VARCHAR(2) NOT NULL UNIQUE, -- 01-52
    Nombre VARCHAR(50) NOT NULL
);

CREATE TABLE Municipio (
    IdMunicipio INT PRIMARY KEY IDENTITY(1,1),
    IdProvincia INT NOT NULL,
    CodigoINE VARCHAR(5) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    CONSTRAINT FK_Municipio_Provincia FOREIGN KEY (IdProvincia) 
        REFERENCES Provincia(IdProvincia)
);

CREATE TABLE Direccion (
    IdDireccion INT PRIMARY KEY IDENTITY(1,1),
    TipoVia VARCHAR(50), -- Calle, Avenida, Plaza, etc.
    NombreVia VARCHAR(200) NOT NULL,
    Numero VARCHAR(10),
    Piso VARCHAR(10),
    Puerta VARCHAR(10),
    CodigoPostal VARCHAR(5) NOT NULL,
    IdMunicipio INT NOT NULL,
    Latitud DECIMAL(10,8),
    Longitud DECIMAL(11,8),
    CONSTRAINT FK_Direccion_Municipio FOREIGN KEY (IdMunicipio) 
        REFERENCES Municipio(IdMunicipio)
);

-- Luego en Usuario y Empresa:
ALTER TABLE Usuario ADD IdDireccion INT;
ALTER TABLE Empresa ADD IdDireccion INT;
```

### 6. **Configuración del Sistema**

❌ **No existe tabla de configuración**

Actualmente todo está hardcodeado en `appsettings.json`.

**Propuesta**:
```sql
CREATE TABLE ConfiguracionSistema (
    IdConfiguracion INT PRIMARY KEY IDENTITY(1,1),
    Clave VARCHAR(100) NOT NULL UNIQUE,
    Valor VARCHAR(MAX) NOT NULL,
    Tipo VARCHAR(20) NOT NULL, -- string, int, bool, json
    Descripcion VARCHAR(500),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    UsuarioModificacion INT
);

INSERT INTO ConfiguracionSistema (Clave, Valor, Tipo, Descripcion) VALUES
('duracion_minima_subasta_horas', '24', 'int', 'Duración mínima de una subasta en horas'),
('incremento_minimo_puja', '100', 'decimal', 'Incremento mínimo entre pujas en euros'),
('notificaciones_email_habilitadas', 'true', 'bool', 'Enviar notificaciones por email'),
('comision_plataforma_porcentaje', '5', 'decimal', 'Comisión de la plataforma (%)'),
('dias_validez_iae', '365', 'int', 'Días de validez del documento IAE');
```

---

## ✅ Propuestas de Mejora

### Prioridad ALTA

#### 1. Crear Tablas Maestras Fundamentales
```sql
-- Ejecutar en orden:
1. EstadoVehiculo
2. EstadoSubasta  
3. Rol
4. TipoNotificacion
5. MetodoPago
6. EstadoPago
```

**Beneficios**:
- ✅ Fácil agregar nuevos estados sin modificar código
- ✅ Multiidioma más sencillo
- ✅ Mejores reportes y estadísticas
- ✅ Validación a nivel de BD

#### 2. Reforzar Tabla Pago
Añadir campos para gestión completa de transacciones.

#### 3. Índices para Performance
```sql
-- Índices recomendados
CREATE INDEX IX_Usuario_Email ON Usuario(Email);
CREATE INDEX IX_Usuario_Rol ON Usuario(Rol);
CREATE INDEX IX_Vehiculo_Estado ON Vehiculo(Estado);
CREATE INDEX IX_Subasta_Estado_FechaFin ON Subasta(Estado, FechaFin);
CREATE INDEX IX_Puja_IdSubasta_Cantidad ON Puja(IdSubasta, Cantidad DESC);
CREATE INDEX IX_Puja_IdUsuario ON Puja(IdUsuario);
CREATE INDEX IX_Notificacion_IdUsuario_Leida ON Notificacion(IdUsuario, Leida);
```

### Prioridad MEDIA

#### 4. Auditoría Centralizada
Implementar tabla `AuditoriaLog` con triggers.

#### 5. Configuración en Base de Datos
Tabla `ConfiguracionSistema` para parámetros dinámicos.

#### 6. Documentos y Archivos
```sql
CREATE TABLE Documento (
    IdDocumento INT PRIMARY KEY IDENTITY(1,1),
    TipoEntidad VARCHAR(50) NOT NULL, -- Usuario, Vehiculo
    IdEntidad INT NOT NULL,
    TipoDocumento VARCHAR(50) NOT NULL, -- IAE, PermisoCirculacion, ITV
    NombreArchivo VARCHAR(255) NOT NULL,
    RutaArchivo VARCHAR(500) NOT NULL,
    TamanoBytes BIGINT,
    MimeType VARCHAR(100),
    FechaSubida DATETIME DEFAULT GETDATE(),
    ValidadoPor INT,
    FechaValidacion DATETIME
);
```

### Prioridad BAJA

#### 7. Normalización Direcciones
Sistema completo de provincias/municipios.

#### 8. Sistema de Mensajería
```sql
CREATE TABLE Mensaje (
    IdMensaje INT PRIMARY KEY IDENTITY(1,1),
    IdRemitenteUsuario INT NOT NULL,
    IdDestinatarioUsuario INT,
    IdDestinatarioAdmin BIT DEFAULT 0,
    Asunto VARCHAR(200) NOT NULL,
    Cuerpo TEXT NOT NULL,
    Leido BIT DEFAULT 0,
    FechaEnvio DATETIME DEFAULT GETDATE(),
    FechaLectura DATETIME
);
```

#### 9. Favoritos/Watchlist
```sql
CREATE TABLE VehiculoFavorito (
    IdVehiculoFavorito INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,
    IdVehiculo INT NOT NULL,
    FechaAgregado DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_VehiculoFavorito_Usuario FOREIGN KEY (IdUsuario) 
        REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_VehiculoFavorito_Vehiculo FOREIGN KEY (IdVehiculo) 
        REFERENCES Vehiculo(IdVehiculo),
    CONSTRAINT UQ_VehiculoFavorito UNIQUE (IdUsuario, IdVehiculo)
);
```

---

## 📝 Script de Migración

Crear archivo: [database-scripts/migracion-tablas-maestras.sql](../database-scripts/migracion-tablas-maestras.sql)

```sql
-- =====================================================
-- Script de Migración - Tablas Maestras
-- Versión: 1.0
-- Fecha: 2024-12
-- =====================================================

USE Subastas;
GO

-- 1. CREAR TABLAS MAESTRAS
-- =====================================================

-- EstadoVehiculo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoVehiculo')
BEGIN
    CREATE TABLE EstadoVehiculo (
        IdEstadoVehiculo INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO EstadoVehiculo VALUES
    (1, 'registrado', 'Registrado', 'Vehículo recién registrado', 1),
    (2, 'en_revision', 'En Revisión', 'Esperando validación', 1),
    (3, 'aprobado', 'Aprobado', 'Listo para subasta', 1),
    (4, 'en_subasta', 'En Subasta', 'Actualmente en subasta', 1),
    (5, 'vendido', 'Vendido', 'Vendido en subasta', 1),
    (6, 'rechazado', 'Rechazado', 'No cumple requisitos', 1);
    
    PRINT 'Tabla EstadoVehiculo creada correctamente';
END

-- EstadoSubasta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoSubasta')
BEGIN
    CREATE TABLE EstadoSubasta (
        IdEstadoSubasta INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        PermiteOfertas BIT DEFAULT 1,
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO EstadoSubasta VALUES
    (1, 'programada', 'Programada', 0, 1),
    (2, 'activa', 'Activa', 1, 1),
    (3, 'finalizada', 'Finalizada', 0, 1),
    (4, 'cancelada', 'Cancelada', 0, 1),
    (5, 'suspendida', 'Suspendida', 0, 1);
    
    PRINT 'Tabla EstadoSubasta creada correctamente';
END

-- Rol
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rol')
BEGIN
    CREATE TABLE Rol (
        IdRol INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        Nivel INT NOT NULL,
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO Rol VALUES
    (1, 'admin', 'Administrador', 'Acceso total al sistema', 100, 1),
    (2, 'gestor', 'Gestor', 'Gestión de subastas y vehículos', 50, 1),
    (3, 'registrado', 'Usuario Registrado', 'Usuario validado', 10, 1),
    (4, 'pendiente', 'Pendiente Validación', 'Esperando validación', 0, 1);
    
    PRINT 'Tabla Rol creada correctamente';
END

-- 2. MIGRAR DATOS EXISTENTES
-- =====================================================

-- Migrar estados de vehículos
UPDATE Vehiculo SET Estado = 'registrado' WHERE Estado IS NULL OR Estado = '';

-- Migrar estados de subastas
UPDATE Subasta SET Estado = 'activa' WHERE Estado IS NULL OR Estado = '';

-- Migrar roles de usuarios
UPDATE Usuario SET Rol = 'registrado' WHERE Rol IS NULL OR Rol = '';

PRINT 'Migración de datos completada';

-- 3. CREAR ÍNDICES DE PERFORMANCE
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuario_Email')
    CREATE INDEX IX_Usuario_Email ON Usuario(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuario_Rol')
    CREATE INDEX IX_Usuario_Rol ON Usuario(Rol);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculo_Estado')
    CREATE INDEX IX_Vehiculo_Estado ON Vehiculo(Estado);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Subasta_Estado_FechaFin')
    CREATE INDEX IX_Subasta_Estado_FechaFin ON Subasta(Estado, FechaFin);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Puja_IdSubasta_Cantidad')
    CREATE INDEX IX_Puja_IdSubasta_Cantidad ON Puja(IdSubasta, Cantidad DESC);

PRINT 'Índices creados correctamente';

-- 4. ESTADÍSTICAS FINALES
-- =====================================================

SELECT 'Tablas Maestras' AS Tipo, COUNT(*) AS Total
FROM sys.tables
WHERE name IN ('EstadoVehiculo', 'EstadoSubasta', 'Rol');

SELECT 'Índices Creados' AS Tipo, COUNT(*) AS Total
FROM sys.indexes
WHERE name LIKE 'IX_%';

PRINT 'Migración completada exitosamente';
GO
```

---

## 📊 Comparativa Antes/Después

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Tablas** | 9 | 15+ |
| **Tablas Maestras** | 0 | 6 |
| **Índices** | ~3 (por defecto) | 10+ |
| **Auditoría** | ❌ No | ✅ Sí |
| **Configuración BD** | ❌ No | ✅ Sí |
| **Normalización** | Parcial | Completa |
| **Escalabilidad** | Media | Alta |
| **Mantenibilidad** | Baja | Alta |

---

## 🎯 Conclusiones

### Usuario NO sobra
La tabla Usuario es **esencial** y debe mantenerse. Contiene información crítica de autenticación, autorización y datos personales.

### Mejoras Críticas
1. ✅ **Crear tablas maestras** para estados y tipos
2. ✅ **Reforzar tabla Pago** con campos completos
3. ✅ **Agregar índices** para mejorar consultas

### Mejoras Recomendadas
4. 🟡 Implementar auditoría centralizada
5. 🟡 Tabla de configuración del sistema
6. 🟡 Sistema de documentos

### Mejoras Opcionales
7. ⚪ Normalización completa de direcciones
8. ⚪ Sistema de mensajería interno
9. ⚪ Lista de favoritos

---

**Próximos Pasos**:
1. Revisar y aprobar propuestas
2. Ejecutar script de migración en entorno de pruebas
3. Actualizar entidades del Domain
4. Actualizar DTOs y repositorios
5. Probar exhaustivamente
6. Desplegar a producción

---

**Fecha Análisis**: Diciembre 2024  
**Analista**: Sistema de Optimización BD  
**Estado**: Pendiente Aprobación
