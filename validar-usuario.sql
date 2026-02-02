-- Script para validar manualmente un usuario

-- Ver usuarios pendientes de validación
SELECT IdUsuario, Email, Nombre, Apellidos, Validado, DocumentoIAE 
FROM Usuario 
WHERE Validado = 0 AND Activo = 1;

-- Validar usuario específico (reemplaza el email con el tuyo)
-- UPDATE Usuario SET Validado = 1 WHERE Email = 'tu_email@ejemplo.com';

-- Validar todos los usuarios pendientes (usar con precaución)
-- UPDATE Usuario SET Validado = 1 WHERE Validado = 0 AND Activo = 1;

-- Verificar usuarios validados
SELECT IdUsuario, Email, Nombre, Apellidos, Validado 
FROM Usuario 
WHERE Validado = 1 AND Activo = 1;
