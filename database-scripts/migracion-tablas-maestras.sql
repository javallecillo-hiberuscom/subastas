-- =====================================================
-- Script de Migración - Tablas Maestras y Optimizaciones
-- Proyecto: Sistema de Subastas
-- Versión: 1.0
-- Fecha: Diciembre 2024
-- =====================================================

USE Subastas;
GO

PRINT '========================================';
PRINT 'INICIO MIGRACIÓN TABLAS MAESTRAS';
PRINT '========================================';
PRINT '';

-- =====================================================
-- 1. CREAR TABLAS MAESTRAS
-- =====================================================

PRINT '1. Creando tablas maestras...';

-- EstadoVehiculo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoVehiculo')
BEGIN
    CREATE TABLE EstadoVehiculo (
        IdEstadoVehiculo INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        Activo BIT DEFAULT 1,
        Orden INT DEFAULT 0
    );
    
    INSERT INTO EstadoVehiculo (IdEstadoVehiculo, Codigo, Nombre, Descripcion, Orden) VALUES
    (1, 'registrado', 'Registrado', 'Vehículo recién registrado en el sistema', 1),
    (2, 'en_revision', 'En Revisión', 'Documentación en proceso de validación', 2),
    (3, 'aprobado', 'Aprobado', 'Vehículo validado y listo para subasta', 3),
    (4, 'en_subasta', 'En Subasta', 'Actualmente participando en subasta activa', 4),
    (5, 'vendido', 'Vendido', 'Vehículo vendido en subasta', 5),
    (6, 'rechazado', 'Rechazado', 'No cumple requisitos para subasta', 6),
    (7, 'inactivo', 'Inactivo', 'Vehículo dado de baja', 7);
    
    PRINT '  ✓ Tabla EstadoVehiculo creada (7 registros)';
END
ELSE
    PRINT '  - EstadoVehiculo ya existe';

-- EstadoSubasta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoSubasta')
BEGIN
    CREATE TABLE EstadoSubasta (
        IdEstadoSubasta INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        PermiteOfertas BIT DEFAULT 0,
        Activo BIT DEFAULT 1,
        Orden INT DEFAULT 0
    );
    
    INSERT INTO EstadoSubasta (IdEstadoSubasta, Codigo, Nombre, Descripcion, PermiteOfertas, Orden) VALUES
    (1, 'programada', 'Programada', 'Subasta programada para fecha futura', 0, 1),
    (2, 'activa', 'Activa', 'Subasta en curso aceptando ofertas', 1, 2),
    (3, 'finalizada', 'Finalizada', 'Subasta concluida con ganador', 0, 3),
    (4, 'cancelada', 'Cancelada', 'Subasta cancelada por administrador', 0, 4),
    (5, 'suspendida', 'Suspendida', 'Subasta pausada temporalmente', 0, 5),
    (6, 'desierta', 'Desierta', 'Finalizada sin pujas', 0, 6);
    
    PRINT '  ✓ Tabla EstadoSubasta creada (6 registros)';
END
ELSE
    PRINT '  - EstadoSubasta ya existe';

-- Rol
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rol')
BEGIN
    CREATE TABLE Rol (
        IdRol INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        Nivel INT NOT NULL, -- Nivel de privilegios (mayor = más privilegios)
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO Rol (IdRol, Codigo, Nombre, Descripcion, Nivel) VALUES
    (1, 'admin', 'Administrador', 'Acceso total al sistema con capacidad de gestión completa', 100),
    (2, 'gestor', 'Gestor', 'Gestión de subastas, vehículos y validación de usuarios', 50),
    (3, 'registrado', 'Usuario Registrado', 'Usuario validado con acceso completo a subastas', 10),
    (4, 'pendiente', 'Pendiente Validación', 'Usuario registrado esperando validación de documentación', 0);
    
    PRINT '  ✓ Tabla Rol creada (4 registros)';
END
ELSE
    PRINT '  - Rol ya existe';

-- TipoNotificacion
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoNotificacion')
BEGIN
    CREATE TABLE TipoNotificacion (
        IdTipoNotificacion INT PRIMARY KEY,
        Codigo VARCHAR(30) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Plantilla VARCHAR(500), -- Template del mensaje con placeholders
        Prioridad INT DEFAULT 1, -- 1=baja, 2=media, 3=alta, 4=crítica
        RequiereAccion BIT DEFAULT 0,
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO TipoNotificacion (IdTipoNotificacion, Codigo, Nombre, Plantilla, Prioridad, RequiereAccion) VALUES
    (1, 'nueva_puja', 'Nueva Puja', 'Nueva puja de {cantidad}€ en {vehiculo}', 2, 0),
    (2, 'subasta_ganada', 'Subasta Ganada', '¡Felicidades! Has ganado la subasta de {vehiculo} por {cantidad}€', 3, 1),
    (3, 'subasta_superada', 'Puja Superada', 'Tu puja ha sido superada en {vehiculo}. Nueva puja: {cantidad}€', 2, 0),
    (4, 'nueva_validacion', 'Validación Pendiente', 'Nuevo usuario {nombre} pendiente de validación', 2, 1),
    (5, 'usuario_validado', 'Cuenta Validada', 'Tu cuenta ha sido validada. Ya puedes participar en subastas', 2, 0),
    (6, 'usuario_rechazado', 'Validación Rechazada', 'Tu solicitud ha sido rechazada. Motivo: {motivo}', 3, 0),
    (7, 'subasta_iniciada', 'Subasta Iniciada', 'La subasta de {vehiculo} ha comenzado', 2, 0),
    (8, 'subasta_finalizada', 'Subasta Finalizada', 'La subasta de {vehiculo} ha finalizado', 2, 0),
    (9, 'recordatorio_pago', 'Recordatorio de Pago', 'Tienes un pago pendiente de {cantidad}€ por {vehiculo}', 3, 1),
    (10, 'pago_confirmado', 'Pago Confirmado', 'Tu pago de {cantidad}€ ha sido confirmado', 2, 0);
    
    PRINT '  ✓ Tabla TipoNotificacion creada (10 registros)';
END
ELSE
    PRINT '  - TipoNotificacion ya existe';

-- MetodoPago
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetodoPago')
BEGIN
    CREATE TABLE MetodoPago (
        IdMetodoPago INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        ComisionPorcentaje DECIMAL(5,2) DEFAULT 0,
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO MetodoPago (IdMetodoPago, Codigo, Nombre, Descripcion, ComisionPorcentaje) VALUES
    (1, 'transferencia', 'Transferencia Bancaria', 'Transferencia bancaria nacional', 0),
    (2, 'tarjeta', 'Tarjeta de Crédito/Débito', 'Pago con tarjeta Visa/Mastercard', 1.5),
    (3, 'bizum', 'Bizum', 'Pago instantáneo Bizum', 0.5),
    (4, 'efectivo', 'Efectivo', 'Pago en efectivo en oficina', 0);
    
    PRINT '  ✓ Tabla MetodoPago creada (4 registros)';
END
ELSE
    PRINT '  - MetodoPago ya existe';

-- EstadoPago
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadoPago')
BEGIN
    CREATE TABLE EstadoPago (
        IdEstadoPago INT PRIMARY KEY,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        EsFinal BIT DEFAULT 0, -- Indica si es un estado terminal
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO EstadoPago (IdEstadoPago, Codigo, Nombre, Descripcion, EsFinal) VALUES
    (1, 'pendiente', 'Pendiente', 'Pago pendiente de iniciar', 0),
    (2, 'procesando', 'Procesando', 'Pago en proceso de verificación', 0),
    (3, 'completado', 'Completado', 'Pago completado exitosamente', 1),
    (4, 'fallido', 'Fallido', 'Pago rechazado o fallido', 1),
    (5, 'cancelado', 'Cancelado', 'Pago cancelado por usuario', 1),
    (6, 'reembolsado', 'Reembolsado', 'Pago devuelto al usuario', 1),
    (7, 'en_disputa', 'En Disputa', 'Pago en proceso de reclamación', 0);
    
    PRINT '  ✓ Tabla EstadoPago creada (7 registros)';
END
ELSE
    PRINT '  - EstadoPago ya existe';

-- MarcaVehiculo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MarcaVehiculo')
BEGIN
    CREATE TABLE MarcaVehiculo (
        IdMarca INT PRIMARY KEY IDENTITY(1,1),
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        Pais VARCHAR(50),
        Logo VARCHAR(255), -- URL del logo
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO MarcaVehiculo (Nombre, Pais) VALUES
    ('Mercedes-Benz', 'Alemania'),
    ('Volkswagen', 'Alemania'),
    ('Renault', 'Francia'),
    ('Peugeot', 'Francia'),
    ('Citroën', 'Francia'),
    ('Seat', 'España'),
    ('Iveco', 'Italia'),
    ('Scania', 'Suecia'),
    ('Volvo', 'Suecia'),
    ('MAN', 'Alemania'),
    ('DAF', 'Países Bajos'),
    ('Ford', 'Estados Unidos'),
    ('Opel', 'Alemania'),
    ('Fiat', 'Italia'),
    ('Nissan', 'Japón'),
    ('Toyota', 'Japón'),
    ('Isuzu', 'Japón'),
    ('Mitsubishi', 'Japón');
    
    PRINT '  ✓ Tabla MarcaVehiculo creada (18 registros)';
END
ELSE
    PRINT '  - MarcaVehiculo ya existe';

-- TipoVehiculo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoVehiculo')
BEGIN
    CREATE TABLE TipoVehiculo (
        IdTipoVehiculo INT PRIMARY KEY,
        Codigo VARCHAR(30) NOT NULL UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        Descripcion VARCHAR(200),
        RequierePermisoEspecial BIT DEFAULT 0,
        Activo BIT DEFAULT 1
    );
    
    INSERT INTO TipoVehiculo (IdTipoVehiculo, Codigo, Nombre, Descripcion, RequierePermisoEspecial) VALUES
    (1, 'camion_rigido', 'Camión Rígido', 'Camión de estructura rígida sin articulación', 1),
    (2, 'camion_articulado', 'Camión Articulado', 'Camión con cabeza tractora y remolque', 1),
    (3, 'furgon', 'Furgón', 'Vehículo de transporte cerrado tipo furgoneta', 0),
    (4, 'furgoneta', 'Furgoneta', 'Vehículo comercial ligero', 0),
    (5, 'trailer', 'Tráiler', 'Remolque de gran tonelaje', 1),
    (6, 'semirremolque', 'Semirremolque', 'Remolque sin eje delantero', 1),
    (7, 'chasis_cabina', 'Chasis Cabina', 'Cabina con chasis para adaptaciones', 1),
    (8, 'frigorifico', 'Frigorífico', 'Vehículo con cámara frigorífica', 1),
    (9, 'cisterna', 'Cisterna', 'Vehículo para transporte de líquidos', 1),
    (10, 'portavehiculos', 'Portavehículos', 'Plataforma para transporte de vehículos', 1);
    
    PRINT '  ✓ Tabla TipoVehiculo creada (10 registros)';
END
ELSE
    PRINT '  - TipoVehiculo ya existe';

PRINT '';

-- =====================================================
-- 2. MEJORAR TABLA PAGO
-- =====================================================

PRINT '2. Mejorando tabla Pago...';

-- Verificar si Pago necesita campos adicionales
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdMetodoPago')
BEGIN
    ALTER TABLE Pago ADD 
        IdMetodoPago INT,
        IdEstadoPago INT DEFAULT 1,
        Cantidad DECIMAL(10,2),
        Referencia VARCHAR(100),
        FechaPago DATETIME,
        FechaConfirmacion DATETIME,
        DatosTransaccion TEXT;
    
    PRINT '  ✓ Campos adicionales agregados a Pago';
END
ELSE
    PRINT '  - Pago ya tiene los campos necesarios';

PRINT '';

-- =====================================================
-- 3. MIGRAR DATOS EXISTENTES
-- =====================================================

PRINT '3. Migrando datos existentes...';

-- Migrar estados de vehículos (normalizar valores)
UPDATE Vehiculo SET Estado = 'registrado' 
WHERE Estado IS NULL OR Estado = '' OR LTRIM(RTRIM(Estado)) = '';
PRINT '  ✓ Estados de Vehiculo normalizados';

-- Migrar estados de subastas
UPDATE Subasta SET Estado = 'activa' 
WHERE Estado IS NULL OR Estado = '' OR LTRIM(RTRIM(Estado)) = '';
PRINT '  ✓ Estados de Subasta normalizados';

-- Migrar roles de usuarios
UPDATE Usuario SET Rol = 'registrado' 
WHERE Rol IS NULL OR Rol = '' OR LTRIM(RTRIM(Rol)) = '';
PRINT '  ✓ Roles de Usuario normalizados';

PRINT '';

-- =====================================================
-- 4. CREAR ÍNDICES DE PERFORMANCE
-- =====================================================

PRINT '4. Creando índices de rendimiento...';

-- Usuario
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuario_Email' AND object_id = OBJECT_ID('Usuario'))
BEGIN
    CREATE INDEX IX_Usuario_Email ON Usuario(Email);
    PRINT '  ✓ IX_Usuario_Email';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuario_Rol' AND object_id = OBJECT_ID('Usuario'))
BEGIN
    CREATE INDEX IX_Usuario_Rol ON Usuario(Rol);
    PRINT '  ✓ IX_Usuario_Rol';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuario_Validado' AND object_id = OBJECT_ID('Usuario'))
BEGIN
    CREATE INDEX IX_Usuario_Validado ON Usuario(Validado);
    PRINT '  ✓ IX_Usuario_Validado';
END

-- Vehiculo
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculo_Estado' AND object_id = OBJECT_ID('Vehiculo'))
BEGIN
    CREATE INDEX IX_Vehiculo_Estado ON Vehiculo(Estado);
    PRINT '  ✓ IX_Vehiculo_Estado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculo_Marca_Modelo' AND object_id = OBJECT_ID('Vehiculo'))
BEGIN
    CREATE INDEX IX_Vehiculo_Marca_Modelo ON Vehiculo(Marca, Modelo);
    PRINT '  ✓ IX_Vehiculo_Marca_Modelo';
END

-- Subasta
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Subasta_Estado_FechaFin' AND object_id = OBJECT_ID('Subasta'))
BEGIN
    CREATE INDEX IX_Subasta_Estado_FechaFin ON Subasta(Estado, FechaFin);
    PRINT '  ✓ IX_Subasta_Estado_FechaFin';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Subasta_IdVehiculo' AND object_id = OBJECT_ID('Subasta'))
BEGIN
    CREATE INDEX IX_Subasta_IdVehiculo ON Subasta(IdVehiculo);
    PRINT '  ✓ IX_Subasta_IdVehiculo';
END

-- Puja
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Puja_IdSubasta_Cantidad' AND object_id = OBJECT_ID('Puja'))
BEGIN
    CREATE INDEX IX_Puja_IdSubasta_Cantidad ON Puja(IdSubasta, Cantidad DESC);
    PRINT '  ✓ IX_Puja_IdSubasta_Cantidad';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Puja_IdUsuario' AND object_id = OBJECT_ID('Puja'))
BEGIN
    CREATE INDEX IX_Puja_IdUsuario ON Puja(IdUsuario);
    PRINT '  ✓ IX_Puja_IdUsuario';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Puja_FechaPuja' AND object_id = OBJECT_ID('Puja'))
BEGIN
    CREATE INDEX IX_Puja_FechaPuja ON Puja(FechaPuja DESC);
    PRINT '  ✓ IX_Puja_FechaPuja';
END

-- Notificacion
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notificacion_IdUsuario_Leida' AND object_id = OBJECT_ID('Notificacion'))
BEGIN
    CREATE INDEX IX_Notificacion_IdUsuario_Leida ON Notificacion(IdUsuario, Leida);
    PRINT '  ✓ IX_Notificacion_IdUsuario_Leida';
END

-- NotificacionAdmin
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotificacionAdmin_Leida' AND object_id = OBJECT_ID('NotificacionAdmin'))
BEGIN
    CREATE INDEX IX_NotificacionAdmin_Leida ON NotificacionAdmin(Leida);
    PRINT '  ✓ IX_NotificacionAdmin_Leida';
END

PRINT '';

-- =====================================================
-- 5. CREAR TABLA DE CONFIGURACIÓN (OPCIONAL)
-- =====================================================

PRINT '5. Creando tabla de configuración del sistema...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConfiguracionSistema')
BEGIN
    CREATE TABLE ConfiguracionSistema (
        IdConfiguracion INT PRIMARY KEY IDENTITY(1,1),
        Clave VARCHAR(100) NOT NULL UNIQUE,
        Valor VARCHAR(MAX) NOT NULL,
        Tipo VARCHAR(20) NOT NULL, -- string, int, bool, decimal, json
        Descripcion VARCHAR(500),
        FechaModificacion DATETIME DEFAULT GETDATE(),
        UsuarioModificacion INT
    );
    
    INSERT INTO ConfiguracionSistema (Clave, Valor, Tipo, Descripcion) VALUES
    ('duracion_minima_subasta_horas', '24', 'int', 'Duración mínima de una subasta en horas'),
    ('incremento_minimo_puja', '100', 'decimal', 'Incremento mínimo entre pujas en euros'),
    ('notificaciones_email_habilitadas', 'true', 'bool', 'Enviar notificaciones por email'),
    ('comision_plataforma_porcentaje', '5', 'decimal', 'Comisión de la plataforma sobre venta (%)'),
    ('dias_validez_iae', '365', 'int', 'Días de validez del documento IAE'),
    ('max_imagenes_por_vehiculo', '10', 'int', 'Número máximo de imágenes por vehículo'),
    ('dias_pago_subasta', '7', 'int', 'Días límite para realizar el pago tras ganar subasta'),
    ('email_contacto_soporte', 'soporte@subastas.com', 'string', 'Email de contacto para soporte');
    
    PRINT '  ✓ Tabla ConfiguracionSistema creada (8 registros)';
END
ELSE
    PRINT '  - ConfiguracionSistema ya existe';

PRINT '';

-- =====================================================
-- 6. ESTADÍSTICAS FINALES
-- =====================================================

PRINT '========================================';
PRINT 'RESUMEN DE MIGRACIÓN';
PRINT '========================================';

SELECT 'Tablas Maestras Creadas' AS Categoria, COUNT(*) AS Total
FROM sys.tables
WHERE name IN ('EstadoVehiculo', 'EstadoSubasta', 'Rol', 'TipoNotificacion', 
               'MetodoPago', 'EstadoPago', 'MarcaVehiculo', 'TipoVehiculo', 
               'ConfiguracionSistema');

SELECT 'Índices de Performance' AS Categoria, COUNT(*) AS Total
FROM sys.indexes
WHERE name LIKE 'IX_%' 
AND object_id IN (
    OBJECT_ID('Usuario'), 
    OBJECT_ID('Vehiculo'), 
    OBJECT_ID('Subasta'), 
    OBJECT_ID('Puja'), 
    OBJECT_ID('Notificacion'),
    OBJECT_ID('NotificacionAdmin')
);

SELECT 'Registros en Tablas Maestras' AS Categoria, SUM(Total) AS Total
FROM (
    SELECT COUNT(*) AS Total FROM EstadoVehiculo
    UNION ALL SELECT COUNT(*) FROM EstadoSubasta
    UNION ALL SELECT COUNT(*) FROM Rol
    UNION ALL SELECT COUNT(*) FROM TipoNotificacion
    UNION ALL SELECT COUNT(*) FROM MetodoPago
    UNION ALL SELECT COUNT(*) FROM EstadoPago
    UNION ALL SELECT COUNT(*) FROM MarcaVehiculo
    UNION ALL SELECT COUNT(*) FROM TipoVehiculo
    UNION ALL SELECT COUNT(*) FROM ConfiguracionSistema
) AS Totales;

PRINT '';
PRINT '✓ MIGRACIÓN COMPLETADA EXITOSAMENTE';
PRINT '';
PRINT 'Próximos pasos:';
PRINT '1. Actualizar entidades del Domain en .NET';
PRINT '2. Actualizar repositorios e interfaces';
PRINT '3. Modificar DTOs de respuesta';
PRINT '4. Actualizar controladores';
PRINT '5. Probar exhaustivamente';
PRINT '';
GO
