# 1. Imagen base con el runtime de ASP.NET Core (.NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Imagen con SDK de .NET 10 para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivo de solución
COPY src/TarjetasCredito.slnx ./

# Copiar archivos de proyecto por separado (.csproj) para optimizar el caché de Docker (dotnet restore)
COPY src/TarjetasCredito.Client/TarjetasCredito.Client.csproj ./TarjetasCredito.Client/
COPY src/TarjetasCredito.Server/TarjetasCredito.Server.csproj ./TarjetasCredito.Server/
COPY src/TarjetasCredito.Shared/TarjetasCredito.Shared.csproj ./TarjetasCredito.Shared/
COPY src/TarjetasCredito.Domain/TarjetasCredito.Domain.csproj ./TarjetasCredito.Domain/
COPY src/TarjetasCredito.Infrastructure/TarjetasCredito.Infrastructure.csproj ./TarjetasCredito.Infrastructure/

# Restaurar paquetes NuGet
RUN dotnet restore

# Copiar todo el código fuente de los proyectos
COPY src/TarjetasCredito.Client/ ./TarjetasCredito.Client/
COPY src/TarjetasCredito.Server/ ./TarjetasCredito.Server/
COPY src/TarjetasCredito.Shared/ ./TarjetasCredito.Shared/
COPY src/TarjetasCredito.Domain/ ./TarjetasCredito.Domain/
COPY src/TarjetasCredito.Infrastructure/ ./TarjetasCredito.Infrastructure/

# Publicar el servidor (compila e incluye la PWA WebAssembly automáticamente)
RUN dotnet publish TarjetasCredito.Server/TarjetasCredito.Server.csproj -c Release -o /app/publish

# 3. Imagen de producción final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# La base de datos SQLite y las llaves de Data Protection (cifran el bearer token de Identity, ver
# SPEC-004) deben persistir entre recreaciones del contenedor — ver SPEC-001 "Despliegue con Docker".
VOLUME /app/data

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TarjetasCredito.Server.dll"]
