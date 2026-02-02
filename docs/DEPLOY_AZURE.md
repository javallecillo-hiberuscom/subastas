# Guía de Despliegue en Azure

## OPCIÓN 1: Azure App Service (Recomendada)

### Requisitos
- Suscripción de Azure (VS Professional)
- Azure CLI instalado: https://aka.ms/installazurecli

### Paso 1: Iniciar sesión en Azure

```powershell
az login
```

### Paso 2: Crear un Resource Group

```powershell
az group create --name rg-subastas --location westeurope
```

### Paso 3: Crear un App Service Plan

```powershell
# Plan gratuito para pruebas
az appservice plan create --name plan-subastas --resource-group rg-subastas --sku F1 --is-linux

# O plan básico con más recursos (recomendado)
az appservice plan create --name plan-subastas --resource-group rg-subastas --sku B1 --is-linux
```

### Paso 4: Crear la Web App

```powershell
az webapp create --resource-group rg-subastas --plan plan-subastas --name subastas-api-<tu-nombre-unico> --runtime "DOTNET|8.0"
```

**Nota:** Reemplaza `<tu-nombre-unico>` con un nombre único (ej: subastas-api-borox)

### Paso 5: Configurar las Variables de Entorno

```powershell
az webapp config appsettings set --resource-group rg-subastas --name subastas-api-<tu-nombre-unico> --settings `
  "ConnectionStrings__SubastaConnection=Server=tcp:fpcursos.database.windows.net,1433;Initial Catalog=subasta;User ID=adminoc;Password=A0a0f0f011;Encrypt=True;" `
  "JwtSettings__SecretKey=TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES" `
  "JwtSettings__Issuer=SubastasAPI" `
  "JwtSettings__Audience=SubastasClient" `
  "JwtSettings__ExpirationMinutes=60" `
  "ASPNETCORE_ENVIRONMENT=Production"
```

### Paso 6: Configurar CORS

```powershell
az webapp cors add --resource-group rg-subastas --name subastas-api-<tu-nombre-unico> --allowed-origins "*"
```

### Paso 7: Publicar la aplicación desde Visual Studio

1. Clic derecho en el proyecto `Subastas.WebApi`
2. Seleccionar **Publicar**
3. Elegir **Azure**
4. Seleccionar **Azure App Service (Linux)**
5. Iniciar sesión con tu cuenta de Azure
6. Seleccionar tu App Service (`subastas-api-<tu-nombre-unico>`)
7. Clic en **Finalizar**
8. Clic en **Publicar**

### Paso 8: Verificar el despliegue

```powershell
# Abrir la URL en el navegador
az webapp browse --resource-group rg-subastas --name subastas-api-<tu-nombre-unico>
```

Tu API estará disponible en: `https://subastas-api-<tu-nombre-unico>.azurewebsites.net`

### Paso 9: Actualizar Angular con la nueva URL

En tu proyecto Angular, actualiza el archivo de configuración:

```typescript
// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://subastas-api-<tu-nombre-unico>.azurewebsites.net/api'
};
```

---

## OPCIÓN 2: Azure Container Apps (con Docker)

### Requisitos adicionales
- Docker Desktop instalado

### Paso 1: Crear Dockerfile

Ejecuta el siguiente comando para crear el Dockerfile:

```powershell
# Ver instrucciones en el archivo Dockerfile creado
```

### Paso 2: Crear .dockerignore

```powershell
# Ver archivo .dockerignore creado
```

### Paso 3: Construir la imagen Docker

```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas
docker build -t subastas-api:latest -f src/Subastas.WebApi/Dockerfile .
```

### Paso 4: Crear Azure Container Registry

```powershell
az acr create --resource-group rg-subastas --name acrsubastas --sku Basic
```

### Paso 5: Iniciar sesión en ACR

```powershell
az acr login --name acrsubastas
```

### Paso 6: Etiquetar y subir la imagen

```powershell
$acrLoginServer = az acr show --name acrsubastas --query loginServer --output tsv
docker tag subastas-api:latest "$acrLoginServer/subastas-api:latest"
docker push "$acrLoginServer/subastas-api:latest"
```

### Paso 7: Habilitar admin en ACR

```powershell
az acr update --name acrsubastas --admin-enabled true
```

### Paso 8: Obtener credenciales de ACR

```powershell
$acrPassword = az acr credential show --name acrsubastas --query "passwords[0].value" --output tsv
```

### Paso 9: Crear Container App Environment

```powershell
az containerapp env create --name env-subastas --resource-group rg-subastas --location westeurope
```

### Paso 10: Crear Container App

```powershell
az containerapp create `
  --name subastas-api `
  --resource-group rg-subastas `
  --environment env-subastas `
  --image "$acrLoginServer/subastas-api:latest" `
  --registry-server $acrLoginServer `
  --registry-username acrsubastas `
  --registry-password $acrPassword `
  --target-port 8080 `
  --ingress external `
  --env-vars "ConnectionStrings__SubastaConnection=Server=tcp:fpcursos.database.windows.net,1433;Initial Catalog=subasta;User ID=adminoc;Password=A0a0f0f011;Encrypt=True;" `
    "JwtSettings__SecretKey=TU_CLAVE_SECRETA_SUPER_SEGURA_DE_AL_MENOS_32_CARACTERES" `
    "ASPNETCORE_ENVIRONMENT=Production"
```

### Paso 11: Obtener la URL de la aplicación

```powershell
az containerapp show --name subastas-api --resource-group rg-subastas --query properties.configuration.ingress.fqdn --output tsv
```

---

## Publicación Manual (Sin CI/CD)

### Desde Visual Studio (Más fácil)

1. **Clic derecho** en `Subastas.WebApi` → **Publicar**
2. Seleccionar **Azure**
3. Elegir **Azure App Service (Linux)**
4. Iniciar sesión
5. Crear nuevo o seleccionar App Service existente
6. **Publicar**

### Desde PowerShell/CMD

```powershell
# Navegar a la carpeta del proyecto
cd c:\Users\JoseAntonioVallecill\source\repos\subastas\src\Subastas.WebApi

# Publicar para producción
dotnet publish -c Release -o ./publish

# Crear un archivo ZIP
Compress-Archive -Path ./publish/* -DestinationPath ./subastas-api.zip -Force

# Subir a Azure
az webapp deploy --resource-group rg-subastas --name subastas-api-<tu-nombre-unico> --src-path ./subastas-api.zip --type zip
```

---

## Configuración de Archivos Estáticos en Azure

Después del despliegue, necesitas configurar el servidor para servir archivos estáticos:

### Opción A: Crear web.config (para Windows App Service)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\Subastas.WebApi.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    <staticContent>
      <mimeMap fileExtension=".jpg" mimeType="image/jpeg" />
      <mimeMap fileExtension=".jpeg" mimeType="image/jpeg" />
      <mimeMap fileExtension=".png" mimeType="image/png" />
      <mimeMap fileExtension=".pdf" mimeType="application/pdf" />
    </staticContent>
  </system.webServer>
</configuration>
```

### Opción B: Azure App Service Configuration (Linux)

Los archivos estáticos ya funcionan con el código actual en Linux App Service.

---

## Costos Aproximados (VS Professional incluye crédito)

- **App Service B1**: ~13€/mes (recomendado)
- **App Service F1**: Gratis (limitado, para pruebas)
- **Container Apps**: ~5-10€/mes (solo cuando se usa)
- **Azure SQL Database**: Ya lo tienes configurado

**Ventaja VS Professional**: Incluye **50-150€ de crédito mensual gratis**

---

## Actualizar Angular para producción

```typescript
// src/app/services/auth.service.ts
private readonly API_URL = environment.apiUrl + '/usuarios';

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://subastas-api-<tu-nombre>.azurewebsites.net/api'
};
```

Compilar Angular para producción:

```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas\front
ng build --configuration production
```

Los archivos compilados estarán en `dist/` y los puedes servir desde:
- Azure Static Web Apps (gratis)
- Azure Storage con Static Website
- Netlify
- Vercel

---

## Monitoreo y Logs

### Ver logs en tiempo real

```powershell
az webapp log tail --resource-group rg-subastas --name subastas-api-<tu-nombre-unico>
```

### Habilitar Application Insights

```powershell
az monitor app-insights component create --app subastas-insights --location westeurope --resource-group rg-subastas
```

---

## Solución de Problemas Comunes

### Error de conexión a base de datos
- Verificar que el firewall de Azure SQL permite conexiones desde Azure
- En Azure Portal → SQL Server → Firewalls and virtual networks → "Allow Azure services"

### Error 500 al iniciar
- Revisar logs: `az webapp log tail`
- Verificar variables de entorno en Configuration

### Archivos estáticos no se sirven
- Verificar que las carpetas `wwwroot` y `Uploads` se incluyen en la publicación
- Añadir en `.csproj`:
  ```xml
  <ItemGroup>
    <Content Include="wwwroot\**" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="Uploads\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
  ```
