-- Script para arreglar la Foreign Key de idEmpresa en Usuario
USE subasta;
GO

-- 1. Eliminar la constraint FK existente si existe
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Usuario_Empresa')
BEGIN
    ALTER TABLE [dbo].[Usuario]
    DROP CONSTRAINT [FK_Usuario_Empresa];
    PRINT 'FK_Usuario_Empresa eliminada';
END
GO

-- 2. Recrear la FK con ON DELETE SET NULL
ALTER TABLE [dbo].[Usuario]
ADD CONSTRAINT [FK_Usuario_Empresa]
FOREIGN KEY ([idEmpresa])
REFERENCES [dbo].[Empresa]([idEmpresa])
ON DELETE SET NULL;
GO

PRINT 'FK_Usuario_Empresa recreada con ON DELETE SET NULL';
GO

-- 3. Verificar la configuración
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn,
    fk.delete_referential_action_desc AS DeleteAction
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Usuario'
AND COL_NAME(fc.parent_object_id, fc.parent_column_id) = 'idEmpresa';
GO
