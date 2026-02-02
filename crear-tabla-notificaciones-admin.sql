-- Script para crear la tabla de notificaciones para administradores

CREATE TABLE [dbo].[NotificacionAdmin] (
    [idNotificacion] INT IDENTITY(1,1) NOT NULL,
    [titulo] VARCHAR(200) NOT NULL,
    [mensaje] VARCHAR(500) NOT NULL,
    [tipo] VARCHAR(50) NOT NULL, -- 'registro', 'documento_subido', 'puja', 'otro'
    [idUsuario] INT NULL, -- Usuario relacionado con la notificación
    [leida] TINYINT NOT NULL DEFAULT 0, -- 0 = no leída, 1 = leída
    [fechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
    [datosAdicionales] VARCHAR(500) NULL, -- JSON con info adicional
    CONSTRAINT [PK_NotificacionAdmin] PRIMARY KEY CLUSTERED ([idNotificacion] ASC),
    CONSTRAINT [FK_NotificacionAdmin_Usuario] FOREIGN KEY ([idUsuario]) 
        REFERENCES [dbo].[Usuario] ([idUsuario]) ON DELETE SET NULL
);

-- Índice para mejorar el rendimiento de consultas por leída
CREATE NONCLUSTERED INDEX [IX_NotificacionAdmin_Leida] 
ON [dbo].[NotificacionAdmin] ([leida] ASC);

-- Índice para consultas por tipo
CREATE NONCLUSTERED INDEX [IX_NotificacionAdmin_Tipo] 
ON [dbo].[NotificacionAdmin] ([tipo] ASC);

-- Índice para consultas por fecha
CREATE NONCLUSTERED INDEX [IX_NotificacionAdmin_Fecha] 
ON [dbo].[NotificacionAdmin] ([fechaCreacion] DESC);

GO
