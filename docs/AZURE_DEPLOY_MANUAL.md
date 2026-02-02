# Despliegue Manual Completado

## ✅ Recursos Creados en Azure

Tu API ya está lista en Azure con los siguientes recursos:

- **URL de la API**: https://subastas-api-borox.azurewebsites.net
- **Grupo de Recursos**: rg-subastas
- **App Service Plan**: plan-subastas (B1)
- **Web App**: subastas-api-borox

## 📝 Variables de Entorno Configuradas

- ✅ ConnectionStrings__SubastaConnection
- ✅ JwtSettings (SecretKey, Issuer, Audience, ExpirationMinutes)
- ✅ ASPNETCORE_ENVIRONMENT = Production
- ✅ CORS configurado (permite acceso desde cualquier origen)

## 🚀 Método de Despliegue Recomendado

### OPCIÓN 1: Visual Studio (MÁS FÁCIL)

1. Abre Visual Studio
2. Clic derecho en proyecto `Subastas.WebApi`
3. Click en **Publicar**
4. Click en **Agregar un destino de publicación**
5. Selecciona **Azure**
6. Selecciona **Azure App Service (Linux)**
7. **Inicia sesión** con tu cuenta Microsoft (javallecillo@hiberus.com)
8. En la lista, selecciona:
   - **Suscripción**: Suscripción de Visual Studio Professional
   - **Grupo de recursos**: rg-subastas
   - **App Service**: subastas-api-borox
9. Click **Finalizar**
10. Click **Publicar**

Visual Studio subirá tu código automáticamente.

### OPCIÓN 2: Desde VS Code (Extensión Azure App Service)

1. Instala la extensión "Azure App Service" en VS Code
2. Inicia sesión en Azure
3. Clic derecho en `subastas-api-borox` en el panel de Azure
4. Selecciona "Deploy to Web App"
5. Selecciona la carpeta `src/Subastas.WebApi/publish`

### OPCIÓN 3: Git Deploy (Desde el repositorio)

```powershell
# Si tienes Git configurado
cd c:\Users\JoseAntonioVallecill\source\repos\subastas

# Inicializar git si no lo has hecho
git init
git add .
git commit -m "Initial commit"

# Obtener credenciales de deployment
az webapp deployment list-publishing-credentials --name subastas-api-borox --resource-group rg-subastas

# Configurar remote y push
git remote add azure https://subastas-api-borox.scm.azurewebsites.net/subastas-api-borox.git
git push azure master
```

## 🔧 Configuración Post-Despliegue

### Verificar Firewall de Azure SQL

Para que Azure App Service pueda conectarse a tu base de datos:

1. Ve a https://portal.azure.com
2. Busca "fpcursos" (tu SQL Server)
3. Ve a **Seguridad** → **Firewall y redes virtuales**
4. Asegúrate que está marcado: **"Permitir que los servicios y recursos de Azure accedan a este servidor"**
5. Si no está marcado, márcalo y guarda

### Verificar que la aplicación funciona

Después de desplegar, prueba estos endpoints:

```powershell
# Test básico
curl https://subastas-api-borox.azurewebsites.net

# Ver Swagger (si está habilitado en producción)
Start-Process "https://subastas-api-borox.azurewebsites.net/swagger"
```

### Ver logs en tiempo real

```powershell
az webapp log tail --name subastas-api-borox --resource-group rg-subastas
```

## 📱 Actualizar Angular con la URL de Producción

Una vez que la API esté funcionando en Azure, actualiza tu frontend:

### Archivo: `front/src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://subastas-api-borox.azurewebsites.net/api'
};
```

### Archivo: `front/src/app/services/auth.service.ts`

Asegúrate de usar el environment:

```typescript
import { environment } from '../../environments/environment';

// ...
private readonly API_URL = environment.apiUrl + '/usuarios';
```

### Compilar Angular para producción

```powershell
cd c:\Users\JoseAntonioVallecill\source\repos\subastas\front
ng build --configuration production
```

Los archivos estarán en `front/dist/` listos para subir a un hosting.

## 🌐 Opciones para desplegar Angular

### 1. Azure Static Web Apps (GRATIS)

```powershell
# Crear Static Web App
az staticwebapp create `
  --name subastas-frontend `
  --resource-group rg-subastas `
  --location westeurope `
  --source . `
  --branch main `
  --app-location "front" `
  --output-location "dist"
```

### 2. Azure Storage + Static Website (MUY BARATO ~1€/mes)

```powershell
# Crear storage account
az storage account create `
  --name subastasstorage `
  --resource-group rg-subastas `
  --location westeurope `
  --sku Standard_LRS

# Habilitar static website
az storage blob service-properties update `
  --account-name subastasstorage `
  --static-website `
  --index-document index.html `
  --404-document index.html

# Subir archivos
az storage blob upload-batch `
  --account-name subastasstorage `
  --source front/dist/front/browser `
  --destination '$web'
```

### 3. Netlify / Vercel (GRATIS y muy fácil)

1. Ve a https://netlify.com o https://vercel.com
2. Conecta tu repositorio de Git
3. Configura:
   - Build command: `npm run build`
   - Publish directory: `dist/front/browser`
4. Deploy automático

## 💰 Costos Mensuales Estimados

- **App Service B1**: ~13€/mes (incluido en crédito VS Professional 50-150€/mes)
- **Azure SQL Database**: Ya lo tienes (S0 ~13€/mes)
- **Total nuevo**: ~13€/mes

**¡Tu crédito de VS Professional cubre todo!**

## 🔍 Troubleshooting

### La API no responde

```powershell
# Ver logs
az webapp log tail --name subastas-api-borox --resource-group rg-subastas

# Restart
az webapp restart --name subastas-api-borox --resource-group rg-subastas
```

### Error de base de datos

Verifica que el firewall de Azure SQL permite conexiones desde Azure.

### Variables de entorno no están

```powershell
# Listar variables
az webapp config appsettings list --name subastas-api-borox --resource-group rg-subastas
```

## 📞 Soporte

Si tienes problemas:

1. Revisa los logs: `az webapp log tail`
2. Verifica Application Insights en Azure Portal
3. Prueba los endpoints con Postman o curl

## ✅ Checklist Final

- [ ] API desplegada en Azure
- [ ] Variables de entorno configuradas
- [ ] Firewall de SQL permite Azure
- [ ] API responde en https://subastas-api-borox.azurewebsites.net
- [ ] Frontend actualizado con nueva URL
- [ ] Frontend compilado para producción
- [ ] Frontend desplegado

¡Listo para producción! 🎉
