# Script para desplegar Frontend a Azure Static Web Apps
param(
    [switch]$SkipBuild
)

Write-Host "=== DEPLOYMENT FRONTEND A AZURE STATIC WEB APPS ===" -ForegroundColor Cyan

# Variables
$staticWebAppName = "blue-flower-00b3c6b03"
$frontDir = "C:\Users\JoseAntonioVallecill\source\repos\subastas\front"
$distDir = "$frontDir\dist\front\browser"
$tokenFile = "$frontDir\deployment-token.txt"

# Build si no se skipea
if (-not $SkipBuild) {
    Write-Host "`nCompilando aplicación Angular..." -ForegroundColor Green
    cd $frontDir
    npm run build -- --configuration=production
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Falló la compilación" -ForegroundColor Red
        exit 1
    }
}

# Verificar que existe el build
if (-not (Test-Path $distDir)) {
    Write-Host "ERROR: No se encuentra el directorio de build: $distDir" -ForegroundColor Red
    exit 1
}

# Verificar token
if (-not (Test-Path $tokenFile)) {
    Write-Host "ERROR: No se encuentra el archivo de token: $tokenFile" -ForegroundColor Red
    exit 1
}

$deploymentToken = Get-Content $tokenFile -Raw
$deploymentToken = $deploymentToken.Trim()

Write-Host "`nVerificando Azure Static Web Apps CLI..." -ForegroundColor Green
$swaInstalled = Get-Command swa -ErrorAction SilentlyContinue
if (-not $swaInstalled) {
    Write-Host "Instalando @azure/static-web-apps-cli..." -ForegroundColor Yellow
    npm install -g @azure/static-web-apps-cli
}

Write-Host "`nDesplegando a Azure Static Web Apps..." -ForegroundColor Green
Write-Host "Directorio: $distDir" -ForegroundColor Yellow

cd $distDir

# Desplegar usando SWA CLI
swa deploy . --deployment-token $deploymentToken --env production

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=== DEPLOYMENT EXITOSO ===" -ForegroundColor Green
    Write-Host "URL: https://$staticWebAppName.1.azurestaticapps.net" -ForegroundColor Cyan
    Write-Host "`nEspera 2-3 minutos para que los cambios se propaguen" -ForegroundColor Yellow
} else {
    Write-Host "`n=== ERROR EN DEPLOYMENT ===" -ForegroundColor Red
    Write-Host "Código de salida: $LASTEXITCODE" -ForegroundColor Yellow
}
