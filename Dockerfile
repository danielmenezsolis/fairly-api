# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración
# y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# -------------------------------
# Fase base: entorno de ejecución
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Forzar IPv4 (evita error "Network is unreachable")
RUN echo "precedence ::ffff:0:0/96  100" | tee -a /etc/gai.conf > /dev/null

# -------------------------------
# Fase build: compilar el proyecto
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FairlyApi/FairlyApi.csproj", "FairlyApi/"]
RUN dotnet restore "./FairlyApi/FairlyApi.csproj"
COPY . .
WORKDIR "/src/FairlyApi"
RUN dotnet build "./FairlyApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -------------------------------
# Fase publish: publicar binarios
# -------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FairlyApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------------------------------
# Fase final: imagen de ejecución
# -------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ⚙️ Desactivar IPv6 completamente (después de copiar archivos)
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=1

# Configurar la URL del servidor ASP.NET
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FairlyApi.dll"]