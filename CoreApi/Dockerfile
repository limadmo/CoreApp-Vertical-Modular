# Dockerfile para CoreApi - Sistema SAAS Multi-tenant Brasileiro
# Otimizado para produção com Dokploy + Traefik

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore --no-cache

# Copy source code and build
COPY . ./
RUN dotnet build -c Release --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install required packages for PostgreSQL and timezone
RUN apt-get update && apt-get install -y \
    tzdata \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Set timezone to São Paulo (Brasil)
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --ingroup dotnet dotnet

# Copy published app
COPY --from=publish --chown=dotnet:dotnet /app/publish .

# Set user
USER dotnet

# Health check for Docker/Dokploy
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Start command with seed automático
ENTRYPOINT ["dotnet", "CoreApi.dll", "--seed-all"]