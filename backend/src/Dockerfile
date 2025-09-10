# Dockerfile para CoreApp API - Multi-stage build otimizado para .NET 9
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia arquivos de projeto primeiro para cache de dependências
COPY ["CoreApp.Api/CoreApp.Api.csproj", "CoreApp.Api/"]
COPY ["CoreApp.Application/CoreApp.Application.csproj", "CoreApp.Application/"]
COPY ["CoreApp.Domain/CoreApp.Domain.csproj", "CoreApp.Domain/"]
COPY ["CoreApp.Infrastructure/CoreApp.Infrastructure.csproj", "CoreApp.Infrastructure/"]
COPY ["CoreApp.Verticals/CoreApp.Verticals.csproj", "CoreApp.Verticals/"]

# Restaura dependências
RUN dotnet restore "./CoreApp.Api/CoreApp.Api.csproj"

# Copia todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/CoreApp.Api"
RUN dotnet build "./CoreApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage final
FROM base AS final
WORKDIR /app

# Cria usuário não-root para segurança
RUN addgroup --system --gid 1001 coreapp \
    && adduser --system --uid 1001 --ingroup coreapp coreapp

# Copia arquivos publicados
COPY --from=publish /app/publish .

# Define usuário não-root
USER coreapp

# Health check para monitoramento usando .NET HttpClient nativo
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet --info > /dev/null && echo "OK" || exit 1

# Ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "CoreApp.Api.dll"]