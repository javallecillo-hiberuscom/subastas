# Sistema de Autenticación - Subastas Frontend

## 📋 Descripción

Sistema de autenticación completo para la aplicación Angular de subastas que se conecta con el backend en `https://localhost:7249`.

## 🏗️ Estructura Creada

```
src/app/
├── models/
│   └── auth.models.ts          # Modelos de datos de autenticación
├── services/
│   └── auth.service.ts         # Servicio de autenticación
├── guards/
│   └── auth.guard.ts           # Guard para proteger rutas
├── interceptors/
│   └── auth.interceptor.ts    # Interceptor HTTP para tokens
├── login/
│   ├── login.component.ts      # Componente de login
│   ├── login.component.html    # Template del login
│   └── login.component.css     # Estilos del login
└── dashboard/
    ├── dashboard.component.ts   # Componente del dashboard
    ├── dashboard.component.html # Template del dashboard
    └── dashboard.component.css  # Estilos del dashboard
```

## 🔧 Configuración del Backend

### Importante: Ajustar el endpoint de autenticación

El servicio está configurado para conectarse a `https://localhost:7249/api/auth/login`.

**Debes verificar y ajustar el endpoint según la configuración de tu backend:**

1. Abre el archivo `src/app/services/auth.service.ts`
2. Busca la línea: `private readonly API_URL = 'https://localhost:7249/api';`
3. Modifica según tu backend:
   - Si tu endpoint es `/api/auth/login` → Déjalo como está
   - Si tu endpoint es `/auth/login` → Cambia a `'https://localhost:7249'`
   - Si usa otro path → Ajusta según corresponda

### Estructura esperada del Response

El backend debe devolver una respuesta JSON con esta estructura:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "123",
    "email": "usuario@ejemplo.com",
    "nombre": "Juan Pérez",
    "rol": "admin"
  }
}
```

### Configuración CORS en el Backend

Asegúrate de que tu backend ASP.NET Core tenga configurado CORS para permitir peticiones desde Angular:

```csharp
// En Program.cs o Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200") // Puerto de Angular
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Antes de app.Run()
app.UseCors("AllowAngular");
```

## 🚀 Uso

### Iniciar la aplicación

```bash
npm start
```

La aplicación se abrirá en `http://localhost:4200` y te redirigirá automáticamente al login.

### Flujo de autenticación

1. El usuario accede a la aplicación
2. Si no está autenticado, es redirigido a `/login`
3. Ingresa sus credenciales (email y contraseña)
4. Al autenticarse, el token se guarda en localStorage
5. Es redirigido al dashboard (`/dashboard`)
6. Todas las peticiones HTTP incluyen automáticamente el token

### Rutas

- `/login` - Página de inicio de sesión (acceso público)
- `/dashboard` - Dashboard principal (requiere autenticación)
- Cualquier otra ruta → redirige a `/login`

## 🔐 Características de Seguridad

- ✅ Guard de autenticación en rutas protegidas
- ✅ Interceptor HTTP que añade el token a todas las peticiones
- ✅ Manejo automático de sesiones expiradas (401)
- ✅ Almacenamiento seguro en localStorage
- ✅ Validación de formularios
- ✅ Manejo de errores de autenticación

## 📝 Modelos de Datos

### LoginRequest
```typescript
{
  email: string;
  password: string;
}
```

### LoginResponse
```typescript
{
  token: string;
  refreshToken?: string;
  user: User;
}
```

### User
```typescript
{
  id: string;
  email: string;
  nombre?: string;
  rol?: string;
}
```

## 🎨 Componente de Login

El componente incluye:
- Formulario reactivo con validación
- Indicador de carga durante el login
- Mensajes de error claros
- Toggle para mostrar/ocultar contraseña
- Diseño responsive y moderno
- Animaciones suaves

## 🔄 Próximos Pasos

1. **Ajustar el endpoint del backend** en `auth.service.ts`
2. **Verificar la estructura del response** del backend
3. **Configurar CORS** en el backend
4. **Probar la conexión** con el backend
5. Agregar más rutas protegidas según necesites
6. Implementar refresh token si es necesario
7. Agregar recordar sesión (Remember me)

## 🛠️ Personalización

### Cambiar la URL del backend

En `src/app/services/auth.service.ts`:
```typescript
private readonly API_URL = 'https://tu-backend.com/api';
```

### Agregar más rutas protegidas

En `src/app/app.routes.ts`:
```typescript
{
  path: 'mi-ruta',
  canActivate: [authGuard],
  loadComponent: () => import('./mi-componente/mi-componente.component')
    .then(m => m.MiComponente)
}
```

### Personalizar el diseño del login

Edita `src/app/login/login.component.css` para cambiar colores, fuentes, etc.

## 📞 Soporte

Si el backend devuelve un formato diferente, ajusta el servicio de autenticación en consecuencia.
