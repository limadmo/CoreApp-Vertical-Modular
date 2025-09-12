# CoreApp Backend - Dockerfile para .NET 9.0.1
# Multi-stage build para otimização de imagem

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar todo o código fonte
COPY src/ ./src/

# Build da aplicação com restore
WORKDIR /app/src/CoreApp.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Development with SDK for migrations
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Configurações de ambiente brasileiro
ENV TZ=America/Sao_Paulo
ENV DOTNET_CULTURE=pt-BR
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Instalar dependências do sistema (para PostgreSQL e timezone brasileiros)
RUN apt-get update && apt-get install -y \
    tzdata \
    locales \
    && locale-gen pt_BR.UTF-8 \
    && rm -rf /var/lib/apt/lists/*

# Configurar timezone brasileiro
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1001 coreapp \
    && adduser --system --uid 1001 --ingroup coreapp coreapp

# Copiar código fonte para desenvolvimento e build publicado
COPY --from=build /app/publish /app
COPY src/ /app/src/
RUN chown -R coreapp:coreapp /app

# Instalar Entity Framework tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Criar diretório para logs
RUN mkdir -p /app/logs && chown -R coreapp:coreapp /app/logs

# Trocar para usuário não-root
USER coreapp

# Expor porta da API
EXPOSE 8080

# Health check para verificar se API está respondendo
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Comando de entrada da aplicação
ENTRYPOINT ["dotnet", "CoreApp.Api.dll"]