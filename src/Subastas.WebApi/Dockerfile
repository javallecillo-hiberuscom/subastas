# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Subastas.WebApi/Subastas.WebApi.csproj", "Subastas.WebApi/"]
COPY ["src/Subastas.Application/Subastas.Application.csproj", "Subastas.Application/"]
COPY ["src/Subastas.Domain/Subastas.Domain.csproj", "Subastas.Domain/"]
COPY ["src/Subastas.Infrastructure/Subastas.Infrastructure.csproj", "Subastas.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Subastas.WebApi/Subastas.WebApi.csproj"

# Copiar el resto del código
COPY src/ .

# Compilar y publicar
WORKDIR "/src/Subastas.WebApi"
RUN dotnet build "Subastas.WebApi.csproj" -c Release -o /app/build
RUN dotnet publish "Subastas.WebApi.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar archivos publicados
COPY --from=build /app/publish .

# Crear directorios para archivos estáticos
RUN mkdir -p wwwroot/img Uploads/IAE

# Exponer puerto
EXPOSE 8080

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicio
ENTRYPOINT ["dotnet", "Subastas.WebApi.dll"]
