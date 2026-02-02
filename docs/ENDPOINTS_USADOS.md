# Endpoints utilizados por el Frontend

## Autenticación
- **POST** `/api/Auth/registro` - Registro de nuevos usuarios (incluye foto de perfil opcional)
- **POST** `/api/usuarios/login` - Login de usuarios (devuelve foto de perfil en response)

## Usuarios
- **GET** `/api/Usuarios` - Listar todos los usuarios (admin)
- **GET** `/api/Usuarios/{id}` - Obtener usuario específico
- **POST** `/api/Usuarios` - Crear usuario (admin)
- **PUT** `/api/Usuarios/{id}` - Actualizar usuario
- **DELETE** `/api/Usuarios/{id}` - Eliminar usuario (admin)
- **GET** `/api/Usuarios/{id}/foto-perfil` - Obtener foto de perfil
- **PUT** `/api/Usuarios/{id}/foto-perfil` - Actualizar foto de perfil (payload: { fotoBase64: string })
- **DELETE** `/api/Usuarios/{id}/foto-perfil` - Eliminar foto de perfil
- **POST** `/api/Usuarios/recuperar-contrasena` - Solicitar código recuperación contraseña
- **POST** `/api/Usuarios/restablecer-contrasena` - Restablecer contraseña con código

## Vehículos
- **GET** `/api/Vehiculos` - Listar todos los vehículos
- **GET** `/api/Vehiculos/{id}` - Obtener vehículo específico
- **POST** `/api/Vehiculos` - Crear vehículo (admin)
- **PUT** `/api/Vehiculos/{id}` - Actualizar vehículo (admin)
- **DELETE** `/api/Vehiculos/{id}` - Eliminar vehículo (admin)
- **POST** `/api/Vehiculos/{id}/imagenes` - Agregar imágenes a vehículo
- **DELETE** `/api/Vehiculos/{idVehiculo}/imagenes/{idImagen}` - Eliminar imagen

## Subastas
- **GET** `/api/Subastas?activas=true` - Listar subastas activas

## Pujas
- **GET** `/api/Pujas` - Listar todas las pujas
- **GET** `/api/Pujas/usuario/{idUsuario}` - Pujas de un usuario
- **POST** `/api/Pujas` - Crear nueva puja

## Empresas
- **GET** `/api/Empresas` - Listar todas las empresas (admin)
- **GET** `/api/Empresas/{id}` - Obtener empresa específica
- **POST** `/api/Empresas` - Crear empresa
- **PUT** `/api/Empresas/{id}` - Actualizar empresa
- **DELETE** `/api/Empresas/{id}` - Eliminar empresa (admin)

## Notificaciones
- **GET** `/api/Notificaciones/{idUsuario}` - Obtener notificaciones de usuario
- **PUT** `/api/Notificaciones/{id}/leer` - Marcar notificación como leída
- **POST** `/api/Notificaciones/procesar-finalizadas` - Procesar subastas finalizadas y crear notificaciones

## Total de endpoints únicos: 29
