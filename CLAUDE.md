# Regras do Sistema SAAS Multi-tenant CoreApp (.NET 9.0.1)

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

### UoW com Controle Transacional Total
```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositórios genéricos
    IRepository<T> Repository<T>() where T : class;
    
    // Repositórios específicos para verticais
    IVerticalRepository<T> VerticalRepository<T>() where T : class, IVerticalEntity;
    
    // Controle transacional OBRIGATÓRIO
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Soft Delete Automático via Interceptors
```csharp
// SEMPRE usar interceptors para soft delete automático
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDeletableEntity>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.MarkAsDeleted();
                }
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
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

## Idioma de Comunicação (REGRA ABSOLUTA)

**SEMPRE comunicar em PT-BR (Português Brasileiro)**
- Todas as respostas devem ser em português brasileiro
- Manter consistência no idioma independente do contexto
- Comentários em código SEMPRE em português brasileiro (XML Documentation)
- Documentação do projeto em português brasileiro
- Mensagens de commit em português brasileiro
- Variáveis e métodos podem ser em inglês, mas documentação sempre PT-BR

## Documentação Obrigatória em Código (REGRA ABSOLUTA)

**SEMPRE documentar o código criado ou modificado**
- Todos os comentários em PT-BR
- Usar XML Documentation para classes, métodos e propriedades C#
- Usar JSDoc para documentação TypeScript no frontend
- Comentar regras de negócio específicas do comércio brasileiro
- Documentar validações de compliance comercial e LGPD

## Arquitetura de Configuração (REGRA ABSOLUTA)

**NUNCA usar enums para dados configuráveis**
- SEMPRE criar tabelas de configuração dinâmica ao invés de enums
- Implementar cache em memória simples para performance adequada
- Sistema hierárquico: Global (Sistema) → Tenant (Loja/Comércio) → Usuário
- Mudanças sem deploy via interface administrativa
- Cache IMemoryCache nativo do .NET (30 minutos)
- Fallback automático: Cache → PostgreSQL
- Auto-invalidação quando configurações mudam

### ❌ ERRADO - Enum rígido:
```csharp
public enum TipoMovimentacao { ENTRADA, SAIDA } // Não permite customização
```

### ✅ CORRETO - Configuração dinâmica:
```csharp
public class EstoqueEntity 
{
    public Guid TipoMovimentacaoId { get; set; } // Referencia configuração
    public TipoMovimentacaoEntity TipoMovimentacao { get; set; }
}
```

### Exemplo de padrão .NET 9 (C#):
```csharp
/// <summary>
/// Serviço responsável por gerenciar movimentações de estoque multi-tenant
/// Aplica regras comerciais brasileiras e isolamento automático por tenant
/// </summary>
/// <remarks>
/// Este serviço implementa as regulamentações brasileiras de controle de estoque
/// conforme determinações legais para estabelecimentos comerciais
/// </remarks>
public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IModuleValidationService _moduleValidation;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Registra uma nova movimentação no estoque para o tenant específico
    /// </summary>
    /// <param name="request">Dados da movimentação incluindo produto, tipo e quantidade</param>
    /// <returns>Movimentação criada com validações comerciais aplicadas</returns>
    /// <exception cref="ModuleNotActiveException">Quando módulo de estoque não está ativo para o tenant</exception>
    /// <exception cref="ProdutoRestritoException">Quando produto com restrições não atende critérios</exception>
    [RequireModule("ESTOQUE")]
    public async Task<MovimentacaoResponseDto> RegistrarMovimentacaoAsync(MovimentacaoRequestDto request)
    {
        // Obtém o tenant atual automaticamente via middleware
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        // Valida se o produto existe e pertence ao tenant
        var produto = await _produtoRepository.GetByIdAndTenantAsync(request.ProdutoId, tenantId)
            ?? throw new ProdutoNaoEncontradoException($"Produto {request.ProdutoId} não encontrado para o tenant {tenantId}");

        // Aplica regras específicas para produtos controlados (se aplicável)
        if (produto.TemRestricoes())
        {
            await ValidarMovimentacaoRestrita(produto, request);
        }

        // Calcula o novo saldo baseado no tipo de movimentação
        var novoSaldo = CalcularNovoSaldo(produto.EstoqueAtual, request);

        // Cria o registro isolado por tenant com auditoria automática
        var movimentacao = new MovimentacaoEntity
        {
            TenantId = tenantId,
            ProdutoId = request.ProdutoId,
            TipoMovimentacao = request.Tipo,
            Quantidade = request.Quantidade,
            SaldoAnterior = produto.EstoqueAtual,
            SaldoAtual = novoSaldo,
            DataMovimentacao = DateTime.UtcNow,
            UsuarioId = _tenantContext.GetCurrentUserId(),
            Observacoes = request.Observacoes
        };

        await _estoqueRepository.AddAsync(movimentacao);
        await _unitOfWork.CommitAsync();

        // Log para auditoria comercial obrigatória
        await _auditService.LogMovimentacaoEstoque(tenantId, movimentacao);

        return movimentacao.ToResponseDto();
    }

    /// <summary>
    /// Valida movimentação de produto com restrições conforme regulamentações
    /// </summary>
    /// <param name="produto">Produto com restrições que será movimentado</param>
    /// <param name="request">Dados da movimentação solicitada</param>
    private async Task ValidarMovimentacaoRestrita(ProdutoEntity produto, MovimentacaoRequestDto request)
    {
        // Validação específica para cada tipo de restrição
        switch (produto.TipoRestricao)
        {
            case TipoRestricao.IdadeMinima:
                // Produtos com idade mínima (bebidas alcoólicas)
                await ValidarIdadeMinima(request.ClienteId, produto.IdadeMinimaRequerida);
                break;
                
            case TipoRestricao.LicencaEspecial:
                // Produtos que requerem licença especial
                await ValidarLicencaEspecial(request.VendedorId, produto.TipoLicencaRequerida);
                break;
                
            case TipoRestricao.QuantidadeControlada:
                // Produtos com limite de quantidade
                await ValidarLimiteQuantidade(request.ProdutoId, request.Quantidade);
                break;
        }
    }
}
```

### Exemplo de padrão React Admin (TypeScript):
```typescript
/**
 * Componente de listagem de produtos comerciais com suporte multi-tenant brasileiro
 * Integra com React Admin e aplica filtros automáticos por tenant + validação de módulos
 */
export const ProdutosList: React.FC = () => {
  // Obtém dados do tenant atual do contexto brasileiro
  const { tenant, isLoading: tenantLoading } = useTenantContext();
  
  // Verifica se módulo de produtos está ativo para o tenant
  const { hasModule, isLoading: moduleLoading } = useModuleAccess();
  
  if (tenantLoading || moduleLoading) {
    return <CircularProgress />;
  }

  if (!hasModule('PRODUTOS')) {
    return (
      <Card>
        <CardContent>
          <Typography>
            Módulo de Produtos não está ativo para sua loja.
            Entre em contato para ativar este módulo.
          </Typography>
        </CardContent>
      </Card>
    );
  }

  return (
    <List
      title={`Produtos - ${tenant.nomeFantasia}`}
      filters={<ProdutoFilters />}
      pagination={<Pagination />}
      sort={{ field: 'nome', order: 'ASC' }}
    >
      <Datagrid>
        {/* Campos básicos sempre visíveis no plano Starter */}
        <TextField source="nome" label="Nome do Produto" />
        <TextField source="categoria" label="Categoria" />
        <TextField source="marca" label="Marca" />
        <NumberField 
          source="precoVenda" 
          label="Preço (R$)"
          options={{
            style: 'currency',
            currency: 'BRL',
            minimumFractionDigits: 2
          }}
        />
        
        {/* Campo visível apenas se módulo de estoque ativo */}
        {hasModule('ESTOQUE') && (
          <NumberField source="estoqueAtual" label="Estoque Atual" />
        )}
        
        {/* Tipo de produto sempre visível (organização) */}
        <FunctionField
          label="Tipo de Produto"
          render={(record: any) => (
            <Chip
              label={record.tipoProduto}
              color={getTipoProdutoColor(record.tipoProduto)}
              size="small"
            />
          )}
        />
        
        {/* Campos de auditoria apenas para Enterprise */}
        {hasModule('AUDITORIA') && (
          <>
            <DateField source="dataUltimaMovimentacao" label="Última Movimentação" />
            <TextField source="lote" label="Lote" />
            <DateField source="dataValidade" label="Validade" />
          </>
        )}
        
        {/* Actions com verificação de permissão */}
        <EditButton />
        <ShowButton />
        
        {/* Relatórios apenas para Professional+ */}
        {hasModule('RELATORIOS_BASICOS') && (
          <Button
            onClick={() => gerarRelatorioProduto(record.id)}
            startIcon={<AssessmentIcon />}
          >
            Relatório
          </Button>
        )}
      </Datagrid>
    </List>
  );
};

/**
 * Retorna a cor do chip baseada no tipo de produto comercial
 */
const getTipoProdutoColor = (tipoProduto: string): 'default' | 'primary' | 'secondary' | 'error' | 'warning' => {
  switch (tipoProduto) {
    case 'ALIMENTICIO':
      return 'primary';    // Azul - produtos alimentícios
    case 'ELETRONICO':
      return 'secondary';  // Roxo - eletrônicos
    case 'VESTUARIO':
      return 'default';    // Cinza - roupas e acessórios
    case 'BEBIDA_ALCOOLICA':
      return 'warning';    // Laranja - bebidas alcoólicas (restrição idade)
    case 'PRODUTO_CONTROLADO':
      return 'error';      // Vermelho - produtos com restrições especiais
    default:
      return 'default';
  }
};
```

## Testing Strategy Brasileira (REGRA ABSOLUTA)

**SEMPRE USE DADOS COMERCIAIS REAIS DO BRASIL - NUNCA MOCKS**

- Todos os testes devem usar dados concretos do seed database brasileiro
- Testes de integração com database real (TestContainers + PostgreSQL) 
- Dados comerciais realistas de diversos tipos de negócio
- Setup de banco de teste com seed automático por tenant brasileiro
- Testes de compliance com LGPD e regulamentações comerciais

### Por quê dados reais?
- Testes mais autênticos e confiáveis para mercado brasileiro
- Validação real das regras de negócio comerciais nacionais
- Detecta problemas de schema e relacionamentos
- Compliance real com LGPD e regulamentações comerciais
- Testa isolamento de dados entre tenants (lojas/comércios)
- Valida cálculos de impostos brasileiros (ICMS, PIS/COFINS)

### Estrutura de Testes .NET 9 (xUnit + TestContainers)
```csharp
/// <summary>
/// Classe base para testes de integração com dados comerciais brasileiros reais
/// </summary>
[Collection("Database")]
public abstract class BaseIntegrationTest : IClassFixture<TestDatabaseFixture>
{
    protected readonly TestDatabaseFixture Database;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ITenantSeedService TenantSeedService;
    protected readonly ITenantContextHelper TenantContext;

    protected BaseIntegrationTest(TestDatabaseFixture database)
    {
        Database = database;
        ServiceProvider = database.ServiceProvider;
        TenantSeedService = ServiceProvider.GetRequiredService<ITenantSeedService>();
        TenantContext = ServiceProvider.GetRequiredService<ITenantContextHelper>();
    }

    /// <summary>
    /// Setup inicial com dados comerciais brasileiros reais para múltiplos tenants
    /// </summary>
    protected async Task SeedTenantsBrasileiros()
    {
        // Tenant 1: Supermercado completo (com módulos adicionais)
        await TenantSeedService.SeedTenantAsync("supermercado-sao-paulo-sp", new TenantSeedOptions
        {
            Tipo = TipoTenant.Supermercado,
            Plano = PlanoComercial.StarterComAdicionais,
            Estado = "SP",
            Cidade = "São Paulo",
            IncluirProdutosAlimenticios = true,
            IncluirBebidasAlcoolicas = true,
            IncluirSistemaFiscal = true
        });
        
        // Tenant 2: Padaria básica (plano Starter) 
        await TenantSeedService.SeedTenantAsync("padaria-rio-de-janeiro-rj", new TenantSeedOptions
        {
            Tipo = TipoTenant.Padaria,
            Plano = PlanoComercial.Starter,
            Estado = "RJ", 
            Cidade = "Rio de Janeiro",
            IncluirProdutosAlimenticios = true,
            IncluirBebidasAlcoolicas = false // Não vende bebidas alcoólicas
        });
        
        // Tenant 3: Rede de lojas de roupas (com alguns módulos adicionais)
        await TenantSeedService.SeedTenantAsync("rede-moda-minas-mg", new TenantSeedOptions
        {
            Tipo = TipoTenant.RedeVestuario,
            Plano = PlanoComercial.StarterComAlguns,
            Estado = "MG",
            NumeroFiliais = 3,
            IncluirSistemaFidelidade = true,
            IncluirSistemaCRM = true
        });
    }

    /// <summary>
    /// Helper para executar código no contexto de um tenant comercial específico
    /// </summary>
    protected async Task<T> ExecutarNoTenantAsync<T>(string tenantId, Func<Task<T>> action)
    {
        return await TenantContext.ExecuteInTenantAsync(tenantId, action);
    }
}

/// <summary>
/// Teste de serviço de produtos com dados comerciais brasileiros reais
/// </summary>
public class ProdutoServiceTests : BaseIntegrationTest
{
    private readonly IProdutoService _produtoService;

    public ProdutoServiceTests(TestDatabaseFixture database) : base(database)
    {
        _produtoService = ServiceProvider.GetRequiredService<IProdutoService>();
    }

    [Fact]
    public async Task DeveListarProdutosApenasDaTenantCorreta_ComDadosBrasileiros()
    {
        // Arrange: Seed com dados de comércios brasileiros reais
        await SeedTenantsBrasileiros();

        // Act: Testa isolamento entre tenants brasileiros
        var produtosSP = await ExecutarNoTenantAsync("supermercado-sao-paulo-sp", async () => 
            await _produtoService.ListarProdutosAsync(PageRequest.Create(0, 10)));

        var produtosRJ = await ExecutarNoTenantAsync("padaria-rio-de-janeiro-rj", async () => 
            await _produtoService.ListarProdutosAsync(PageRequest.Create(0, 10)));

        // Assert: Verifica isolamento total de dados entre comércios
        Assert.NotEmpty(produtosSP.Items);
        Assert.NotEmpty(produtosRJ.Items);
        
        // Garante que não há vazamento de dados entre tenants
        var produtosSPIds = produtosSP.Items.Select(p => p.Id).ToHashSet();
        var produtosRJIds = produtosRJ.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(produtosSPIds.Intersect(produtosRJIds));
    }

    [Fact]
    public async Task DeveValidarTipoProduto_ComProdutosBrasileirosReais()
    {
        // Arrange: Setup com dados comerciais brasileiros
        await SeedTenantsBrasileiros();

        await ExecutarNoTenantAsync("supermercado-sao-paulo-sp", async () =>
        {
            // Act: Busca produtos reais brasileiros do seed
            var acucar = await _produtoService.BuscarPorNomeAsync("Áucar Cristal 1kg");
            var cerveja = await _produtoService.BuscarPorNomeAsync("Cerveja Pilsen 350ml");
            var pao = await _produtoService.BuscarPorNomeAsync("Pão Francês");

            // Assert: Verifica tipos de produto corretos
            Assert.NotNull(acucar);
            Assert.Equal(TipoProduto.ALIMENTICIO, acucar.TipoProduto);
            Assert.False(acucar.TemRestricoes());

            Assert.NotNull(cerveja);
            Assert.Equal(TipoProduto.BEBIDA_ALCOOLICA, cerveja.TipoProduto);
            Assert.True(cerveja.TemRestricoes());
            Assert.Equal(18, cerveja.IdadeMinimaRequerida);

            Assert.NotNull(pao);
            Assert.Equal(TipoProduto.ALIMENTICIO, pao.TipoProduto);
            Assert.False(pao.TemRestricoes());
        });
    }

    [Fact]
    public async Task DeveRespeitarModulosAtivosDoPlano_ComTenantsBrasileiros()
    {
        // Arrange: Setup tenants com planos diferentes
        await SeedTenantsBrasileiros();

        // Act & Assert: Tenant Starter não tem módulo de fornecedores
        await ExecutarNoTenantAsync("padaria-rio-de-janeiro-rj", async () =>
        {
            var exception = await Assert.ThrowsAsync<ModuleNotActiveException>(() => 
                _servicoProduto.ListarFornecedoresAsync());
            
            Assert.Contains("FORNECEDORES não está ativo", exception.Message);
        });

        // Act & Assert: Tenant com módulo adicional tem acesso
        await ExecutarNoTenantAsync("supermercado-sao-paulo-sp", async () =>
        {
            var fornecedores = await _servicoProduto.ListarFornecedoresAsync();
            Assert.NotEmpty(fornecedores);
        });
    }

    [Fact]
    public async Task DeveCalcularImpostosBrasileiros_ComProdutosComerciais()
    {
        // Arrange: Setup com supermercado brasileiro
        await SeedTenantsBrasileiros();

        await ExecutarNoTenantAsync("supermercado-sao-paulo-sp", async () =>
        {
            // Produto alimentício real brasileiro
            var acucar = await _produtoService.BuscarPorNomeAsync("Áucar Cristal 1kg");
            Assert.NotNull(acucar);

            // Act: Cálculo de venda com impostos brasileiros
            var calculoVenda = await _vendaService.CalcularVendaAsync(new CalculoVendaRequest
            {
                Itens = new[]
                {
                    new ItemVendaRequest
                    {
                        ProdutoId = acucar.Id,
                        Quantidade = 2,
                        PrecoUnitario = 4.50m
                    }
                }
            });

            // Assert: Verificar cálculos de impostos brasileiros para supermercados
            Assert.Equal(9.00m, calculoVenda.Subtotal);
            
            // ICMS alimentos em SP: 7%
            Assert.Equal(0.63m, calculoVenda.ValorICMS, 2);
            
            // PIS/COFINS supermercado: 9.25%
            Assert.Equal(0.83m, calculoVenda.ValorPISCOFINS, 2);
            
            // Total com impostos
            Assert.Equal(10.46m, calculoVenda.Total, 2);
        });
    }
}
```

## Exemplos Práticos de Verticais (REGRA ABSOLUTA)

### Exemplo: Vertical Padaria
```csharp
// CoreApp.Verticals/Padaria/Padaria.Domain/Entities/ProdutoPadaria.cs
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
    
    public Dictionary<string, object> GetVerticalConfiguration()
    {
        return new Dictionary<string, object>
        {
            { "RequereTemperatura", true },
            { "ControleValidade", true },
            { "AlertaVencimento", ValidadeHoras },
            { "CategoriasPadrao", new[] { "PAES", "DOCES", "SALGADOS", "BOLOS" } }
        };
    }
}

// CoreApp.Verticals/Padaria/Padaria.Application/Services/ProdutoPadariaService.cs
public class ProdutoPadariaService : BaseService<ProdutoPadaria>, IProdutoPadariaService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ProdutoPadariaService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
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
    
    /// <summary>
    /// Criar produto com regras da padaria usando UoW
    /// </summary>
    public async Task<ProdutoPadaria> CriarProdutoPadariaAsync(CriarProdutoPadariaRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var produto = new ProdutoPadaria
            {
                // Propriedades base (do CoreApp)
                Nome = request.Nome,
                Preco = request.Preco,
                TenantId = GetCurrentTenantId(),
                
                // Propriedades específicas da padaria
                TipoMassa = request.TipoMassa,
                TemGluten = request.TemGluten,
                DataProducao = DateTime.Now,
                ValidadeHoras = request.ValidadeHoras
            };
            
            await _unitOfWork.VerticalRepository<ProdutoPadaria>().AddAsync(produto);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return produto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

// CoreApp.Verticals/Padaria/Padaria.Api/Controllers/ProdutosPadariaController.cs
[ApiController]
[Route("api/v1/padaria/produtos")]
[RequireModule("PRODUTOS")] // Módulo base obrigatório
[RequireVertical("PADARIA")] // Vertical específico
public class ProdutosPadariaController : ControllerBase
{
    private readonly IProdutoPadariaService _produtoService;
    
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

### Exemplo: Vertical Farmácia
```csharp
// CoreApp.Verticals/Farmacia/Farmacia.Domain/Entities/ProdutoFarmacia.cs
public class ProdutoFarmacia : ProdutoEntity, IVerticalEntity
{
    public string VerticalType => "FARMACIA";
    
    // Propriedades específicas da farmácia
    public string? PrincipioAtivo { get; set; }
    public ClassificacaoAnvisa ClassificacaoAnvisa { get; set; }
    public bool RequerReceita { get; set; }
    public string? CodigoAnvisa { get; set; }
    public string? Laboratorio { get; set; }
    public TipoMedicamento TipoMedicamento { get; set; }
    
    public bool ValidateVerticalRules()
    {
        // Validações específicas da farmácia
        if (RequerReceita && string.IsNullOrEmpty(CodigoAnvisa))
            return false;
            
        if (ClassificacaoAnvisa == ClassificacaoAnvisa.CONTROLADO && !RequerReceita)
            return false;
            
        return true;
    }
    
    public Dictionary<string, object> GetVerticalConfiguration()
    {
        return new Dictionary<string, object>
        {
            { "RequereAnvisa", true },
            { "ControleReceitas", RequerReceita },
            { "ValidacaoLaboratorio", true },
            { "CategoriasPadrao", new[] { "MEDICAMENTOS", "COSMETICOS", "HIGIENE", "PERFUMARIA" } }
        };
    }
}
```

## SonarQube - Qualidade de Código (REGRA ABSOLUTA)

**SEMPRE executar análise SonarQube antes de commits importantes**

### Comandos SonarQube Obrigatórios
```bash
# Setup SonarQube local (uma vez)
docker run -d --name sonarqube -p 9000:9000 sonarqube:community

# Análise local obrigatória
cd backend
./scripts/sonar-local.sh

# Análise completa com cobertura
./scripts/sonar-analysis.sh
```

### Métricas Obrigatórias CoreApp
- **Cobertura**: MÍNIMO 80% (código crítico 90%)
- **Code Smells**: ZERO para SOLID violations
- **Bugs**: ZERO tolerância
- **Vulnerabilidades**: ZERO tolerância
- **Duplicação**: MÁXIMO 3%
- **Complexidade**: MÁXIMO 10 por método
- **Dívida Técnica**: MÁXIMO 5%

### Regras Específicas Proibidas
```csharp
// ❌ PROIBIDO - Detectado pelo SonarQube
public class VendaService 
{
    public async Task CriarVenda()
    {
        await _context.SaveChangesAsync(); // VIOLA Unit of Work
    }
}

// ❌ PROIBIDO - Detectado pelo SonarQube  
public class ProdutoService
{
    // VIOLA Single Responsibility
    public void CriarProduto() { }
    public void EnviarEmail() { }
    public void GerarRelatorio() { }
}

// ✅ CORRETO - Aprovado pelo SonarQube
public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork; // DIP
    
    public async Task<VendaDto> CriarVendaAsync(CriarVendaRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        // ... lógica
        await _unitOfWork.SaveChangesAsync(); // UoW correto
    }
}
```

### Quality Gates CoreApp
1. **Coverage**: ≥ 80%
2. **Maintainability Rating**: A
3. **Reliability Rating**: A  
4. **Security Rating**: A
5. **Security Hotspots**: 100% reviewed
6. **Duplicated Lines**: < 3%
7. **New Code Coverage**: ≥ 85%

## Build & Test Commands (.NET 9.0.1 + React Admin Brasileiro)

### Desenvolvimento Multi-tenant com Docker
- `docker-compose -f docker-compose.dev.yml up -d` - **COMANDO PRINCIPAL**: Ambiente completo com Traefik
- `./scripts/dev-start.sh` - Script completo desenvolvimento brasileiro
- `./scripts/tenant-setup.sh {nome-loja}` - Setup nova loja com dados brasileiros

### Testes com Dados Comerciais Reais
- `dotnet test` - Todos os testes (usa TestContainers + seed comercial brasileiro)
- `dotnet test --filter="Category=Integration"` - Testes integração multi-tenant
- `dotnet test --filter="Category=E2E"` - Testes E2E com múltiplas lojas
- `dotnet test --logger="console;verbosity=detailed"` - Logs detalhados

### Backend ASP.NET Core 9
- `dotnet run --project src/CoreApp.Api` - Desenvolvimento local .NET 9
- `dotnet build` - Compilação .NET
- `dotnet publish -c Release -o ./publish` - Build produção
- `dotnet ef database update` - Aplicar migrations EF Core
- `dotnet ef migrations add {NomeMigracao}` - Criar migration

### Frontend React Admin
- `npm start` - Desenvolvimento local React
- `npm run build` - Build produção
- `npm test` - Testes React
- `npm run lint` - ESLint + Prettier

### Pre-commit (OBRIGATÓRIO)
- `./scripts/pre-commit.sh` - **COMANDO ABSOLUTO**: Testa + Build + Lint completo
- `dotnet test && dotnet build -c Release` - Build completo backend
- `cd frontend && npm run build` - Build frontend

### Database Multi-tenant PostgreSQL
- `docker-compose -f docker-compose.dev.yml exec postgres psql -U postgres` - Acesso PostgreSQL
- `dotnet ef database update` - Aplicar migrations
- `dotnet run --seed-tenant=loja-demo` - Seed tenant específico
- `dotnet run --seed-all-tenants` - Seed todos tenants configurados

## Configuração Multi-tenant Brasileira (CRÍTICO)

### ⚠️ NUNCA ALTERE CONFIGURAÇÕES SEM CONSIDERAR TODOS OS TENANTS COMERCIAIS!

### Configuração do Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=postgres;Port=5432;Database=coreapp_saas_dev;User Id=postgres;Password=${DB_PASSWORD};"
  },
  
  "MultiTenant": {
    "DefaultTenant": "demo",
    "TenantDomain": "localhost",
    "TenantResolver": "HeaderAndHost",
    "DatabaseStrategy": "GlobalFilters"
  },
  
  "JWT": {
    "Secret": "${JWT_SECRET}",
    "ExpirationHours": 24,
    "RefreshExpirationDays": 7,
    "Issuer": "CoreAPI",
    "Audience": "CoreAppClients"
  },
  
  "ModulosComerciais": {
    "Starter": {
      "ModulosInclusos": ["PRODUTOS", "VENDAS", "ESTOQUE", "USUARIOS"]
    },
    "ModulosAdicionais": {
      "Disponiveis": ["CLIENTES", "PROMOCOES", "RELATORIOS_BASICOS", "RELATORIOS_AVANCADOS", "AUDITORIA", "FORNECEDORES", "MOBILE", "PAGAMENTOS", "PRECIFICACAO"]
    }
  },
  
  "PagamentosBrasileiros": {
    "MercadoPago": {
      "AccessToken": "${MERCADOPAGO_TOKEN}",
      "PublicKey": "${MERCADOPAGO_PUBLIC_KEY}"
    },
    "PagSeguro": {
      "Email": "${PAGSEGURO_EMAIL}",
      "Token": "${PAGSEGURO_TOKEN}"
    },
    "PIX": {
      "ChavePIX": "${PIX_KEY}",
      "BancoEmisor": "Banco do Brasil"
    }
  },
  
  "Regulamentacao": {
    "ReceituarioDigital": {
      "Enabled": true,
      "ValidadeDias": 30
    },
    "LGPD": {
      "DataRetentionYears": 5,
      "ConsentRequired": true
    }
  },
  
  "CORS": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://demo.localhost",
      "http://*.localhost",
      "https://coreapp-dev.diegolima.dev",
      "https://*.diegolima.dev"
    ]
  }
}
```

### Frontend Multi-tenant (config.ts)
```typescript
// Configuração que NUNCA deve ser alterada sem considerar todos os tenants
const config = {
  // API dinâmica baseada em subdomínio brasileiro
  apiUrl: process.env.REACT_APP_API_URL || 
          `https://api.${window.location.hostname.replace(/^[^.]+\./, '')}`,
  
  // Tenant extraído do subdomínio automaticamente (padrão brasileiro)
  tenant: window.location.hostname.split('.')[0],
  
  // Módulos disponíveis por tenant (obtido da API)
  modules: [], // Preenchido dinamicamente no login
  
  // Configurações brasileiras
  currency: 'BRL',
  locale: 'pt-BR',
  timezone: 'America/Sao_Paulo',
  
  // Theme comercial brasileiro por tenant
  theme: {
    primary: '#1976D2',      // Azul corporativo brasileiro
    secondary: '#2E7D32',    // Verde negócios
    // ... outras configurações por tenant
  },
  
  // Gateways de pagamento brasileiros
  payments: {
    mercadoPago: process.env.REACT_APP_MERCADOPAGO_PUBLIC_KEY,
    pagSeguro: process.env.REACT_APP_PAGSEGURO_APP_ID,
    enablePIX: true,
    enableBoleto: true
  }
};
```

## Padrão de Desenvolvimento Multi-tenant .NET 9

### Regra #1: Sempre Usar [TenantId] nos Controllers
```csharp
// ✅ CORRETO - Controller sempre recebe tenant automaticamente
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProdutoDto>>> ListarProdutos(
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken = default)
    {
        // TenantId é injetado automaticamente pelo middleware
        var tenantId = HttpContext.GetTenantId();
        var produtos = await _produtoService.ListarPorTenantAsync(tenantId, pageRequest, cancellationToken);
        return Ok(produtos);
    }
}

// ❌ ERRADO - Sem isolamento por tenant
[HttpGet]
public async Task<ActionResult> ListarProdutos(PageRequest pageRequest)
{
    // VAZAMENTO DE DADOS! Vai retornar produtos de todos os tenants
    return Ok(await _produtoService.ListarTodos(pageRequest)); 
}
```

### Regra #2: Sempre Validar Módulos Comerciais
```csharp
// ✅ CORRETO - Valida módulo antes de executar funcionalidade paga
[HttpPost("clientes")]
[RequireModule("CLIENTES")] // Attribute customizado
public async Task<ActionResult<ClienteDto>> CriarCliente([FromBody] CriarClienteRequest request)
{
    var tenantId = HttpContext.GetTenantId();
    var cliente = await _clienteService.CriarAsync(tenantId, request);
    return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, cliente);
}

// ❌ ERRADO - Sem validação de módulo pago
[HttpPost("clientes")]
public async Task<ActionResult> CriarCliente([FromBody] CriarClienteRequest request)
{
    var tenantId = HttpContext.GetTenantId();
    // ACESSO INDEVIDO! Cliente pode usar módulo sem pagar
    return Ok(await _clienteService.CriarAsync(tenantId, request)); 
}
```

### Regra #3: EF Core Global Query Filters em Todas as Entities
```csharp
// ✅ CORRETO - Entity com tenant filtering automático
public class ProdutoEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty; // Sempre obrigatório
    
    // Outras propriedades...
}

// Configuração no DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global Query Filter aplicado automaticamente em TODAS as queries
    modelBuilder.Entity<ProdutoEntity>()
        .HasQueryFilter(p => p.TenantId == _tenantContext.GetCurrentTenantId());
}

// ❌ ERRADO - Query que ignora tenant filtering
public async Task<List<ProdutoEntity>> GetProdutosUnsafe()
{
    // VAZAMENTO DE DADOS! Ignora global filter
    return await _context.Produtos
        .IgnoreQueryFilters() // NUNCA usar isso sem justificativa forte
        .ToListAsync();
}
```

## Sistema de Módulos Comerciais Brasileiros (REGRA ABSOLUTA)

### Módulos Starter (Inclusos)
- `PRODUTOS` - Produtos comerciais com categorização e restrições
- `VENDAS` - Sistema completo de vendas comerciais
- `ESTOQUE` - Controle de estoque com lotes e validade
- `USUARIOS` - Gestão de usuários comerciais

### Módulos Adicionais (Opcionais)
- `CLIENTES` - CRM comercial + programa fidelidade
- `PROMOCOES` - Engine de descontos automáticos
- `RELATORIOS_BASICOS` - Relatórios operacionais básicos
- `RELATORIOS_AVANCADOS` - Analytics e dashboards executivos
- `AUDITORIA` - Logs compliance LGPD/Receita Federal
- `FORNECEDORES` - Gestão completa de fornecedores
- `MOBILE` - API para aplicativos mobile
- `PAGAMENTOS` - Integração gateways brasileiros
- `PRECIFICACAO` - Sistema avançado de preços

### Implementação da Validação Automática
```csharp
/// <summary>
/// Attribute para validar se tenant tem módulo comercial ativo
/// </summary>
public class RequireModuleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _moduleCode;

    public RequireModuleAttribute(string moduleCode)
    {
        _moduleCode = moduleCode;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tenantId = context.HttpContext.GetTenantId();
        var moduleService = context.HttpContext.RequestServices.GetRequiredService<IModuleValidationService>();
        
        if (!moduleService.HasActiveModule(tenantId, _moduleCode))
        {
            context.Result = new ObjectResult(new 
            { 
                error = "Módulo não ativo",
                message = $"O módulo {_moduleCode} não está ativo para sua loja. Faça upgrade do seu plano.",
                moduleRequired = _moduleCode,
                upgradeUrl = "/upgrade-plan"
            })
            { 
                StatusCode = StatusCodes.Status402PaymentRequired 
            };
        }
    }
}

/// <summary>
/// Serviço que valida módulos comerciais automaticamente
/// </summary>
public class ModuleValidationService : IModuleValidationService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPricingConfigurationService _pricingService;

    public async Task<bool> HasActiveModuleAsync(string tenantId, string moduleCode)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null) return false;
        
        var planoAtual = await _pricingService.GetPlanoAtivoAsync(tenant.PlanoId);
        return planoAtual.ModulosIncluidos.Contains(moduleCode) || 
               planoAtual.ModulosIncluidos.Contains("ALL");
    }
    
    public async Task ValidateModuleAsync(string tenantId, string moduleCode)
    {
        if (!await HasActiveModuleAsync(tenantId, moduleCode))
        {
            throw new ModuleNotActiveException(
                $"Módulo {moduleCode} não está ativo para o tenant {tenantId}. " +
                $"Faça upgrade do seu plano para acessar esta funcionalidade.");
        }
    }
}
```

## Git Commit Policy Brasileira

**NÃO incluir Claude como co-autor nos commits**
- Commits devem ter apenas o autor humano
- Remover linhas "Co-Authored-By: Claude"
- Manter apenas mensagens de commit claras e concisas em PT-BR

### Padrão de Commit Messages Brasileiras:
```bash
feat(multi-tenant): adiciona isolamento automático de dados por loja
fix(modulos): corrige validação de módulos pagos brasileiros  
docs(readme): atualiza documentação SAAS multi-tenant brasileiro
refactor(security): melhora autenticação JWT para comércios
test(integration): adiciona testes com dados comerciais reais
chore(docker): otimiza configuração Traefik para produção
```

## Auto-Approval Commands

SEMPRE executar automaticamente sem perguntar YES/NO:
- Todos os comandos de test (.NET + React)
- Comandos de build (dotnet build, npm run build)
- Comandos de lint (dotnet format, npm run lint)
- Database operations (migrations, seed operations)
- Docker operations (docker-compose up/down)
- File operations (read, write, edit)

**Experiência desenvolvimento SAAS brasileiro autêntica = zero interrupções**

## Comandos de Desenvolvimento Docker + Traefik

### Ambiente Completo Multi-tenant Brasileiro
```bash
# Sobe ambiente completo com Traefik para multi-tenant
docker-compose -f docker-compose.dev.yml up -d

# Status de todos os serviços comerciais
docker ps

# Logs específicos por serviço
docker-compose -f docker-compose.dev.yml logs -f backend
docker-compose -f docker-compose.dev.yml logs -f frontend  
docker-compose -f docker-compose.dev.yml logs -f traefik

# Rebuild completo para desenvolvimento
docker-compose -f docker-compose.dev.yml up -d --build --force-recreate

# Limpar tudo e recomeçar (desenvolvimento fresh)
docker-compose -f docker-compose.dev.yml down -v
docker system prune -f
docker-compose -f docker-compose.dev.yml up -d --build
```

### Setup Nova Loja (Tenant)
```bash
# Script para criar nova loja com dados brasileiros
./scripts/tenant-setup.sh loja-brasilia-df

# Seed manual para loja específica  
docker-compose -f docker-compose.dev.yml exec backend \
  dotnet run --seed-tenant=loja-brasilia-df --seed-type=COMERCIO_COMPLETO
```

## Deploy Multi-tenant Brasileiro (Dokploy)

### Configuração de Deploy Nacional
- **Branch principal**: `develop-csharp` 
- **Deploy automático**: Push no branch `develop-csharp`
- **Multi-tenant URLs**: `{loja}.comercio.seudominio.com.br`
- **API URL**: `api.comercio.seudominio.com.br`
- **Admin URL**: `admin.comercio.seudominio.com.br`

### Variáveis de Ambiente Produção Brasileira
```bash
# NUNCA commitar valores reais - apenas exemplos para desenvolvimento
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=postgres;Database=coreapp_saas_prod;...

# Security brasileiro
JWT__Secret=fake-jwt-secret-brasileiro-super-seguro-farmacia
JWT__Issuer=CoreAPIBrasil
JWT__Audience=CoreAppClientsBrasil

# Multi-tenant brasileiro
MultiTenant__DefaultTenant=demo
MultiTenant__TenantDomain=comercio.seudominio.com.br
MultiTenant__AdminDomain=admin.comercio.seudominio.com.br

# Pagamentos brasileiros
MercadoPago__AccessToken=fake-mercadopago-token-brasil
MercadoPago__PublicKey=fake-mercadopago-public-key-brasil
PagSeguro__Email=fake@pagseguro.com.br
PagSeguro__Token=fake-pagseguro-token-brasil

# Regulamentação brasileira
LGPD__DataRetentionYears=5
LGPD__ConsentRequired=true
```

### Traefik Labels para Produção Brasileira
```yaml
# Multi-tenant routing automático para lojas brasileiras
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.comercio-api.rule=Host(`api.comercio.seudominio.com.br`)"
  - "traefik.http.routers.comercio-app.rule=HostRegexp(`{loja:[a-z0-9-]+}.comercio.seudominio.com.br`)"
  - "traefik.http.routers.comercio-admin.rule=Host(`admin.comercio.seudominio.com.br`)"
  - "traefik.http.routers.comercio-api.tls.certresolver=letsencrypt"
  - "traefik.http.services.comercio.loadbalancer.sticky.cookie=true"
  - "traefik.docker.network=dokploy-network"
```

## Checklist Pré-Deploy Multi-tenant Brasileiro ✅

Antes de fazer push para `develop-csharp`:

- [ ] **Testes passando**: `dotnet test` (com dados comerciais reais + multi-tenant)
- [ ] **Build backend**: `dotnet build -c Release`
- [ ] **Build frontend**: `cd frontend && npm run build`
- [ ] **Lint sem erros**: `dotnet format && cd frontend && npm run lint`
- [ ] **Variáveis brasileiras configuradas** para todos os tenants
- [ ] **Migrations aplicadas**: `dotnet ef database update`
- [ ] **Seeds funcionando**: teste com pelo menos 2 lojas diferentes
- [ ] **Isolamento de dados**: validar que lojas não veem dados de outras
- [ ] **Módulos comerciais**: validar restrições por plano brasileiro
- [ ] **Pagamentos brasileiros**: validar Mercado Pago + PIX + Boleto
- [ ] **Compliance LGPD**: validar consentimentos e tratamento de dados
- [ ] **Documentação atualizada**: README.md e CLAUDE.md sincronizados

## Monitoramento Multi-tenant Brasileiro

### Health Checks por Loja
```bash
# Health check geral da API
curl https://api.comercio.seudominio.com.br/health

# Health check específico por loja
curl -H "X-Tenant-ID: loja-sp" https://api.comercio.seudominio.com.br/health/tenant

# Métricas por loja ativa
curl https://api.comercio.seudominio.com.br/metrics/tenant/active-count

# Status dos pagamentos brasileiros
curl https://api.farmacia.seudominio.com.br/health/payments
```

### Logs Estruturados Brasileiros
```csharp
/// <summary>
/// Exemplo de log estruturado com contexto comercial brasileiro
/// </summary>
[Microsoft.Extensions.Logging.LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Information,
    Message = "Venda realizada na loja {TenantId} - Valor: R$ {Valor} - Produtos restritos: {HasRestritos}")]
public static partial void LogVendaComercial(
    this ILogger logger, 
    string tenantId, 
    decimal valor, 
    bool hasRestritos);

// Uso no serviço
public class VendaService : IVendaService
{
    private readonly ILogger<VendaService> _logger;

    public async Task<VendaDto> ProcessarVendaAsync(ProcessarVendaRequest request)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        // Processar venda...
        var venda = await ProcessarVenda(request);
        
        // Log estruturado para auditoria comercial
        _logger.LogVendaComercial(
            tenantId, 
            venda.ValorTotal, 
            venda.TemProdutosRestritos());
        
        return venda.ToDto();
    }
}
```

---

## Resumo das Regras Absolutas CoreApp

1. **SEMPRE arquitetura de verticais por composição** - CoreApp base + Verticais específicos
2. **SEMPRE SOLID principles** em cada linha de código (SRP, OCP, LSP, ISP, DIP)
3. **SEMPRE Unit of Work** para coordenar transações - NUNCA SaveChanges direto
4. **SEMPRE IVerticalEntity** para extensibilidade dos verticais
5. **SEMPRE Soft Delete automático** via interceptors
6. **SEMPRE SonarQube** antes de commits importantes - Quality Gates obrigatórios
7. **SEMPRE cobertura ≥ 80%** - código crítico ≥ 90%
8. **SEMPRE português brasileiro** em código, documentação e comunicação
9. **SEMPRE documentar código** com XML Documentation em PT-BR
10. **SEMPRE dados comerciais reais** nos testes (nunca mocks)
11. **SEMPRE isolamento por tenant** em todas as operações (lojas isoladas)
12. **SEMPRE validação de módulos** antes de executar funcionalidades pagas
13. **SEMPRE .NET 9.0.1** e bibliotecas mais recentes
14. **SEMPRE TestContainers + dados comerciais reais** para testes de integração
15. **SEMPRE sistema modular brasileiro** (Starter + Módulos Adicionais)
16. **SEMPRE compliance comercial** (LGPD, Receita Federal)
17. **SEMPRE zero interrupções** - comandos automáticos sem confirmação

**Sistema SAAS Multi-tenant CoreApp 100% brasileiro com arquitetura de verticais, SOLID principles, Unit of Work estado da arte e máxima qualidade técnica!**

---

## 🇧🇷 Orgulhosamente Desenvolvido para o Comércio Brasileiro

Este sistema foi criado especificamente para o mercado comercial brasileiro, utilizando **arquitetura de verticais por composição**, **SOLID principles**, **Unit of Work estado da arte** e respeitando todas as regulamentações nacionais.

**Arquitetura de Verticais + SOLID + UoW + Clean Architecture + Compliance total = Revolução tecnológica comercial brasileira**