# CoreApp - Sistema SAAS Multi-tenant Brasileiro

Sistema SAAS multi-tenant desenvolvido em .NET 9 com arquitetura de verticais dinâmicas para comércios brasileiros.

## 🏗️ Arquitetura

- **Backend**: ASP.NET Core 9 + Entity Framework Core
- **Reverse Proxy**: Traefik v3.0 para roteamento multi-tenant
- **Database**: PostgreSQL 16 com isolamento por tenant
- **Cache**: Redis para performance
- **Containerização**: Docker + Docker Compose

## 🌐 URLs Multi-tenant

### Desenvolvimento Local
- **API Principal**: http://api.localhost
- **Swagger UI**: http://api.localhost (raiz)
- **Health Check**: http://api.localhost/health

### Multi-tenant por Subdomínio
- **Demo**: http://demo.api.localhost
- **Padaria**: http://padaria-centro.api.localhost  
- **Farmácia**: http://farmacia-saude.api.localhost

### Serviços Administrativos
- **Traefik Dashboard**: http://localhost:8080
- **pgAdmin**: http://pgadmin.localhost
- **Mailhog**: http://mail.localhost

## 🚀 Executar Desenvolvimento

```bash
# Ambiente completo multi-tenant
docker-compose -f docker-compose.dev.yml up -d

# Verificar status
docker ps

# Logs da API
docker logs coreapp-api -f
```

## 🏪 Sistema de Verticais

### Verticais Implementadas
- **Padaria**: Módulo para padarias e confeitarias
- **Farmácia**: Módulo para farmácias (em desenvolvimento)
- **Supermercado**: Módulo para supermercados (planejado)

### Ativação de Verticais
```bash
# Ativar vertical Padaria para tenant
POST /api/Example/ativar-vertical/Padaria
X-Tenant-ID: padaria-centro
```

## 🔧 Configuração Multi-tenant

### Resolução de Tenant
1. **Header X-Tenant-ID** (prioridade alta)
2. **Subdomínio** (ex: demo.api.localhost)
3. **Fallback**: tenant padrão "demo"

### Isolamento de Dados
- Global Query Filters no EF Core
- Middleware de resolução automática
- Validação de módulos por tenant

## 🛠️ Desenvolvimento

### Estrutura de Projetos
```
src/
├── CoreApp.Api/          # API Controllers + Startup
├── CoreApp.Application/  # Serviços de aplicação
├── CoreApp.Domain/       # Entidades e interfaces
├── CoreApp.Infrastructure/ # Repositórios e contexto
└── CoreApp.Verticals/    # Sistema de verticais dinâmicas
```

### Comandos Úteis
```bash
# Build
dotnet build

# Testes
dotnet test

# Migrations
dotnet ef database update

# Swagger JSON
curl http://api.localhost/swagger/v1/swagger.json
```

## 🗄️ Database

### PostgreSQL Multi-tenant
- **Host**: localhost:5432
- **Database**: coreapp_saas_dev
- **Usuário**: postgres
- **Schema**: Isolamento via TenantId em todas as entidades

### Redis Cache
- **Host**: localhost:6379
- **Uso**: Cache de configurações e sessões

## 🔐 Segurança

### Autenticação
- JWT Tokens com refresh
- Isolamento automático por tenant
- Validação de módulos comerciais

### Compliance
- LGPD: Consentimento e retenção de dados
- Auditoria: Logs estruturados para conformidade

## 📊 Monitoramento

### Health Checks
```bash
# Status geral
curl http://api.localhost/health

# Status por tenant
curl -H "X-Tenant-ID: demo" http://api.localhost/health
```

### Métricas
- Uptime da aplicação
- Status de conexão com database
- Uso de memória
- Tenant ativo no contexto

## 🚢 Deploy

### Ambiente de Produção
- Docker Compose com Traefik
- SSL automático via Let's Encrypt
- Multi-tenant por subdomínio
- Monitoramento de saúde

### Variáveis de Ambiente
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
JWT__Secret=...
MultiTenant__DefaultTenant=demo
```

---

**Sistema CoreApp - Transformando o comércio brasileiro com tecnologia de ponta! 🇧🇷**