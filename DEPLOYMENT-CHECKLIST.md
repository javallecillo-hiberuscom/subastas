# ? Checklist de despliegue en Azure App Service

## ?? Antes de publicar desde Visual Studio

- [ ] **Compilar y verificar** que el proyecto funciona localmente
- [ ] **Ejecutar** `dotnet restore` para restaurar paquetes
- [ ] **Verificar** que `appsettings.Production.json` existe
- [ ] **Verificar** que `web.config` existe

## ?? Configuración en Azure Portal

### Application Settings
- [ ] Configurar `JwtSettings__SecretKey`
- [ ] Configurar `JwtSettings__Issuer`
- [ ] Configurar `JwtSettings__Audience`
- [ ] Configurar `JwtSettings__ExpirationMinutes`
- [ ] Configurar `ASPNETCORE_ENVIRONMENT = Production`

### Connection Strings
- [ ] Configurar `SubastaConnection` (tipo: SQLAzure)

### General Settings
- [ ] Verificar Stack = **.NET 8 (LTS)**
- [ ] Verificar Platform = **64 Bit**
- [ ] Activar **Always On** (para evitar sleep)
- [ ] Activar **HTTPS Only**

### Monitoring & Logs
- [ ] Habilitar **Application logging** (Filesystem - Information)
- [ ] Habilitar **Detailed error messages**
- [ ] Habilitar **Failed request tracing**
- [ ] Habilitar **Web server logging**

### Networking & Security
- [ ] Configurar firewall de SQL Server para permitir Azure Services
- [ ] (Opcional) Configurar CORS si tienes frontend externo
- [ ] (Opcional) Configurar dominio personalizado

## ?? Publicación desde Visual Studio

- [ ] Click derecho en proyecto **Subastas.WebApi**
- [ ] Seleccionar **Publish**
- [ ] Elegir perfil **"SubastasWebApi20260202162157 - Zip Deploy"**
- [ ] Click en **Publish**
- [ ] Esperar a que termine (puede tomar 2-5 minutos)

## ? Verificación post-despliegue

### Verificar endpoints
- [ ] Abrir `https://tu-app.azurewebsites.net/health`
  - Debe mostrar: `{"status":"Healthy","timestamp":"..."}`
- [ ] Abrir `https://tu-app.azurewebsites.net/`
  - Debe mostrar Swagger UI
- [ ] Probar un endpoint GET simple desde Swagger
  - Debe responder correctamente (no 500)

### Verificar logs
- [ ] Ir a **Log stream** en Azure Portal
- [ ] Ver que aparece: `"Iniciando aplicación Subastas API..."`
- [ ] Ver que aparece: `"Aplicación configurada correctamente..."`
- [ ] Verificar que NO hay errores rojos

### Verificar base de datos
- [ ] Probar un endpoint que consulte la base de datos
- [ ] Verificar que la conexión funciona
- [ ] Si falla, revisar firewall de SQL Server

## ?? Si algo falla

- [ ] Revisar **Log stream** para ver el error exacto
- [ ] Verificar que todas las Application Settings están correctas
- [ ] Verificar que la Connection String es correcta
- [ ] Verificar que el firewall de SQL permite Azure Services
- [ ] Intentar reiniciar el App Service
- [ ] Volver a publicar desde Visual Studio

## ?? Monitoreo continuo

- [ ] Configurar Application Insights (opcional pero recomendado)
- [ ] Configurar alertas de disponibilidad
- [ ] Revisar métricas de CPU y memoria
- [ ] Configurar backup automático (opcional)

## ?? Seguridad para producción

- [ ] Cambiar `JwtSettings__SecretKey` por una clave única
- [ ] Usar **Azure Key Vault** para secretos
- [ ] Configurar **Managed Identity** para la base de datos
- [ ] Habilitar **Application Gateway** o **Front Door** (opcional)
- [ ] Configurar **SSL/TLS** con certificado personalizado (opcional)

---

## ?? ¡Listo!

Si todos los checkboxes están marcados y `/health` responde correctamente, ¡tu aplicación está corriendo en Azure! ??
