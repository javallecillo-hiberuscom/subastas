# ?? Configuración rápida en Azure Portal

## ?? Configuración mínima requerida

### Paso 1: Application Settings
Ve a: **Azure Portal ? Tu App Service ? Configuration ? Application settings**

Haz clic en **"+ New application setting"** para cada una:

| Nombre | Valor |
|--------|-------|
| `JwtSettings__SecretKey` | `TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES` |
| `JwtSettings__Issuer` | `SubastasAPI` |
| `JwtSettings__Audience` | `SubastasClient` |
| `JwtSettings__ExpirationMinutes` | `60` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Paso 2: Connection Strings
Ve a: **Azure Portal ? Tu App Service ? Configuration ? Connection strings**

Haz clic en **"+ New connection string"**:

- **Name**: `SubastaConnection`
- **Value**: `Server=tcp:fpcursos.database.windows.net,1433;Initial Catalog=subasta;Persist Security Info=False;User ID=adminoc;Password=A0a0f0f011;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
- **Type**: `SQLAzure`

### Paso 3: General Settings
Ve a: **Azure Portal ? Tu App Service ? Configuration ? General settings**

Verifica:
- **Stack**: `.NET`
- **Major version**: `.NET 8 (LTS)`
- **Platform**: `64 Bit` (recomendado)
- **Always On**: `On` (evita que la app se duerma)
- **HTTPS Only**: `On` (recomendado)

### Paso 4: Guardar cambios
?? **IMPORTANTE**: Haz clic en **"Save"** arriba y espera a que se reinicie la aplicación (30-60 segundos)

---

## ?? Habilitar logs (para diagnóstico)

Ve a: **Azure Portal ? Tu App Service ? Monitoring ? App Service logs**

Configura:
- **Application logging (Filesystem)**: `On` - Level: `Information`
- **Detailed error messages**: `On`
- **Failed request tracing**: `On`
- **Web server logging**: `File System`

Haz clic en **"Save"**

---

## ?? Ver logs en tiempo real

Ve a: **Azure Portal ? Tu App Service ? Monitoring ? Log stream**

Aquí verás los logs en vivo. Deberías ver:
```
Iniciando aplicación Subastas API en entorno: Production
Aplicación configurada correctamente. Iniciando servidor...
```

---

## ??? Configurar firewall de SQL Server (si es necesario)

Si ves errores de conexión a la base de datos:

1. Ve a: **Azure Portal ? Tu SQL Server (fpcursos) ? Security ? Networking**
2. En **Firewall rules**, activa: **"Allow Azure services and resources to access this server"**
3. O agrega la IP de salida de tu App Service manualmente
4. Haz clic en **"Save"**

---

## ? Verificar que funciona

1. Ve a: `https://tu-app.azurewebsites.net/health`
 - Deberías ver: `{"status":"Healthy","timestamp":"..."}`

2. Ve a: `https://tu-app.azurewebsites.net/`
   - Deberías ver la interfaz de Swagger

---

## ?? Si sigues teniendo problemas

1. Revisa los logs en **Log stream**
2. Busca líneas que digan "ERROR" o "Exception"
3. Copia el error y compártelo para ayudarte a solucionarlo

---

## ?? Notas de seguridad

?? **Para producción**:
1. **Cambia** `JwtSettings__SecretKey` por una clave única y segura
2. **Usa Azure Key Vault** para almacenar secretos sensibles
3. **No expongas** credenciales en código fuente
4. **Configura** Application Insights para monitoreo continuo
