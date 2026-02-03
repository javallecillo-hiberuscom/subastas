Write-Host "=== Iniciando compilación y despliegue ===" -ForegroundColor Cyan

Set-Location "C:\Users\JoseAntonioVallecill\source\repos\subastas"

Write-Host "`n1. Compilando backend..." -ForegroundColor Yellow
dotnet publish src\Subastas.WebApi\Subastas.WebApi.csproj -c Release -o publish --nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Compilación exitosa" -ForegroundColor Green
    
    Write-Host "`n2. Comprimiendo archivos..." -ForegroundColor Yellow
    Remove-Item .\backend-deploy.zip -ErrorAction SilentlyContinue
    Compress-Archive -Path .\publish\* -DestinationPath .\backend-deploy.zip -Force
    Write-Host "✓ Archivos comprimidos" -ForegroundColor Green
    
    Write-Host "`n3. Desplegando a Azure..." -ForegroundColor Yellow
    az webapp deploy --resource-group Curso --name SubastasWebApi20260202162157 --src-path .\backend-deploy.zip --type zip
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ DESPLIEGUE COMPLETADO EXITOSAMENTE" -ForegroundColor Green
    } else {
        Write-Host "`n✗ Error en el despliegue" -ForegroundColor Red
    }
} else {
    Write-Host "`n✗ Error en la compilación" -ForegroundColor Red
}

Write-Host "`n=== Proceso finalizado ===" -ForegroundColor Cyan
Read-Host "Presiona Enter para cerrar"
