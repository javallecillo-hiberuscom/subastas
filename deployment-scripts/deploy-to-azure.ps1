# Script para desplegar a Azure App Service usando credenciales de publicación
# Automatiza el deployment sin necesidad de Azure CLI

param(
    [string]$appName = "SubastasWebApi20260202162157",
    [string]$zipFile = "C:\Users\JoseAntonioVallecill\source\repos\subastas\src\Subastas.WebApi\backend-deploy.zip"
)

Write-Host "=== DEPLOYMENT AUTOMÁTICO A AZURE ===" -ForegroundColor Cyan
Write-Host "App Service: $appName" -ForegroundColor Yellow
Write-Host "Archivo: $zipFile" -ForegroundColor Yellow

# Verificar que el archivo existe
if (-not (Test-Path $zipFile)) {
    Write-Host "ERROR: No se encuentra el archivo $zipFile" -ForegroundColor Red
    exit 1
}

Write-Host "`nPaso 1: Obteniendo credenciales de publicación..." -ForegroundColor Green

# Intentar obtener las credenciales desde el portal
$kuduUrl = "https://$appName.scm.azurewebsites.net/api/zipdeploy"
$siteUrl = "https://$appName.azurewebsites.net"

Write-Host "URL Kudu: $kuduUrl" -ForegroundColor Cyan

# Método 1: Usando Basic Auth con credenciales del portal
Write-Host "`n=== NECESITAS CREDENCIALES DE PUBLICACIÓN ===" -ForegroundColor Yellow
Write-Host @"

Para obtenerlas:
1. Ve a: https://portal.azure.com
2. Busca: $appName
3. Click en "Get publish profile" (arriba a la derecha)
4. Abre el archivo XML descargado
5. Busca la sección <publishProfile profileName="..." publishMethod="MSDeploy">
6. Copia: userName y userPWD

"@ -ForegroundColor White

$username = Read-Host "Ingresa el userName (formato: $appName\`$nombre)"
if ([string]::IsNullOrEmpty($username)) {
    Write-Host "`n=== DEPLOYMENT MANUAL ===" -ForegroundColor Yellow
    Write-Host "Ve al Portal de Azure:" -ForegroundColor White
    Write-Host "1. https://portal.azure.com" -ForegroundColor Cyan
    Write-Host "2. Busca: $appName" -ForegroundColor Cyan
    Write-Host "3. Deployment Center -> Zip Deploy" -ForegroundColor Cyan
    Write-Host "4. Arrastra: $zipFile" -ForegroundColor Cyan
    
    # Abrir Portal
    Start-Process "https://portal.azure.com/#@/resource/subscriptions/*/resourceGroups/rg-subastas/providers/Microsoft.Web/sites/$appName/vstscd"
    exit 0
}

$password = Read-Host "Ingresa el userPWD" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host "`nPaso 2: Subiendo archivo ZIP..." -ForegroundColor Green

# Crear credenciales
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username, $plainPassword)))

try {
    # Hacer POST a Kudu API
    $headers = @{
        Authorization = "Basic $base64Auth"
    }
    
    Write-Host "Enviando a Kudu API..." -ForegroundColor Yellow
    
    Invoke-RestMethod -Uri $kuduUrl -Method POST -InFile $zipFile -Headers $headers -ContentType "application/zip" -TimeoutSec 300
    
    Write-Host "`n✅ DEPLOYMENT EXITOSO!" -ForegroundColor Green
    Write-Host "La aplicación se está reiniciando..." -ForegroundColor Yellow
    
    Start-Sleep -Seconds 10
    
    Write-Host "`nPaso 3: Verificando deployment..." -ForegroundColor Green
    
    # Verificar health endpoint
    try {
        $health = Invoke-RestMethod -Uri "$siteUrl/health" -TimeoutSec 30
        Write-Host "✅ Health check: OK" -ForegroundColor Green
        Write-Host "Respuesta: $health" -ForegroundColor Cyan
    } catch {
        Write-Host "⚠️ Health check falló, pero el deployment se completó" -ForegroundColor Yellow
        Write-Host "Verifica manualmente: $siteUrl/health" -ForegroundColor Cyan
    }
    
    Write-Host "`n=== DEPLOYMENT COMPLETADO ===" -ForegroundColor Green
    Write-Host "API URL: $siteUrl" -ForegroundColor Cyan
    Write-Host "Swagger: $siteUrl/swagger" -ForegroundColor Cyan
    Write-Host "Health: $siteUrl/health" -ForegroundColor Cyan
    
    # Abrir Swagger
    Start-Process "$siteUrl/swagger"
    
} catch {
    Write-Host "`n❌ ERROR EN DEPLOYMENT" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    Write-Host "`n=== USAR PORTAL DE AZURE ===" -ForegroundColor Yellow
    Write-Host "1. Ve a: https://portal.azure.com" -ForegroundColor Cyan
    Write-Host "2. Busca: $appName" -ForegroundColor Cyan
    Write-Host "3. Deployment Center -> Zip Deploy" -ForegroundColor Cyan
    Write-Host "4. Arrastra: $zipFile" -ForegroundColor Cyan
    
    Start-Process "https://portal.azure.com/#@/resource/subscriptions/*/resourceGroups/rg-subastas/providers/Microsoft.Web/sites/$appName/vstscd"
}
