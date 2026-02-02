-- Script para verificar y probar las notificaciones

-- 1. Verificar si la tabla existe
IF OBJECT_ID('dbo.NotificacionAdmin', 'U') IS NOT NULL
    PRINT 'La tabla NotificacionAdmin existe'
ELSE
    PRINT 'ERROR: La tabla NotificacionAdmin NO existe - ejecuta crear-tabla-notificaciones-admin.sql'

-- 2. Ver todas las notificaciones actuales
SELECT * FROM [dbo].[NotificacionAdmin] ORDER BY fechaCreacion DESC;

-- 3. Insertar una notificación de prueba manualmente
INSERT INTO [dbo].[NotificacionAdmin] (titulo, mensaje, tipo, idUsuario, leida, fechaCreacion)
VALUES ('Prueba de notificación', 'Esta es una notificación de prueba', 'otro', 2, 0, GETDATE());

-- 4. Verificar que se insertó
SELECT * FROM [dbo].[NotificacionAdmin] ORDER BY fechaCreacion DESC;

-- 5. Contar notificaciones no leídas
SELECT COUNT(*) as NotificacionesNoLeidas 
FROM [dbo].[NotificacionAdmin] 
WHERE leida = 0;
