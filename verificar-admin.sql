-- Script para verificar y corregir el rol de administrador

-- 1. Verificar qué roles existen actualmente
SELECT DISTINCT rol, COUNT(*) as cantidad
FROM [dbo].[Usuario]
GROUP BY rol;

-- 2. Ver usuarios con rol similar a 'administrador'
SELECT idUsuario, nombre, apellidos, email, rol, validado, activo
FROM [dbo].[Usuario]
WHERE rol LIKE '%admin%' OR rol LIKE '%Administrador%';

-- 3. EJECUTAR: Normalizar todos los roles de admin a 'administrador' en minúsculas
UPDATE [dbo].[Usuario] 
SET rol = 'administrador' 
WHERE rol IN ('admin', 'Admin', 'ADMIN', 'Administrador', 'ADMINISTRADOR');

-- 4. EJECUTAR: Corregir roles incorrectos - 'validado' NO es un rol válido
-- El rol debe ser 'registrado' o 'administrador'
-- El estado de validación se controla con el campo validado (0 o 1)
UPDATE [dbo].[Usuario] 
SET rol = 'registrado' 
WHERE rol IN ('validado', 'Validado', 'VALIDADO');

-- 5. Verificar que el usuario con id 1 sea administrador
UPDATE [dbo].[Usuario] 
SET rol = 'administrador', validado = 1, activo = 1
WHERE idUsuario = 1;

-- 6. Verificar el resultado final
SELECT idUsuario, nombre, apellidos, email, rol, validado, activo, documentoIAE
FROM [dbo].[Usuario]
ORDER BY idUsuario;

-- IMPORTANTE: 
-- rol debe ser: 'administrador' o 'registrado'
-- validado debe ser: 0 (no validado) o 1 (validado)
-- El admin valida cambiando validado de 0 a 1, NO cambiando el rol
