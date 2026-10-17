# =============================================================================
# DES104 - Desafío práctico 3 - RT221663
# Imagen de la API de Pedidos (build multi-etapa)
# =============================================================================

# ---------- Etapa 1: compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primero solo los archivos de proyecto, para aprovechar la caché de "restore"
COPY Directory.Build.props Directory.Packages.props RT221663_desafio03.sln ./
COPY src/Desafio03.Entidades/Desafio03.Entidades.csproj     src/Desafio03.Entidades/
COPY src/Desafio03.AccesoDatos/Desafio03.AccesoDatos.csproj src/Desafio03.AccesoDatos/
COPY src/Desafio03.Negocio/Desafio03.Negocio.csproj         src/Desafio03.Negocio/
COPY src/Desafio03.API/Desafio03.API.csproj                 src/Desafio03.API/
COPY tests/Desafio03.Pruebas/Desafio03.Pruebas.csproj       tests/Desafio03.Pruebas/
RUN dotnet restore RT221663_desafio03.sln

# Luego el código fuente
COPY . .

# ---------- Etapa opcional: pruebas unitarias ----------
# docker compose --profile pruebas run --rm pruebas
FROM build AS pruebas
WORKDIR /src
ENTRYPOINT ["dotnet", "test", "tests/Desafio03.Pruebas/Desafio03.Pruebas.csproj", "--no-restore", "--logger", "console;verbosity=normal"]

# ---------- Etapa 2: publicación ----------
FROM build AS publish
RUN dotnet publish src/Desafio03.API/Desafio03.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 3: imagen final ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Desafio03.API.dll"]
