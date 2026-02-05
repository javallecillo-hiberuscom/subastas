# Casos de Uso Principales

## 1. Registro de Usuario
- El usuario accede al formulario de registro.
- Ingresa datos personales, email, contraseña, DNI/NIF (validado con letra de control).
- El sistema valida los datos y crea el usuario en estado "pendiente de validación".
- El usuario recibe notificación de registro exitoso.

## 2. Login de Usuario
- El usuario accede al formulario de login.
- Ingresa email y contraseña.
- El sistema valida credenciales y devuelve JWT.
- Si el usuario está validado, accede a todas las funcionalidades; si no, acceso limitado.

## 3. Validación de Usuario por Admin
- El admin accede al panel de usuarios pendientes.
- Selecciona un usuario y lo valida.
- El usuario pasa a estado "validado" y recibe notificación.
- El frontend refresca automáticamente el estado del usuario.

## 4. Gestión de Perfil
- El usuario accede a su perfil.
- Puede modificar nombre, apellidos, teléfono, etc.
- El sistema actualiza los datos y muestra confirmación.

## 5. Subasta y Puja
- El usuario accede a la lista de subastas activas.
- Puede ver detalles y realizar pujas.
- El sistema valida la puja y actualiza el estado de la subasta.
- El usuario recibe notificaciones de puja ganada/superada.

## 6. Notificaciones
- El usuario y el admin reciben notificaciones según eventos (registro, validación, puja, etc.).
- El usuario puede marcar notificaciones como leídas y eliminarlas.

## 7. Gestión Administrativa
- El admin puede crear, editar y eliminar notificaciones administrativas.
- Puede gestionar usuarios, subastas y vehículos.

## 8. Seguridad y Autenticación
- El sistema valida JWT en cada petición protegida.
- El usuario puede cerrar sesión y el token se invalida.

---

Estos casos de uso cubren los flujos principales del sistema. Si necesitas algún caso específico, indícalo y lo añado.