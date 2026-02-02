# ?? Guía de solución de problemas HTTP 500.30

## ? Pasos inmediatos para resolver el error

### 1. **Habilitar logs en Azure App Service**

Ve a Azure Portal ? Tu App Service ? Monitoring ? App Service logs:
- **Application Logging**: Filesystem (Nivel: Information)
- **Detailed error messages**: On
- **Failed request tracing**: On
- **Web server logging**: File System

### 2. **Ver los logs en tiempo real**

Opción A - Desde Azure Portal:
```
Azure Portal ? Tu App Service ? Monitoring ? Log stream
```

Opción B - Desde Azure CLI:
```powershell
az webapp log tail --name SubastasWebApi20260202162157 --resource-group <tu-resource-group>
```

### 3. **Configurar variables de entorno**

**IMPORTANTE**: Tu aplicación requiere estas configuraciones en Azure App Service:

#### En Azure Portal:
1. Ve a: **Configuration ? Application settings**
2. Agrega estas configuraciones:

```
JwtSettings__SecretKey = TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES
JwtSettings__Issuer = SubastasAPI
JwtSettings__Audience = SubastasClient
JwtSettings__ExpirationMinutes = 60
ASPNETCORE_ENVIRONMENT = Production
```

3. Ve a: **Configuration ? Connection strings**
4. Agrega:
```
Nombre: SubastaConnection
Valor: Server=tcp:fpcursos.database.windows.net,1433;Initial Catalog=subasta;Persist Security Info=False;User ID=adminoc;Password=A0a0f0f011;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Tipo: SQLAzure
```

5. Haz clic en **Save** y espera a que se reinicie la aplicación

### 4. **O usa el script PowerShell**

Ejecuta el archivo `configure-azure.ps1` que creamos:
```powershell
.\configure-azure.ps1
```

(Antes de ejecutar, edita el script y actualiza el nombre de tu resource group)

---

## ?? Causas comunes del error 500.30

### ? Problema 1: JWT SecretKey no configurada
**Síntoma**: La app falla al iniciar con excepción "JWT SecretKey no configurada"
**Solución**: Configurar `JwtSettings__SecretKey` en Application Settings

### ? Problema 2: Connection String faltante
**Síntoma**: Error al iniciar DbContext
**Solución**: Configurar `SubastaConnection` en Connection Strings

### ? Problema 3: Versión incorrecta de .NET
**Síntoma**: "Failed to find a version of .NET that supports the application"
**Solución**: 
1. Ve a Azure Portal ? Tu App Service ? Configuration ? General settings
2. Verifica que **Stack** sea **.NET**
3. Verifica que **Major version** sea **.NET 8 (LTS)**
4. Verifica que **Minor version** sea la última disponible

### ? Problema 4: Archivos no publicados correctamente
**Síntoma**: Dll faltantes o archivos de configuración incorrectos
**Solución**: 
1. En Visual Studio, click derecho en Subastas.WebApi
2. Clean Solution
3. Rebuild Solution
4. Publish de nuevo

### ? Problema 5: Base de datos no accesible desde Azure
**Síntoma**: Timeout al conectar a SQL Server
**Solución**:
1. Ve a Azure SQL Server ? Networking
2. Agrega la IP de tu App Service a las reglas de firewall
3. O activa "Allow Azure services and resources to access this server"

---

## ?? Verificar que la aplicación funciona

### 1. Health Check
Después de configurar y publicar, ve a:
```
https://tu-app.azurewebsites.net/health
```

Deberías ver:
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-02T..."
}
```

### 2. Swagger UI
```
https://tu-app.azurewebsites.net/
```

Deberías ver la interfaz de Swagger con todos los endpoints.

---

## ?? Comandos útiles de Azure CLI

```powershell
# Ver logs en tiempo real
az webapp log tail --name <app-name> --resource-group <rg-name>

# Descargar logs recientes
az webapp log download --name <app-name> --resource-group <rg-name> --log-file logs.zip

# Reiniciar la aplicación
az webapp restart --name <app-name> --resource-group <rg-name>

# Ver configuración actual
az webapp config appsettings list --name <app-name> --resource-group <rg-name>

# Ver connection strings actuales
az webapp config connection-string list --name <app-name> --resource-group <rg-name>
```

---

## ?? Checklist final

- [ ] Configuradas variables de entorno en Azure
- [ ] Configurada connection string en Azure
- [ ] Verificado Stack = .NET 8 en Configuration
- [ ] Habilitados logs de aplicación
- [ ] Firewall de SQL Server permite acceso desde Azure
- [ ] Publicada la aplicación desde Visual Studio
- [ ] Verificado endpoint `/health` funciona
- [ ] Verificado Swagger UI carga correctamente

---

## ?? Siguiente paso

Una vez configuradas las variables de entorno:
1. **Guarda** la configuración en Azure Portal
2. Espera 30 segundos a que se reinicie
3. Abre: `https://tu-app.azurewebsites.net/health`
4. Si ves `"status": "Healthy"`, ¡todo funcionó! ??

Si aún tienes problemas, comparte los logs y te ayudaré a diagnosticar.
