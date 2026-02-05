-- Verificar el usuario Lucía
SELECT IdUsuario, Nombre, Apellidos, Email, Rol, Validado, Activo
FROM Usuarios
WHERE Email = 'lucia@admin.com';

-- Actualizar Lucía para que sea Admin si no lo es
UPDATE Usuarios
SET Rol = 'Admin', Validado = 1, Activo = 1
WHERE Email = 'lucia@admin.com';

-- Verificar el cambio
SELECT IdUsuario, Nombre, Apellidos, Email, Rol, Validado, Activo
FROM Usuarios
WHERE Email = 'lucia@admin.com';
