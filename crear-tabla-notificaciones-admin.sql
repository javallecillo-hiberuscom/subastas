-- Crear tabla NotificacionAdmin si no existe
USE subasta;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificacionAdmin')
BEGIN
    CREATE TABLE [dbo].[NotificacionAdmin] (
        [idNotificacion] INT IDENTITY(1,1) PRIMARY KEY,
        [titulo] VARCHAR(200) NOT NULL,
        [mensaje] VARCHAR(500) NOT NULL,
        [tipo] VARCHAR(50) NOT NULL,
        [idUsuario] INT NULL,
        [leida] TINYINT NOT NULL DEFAULT 0,
        [fechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NotificacionAdmin_Usuario FOREIGN KEY ([idUsuario])
            REFERENCES [dbo].[Usuario]([idUsuario])
            ON DELETE SET NULL
    );
    
    PRINT 'Tabla NotificacionAdmin creada correctamente';
END
ELSE
BEGIN
    PRINT 'La tabla NotificacionAdmin ya existe';
END
GO
