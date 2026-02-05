# Guía de instalación y configuración

## Requisitos previos
- Node.js y npm
- .NET 8 SDK
- SQL Server (o MySQL compatible)

## Instalación local
1. Clonar el repositorio.
2. Instalar dependencias frontend:
   ```bash
   cd front
   npm install
   ```
3. Instalar dependencias backend:
   ```bash
   cd back/subastas
   dotnet restore
   ```
4. Configurar la base de datos en `appsettings.json`.
5. Ejecutar migraciones:
   ```bash
   dotnet ef database update
   ```
6. Lanzar backend:
   ```bash
   dotnet run
   ```
7. Lanzar frontend:
   ```bash
   npm start
   ```

## Instalación en hosting
- Subir el proyecto a Azure Static Web Apps y App Service siguiendo la documentación oficial.
- Configurar variables de entorno y cadena de conexión.
