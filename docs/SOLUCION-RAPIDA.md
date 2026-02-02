# ?? Solución al Error HTTP 500.30 - Pasos Inmediatos

## ? El problema

Tu aplicación falla al iniciar en Azure porque le faltan configuraciones críticas (JWT SecretKey y otras variables de entorno).

---

## ? SOLUCIÓN RÁPIDA (5 minutos)

### Paso 1: Configurar en Azure Portal

1. **Abre Azure Portal**: https://portal.azure.com
2. **Busca tu App Service**: `SubastasWebApi20260202162157` (o el nombre que hayas usado)
3. **Ve a**: `Configuration` (en el menú lateral)

### Paso 2: Agregar Application Settings

Haz clic en **"Application settings"** ? **"+ New application setting"**

Agrega **CADA UNA** de estas (copia y pega):

```
Name: JwtSettings__SecretKey
Value: TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES
```

```
Name: JwtSettings__Issuer
Value: SubastasAPI
```

```
Name: JwtSettings__Audience
Value: SubastasClient
```

```
Name: JwtSettings__ExpirationMinutes
Value: 60
```

```
Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

### Paso 3: Agregar Connection String

Haz clic en **"Connection strings"** ? **"+ New connection string"**

```
Name: SubastaConnection
Value: Server=tcp:fpcursos.database.windows.net,1433;Initial Catalog=subasta;Persist Security Info=False;User ID=adminoc;Password=A0a0f0f011;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Type: SQLAzure
```

### Paso 4: Verificar .NET Runtime

En la misma página de **Configuration**, ve a **"General settings"**:

- **Stack**: `.NET`
- **Major version**: `.NET 8 (LTS)`

### Paso 5: Guardar y esperar

1. Haz clic en **"Save"** (arriba)
2. Confirma el reinicio
3. **Espera 30-60 segundos** que se reinicie

---

## ?? Paso 6: Verificar que funciona

Abre en tu navegador (reemplaza con tu URL real):

```
https://subastaswebapi20260202162157.azurewebsites.net/health
```

? **Si ves esto**, ¡funcionó!
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-02T16:30:00Z"
}
```

? **Si aún da error 500**, ve al siguiente paso:

---

## ?? Paso 7: Ver los logs (si sigue fallando)

1. En Azure Portal, ve a: **Monitoring ? App Service logs**
2. Activa:
   - **Application logging**: `On` (Filesystem - Information)
 - **Detailed error messages**: `On`
   - **Failed request tracing**: `On`

3. Guarda

4. Ve a: **Monitoring ? Log stream**

5. Busca líneas con **"ERROR"** o **"Exception"**

6. Copia el error y compártelo

---

## ??? Paso 8: Configurar firewall de SQL (si es necesario)

Si ves errores de conexión a la base de datos:

1. Ve a: **Azure Portal ? SQL Server (fpcursos) ? Networking**
2. Activa: **"Allow Azure services and resources to access this server"**
3. Guarda

---

## ?? Resumen de cambios realizados

He creado/modificado estos archivos en tu proyecto:

? `Program.cs` - Agregado logging detallado y manejo de errores
? `appsettings.Production.json` - Configuración para Azure
? `web.config` - Configuración de IIS para Azure
? `Subastas.WebApi.csproj` - Agregado Application Insights
? `configure-azure.ps1` - Script para configurar desde CLI
? `AZURE-CONFIGURATION-GUIDE.md` - Guía detallada
? `DEPLOYMENT-CHECKLIST.md` - Checklist completo
? `AZURE-TROUBLESHOOTING.md` - Guía de solución de problemas

---

## ?? Próximos pasos después de configurar

1. **Volver a publicar** desde Visual Studio:
   - Click derecho en `Subastas.WebApi`
   - **Publish**
   - Seleccionar perfil existente
   - **Publish**

2. **Verificar** que `/health` responde correctamente

3. **Probar** Swagger UI: `https://tu-app.azurewebsites.net/`

---

## ?? Archivos de ayuda

- `AZURE-CONFIGURATION-GUIDE.md` - Configuración paso a paso con capturas
- `DEPLOYMENT-CHECKLIST.md` - Checklist completo de despliegue
- `AZURE-TROUBLESHOOTING.md` - Solución de problemas comunes
- `configure-azure.ps1` - Script automatizado (requiere Azure CLI)

---

## ?? ¿Sigue sin funcionar?

Comparte:
1. Los logs del **Log stream**
2. Captura de pantalla de **Configuration ? Application settings**
3. El error exacto que aparece

¡Y te ayudaré a solucionarlo! ??
