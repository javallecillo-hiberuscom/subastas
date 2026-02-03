# Script para desplegar el backend a Azure App Service usando Kudu API

$appName = "SubastasWebApi20260202162157"
$resourceGroup = "rg-subastas"
$zipPath = "C:\Users\JoseAntonioVallecill\source\repos\subastas\src\Subastas.WebApi\backend-deploy.zip"

Write-Host "=== Desplegando Backend a Azure ===" -ForegroundColor Cyan
Write-Host "App Service: $appName" -ForegroundColor Yellow
Write-Host "Archivo: $zipPath" -ForegroundColor Yellow

# Nota: Necesitas tener Azure CLI instalado o usar las credenciales de publicación
# Opción 1: Si tienes Azure CLI instalado:
# az webapp deploy --resource-group $resourceGroup --name $appName --src-path $zipPath --type zip

# Opción 2: Usando el Portal de Azure
Write-Host "`n=== INSTRUCCIONES MANUALES ===" -ForegroundColor Green
Write-Host "1. Ve a: https://portal.azure.com" -ForegroundColor White
Write-Host "2. Busca tu App Service: $appName" -ForegroundColor White
Write-Host "3. En el menú izquierdo, ve a 'Deployment Center'" -ForegroundColor White
Write-Host "4. Haz clic en 'FTPS credentials' o 'Local Git/FTPS credentials'" -ForegroundColor White
Write-Host "5. O simplemente arrastra el archivo ZIP a la sección de 'Zip Deploy'" -ForegroundColor White
Write-Host "`nArchivo listo en: $zipPath" -ForegroundColor Cyan

# Opción 3: Usando VS Code con la extensión Azure App Service
Write-Host "`n=== USANDO VS CODE ===" -ForegroundColor Green
Write-Host "1. Instala la extensión 'Azure App Service' en VS Code" -ForegroundColor White
Write-Host "2. Click derecho en la carpeta 'publish'" -ForegroundColor White  
Write-Host "3. Selecciona 'Deploy to Web App...'" -ForegroundColor White
Write-Host "4. Selecciona '$appName'" -ForegroundColor White

Write-Host "`nPresiona Enter para abrir el Portal de Azure..." -ForegroundColor Yellow
Read-Host
Start-Process "https://portal.azure.com/#@/resource/subscriptions/SUBSCRIPTION_ID/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$appName/vstscd"
