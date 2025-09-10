# Sistema SAAS Multi-tenant CoreApp (.NET 9 + Raspberry Pi PDV)

![Version](https://img.shields.io/badge/version-3.0.0--SAAS--BR-blue.svg)
![License](https://img.shields.io/badge/license-Proprietary-red.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-green.svg)
![React Admin](https://img.shields.io/badge/React%20Admin-4.16-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)
![Raspberry Pi](https://img.shields.io/badge/Raspberry%20Pi-4-red.svg)
![Docker](https://img.shields.io/badge/Docker-ready-brightgreen.svg)
![Traefik](https://img.shields.io/badge/Traefik-v3.0-purple.svg)
![Brasil](https://img.shields.io/badge/🇧🇷-100%25%20Brasileiro-green.svg)

## 🚀 Visão Geral

Sistema **SAAS multi-tenant 100% brasileiro** com **arquitetura de verticais por composição** para gestão de comércios diversos (padarias, supermercados, lojas de roupas, óticas, etc.) com **PDV Raspberry Pi** que funciona offline. Baseado em **CoreApp base + Verticais específicos** com **SOLID principles** e **Unit of Work estado da arte**. Arquitetura híbrida **Cloud + Edge Computing** com **.NET 9.0.1**, **React Admin** e sincronização **C nativa**. Baseado em **Composição de Verticais** com padrões **SOLID + Clean Architecture + DDD** para máxima extensibilidade e modularidade comercial.

### ⭐ Principais Características

#### **🏗️ Arquitetura de Verticais por Composição**
- **CoreApp Base**: Funcionalidades genéricas para qualquer comércio
- **Verticais Específicos**: Padaria, Farmácia, Supermercado, Ótica, etc.
- **IVerticalEntity**: Interface para extensibilidade sem modificar o core
- **SOLID Principles**: Aplicados em cada camada da arquitetura
- **Unit of Work**: Gerenciamento transacional estado da arte

#### **🏢 Cloud SAAS Multi-tenant**
- **Isolamento total**: Dados de cada loja completamente separados
- **Escalabilidade infinita**: Traefik + Docker para milhares de lojas
- **API versionada `/v1/`**: Padrão Rails para estabilidade


#### **🇧🇷 100% Brasileiro**
- **Pagamentos nacionais**: PIX, Boleto, Mercado Pago
- **Suporte português**: Documentação e interface PT-BR

### 💰 Modelo de Negócio Nacional

#### Planos Comerciais (Todos Pagos)

 **Starter** | Produtos, Vendas, Estoque e Usuários
 **Módulos Adicionais** | Clientes, Promoções, Relatórios e outros

3 meses somente preço de custo.

#### 🧩 Sistema de Módulos Comerciais


#### 🎯 Módulos Starter (Inclusos)
- ✅ **Produtos**: Cadastro completo com categorização
- ✅ **Vendas**: Sistema completo de vendas balcão
- ✅ **Estoque**: Controle com lotes, validade e alertas
- ✅ **Usuários**: Gestão completa com hierarquia comercial

#### 🧩 Módulos Adicionais (Opcionais)
- 👥 **Clientes**: CRM comercial com programa fidelidade
- 🎁 **Promoções**: Engine de descontos automáticos
- 📊 **Relatórios Básicos**: Analytics operacionais
- 📈 **Relatórios Avançados**: Dashboards executivos
- 🔍 **Auditoria**: Logs compliance LGPD completos
- 🏪 **Fornecedores**: Gestão completa de compras
- 📱 **Mobile**: API para aplicativos próprios
- 💰 **Pagamentos**: Integração gateways brasileiros
- 🏷️ **Precificação**: Sistema avançado de preços

### 🌐 Multi-tenant Brasileiro

#### Isolamento por Tenant
- **Database**: PostgreSQL 16 com tenant filtering automático
- **Subdomínios**: `loja1.seudominio.com.br`, `redelojas.seudominio.com.br`
- **Dados**: Isolamento total via EF Core Global Query Filters
- **Personalizações**: Temas e logos por loja/rede

#### Hierarquia Comercial
```
Tenant (Rede de Lojas)
├── Supermercado São Paulo (Centro)
├── Padaria São Paulo (Vila Madalena) 
├── Ótica Rio de Janeiro (Copacabana)
└── Loja Roupas Belo Horizonte (Savassi)
```

## 🏗️ Arquitetura de Verticais por Composição

### Conceito Central: CoreApp + Verticais Específicos

O sistema utiliza **composição ao invés de herança complexa**, permitindo extensibilidade máxima sem modificar o código base:

```
CoreApp/                     ← Core genérico para qualquer comércio
├── CoreApp.Domain/          ← Entidades base + IVerticalEntity
├── CoreApp.Application/     ← Services base + extensibilidade
├── CoreApp.Infrastructure/  ← Repositórios base + UoW
└── CoreApp.Api/            ← Controllers base + endpoints

CoreApp.Verticals/          ← Módulos específicos por vertical
├── Padaria/                ← Especialização para padarias
│   ├── Padaria.Domain/     ← ProdutoPadaria : ProdutoEntity, IVerticalEntity
│   ├── Padaria.Application/ ← ServicoPadaria específico
│   └── Padaria.Api/        ← Controllers específicos padaria
├── Farmacia/               ← Especialização para farmácias  
│   ├── Farmacia.Domain/    ← ProdutoFarmacia : ProdutoEntity, IVerticalEntity
│   └── ...
├── Supermercado/           ← Especialização para supermercados
├── Otica/                  ← Especialização para óticas
└── RestauranteDelivery/    ← Especialização para delivery
```

### Interface Central: IVerticalEntity

```csharp
public interface IVerticalEntity
{
    /// <summary>
    /// Tipo do vertical específico (PADARIA, FARMACIA, SUPERMERCADO, etc.)
    /// </summary>
    string VerticalType { get; }
    
    /// <summary>
    /// Propriedades específicas do vertical em JSON
    /// </summary>
    string? VerticalProperties { get; set; }
    
    /// <summary>
    /// Validações específicas do vertical
    /// </summary>
    bool ValidateVerticalRules();
    
    /// <summary>
    /// Configurações específicas do vertical
    /// </summary>
    Dictionary<string, object> GetVerticalConfiguration();
}
```

### Exemplo Prático: Vertical Padaria

```csharp
// Entidade específica da padaria
public class ProdutoPadaria : ProdutoEntity, IVerticalEntity
{
    public string VerticalType => "PADARIA";
    
    // Propriedades específicas da padaria
    public TipoMassa TipoMassa { get; set; }
    public int TempoForno { get; set; }
    public bool TemGluten { get; set; }
    public bool TemLactose { get; set; }
    public DateTime? DataProducao { get; set; }
    public int ValidadeHoras { get; set; }
    
    public bool ValidateVerticalRules()
    {
        // Validações específicas da padaria
        if (DataProducao.HasValue && ValidadeHoras > 0)
        {
            return DateTime.Now <= DataProducao.Value.AddHours(ValidadeHoras);
        }
        return true;
    }
}

// Service específico da padaria
public class ProdutoPadariaService : BaseService<ProdutoPadaria>
{
    /// <summary>
    /// Busca produtos próximos do vencimento (específico da padaria)
    /// </summary>
    public async Task<List<ProdutoPadaria>> BuscarProximosVencimentoAsync(int horasLimite)
    {
        return await _unitOfWork.VerticalRepository<ProdutoPadaria>()
            .Where(p => p.DataProducao.HasValue && 
                       DateTime.Now.AddHours(horasLimite) >= p.DataProducao.Value.AddHours(p.ValidadeHoras))
            .ToListAsync();
    }
}

// Controller específico da padaria
[ApiController]
[Route("api/v1/padaria/produtos")]
[RequireVertical("PADARIA")]
public class ProdutosPadariaController : ControllerBase
{
    [HttpGet("proximos-vencimento")]
    public async Task<ActionResult<List<ProdutoPadariaDto>>> GetProximosVencimento(
        [FromQuery] int horasLimite = 4)
    {
        var produtos = await _produtoService.BuscarProximosVencimentoAsync(horasLimite);
        return Ok(produtos.Select(p => p.ToDto()));
    }
    
    [HttpGet("sem-gluten")]
    public async Task<ActionResult<List<ProdutoPadariaDto>>> GetProdutosSemGluten()
    {
        var produtos = await _produtoService.BuscarPorCriterioAsync(p => !p.TemGluten);
        return Ok(produtos.Select(p => p.ToDto()));
    }
}
```

### Vantagens da Arquitetura de Verticais

1. **Extensibilidade Total**: Novo vertical = nova pasta, zero modificação do core
2. **Reutilização Máxima**: Core compartilhado entre todos os verticais  
3. **Isolamento Perfeito**: Cada vertical tem suas próprias regras
4. **Deploy Flexível**: Pode deployar apenas os verticais necessários
5. **Manutenção Simples**: Bug no core = fix para todos, bug no vertical = fix isolado
6. **SOLID Compliant**: Cada vertical respeita todos os princípios SOLID

## 🎯 SOLID Principles Aplicados

### S - Single Responsibility Principle
Cada classe tem uma única responsabilidade:
- `VendaService`: APENAS criação de vendas
- `CalculadoraImpostosService`: APENAS cálculos de impostos  
- `ValidadorEstoqueService`: APENAS validações de estoque
- `NotificacaoEmailService`: APENAS envio de emails

### O - Open/Closed Principle
Sistema extensível sem modificar código existente:
```csharp
// Estratégia para cálculos (extensível)
public interface ICalculadoraImposto
{
    decimal Calcular(decimal valor, string estado);
    bool AplicavelPara(TipoImposto tipo);
}

// Implementações fechadas para modificação, abertas para extensão
public class CalculadoraICMS : ICalculadoraImposto { }
public class CalculadoraPIS : ICalculadoraImposto { }
public class CalculadoraISS : ICalculadoraImposto { } // Novo imposto? Apenas adicione
```

### L - Liskov Substitution Principle
Hierarquia correta de entidades:
```csharp
public abstract class BaseEntity
{
    public virtual bool EhValido() => Id != Guid.Empty;
}

public class ProdutoEntity : BaseEntity
{
    // Fortalece a validação (mais restritiva = OK)
    public override bool EhValido() => base.EhValido() && !string.IsNullOrEmpty(Nome);
}
```

### I - Interface Segregation Principle
Interfaces pequenas e específicas:
```csharp
// Interface básica que todos usam
public interface IRepository<T>
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
}

// Interface específica para export
public interface IExportableRepository<T>
{
    Task<byte[]> ExportToPdfAsync();
}

// Interface específica para notificações  
public interface INotifiableRepository<T>
{
    Task SendEmailAsync();
}
```

### D - Dependency Inversion Principle
Dependências de abstrações, não concretizações:
```csharp
public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork;              // ✅ Abstração
    private readonly ICalculadoraImpostosService _calc;   // ✅ Abstração
    private readonly IValidadorEstoqueService _validador; // ✅ Abstração
    private readonly INotificacaoService _notificacao;    // ✅ Abstração
}
```

## ⚡ Unit of Work Estado da Arte

### UoW com Suporte a Verticais
```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositórios base (genéricos)
    IRepository<T> Repository<T>() where T : class;
    
    // Repositórios específicos para verticais
    IVerticalRepository<T> VerticalRepository<T>() where T : class, IVerticalEntity;
    
    // Transações automáticas
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Repositories SEM SaveChanges Automático
```csharp
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
{
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity; // ✅ Sem SaveChanges! UoW controla
    }
    
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return entity; // ✅ Sem SaveChanges! UoW controla
    }
}
```

### Soft Delete Automático via Interceptors
```csharp
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ProcessSoftDeletes(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    private static void ProcessSoftDeletes(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.MarkAsDeleted();
            }
        }
    }
}
```

### Usage Pattern com Transações Automáticas
```csharp
public class VendaService : IVendaService
{
    public async Task<VendaDto> CriarVendaAsync(CriarVendaRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // 1. Criar venda
            var venda = await _unitOfWork.Repository<VendaEntity>().AddAsync(novaVenda);
            
            // 2. Baixar estoque para cada item
            foreach (var item in request.Itens)
            {
                await _unitOfWork.Repository<EstoqueEntity>().BaixarEstoque(item.ProdutoId, item.Quantidade);
            }
            
            // 3. Salvar tudo em uma transação
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return venda.ToDto();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(); // ✅ Rollback automático
            throw;
        }
    }
}
```

## 🛠️ Stack Tecnológica Brasileira

### Backend (.NET 9.0.1 - Estado da Arte)
- **Runtime**: .NET 9.0.1 (Mais Recente)
- **Framework**: ASP.NET Core 9.0.1 com Minimal APIs
- **ORM**: Entity Framework Core 9.0.1 + Global Query Filters + Interceptors
- **Database**: PostgreSQL 16 (multi-tenant com isolamento automático)
- **Security**: ASP.NET Core Identity + JWT 8.2.1
- **OpenAPI**: Swagger integrado (documentação automática)
- **Architecture**: Verticais por Composição + SOLID + Clean Architecture + DDD
- **Patterns**: Unit of Work + Repository + CQRS + MediatR 12.4.1
- **Validation**: FluentValidation 11.3.0
- **Logging**: Serilog 8.0.3 estruturado
- **Cache**: IMemoryCache nativo + Redis para distribuído
- **Testing**: xUnit + TestContainers + dados comerciais reais

### Frontend (React Admin)
- **Framework**: React Admin 4.16.x
- **UI Library**: React 18.3.x + Material-UI 5.x
- **Styling**: Tailwind CSS 3.4 LTS + Temas comerciais
- **Language**: TypeScript 5.3.x
- **Build**: Vite (build ultrarrápido)
- **State**: TanStack Query + Context API

### Infraestrutura Nacional
- **Containers**: Docker + Docker Compose (desenvolvimento local)
- **Reverse Proxy**: Traefik v3.1 (roteamento multi-tenant automático)
- **Database**: PostgreSQL 16.1 (isolamento por tenant + performance otimizada)
- **Cache**: IMemoryCache integrado + Redis 7.2 (distribuído)
- **Deploy**: Dokploy (auto-deploy brasileiro)
- **Monitoring**: Health Checks + OpenTelemetry + Serilog estruturado
- **Message Queue**: RabbitMQ 3.13 (para comunicação entre verticais)
- **CI/CD**: GitHub Actions + Docker multi-stage builds

### Pagamentos Brasileiros
- **Gateway Principal**: Mercado Pago
- **Gateway Backup**: PagSeguro
- **PIX**: Pagamentos instantâneos
- **Boleto**: Para comércios tradicionais
- **Cartão**: Débito/crédito com parcelamento

## 📁 Estrutura do Projeto Modular

```
coreapp-saas/
├── backend/                    # ASP.NET Core 9.0.1
│   ├── src/
│   │   ├── CoreApp.Core/       # Módulos Starter (Inclusos)
│   │   │   ├── Produtos/       # Gestão completa de produtos
│   │   │   ├── Vendas/         # Sistema de vendas completo
│   │   │   ├── Estoque/        # Controle de estoque avançado
│   │   │   └── Usuarios/       # Gestão de usuários comerciais
│   │   │
│   │   ├── CoreApp.Modules/    # Módulos Adicionais (Opcionais)
│   │   │   ├── Clientes/       # CRM + Programa Fidelidade
│   │   │   ├── Promocoes/      # Engine de descontos
│   │   │   ├── RelatoriosBasicos/  # Relatórios operacionais
│   │   │   ├── RelatoriosAvancados/# Analytics executivos
│   │   │   ├── Auditoria/      # Compliance LGPD
│   │   │   ├── Fornecedores/   # Gestão de compras
│   │   │   ├── Mobile/         # API para apps
│   │   │   ├── Pagamentos/     # Gateways brasileiros
│   │   │   └── Precificacao/   # Sistema de preços
│   │   │
│   │   ├── CoreApp.Shared/     # Infraestrutura Compartilhada
│   │   │   ├── MultiTenant/    # Isolamento por tenant
│   │   │   ├── Security/       # JWT + Identity brasileiro
│   │   │   └── Common/         # Utilitários comuns
│   │   │
│   │   └── CoreApp.Api/        # Entry Point ASP.NET Core
│   │       ├── Controllers/    # REST APIs + OpenAPI
│   │       ├── Middleware/     # Tenant resolution
│   │       └── Program.cs      # Configuração .NET 9
│   │
│   ├── Dockerfile              # .NET 9 optimized
│   └── CoreApp.sln            # Solution completa
│
├── frontend/                   # React Admin Application
│   ├── src/
│   │   ├── modules/           # Módulos por disponibilidade
│   │   │   ├── starter/        # Módulos inclusos (Starter)
│   │   │   └── additional/     # Módulos adicionais (Opcionais)
│   │   ├── themes/            # Temas comerciais brasileiros
│   │   │   ├── comercio-moderno.ts    # Padrão (azul + verde)
│   │   │   ├── alto-contraste.ts      # Acessibilidade
│   │   │   ├── modo-escuro.ts         # Dark mode
│   │   │   └── daltonismo.ts          # Color-blind friendly
│   │   ├── components/        # Componentes React Admin
│   │   ├── providers/         # Auth + Data + Tenant providers
│   │   └── utils/             # Utilitários multi-tenant
│   ├── Dockerfile
│   └── package.json
│
├── traefik/                   # Reverse Proxy Multi-tenant
│   ├── traefik.yml           # Configuração principal
│   ├── dynamic.yml           # Roteamento dinâmico brasileiro
│   └── certs/                # Certificados SSL
│
├── scripts/                   # Scripts Brasileiros
│   ├── dev-start.sh          # Ambiente desenvolvimento
│   ├── prod-deploy.sh        # Deploy produção
│   ├── tenant-setup.sh       # Setup nova farmácia
│   └── backup-brasil.sh      # Backup dados brasileiros
│
├── docker-compose.yml        # Produção
├── docker-compose.dev.yml    # Desenvolvimento
└── README.md                 # Este arquivo
```

## 🚀 Como Executar - Desenvolvimento

### Pré-requisitos
- **Docker** 20.10+ e **Docker Compose** v2.0+
- **Git** para clonar o repositório
- **.NET 9 SDK** (opcional, para desenvolvimento sem Docker)

### 🐳 Execução Completa com Docker (Recomendado)

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd coreapp

# Deploy completo do sistema
./scripts/deploy.sh

# Verificar saúde do sistema
./scripts/health-check.sh
```

### 🌐 URLs de Desenvolvimento

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Backend** | http://localhost:8080/health | API REST .NET 9 - Health Check |
| **Swagger UI** | http://localhost:8080/swagger | Documentação Interativa da API |
| **Database Health** | http://localhost:8080/health/database | Status PostgreSQL + Seeds Comerciais |
| **Cache Health** | http://localhost:8080/health/cache | Status Cache em Memória |
| **PostgreSQL** | localhost:5432 | Banco de dados direto |

### 🐳 Comandos Docker

```bash
# Iniciar sistema
docker-compose up -d

# Parar sistema  
docker-compose down

# Limpar volumes e reiniciar do zero
docker-compose down -v
docker-compose up -d --build

# Ver logs em tempo real
docker-compose logs -f coreapi

# Status dos containers
docker-compose ps
```

### 🔍 Análise de Qualidade de Código (SonarQube)

#### Setup SonarQube Local
```bash
# Iniciar SonarQube Community Edition
docker run -d --name sonarqube -p 9000:9000 sonarqube:community

# Aguardar inicialização (3-5 minutos)
# Acesso: http://localhost:9000 (admin/admin)
```

#### Comandos de Análise
```bash
# Análise local simples (recomendado)
cd backend
./scripts/sonar-local.sh

# Análise completa com cobertura de testes
./scripts/sonar-analysis.sh

# Análise manual (avançado)
dotnet sonarscanner begin /k:"coreapp-backend" /d:sonar.host.url="http://localhost:9000"
dotnet build --configuration Release
dotnet test --configuration Release
dotnet sonarscanner end
```

#### Métricas de Qualidade Monitoradas
- **Cobertura de Código**: Mínimo 80% recomendado
- **Code Smells**: Zero tolerância para arquitetura SOLID
- **Bugs**: Zero tolerância para produção
- **Vulnerabilidades**: Zero tolerância para segurança
- **Duplicação**: Máximo 3% (reutilização via composição)
- **Complexidade Ciclomática**: Máximo 10 por método
- **Dívida Técnica**: Máximo 5% do tempo total

#### Regras Específicas CoreApp
- ✅ **SOLID Compliance**: Verificação automática dos 5 princípios
- ✅ **Unit of Work**: Detecção de SaveChanges direto (proibido)
- ✅ **IVerticalEntity**: Validação da implementação correta
- ✅ **Soft Delete**: Verificação de uso dos interceptors
- ✅ **Multi-tenant**: Isolamento automático validado

### 🏥 URLs de Produção (Dokploy + Traefik)

| Ambiente | URL | Tipo |
|----------|-----|------|
| **API Produção** | https://api-dev.diegolima.dev | API REST em produção |
| **Multi-tenant** | https://{tenant}.comercio-dev.diegolima.dev | URLs dinâmicas por loja |
| **Admin** | https://admin-dev.diegolima.dev | Administração geral |

## 🔐 Credenciais de Desenvolvimento

### Tenant Demo (Demonstração)
- **URL**: http://demo.localhost
- **Super Admin**: `admin@demo.com` / `admin123`
- **Gerente**: `gerente@demo.com` / `gerente123`
- **Supervisor**: `supervisor@demo.com` / `super123`
- **Vendedor**: `vendedor@demo.com` / `vend123`
- **Caixa**: `caixa@demo.com` / `caixa123`

### Hierarquia de Usuários Comerciais
| Perfil | Módulos Acessíveis | Funcionalidades |
|--------|-------------------|-----------------|
| **Super Admin** | Todos + Admin | Gestão de tenants, configurações |
| **Admin Loja** | Starter + Contratados | Todos módulos da loja |
| **Gerente** | Operacionais | Produtos, estoque, vendas, relatórios |
| **Supervisor** | Compliance | Auditoria, relatórios avançados |
| **Vendedor** | Básicos | Vendas, cadastro clientes |
| **Caixa** | Mínimos | Finalização de vendas apenas |

## 🧩 Sistema de Módulos e Preços

### Validação Automática por Plano
```csharp
// Exemplo de proteção de módulo comercial
[HttpGet("/api/relatorios/avancados")]
[RequireModule("RELATORIOS_AVANCADOS")] // Módulo Adicional
public async Task<ActionResult> GetAdvancedReports()
{
    // Método só executa se tenant tem módulo ativo
    return Ok(await _reportsService.GetAdvancedReports());
}
```

### Interface de Configuração de Preços
- **Admin Dashboard**: Configurar preços dinamicamente
- **Histórico de Mudanças**: Log com justificativas obrigatórias
- **Preview de Impacto**: Visualizar efeito nos clientes
- **Notificação**: Email automático sobre alterações
- **A/B Testing**: Testar preços diferentes

### Gateways de Pagamento Brasileiros
```csharp
// Integração com múltiplos gateways nacionais
public class BrazilianPaymentService
{
    // Mercado Pago (principal)
    public async Task<PaymentResult> CreateMercadoPagoSubscription(decimal amountBRL)
    
    // PagSeguro (backup)  
    public async Task<PaymentResult> CreatePagSeguroSubscription(decimal amountBRL)
    
    // PIX (instantâneo)
    public async Task<PixQRCode> GeneratePixPayment(decimal amountBRL)
    
    // Boleto (tradicional)
    public async Task<BoletoData> GenerateBoleto(decimal amountBRL)
}
```

## 🎨 Sistema de Temas Brasileiros

### 🏥 Tema Padrão - "Farmácia Moderna"
```javascript
// Identidade visual comercial brasileira
const comercioBrasileiroTheme = {
  primary: {
    main: '#2E7D32',    // Verde saúde (cor principal)
    dark: '#1B5E20',    // Verde escuro
  },
  secondary: {
    main: '#1976D2',    // Azul confiança
    dark: '#1565C0',    // Azul escuro
  },
  success: { main: '#4CAF50' },  // Verde sucesso
  warning: { main: '#FF9800' },  // Laranja alerta
  error: { main: '#F44336' },    // Vermelho erro
}
```

### Temas Alternativos Selecionáveis
1. **Alto Contraste**: Para acessibilidade (WCAG AAA)
2. **Modo Escuro**: Para uso noturno em plantões  
3. **Daltonismo**: Cores específicas para deficiência visual
4. **Farmácia Tradicional**: Cores mais conservadoras

### Personalização por Tenant
- **Logo customizado**: Upload da logo da farmácia
- **Cores principais**: Personalizar paleta de cores
- **Tipografia**: Escolher fonte (Roboto, Poppins, etc.)
- **Layout**: Sidebar, topbar, densidade de informações

## 🧪 Testes com Dados Comerciais Reais

### Estratégia de Testes Brasileira
```csharp
// Sempre usar dados comerciais reais do Brasil
[Test]
public async Task DeveCalcularImpostosSobreProduto_ComDadosReaisBrasil()
{
    // Arrange: Produto real brasileiro
    var acucar = new Produto
    {
        Nome = "Áucar Cristal 1kg",
        Categoria = "Alimentos",
        Marca = "União",
        PrecoVenda = 4.50m,
        TenantId = "supermercado-teste-br"
    };
    
    // Act: Cálculo com impostos brasileiros
    var resultado = await _vendaService.CalcularVenda(acucar, quantidade: 2);
    
    // Assert: Verificar cálculos brasileiros corretos
    Assert.Equal(25.00m, resultado.Subtotal);
    Assert.Equal(0.63m, resultado.ICMS);    // 7% ICMS alimentos
    Assert.Equal(1.60m, resultado.PIS);     // 6.4% PIS/COFINS
}
```

### TestContainers com Dados Comerciais
- **Database real**: PostgreSQL com seed comercial brasileiro
- **Produtos reais**: Base de produtos diversificada
- **Regulamentações**: Testes compliance LGPD
- **Multi-tenant**: Isolamento testado entre lojas

## 🚢 Deploy & Produção Brasileira

### Dokploy Deploy Automático
- **Branch**: `develop-csharp` → Deploy automático
- **Frontend**: https://comercio.seudominio.com.br
- **API**: https://api.seudominio.com.br
- **Multi-tenant**: https://{tenant}.seudominio.com.br

### Configuração Produção Nacional
```yaml
# docker-compose.yml para produção brasileira
services:
  traefik:
    image: traefik:v3.0
    labels:
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
  
  coreapp-backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=postgresql://...
      - JWT__Secret=${JWT_SECRET}
      - MercadoPago__AccessToken=${MERCADOPAGO_TOKEN}
      - PagSeguro__Token=${PAGSEGURO_TOKEN}
    
  coreapp-frontend:
    environment:
      - REACT_APP_API_URL=https://api.seudominio.com.br
      - REACT_APP_MERCADOPAGO_PUBLIC_KEY=${MERCADOPAGO_PUBLIC_KEY}
```

## 📊 Monitoramento & Analytics Brasileiros

### Métricas SAAS
```csharp
// Health checks específicos para farmácias brasileiras
public class ComercioHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        // Verificar conectividade com APIs brasileiras
        var servicosOk = await _servicosExternosService.CheckConnectionAsync();
        
        // Verificar gateways de pagamento brasileiros
        var mercadoPagoOk = await _mercadoPagoService.HealthCheckAsync();
        
        // Verificar compliance LGPD
        var lgpdOk = await _lgpdService.ValidateComplianceAsync();
        
        return servicosOk && mercadoPagoOk && lgpdOk ? 
            HealthCheckResult.Healthy() : 
            HealthCheckResult.Unhealthy();
    }
}
```

### Dashboard Financeiro
- **MRR (Monthly Recurring Revenue)**: Em reais brasileiros
- **Churn Rate**: Taxa de cancelamento por região
- **LTV (Lifetime Value)**: Valor médio por farmácia
- **CAC (Customer Acquisition Cost)**: Custo aquisição brasileiro
- **Conversão**: Funil de vendas por estado

## 🔧 Desenvolvimento Local

### Comandos Essenciais
```bash
# Ambiente completo
docker-compose -f docker-compose.dev.yml up -d

# Rebuild completo
docker-compose -f docker-compose.dev.yml up -d --build --force-recreate

# Logs em tempo real
docker-compose -f docker-compose.dev.yml logs -f backend
docker-compose -f docker-compose.dev.yml logs -f frontend

# Setup nova farmácia (tenant)
./scripts/tenant-setup.sh nova-loja-sp

# Backup desenvolvimento
./scripts/backup-dev.sh
```

### Desenvolvimento Sem Docker
```bash
# Backend .NET 9
cd backend
dotnet restore
dotnet run --project CoreApi

# Frontend React Admin
cd frontend  
npm install
npm start
```

## 🎯 Roadmap SAAS Brasileiro

### ✅ V3.0 - SAAS Multi-tenant (Atual)
- [x] Arquitetura .NET 9 monólito modular
- [x] Sistema comercial com preços brasileiros
- [x] Multi-tenancy com isolamento completo
- [x] React Admin com temas comerciais
- [x] Pagamentos nacionais (Mercado Pago + PagSeguro + PIX)

### 🔄 V3.1 - Expansão Nacional (Q2 2025)
- [ ] Integração com mais redes comerciais
- [ ] App mobile nativo (iOS/Android)
- [ ] Relatórios avançados com BI
- [ ] WhatsApp Business integrado

### 🚀 V3.2 - Escala Nacional (Q3 2025)
- [ ] Auto-scaling Kubernetes
- [ ] Inteligência artificial para vendas
- [ ] Marketplace de produtos
- [ ] Integração com planos de saúde

## 🏛️ Compliance & Regulamentação Brasileira

### Conformidade Total
- **LGPD**: Proteção de dados pessoais com consentimento
- **Receita Federal**: Emissão de NFCe para vendas
- **Código de Defesa do Consumidor**: Compliance comercial
- **Regulamentações Comerciais**: Conformidade com leis setoriais

### Auditoria e Fiscalização  
- **Logs de Auditoria**: Todas as operações registradas
- **Relatórios Fiscais**: Movimento para Receita Federal
- **LGPD Reports**: Consentimentos e tratamento de dados
- **Backup Compliance**: Retenção de dados por 5 anos

## 💡 Diferenciais Competitivos

### Vantagens Técnicas
- **.NET 9 Performance**: Até 40% mais rápido que versões anteriores
- **Monólito Modular**: Simplicidade operacional + flexibilidade comercial
- **Multi-tenant Nativo**: Isolamento automático por farmácia
- **Temas Acessíveis**: WCAG AAA compliance + identidade comercial

### Vantagens Comerciais
- **Preços Brasileiros**: A partir de R$ 149,90 (acessível)
- **Sem Setup**: Zero taxa de implementação
- **Pagamentos Nacionais**: PIX, boleto, cartão parcelado
- **Suporte Regional**: Horário comercial brasileiro

### Vantagens Regulatórias
- **Compliance Comercial**: Desenvolvido para regulamentações brasileiras
- **Controle Fiscal**: NFCe e relatórios automáticos
- **LGPD Ready**: Consentimentos e direitos do titular
- **Multi-setor**: Adapta-se a diferentes tipos de comércio

## 📞 Suporte e Comunidade

### Ambientes de Acesso
- **Desenvolvimento**: http://localhost (farmácia demo)
- **Homologação**: https://staging.comercio.seudominio.com.br  
- **Produção**: https://app.comercio.seudominio.com.br

### Canais de Suporte
- **Documentação**: Wiki técnica completa
- **WhatsApp**: Suporte comercial brasileiro
- **Email**: suporte@comercio.com.br
- **Portal**: Central do cliente

### Comunidade
- **GitHub**: Issues e contribuições
- **Discord**: Comunidade de desenvolvedores
- **YouTube**: Tutoriais e treinamentos
- **Blog**: Novidades e casos de sucesso

---

## 🇧🇷 Desenvolvido com ❤️ para o Brasil

**Sistema SAAS que revoluciona a gestão comercial brasileira com tecnologia de ponta, preços acessíveis e compliance total com a regulamentação nacional.**

*Orgulhosamente brasileiro - SAAS CoreApp v3.0 - .NET 9 + React Admin*

---

## 📋 Licença e Termos

**© 2024 Sistema CoreApp SAAS**  
Software proprietário. Todos os direitos reservados.  
Desenvolvido no Brasil para comércios brasileiros.

**Política de Dados**: Todos os dados são processados e armazenados em território nacional, em conformidade com a LGPD.