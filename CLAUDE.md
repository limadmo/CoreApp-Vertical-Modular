# CoreApp - Regras de Desenvolvimento (.NET 9.0.1)

## Arquitetura de Verticais por Composição (REGRA ABSOLUTA)

**SEMPRE usar composição ao invés de herança complexa**

```
CoreApp/                        # Core genérico para qualquer comércio
├── CoreApp.Domain/             # Entidades base + IVerticalEntity
├── CoreApp.Application/        # Services base + extensibilidade
├── CoreApp.Infrastructure/     # Repositórios base + UoW estado da arte
├── CoreApp.Api/               # Controllers base + endpoints
└── CLAUDE.md                  # Este arquivo (regras absolutas)

CoreApp.Verticals/             # Módulos específicos por vertical
├── Padaria/                   # Especialização para padarias
│   ├── Padaria.Domain/        # ProdutoPadaria : ProdutoEntity, IVerticalEntity
│   ├── Padaria.Application/   # Services específicos padaria
│   └── Padaria.Api/          # Controllers específicos padaria
├── Farmacia/                  # Especialização para farmácias
├── Supermercado/             # Especialização para supermercados
├── Otica/                    # Especialização para óticas
└── RestauranteDelivery/      # Especialização para delivery
```

## SOLID Principles (REGRA ABSOLUTA)

**SEMPRE aplicar todos os 5 princípios SOLID em cada linha de código**

### S - Single Responsibility Principle
- Cada classe tem UMA responsabilidade específica
- `VendaService`: APENAS criação de vendas
- `CalculadoraImpostosService`: APENAS cálculos de impostos
- `ValidadorEstoqueService`: APENAS validações de estoque

### O - Open/Closed Principle  
- Sistema extensível SEM modificar código existente
- Novos verticais = novas pastas, zero alteração do core
- Strategy Pattern para cálculos e validações

### L - Liskov Substitution Principle
- Hierarquias corretas de substituição
- Subclasses FORTALECEM contratos da classe base
- Qualquer `BaseEntity` pode ser substituída por suas filhas

### I - Interface Segregation Principle
- Interfaces pequenas e específicas por necessidade
- `IRepository<T>` básico + `IExportableRepository<T>` específico
- NUNCA interfaces gordas com métodos desnecessários

### D - Dependency Inversion Principle
- Dependências SEMPRE de abstrações, NUNCA de concretizações
- `IUnitOfWork`, `ICalculadoraImpostosService`, `IValidadorEstoqueService`
- Inversão de controle via DI container

## Unit of Work Estado da Arte (REGRA ABSOLUTA)

**SEMPRE usar UoW para coordenar transações - NUNCA SaveChanges direto**

### Interface UoW Completa
```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositórios genéricos
    IRepository<T> Repository<T>() where T : class;
    
    // Repositórios específicos para verticais
    IVerticalRepository<T> VerticalRepository<T>() where T : class, IVerticalEntity;
    
    // Múltiplos bancos de dados
    IRepository<T> Repository<T>(string connectionName) where T : class;
    
    // Transações distribuídas
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Transações distribuídas (múltiplos bancos)
    Task BeginDistributedTransactionAsync(params string[] connectionNames);
    Task CommitDistributedTransactionAsync();
    Task RollbackDistributedTransactionAsync();
    
    // Event Sourcing + CQRS
    Task PublishEventAsync<T>(T domainEvent) where T : class;
    Task<List<T>> GetEventsAsync<T>(Guid aggregateId) where T : class;
    
    // Messageria integrada
    Task PublishMessageAsync<T>(T message, string queue = null) where T : class;
    Task<bool> ProcessMessageAsync<T>(Func<T, Task> handler) where T : class;
    
    // Saga Pattern para transações longas
    Task<Guid> StartSagaAsync<T>(T sagaData) where T : class;
    Task CompleteSagaAsync(Guid sagaId);
    Task CompensateSagaAsync(Guid sagaId);
    
    // Health checks e monitoramento
    Task<bool> IsHealthyAsync();
    Task<Dictionary<string, object>> GetMetricsAsync();
}
```

### Implementação Multi-Database
```csharp
public class AdvancedUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<string, DbContext> _contexts;
    private readonly IMessageBus _messageBus;
    private readonly IEventStore _eventStore;
    private readonly ISagaManager _sagaManager;
    private readonly IDistributedTransactionManager _distributedTxManager;
    
    public async Task BeginDistributedTransactionAsync(params string[] connectionNames)
    {
        foreach (var connectionName in connectionNames)
        {
            var context = _contexts[connectionName];
            await context.Database.BeginTransactionAsync();
        }
        
        // Coordenador de transação distribuída (2PC)
        await _distributedTxManager.PrepareAsync(connectionNames);
    }
    
    public async Task CommitDistributedTransactionAsync()
    {
        try
        {
            // Fase 1: Prepare (todos os bancos confirmam que podem commitar)
            await _distributedTxManager.PrepareAllAsync();
            
            // Fase 2: Commit (todos commitam simultaneamente)
            foreach (var context in _contexts.Values)
            {
                await context.Database.CommitTransactionAsync();
            }
            
            await _distributedTxManager.CommitAllAsync();
        }
        catch
        {
            await RollbackDistributedTransactionAsync();
            throw;
        }
    }
    
    public async Task RollbackDistributedTransactionAsync()
    {
        // Rollback em cascata automático
        foreach (var context in _contexts.Values.Reverse())
        {
            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            catch (Exception ex)
            {
                // Log erro mas continua rollback dos outros
                _logger.LogError(ex, "Erro no rollback da conexão {Connection}", context.Database.GetConnectionString());
            }
        }
        
        // Compensação automática de sagas
        await _sagaManager.CompensateAllActiveAsync();
    }
}
```

### Event Sourcing + CQRS Integrado
```csharp
public class EventSourcingUnitOfWork : IUnitOfWork
{
    private readonly List<IDomainEvent> _events = new();
    private readonly IEventStore _eventStore;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;
    
    public async Task PublishEventAsync<T>(T domainEvent) where T : class
    {
        _events.Add(domainEvent as IDomainEvent);
        
        // Publicação imediata para leitura (CQRS)
        await _queryDispatcher.UpdateReadModelAsync(domainEvent);
        
        // Event store para auditoria
        await _eventStore.AppendAsync(domainEvent);
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Salvar mudanças no banco principal
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // 2. Processar eventos de domínio
        foreach (var evento in _events)
        {
            await _messageBus.PublishAsync(evento);
        }
        
        // 3. Atualizar read models (CQRS)
        await _queryDispatcher.ProcessBatchAsync(_events);
        
        _events.Clear();
        return result;
    }
}
```

### Saga Pattern para Transações Longas
```csharp
public class SagaUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Guid, ISaga> _activeSagas = new();
    
    public async Task<Guid> StartSagaAsync<T>(T sagaData) where T : class
    {
        var sagaId = Guid.NewGuid();
        var saga = _sagaFactory.Create<T>(sagaData);
        
        _activeSagas[sagaId] = saga;
        
        try
        {
            await saga.StartAsync();
            return sagaId;
        }
        catch
        {
            await CompensateSagaAsync(sagaId);
            throw;
        }
    }
    
    public async Task CompensateSagaAsync(Guid sagaId)
    {
        if (_activeSagas.TryGetValue(sagaId, out var saga))
        {
            // Compensação automática em ordem reversa
            await saga.CompensateAsync();
            _activeSagas.Remove(sagaId);
        }
    }
}
```

### Repositories SEM SaveChanges
```csharp
// ✅ CORRETO - Repository apenas modifica contexto
public virtual async Task<TEntity> AddAsync(TEntity entity)
{
    var entry = await _dbSet.AddAsync(entity);
    return entry.Entity; // SEM SaveChanges! UoW controla
}

// ❌ ERRADO - Repository com SaveChanges
public virtual async Task<TEntity> AddAsync(TEntity entity)
{
    var entry = await _dbSet.AddAsync(entity);
    await _context.SaveChangesAsync(); // QUEBRA o padrão UoW
    return entry.Entity;
}
```

## Interface IVerticalEntity (REGRA ABSOLUTA)

**SEMPRE implementar IVerticalEntity para extensibilidade dos verticais**

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

## Multi-tenant (REGRA ABSOLUTA)

### Isolamento Automático por Tenant
```csharp
// Global Query Filter aplicado automaticamente
modelBuilder.Entity<ProdutoEntity>()
    .HasQueryFilter(p => p.TenantId == _tenantContext.GetCurrentTenantId());

// Middleware de resolução automática de tenant
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = ExtractTenantFromRequest(context);
        _tenantContext.SetCurrentTenant(tenantId);
        await next(context);
    }
}
```

### Validação de Módulos Comerciais
```csharp
[RequireModule("CLIENTES")] // Attribute customizado
public async Task<ActionResult<ClienteDto>> CriarCliente([FromBody] CriarClienteRequest request)
{
    var tenantId = HttpContext.GetTenantId();
    var cliente = await _clienteService.CriarAsync(tenantId, request);
    return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, cliente);
}
```

## Configurações Dinâmicas (REGRA ABSOLUTA)

**NUNCA usar enums para dados configuráveis**
- Tabelas de configuração dinâmica
- Cache IMemoryCache (30 minutos)
- Sistema hierárquico: Global → Tenant → Usuário
- Auto-invalidação quando configurações mudam

```csharp
// ✅ CORRETO - Configuração dinâmica
public class EstoqueEntity 
{
    public Guid TipoMovimentacaoId { get; set; } // Referencia configuração
    public TipoMovimentacaoEntity TipoMovimentacao { get; set; }
}

// ❌ ERRADO - Enum rígido
public enum TipoMovimentacao { ENTRADA, SAIDA } // Não permite customização
```

## Comunicação e Documentação (REGRA ABSOLUTA)

**SEMPRE comunicar em PT-BR (Português Brasileiro)**
- Respostas em português brasileiro
- XML Documentation em PT-BR para C#
- **TSDoc em PT-BR para TypeScript** (não JSDoc)
- Mensagens de commit em PT-BR
- Documentação do projeto em PT-BR

### Padrões TypeScript + TSDoc
```typescript
/**
 * Hook para gerenciar produtos comerciais no tenant atual
 * Aplica filtros automáticos por tenant e módulos ativos
 * 
 * @example
 * ```tsx
 * const { produtos, isLoading, criarProduto } = useProdutos();
 * ```
 * 
 * @returns Hook com estado e ações para produtos
 */
export function useProdutos() {
  // Tipagem nativa TypeScript + TSDoc
}
```

## Frontend + Acessibilidade (REGRA ABSOLUTA)

### Stack Frontend Obrigatória
- **React 18.3.x** + **TypeScript 5.3.x** nativo
- **Mantine** para componentes (customizável)
- **Tailwind CSS 4+** para styling
- **TSDoc** para documentação completa

### Navegação por Teclado (OBRIGATÓRIA)
```typescript
/**
 * Padrão de navegação por teclado para todos os componentes
 */
const keyboardNavigation = {
  // Navegação básica
  TAB: 'Navegar para próximo elemento em ordem lógica',
  'SHIFT+TAB': 'Navegação reversa (elemento anterior)',
  ENTER: 'Confirmar subação → ação completa',
  ESC: 'Cancelar ação atual (fechar modais, voltar)',
  
  // Módulos Starter (Inclusos)
  F1: 'Vendas - módulo principal',
  F2: 'Clientes - gestão de clientes',
  F3: 'Produtos - catálogo de produtos', 
  F4: 'Estoque - controle de estoque',
  
  // Módulos Adicionais (Opcionais)
  F5: 'Fornecedores - compras e suprimentos',
  F6: 'Promoções - engine de descontos',
  F7: 'Relatórios - análise de dados',
  F8: 'Auditoria - compliance LGPD',
  F9: 'Configurações - sistema',
  F10: 'Menu principal - navegação geral',
  F11: 'Tela cheia - funcionalidade sistema',
  F12: 'Ajuda contextual - suporte/dev tools',
  
  // Setas direcionais
  'ARROW_UP/DOWN': 'Navegação vertical em listas',
  'ARROW_LEFT/RIGHT': 'Navegação horizontal em tabs',
};
```

### Contraste e Legibilidade (OBRIGATÓRIA)
- **Fonte base**: 16px mínimo (nunca menor)
- **Contraste**: WCAG AAA (7:1 para texto normal)
- **Cards**: Bordas definidas, sombras sutis
- **Formulários**: Campos grandes, labels visíveis
- **Botões**: Altura mínima 44px (touch-friendly)

## Comandos Essenciais

### Desenvolvimento
```bash
# Configurar ambiente development
echo "ENV=development" > .env

# Ambiente completo
docker-compose up -d

# Build backend
dotnet build -c Release

# Testes com dados reais
dotnet test

# Análise SonarQube
./scripts/sonar-local.sh

# Trocar para produção
echo "ENV=production" > .env && docker-compose up -d
```

### Quality Gates
- **Cobertura**: ≥ 80% (código crítico ≥ 90%)
- **SOLID Compliance**: Zero violações
- **Bugs**: Zero tolerância
- **Vulnerabilidades**: Zero tolerância
- **Duplicação**: ≤ 3%
- **Complexidade**: ≤ 10 por método

---

## Resumo das Regras Absolutas

1. **Arquitetura de verticais por composição** - CoreApp base + Verticais específicos
2. **SOLID principles** em cada linha de código (SRP, OCP, LSP, ISP, DIP)
3. **Unit of Work estado da arte** - múltiplos bancos, saga, event sourcing, CQRS
4. **IVerticalEntity** para extensibilidade dos verticais
5. **Multi-tenant automático** - isolamento total por tenant
6. **Configurações dinâmicas** - nunca enums fixos
7. **Frontend moderno** - React + TypeScript + Mantine + Tailwind 4+
8. **Acessibilidade completa** - navegação por teclado (TAB, F1-F12, ESC, ENTER)
9. **TSDoc obrigatório** - documentação TypeScript completa
10. **Português brasileiro** obrigatório em comunicação
11. **Legibilidade garantida** - fonte 16px+, contraste WCAG AAA
12. **Quality Gates** rigorosos - 80%+ cobertura, zero bugs