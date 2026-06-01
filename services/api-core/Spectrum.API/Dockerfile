# Stage 1: Compilación y restauración
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar el archivo del proyecto y restaurar las dependencias de NuGet
COPY services/api-core/Spectrum.API/*.csproj ./services/api-core/Spectrum.API/
RUN dotnet restore ./services/api-core/Spectrum.API/Spectrum.API.csproj

# Copiar absolutamente todo el código fuente del repositorio
COPY . .

# Cambiar el directorio de trabajo hacia la carpeta de la API y publicar en modo Release
WORKDIR /app/services/api-core/Spectrum.API
RUN dotnet publish -c Release -o /out

# Stage 2: Imagen final optimizada y ultra ligera para correr la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /out .

# Configurar variables de entorno esenciales para el contenedor
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Exponer el puerto interno del contenedor
EXPOSE 5000

# Comando para arrancar la API al encender el contenedor
ENTRYPOINT ["dotnet", "Spectrum.API.dll"]