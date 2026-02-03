# 📘 Manual de Despliegue - Sistema de Subastas

## Índice
1. [Requisitos Previos](#requisitos-previos)
2. [Configuración Entorno Local](#configuración-entorno-local)
3. [Despliegue a Azure](#despliegue-a-azure)
4. [Solución de Problemas](#solución-de-problemas)
5. [Comandos Útiles](#comandos-útiles)

---

## 📋 Requisitos Previos

### Software Necesario

| Herramienta | Versión | Descarga |
|------------|---------|----------|
| Node.js | 18.x o superior | [nodejs.org](https://nodejs.org) |
| .NET SDK | 8.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Angular CLI | 18.x | `npm install -g @angular/cli` |
| Git | Última | [git-scm.com](https://git-scm.com) |
| Visual Studio Code | Última | [code.visualstudio.com](https://code.visualstudio.com) |
| Azure CLI | Última | [azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) |

### Extensiones VS Code Recomendadas
- C# Dev Kit
- Angular Language Service
- Azure Tools
- REST Client

### Cuentas Necesarias
- **GitHub Account** (para repositorio)
- **Azure Account** (para despliegue en producción)

---

## 🖥️ Configuración Entorno Local

### 1. Clonar el Repositorio

```powershell
# Clonar proyecto
git clone <URL_REPOSITORIO>
cd subastas

# Verificar estructura
ls
```

### 2. Configurar Base de Datos

#### Opción A: Azure SQL Database (Producción)

1. **Crear Azure SQL Database** (si no existe):
```powershell
# Login en Azure
az login

# Crear grupo de recursos
az group create --name subastas-rg --location westeurope

# Crear servidor SQL
az sql server create `
  --name subastasbidserver `
  --resource-group subastas-rg `
  --location westeurope `
  --admin-user sqladmin `
  --admin-password <TU_PASSWORD>

# Crear base de datos
az sql db create `
  --resource-group subastas-rg `
  --server subastasbidserver `
  --name Subastas `
  --service-objective S0
```

2. **Configurar Firewall**:
```powershell
# Permitir tu IP actual
az sql server firewall-rule create `
  --resource-group subastas-rg `
  --server subastasbidserver `
  --name AllowMyIP `
  --start-ip-address <TU_IP> `
  --end-ip-address <TU_IP>

# Permitir servicios Azure
az sql server firewall-rule create `
  --resource-group subastas-rg `
  --server subastasbidserver `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0
```

3. **Ejecutar Scripts de Base de Datos**:
```powershell
cd database-scripts

# Conectar con sqlcmd
sqlcmd -S subastasbidserver.database.windows.net,1433 `
  -d Subastas `
  -U sqladmin `
  -P <PASSWORD>

# O usar Azure Data Studio / SQL Server Management Studio
```

Ejecutar en orden:
1. `crear-tabla-notificaciones-admin.sql`
2. Otros scripts de creación/actualización

#### Opción B: SQL Server Local (Desarrollo)

```powershell
# Instalar SQL Server Express LocalDB
winget install Microsoft.SQLServer.2022.Express

# Crear base de datos
sqlcmd -S (localdb)\MSSQLLocalDB -Q "CREATE DATABASE Subastas"
```

### 3. Configurar Backend (.NET)

```powershell
cd src/Subastas.WebApi
```

**Editar `appsettings.Development.json`**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=subastasbidserver.database.windows.net,1433;Database=Subastas;User ID=sqladmin;Password=<TU_PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "JwtSettings": {
    "SecretKey": "tu-clave-secreta-super-segura-de-al-menos-32-caracteres",
    "Issuer": "SubastasAPI",
    "Audience": "SubastasFrontend",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": "http://localhost:4200"
  }
}
```

**Restaurar paquetes y compilar**:
```powershell
# Restaurar dependencias
dotnet restore

# Compilar proyecto
dotnet build

# Verificar compilación
# Debe mostrar: "Compilación realizado correctamente"
```

### 4. Configurar Frontend (Angular)

```powershell
cd ../../front
```

**Instalar dependencias**:
```powershell
npm install
```

**Verificar `proxy.conf.dev.json`**:
```json
{
  "/api": {
    "target": "http://localhost:56801",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

### 5. Iniciar Servicios

#### Terminal 1 - Backend

```powershell
# Navegar a WebApi
cd src\Subastas.WebApi

# Ejecutar backend
dotnet run

# Debe mostrar:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:56801
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to shut down.
```

**Verificar backend**: Abrir http://localhost:56801/swagger

#### Terminal 2 - Frontend

```powershell
# Navegar a front
cd front

# Ejecutar frontend
npm start

# Debe mostrar:
# Application bundle generation complete. [1.234 seconds]
# Watch mode enabled. Watching for file changes...
# ➜  Local:   http://localhost:4200/
```

**Verificar frontend**: Abrir http://localhost:4200/

### 6. Verificar Funcionamiento

1. **Probar Login Admin**:
   - Usuario: `lucia@admin.com`
   - Password: `Admin123!`

2. **Verificar Endpoints**:
   - GET http://localhost:56801/api/usuarios
   - GET http://localhost:56801/api/vehiculos
   - GET http://localhost:56801/api/subastas

3. **Verificar Base de Datos**:
```sql
-- Conectar y verificar tablas
SELECT name FROM sys.tables ORDER BY name;

-- Verificar datos
SELECT COUNT(*) AS TotalUsuarios FROM Usuario;
SELECT COUNT(*) AS TotalVehiculos FROM Vehiculo;
SELECT COUNT(*) AS TotalSubastas FROM Subasta;
```

---

## ☁️ Despliegue a Azure

### 1. Preparar Recursos Azure

```powershell
# Login
az login

# Variables
$resourceGroup = "subastas-rg"
$location = "westeurope"
$backendName = "subastas-backend"
$frontendName = "subastas-frontend"

# Crear grupo de recursos (si no existe)
az group create --name $resourceGroup --location $location
```

### 2. Despliegue Backend (App Service)

```powershell
cd src/Subastas.WebApi

# Crear App Service Plan
az appservice plan create `
  --name subastas-plan `
  --resource-group $resourceGroup `
  --sku B1 `
  --is-linux

# Crear Web App
az webapp create `
  --name $backendName `
  --resource-group $resourceGroup `
  --plan subastas-plan `
  --runtime "DOTNET|8.0"

# Configurar connection string
az webapp config connection-string set `
  --name $backendName `
  --resource-group $resourceGroup `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="Server=subastasbidserver.database.windows.net,1433;Database=Subastas;User ID=sqladmin;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;"

# Configurar CORS
az webapp cors add `
  --name $backendName `
  --resource-group $resourceGroup `
  --allowed-origins "https://$frontendName.azurestaticapps.net"

# Publicar código
dotnet publish -c Release -o ./publish

# Comprimir
Compress-Archive -Path ./publish/* -DestinationPath publish.zip -Force

# Desplegar
az webapp deployment source config-zip `
  --name $backendName `
  --resource-group $resourceGroup `
  --src publish.zip
```

**Verificar**: https://subastas-backend.azurewebsites.net/swagger

### 3. Despliegue Frontend (Static Web App)

```powershell
cd ../../front

# Actualizar environment.prod.ts
# apiUrl: 'https://subastas-backend.azurewebsites.net/api'

# Compilar para producción
npm run build -- --configuration production

# Crear Static Web App
az staticwebapp create `
  --name $frontendName `
  --resource-group $resourceGroup `
  --location $location `
  --source . `
  --app-location "/front" `
  --output-location "dist/subastas-app/browser" `
  --branch main

# O usar script automatizado
cd ../deployment-scripts
./deploy-to-azure.ps1
```

### 4. Configurar Dominios (Opcional)

```powershell
# Backend
az webapp config hostname add `
  --webapp-name $backendName `
  --resource-group $resourceGroup `
  --hostname api.tudominio.com

# Frontend
az staticwebapp hostname set `
  --name $frontendName `
  --resource-group $resourceGroup `
  --hostname www.tudominio.com
```

---

## 🔧 Solución de Problemas

### Problema: Backend no arranca

**Error**: `No se ha podido encontrar un proyecto para ejecutar`

**Solución**:
```powershell
# Asegurarse de estar en la carpeta correcta
cd c:\Users\<TU_USUARIO>\source\repos\subastas\src\Subastas.WebApi

# Verificar que existe el .csproj
ls *.csproj

# Ejecutar
dotnet run
```

### Problema: No se puede conectar a Azure SQL

**Error**: `TCP Provider: Host desconocido` o `Login timeout expired`

**Soluciones**:

1. **Verificar Firewall**:
```powershell
# Obtener tu IP pública
$myIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
Write-Host "Tu IP: $myIP"

# Agregar regla de firewall
az sql server firewall-rule create `
  --resource-group subastas-rg `
  --server subastasbidserver `
  --name AllowMyCurrentIP `
  --start-ip-address $myIP `
  --end-ip-address $myIP
```

2. **Verificar Connection String**:
```powershell
# Debe seguir este formato exacto
Server=subastasbidserver.database.windows.net,1433;
Database=Subastas;
User ID=sqladmin;
Password=<TU_PASSWORD>;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

3. **Probar conexión con sqlcmd**:
```powershell
sqlcmd -S subastasbidserver.database.windows.net,1433 `
  -d Subastas `
  -U sqladmin `
  -P <PASSWORD> `
  -Q "SELECT @@VERSION"
```

### Problema: CORS Errors en Frontend

**Error**: `Access to XMLHttpRequest blocked by CORS policy`

**Solución en Backend** ([Program.cs](../src/Subastas.WebApi/Program.cs)):
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", 
                          "https://subastas-frontend.azurestaticapps.net")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### Problema: npm install falla

**Error**: `ERESOLVE unable to resolve dependency tree`

**Solución**:
```powershell
# Limpiar cache
npm cache clean --force

# Eliminar node_modules y package-lock.json
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json

# Reinstalar con legacy peer deps
npm install --legacy-peer-deps
```

### Problema: JWT Token inválido

**Error**: `401 Unauthorized`

**Soluciones**:

1. **Verificar SecretKey** (mínimo 32 caracteres)
2. **Verificar tiempo de expiración**:
```csharp
// En appsettings.json
"ExpirationMinutes": 60
```
3. **Limpiar localStorage del navegador**:
```javascript
// En consola del navegador
localStorage.clear();
```

### Problema: Entity Framework Migrations

**Crear migración**:
```powershell
cd src/Subastas.Infrastructure

dotnet ef migrations add MigracionNombre `
  --startup-project ../Subastas.WebApi `
  --context SubastaContext

dotnet ef database update `
  --startup-project ../Subastas.WebApi
```

---

## 📚 Comandos Útiles

### Backend (.NET)

```powershell
# Compilar
dotnet build

# Ejecutar
dotnet run

# Ejecutar con watch (auto-reload)
dotnet watch run

# Limpiar
dotnet clean

# Restaurar paquetes
dotnet restore

# Publicar para producción
dotnet publish -c Release -o ./publish

# Ver logs
dotnet run --verbosity detailed
```

### Frontend (Angular)

```powershell
# Desarrollo
npm start

# Desarrollo con proxy
npm run start:dev

# Producción (compilar)
npm run build

# Producción optimizada
npm run build -- --configuration production

# Linter
npm run lint

# Tests
npm test

# Limpiar cache
npm cache clean --force

# Actualizar dependencias
npm update

# Ver versiones
npm list --depth=0
```

### Base de Datos

```powershell
# Conectar a Azure SQL
sqlcmd -S subastasbidserver.database.windows.net,1433 `
  -d Subastas `
  -U sqladmin `
  -P <PASSWORD>

# Ejecutar script
sqlcmd -S <SERVER> -d <DB> -U <USER> -P <PASS> -i script.sql

# Backup
sqlcmd -S <SERVER> -d <DB> -U <USER> -P <PASS> `
  -Q "BACKUP DATABASE Subastas TO DISK = 'C:\backup.bak'"

# Listar tablas
SELECT name FROM sys.tables ORDER BY name;

# Ver estructura de tabla
EXEC sp_help 'Usuario';

# Contar registros
SELECT 
  t.name AS TableName,
  SUM(p.rows) AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
GROUP BY t.name
ORDER BY t.name;
```

### Git

```powershell
# Estado
git status

# Añadir cambios
git add .

# Commit
git commit -m "mensaje"

# Push
git push origin main

# Pull
git pull origin main

# Ver historial
git log --oneline -10

# Crear rama
git checkout -b feature/nueva-funcionalidad

# Cambiar rama
git checkout main

# Merge
git merge feature/nueva-funcionalidad
```

### Azure CLI

```powershell
# Login
az login

# Ver suscripciones
az account list --output table

# Cambiar suscripción
az account set --subscription "<SUBSCRIPTION_ID>"

# Ver recursos
az resource list --resource-group subastas-rg --output table

# Ver logs App Service
az webapp log tail --name subastas-backend --resource-group subastas-rg

# Reiniciar App Service
az webapp restart --name subastas-backend --resource-group subastas-rg

# Ver connection strings
az webapp config connection-string list `
  --name subastas-backend `
  --resource-group subastas-rg

# Ver variables de entorno
az webapp config appsettings list `
  --name subastas-backend `
  --resource-group subastas-rg
```

---

## 🚀 Checklist Rápido de Despliegue

### Local
- [ ] Node.js 18+ instalado
- [ ] .NET 8 SDK instalado
- [ ] Git clonado
- [ ] Base de datos creada
- [ ] Firewall SQL configurado
- [ ] appsettings.Development.json configurado
- [ ] `dotnet restore` ejecutado
- [ ] `dotnet build` sin errores
- [ ] `npm install` completado
- [ ] Backend corriendo en :56801
- [ ] Frontend corriendo en :4200
- [ ] Swagger accesible
- [ ] Login funcional

### Azure
- [ ] Grupo de recursos creado
- [ ] SQL Server creado
- [ ] Base de datos creada
- [ ] Firewall configurado
- [ ] App Service Plan creado
- [ ] App Service (backend) creado
- [ ] Static Web App (frontend) creada
- [ ] Connection strings configurados
- [ ] CORS configurado
- [ ] Código desplegado
- [ ] Dominios configurados (opcional)
- [ ] SSL/HTTPS activo
- [ ] Endpoints verificados

---

## 📞 Soporte

### Recursos
- [Documentación .NET](https://learn.microsoft.com/dotnet/)
- [Documentación Angular](https://angular.dev/)
- [Azure Documentation](https://docs.microsoft.com/azure/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

### Logs y Monitoreo
```powershell
# Ver logs backend local
# Los logs aparecen en la consola de dotnet run

# Ver logs Azure
az webapp log tail --name subastas-backend --resource-group subastas-rg

# Habilitar logging detallado
az webapp log config `
  --name subastas-backend `
  --resource-group subastas-rg `
  --application-logging filesystem `
  --level verbose
```

---

## 📝 Notas Importantes

1. **Seguridad**:
   - Nunca commitear contraseñas en Git
   - Usar Azure Key Vault para secretos en producción
   - Habilitar HTTPS siempre
   - Rotar claves JWT periódicamente

2. **Performance**:
   - Usar índices en base de datos
   - Habilitar compresión en App Service
   - Usar CDN para archivos estáticos
   - Implementar caching donde sea posible

3. **Costes Azure**:
   - App Service B1: ~13€/mes
   - SQL Database S0: ~15€/mes
   - Static Web App: Gratis (con límites)
   - **Total estimado**: ~30€/mes

4. **Backup**:
   - Configurar backup automático de SQL Database
   - Hacer commits frecuentes a Git
   - Documentar cambios importantes

---

**Última actualización**: Diciembre 2024  
**Versión**: 1.0
