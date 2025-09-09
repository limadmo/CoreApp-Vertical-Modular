# Regras do Sistema SAAS Multi-tenant Farmácia (.NET 9)

## Estrutura do Projeto C# Brasileiro
```
core-saas/
├── backend/          # ASP.NET Core 9 + Entity Framework Core 9
├── frontend/         # React Admin 4.16.x + Tailwind CSS 3.4 LTS  
├── traefik/          # Reverse Proxy Multi-tenant
├── scripts/          # Scripts brasileiros de automação
└── CLAUDE.md         # Este arquivo (regras absolutas para o projeto)
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
- Comentar regras de negócio específicas de farmácia/ANVISA/Brasil
- Documentar validações de compliance farmacêutico

## Arquitetura de Configuração (REGRA ABSOLUTA)

**NUNCA usar enums para dados configuráveis**
- SEMPRE criar tabelas de configuração dinâmica ao invés de enums
- Implementar cache em memória simples para performance adequada
- Sistema hierárquico: Global (Sistema) → Tenant (Farmácia) → Usuário
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
/// Aplica regras ANVISA e isolamento automático por tenant farmacêutico
/// </summary>
/// <remarks>
/// Este serviço implementa as regulamentações brasileiras de controle de estoque
/// conforme determinações da ANVISA para estabelecimentos farmacêuticos
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
    /// <returns>Movimentação criada com validações ANVISA aplicadas</returns>
    /// <exception cref="ModuleNotActiveException">Quando módulo de estoque não está ativo para o tenant</exception>
    /// <exception cref="ProdutoControlladoException">Quando produto controlado não tem receita válida</exception>
    [RequireModule("STOCK")]
    public async Task<MovimentacaoResponseDto> RegistrarMovimentacaoAsync(MovimentacaoRequestDto request)
    {
        // Obtém o tenant atual automaticamente via middleware
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        // Valida se o produto existe e pertence ao tenant
        var produto = await _produtoRepository.GetByIdAndTenantAsync(request.ProdutoId, tenantId)
            ?? throw new ProdutoNaoEncontradoException($"Produto {request.ProdutoId} não encontrado para o tenant {tenantId}");

        // Aplica regras ANVISA para medicamentos controlados
        if (produto.IsControlado())
        {
            await ValidarMovimentacaoControlada(produto, request);
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

        // Log para auditoria farmacêutica obrigatória
        await _auditService.LogMovimentacaoEstoque(tenantId, movimentacao);

        return movimentacao.ToResponseDto();
    }

    /// <summary>
    /// Valida movimentação de medicamento controlado conforme normas ANVISA
    /// </summary>
    /// <param name="produto">Produto controlado que será movimentado</param>
    /// <param name="request">Dados da movimentação solicitada</param>
    private async Task ValidarMovimentacaoControlada(ProdutoEntity produto, MovimentacaoRequestDto request)
    {
        // Validação específica para cada tipo de lista ANVISA
        switch (produto.ClassificacaoAnvisa)
        {
            case ClassificacaoAnvisa.A1:
            case ClassificacaoAnvisa.A2:
            case ClassificacaoAnvisa.A3:
                // Lista A requer receita especial azul
                await ValidarReceitaEspecial(request.ReceitaId, TipoReceita.Azul);
                break;
                
            case ClassificacaoAnvisa.B1:
            case ClassificacaoAnvisa.B2:
                // Lista B requer receita especial branca
                await ValidarReceitaEspecial(request.ReceitaId, TipoReceita.Branca);
                break;
                
            case ClassificacaoAnvisa.C1:
                // Lista C1 requer receita com 2 vias
                await ValidarReceitaC1(request.ReceitaId);
                break;
        }
    }
}
```

### Exemplo de padrão React Admin (TypeScript):
```typescript
/**
 * Componente de listagem de produtos farmacêuticos com suporte multi-tenant brasileiro
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

  if (!hasModule('PRODUCTS')) {
    return (
      <Card>
        <CardContent>
          <Typography>
            Módulo de Produtos não está ativo para sua farmácia.
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
        <TextField source="principioAtivo" label="Princípio Ativo" />
        <TextField source="laboratorio" label="Laboratório" />
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
        {hasModule('STOCK') && (
          <NumberField source="estoqueAtual" label="Estoque Atual" />
        )}
        
        {/* Classificação ANVISA sempre visível (compliance) */}
        <FunctionField
          label="Classificação ANVISA"
          render={(record: any) => (
            <Chip
              label={record.classificacaoAnvisa}
              color={getClassificacaoColor(record.classificacaoAnvisa)}
              size="small"
            />
          )}
        />
        
        {/* Campos de auditoria apenas para Enterprise */}
        {hasModule('AUDIT') && (
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
        {hasModule('BASIC_REPORTS') && (
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
 * Retorna a cor do chip baseada na classificação ANVISA brasileira
 */
const getClassificacaoColor = (classificacao: string): 'default' | 'primary' | 'secondary' | 'error' | 'warning' => {
  switch (classificacao) {
    case 'ISENTO_PRESCRICAO':
      return 'default';
    case 'SUJEITO_PRESCRICAO':
      return 'primary';
    case 'A1': case 'A2': case 'A3':
      return 'error';      // Lista A - vermelho (psicotrópicos)
    case 'B1': case 'B2':
      return 'warning';    // Lista B - laranja (entorpecentes)
    case 'C1': case 'C2': case 'C3': case 'C4': case 'C5':
      return 'secondary';  // Lista C - roxo (outras controladas)
    default:
      return 'default';
  }
};
```

## Testing Strategy Brasileira (REGRA ABSOLUTA)

**SEMPRE USE DADOS FARMACÊUTICOS REAIS DO BRASIL - NUNCA MOCKS**

- Todos os testes devem usar dados concretos do seed database brasileiro
- Testes de integração com database real (TestContainers + PostgreSQL) 
- Dados farmacêuticos realistas em conformidade com ANVISA
- Setup de banco de teste com seed automático por tenant brasileiro
- Testes de compliance com regulamentações nacionais

### Por quê dados reais?
- Testes mais autênticos e confiáveis para mercado brasileiro
- Validação real das regras de negócio farmacêuticas nacionais
- Detecta problemas de schema e relacionamentos
- Compliance real com regulamentações ANVISA/CFF/LGPD
- Testa isolamento de dados entre tenants (farmácias)
- Valida cálculos de impostos brasileiros (ICMS, PIS/COFINS)

### Estrutura de Testes .NET 9 (xUnit + TestContainers)
```csharp
/// <summary>
/// Classe base para testes de integração com dados farmacêuticos brasileiros reais
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
    /// Setup inicial com dados farmacêuticos brasileiros reais para múltiplos tenants
    /// </summary>
    protected async Task SeedTenantsBrasileiros()
    {
        // Tenant 1: Farmácia independente completa (plano Enterprise)
        await TenantSeedService.SeedTenantAsync("farmacia-sao-paulo-sp", new TenantSeedOptions
        {
            Tipo = TipoTenant.FarmaciaIndependente,
            Plano = PlanoComercial.Enterprise,
            Estado = "SP",
            Cidade = "São Paulo",
            IncluirDadosAnvisa = true,
            IncluirMedicamentosControlados = true,
            IncluirReceituarios = true
        });
        
        // Tenant 2: Farmácia básica (plano Starter) 
        await TenantSeedService.SeedTenantAsync("farmacia-rio-de-janeiro-rj", new TenantSeedOptions
        {
            Tipo = TipoTenant.FarmaciaIndependente,
            Plano = PlanoComercial.Starter,
            Estado = "RJ", 
            Cidade = "Rio de Janeiro",
            IncluirDadosAnvisa = true,
            IncluirMedicamentosControlados = false // Não tem módulo de controlados
        });
        
        // Tenant 3: Rede de farmácias (plano Professional)
        await TenantSeedService.SeedTenantAsync("rede-farmacia-minas-mg", new TenantSeedOptions
        {
            Tipo = TipoTenant.RedeFarmacias,
            Plano = PlanoComercial.Professional,
            Estado = "MG",
            NumeroFiliais = 3,
            IncluirDadosAnvisa = true,
            IncluirSistemaCRM = true
        });
    }

    /// <summary>
    /// Helper para executar código no contexto de um tenant farmacêutico específico
    /// </summary>
    protected async Task<T> ExecutarNoTenantAsync<T>(string tenantId, Func<Task<T>> action)
    {
        return await TenantContext.ExecuteInTenantAsync(tenantId, action);
    }
}

/// <summary>
/// Teste de serviço de produtos com dados farmacêuticos brasileiros reais
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
        // Arrange: Seed com dados de farmácias brasileiras reais
        await SeedTenantsBrasileiros();

        // Act: Testa isolamento entre tenants brasileiros
        var produtosSP = await ExecutarNoTenantAsync("farmacia-sao-paulo-sp", async () => 
            await _produtoService.ListarProdutosAsync(PageRequest.Create(0, 10)));

        var produtosRJ = await ExecutarNoTenantAsync("farmacia-rio-de-janeiro-rj", async () => 
            await _produtoService.ListarProdutosAsync(PageRequest.Create(0, 10)));

        // Assert: Verifica isolamento total de dados entre farmácias
        Assert.NotEmpty(produtosSP.Items);
        Assert.NotEmpty(produtosRJ.Items);
        
        // Garante que não há vazamento de dados entre tenants
        var produtosSPIds = produtosSP.Items.Select(p => p.Id).ToHashSet();
        var produtosRJIds = produtosRJ.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(produtosSPIds.Intersect(produtosRJIds));
    }

    [Fact]
    public async Task DeveValidarClassificacaoAnvisa_ComMedicamentosBrasileirosReais()
    {
        // Arrange: Setup com dados farmacêuticos brasileiros
        await SeedTenantsBrasileiros();

        await ExecutarNoTenantAsync("farmacia-sao-paulo-sp", async () =>
        {
            // Act: Busca medicamentos reais brasileiros do seed
            var dipirona = await _produtoService.BuscarPorNomeAsync("Dipirona Sódica 500mg");
            var clonazepam = await _produtoService.BuscarPorNomeAsync("Clonazepam 2mg");
            var paracetamol = await _produtoService.BuscarPorNomeAsync("Paracetamol 750mg");

            // Assert: Verifica classificações ANVISA corretas
            Assert.NotNull(dipirona);
            Assert.Equal(ClassificacaoAnvisa.ISENTO_PRESCRICAO, dipirona.ClassificacaoAnvisa);
            Assert.False(dipirona.IsControlado());

            Assert.NotNull(clonazepam);
            Assert.Equal(ClassificacaoAnvisa.B1, clonazepam.ClassificacaoAnvisa);
            Assert.True(clonazepam.IsControlado());
            Assert.Equal(TipoReceita.Branca, clonazepam.TipoReceitaNecessaria);

            Assert.NotNull(paracetamol);
            Assert.Equal(ClassificacaoAnvisa.SUJEITO_PRESCRICAO, paracetamol.ClassificacaoAnvisa);
            Assert.False(paracetamol.IsControlado());
        });
    }

    [Fact]
    public async Task DeveRespeitarModulosAtivosDoPlano_ComTenantsBrasileiros()
    {
        // Arrange: Setup tenants com planos diferentes
        await SeedTenantsBrasileiros();

        // Act & Assert: Tenant Starter não tem módulo de fornecedores
        await ExecutarNoTenantAsync("farmacia-rio-de-janeiro-rj", async () =>
        {
            var exception = await Assert.ThrowsAsync<ModuleNotActiveException>(() => 
                _produtoService.ListarFornecedoresAsync());
            
            Assert.Contains("SUPPLIERS não está ativo", exception.Message);
        });

        // Act & Assert: Tenant Enterprise tem todos os módulos
        await ExecutarNoTenantAsync("farmacia-sao-paulo-sp", async () =>
        {
            var fornecedores = await _produtoService.ListarFornecedoresAsync();
            Assert.NotEmpty(fornecedores);
        });
    }

    [Fact]
    public async Task DeveCalcularImpostosBrasileiros_ComProdutosFarmaceuticos()
    {
        // Arrange: Setup com farmácia brasileira
        await SeedTenantsBrasileiros();

        await ExecutarNoTenantAsync("farmacia-sao-paulo-sp", async () =>
        {
            // Produto farmacêutico real brasileiro
            var dipirona = await _produtoService.BuscarPorNomeAsync("Dipirona Sódica 500mg");
            Assert.NotNull(dipirona);

            // Act: Cálculo de venda com impostos brasileiros
            var calculoVenda = await _vendaService.CalcularVendaAsync(new CalculoVendaRequest
            {
                Itens = new[]
                {
                    new ItemVendaRequest
                    {
                        ProdutoId = dipirona.Id,
                        Quantidade = 2,
                        PrecoUnitario = 12.50m
                    }
                }
            });

            // Assert: Verificar cálculos de impostos brasileiros para farmácias
            Assert.Equal(25.00m, calculoVenda.Subtotal);
            
            // ICMS farmácia em SP: 8.5%
            Assert.Equal(2.13m, calculoVenda.ValorICMS, 2);
            
            // PIS/COFINS farmácia: 6.4%
            Assert.Equal(1.60m, calculoVenda.ValorPISCOFINS, 2);
            
            // Total com impostos
            Assert.Equal(28.73m, calculoVenda.Total, 2);
        });
    }
}
```

## Build & Test Commands (.NET 9 + React Admin Brasileiro)

### Desenvolvimento Multi-tenant com Docker
- `docker-compose -f docker-compose.dev.yml up -d` - **COMANDO PRINCIPAL**: Ambiente completo com Traefik
- `./scripts/dev-start.sh` - Script completo desenvolvimento brasileiro
- `./scripts/tenant-setup.sh {nome-farmacia}` - Setup nova farmácia com dados brasileiros

### Testes com Dados Farmacêuticos Reais
- `dotnet test` - Todos os testes (usa TestContainers + seed farmacêutico brasileiro)
- `dotnet test --filter="Category=Integration"` - Testes integração multi-tenant
- `dotnet test --filter="Category=E2E"` - Testes E2E com múltiplas farmácias
- `dotnet test --logger="console;verbosity=detailed"` - Logs detalhados

### Backend ASP.NET Core 9
- `dotnet run --project src/Farmacia.Api` - Desenvolvimento local .NET 9
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
- `dotnet run --seed-tenant=farmacia-demo` - Seed tenant específico
- `dotnet run --seed-all-tenants` - Seed todos tenants configurados

## Configuração Multi-tenant Brasileira (CRÍTICO)

### ⚠️ NUNCA ALTERE CONFIGURAÇÕES SEM CONSIDERAR TODOS OS TENANTS FARMACÊUTICOS!

### Configuração do Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=postgres;Port=5432;Database=farmacia_saas_dev;User Id=postgres;Password=${DB_PASSWORD};"
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
    "Audience": "FarmaciaClients"
  },
  
  "ModulosComerciais": {
    "Starter": {
      "PrecoMensalBRL": 149.90,
      "PrecoAnualBRL": 1499.00,
      "Modulos": ["PRODUCTS", "SALES", "STOCK", "USERS"]
    },
    "Professional": {
      "PrecoMensalBRL": 249.90,
      "PrecoAnualBRL": 2399.00,
      "Modulos": ["PRODUCTS", "SALES", "STOCK", "USERS", "CUSTOMERS", "PROMOTIONS", "BASIC_REPORTS"]
    },
    "Enterprise": {
      "PrecoMensalBRL": 399.90,
      "PrecoAnualBRL": 3599.00,
      "Modulos": ["ALL"]
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
  
  "ANVISA": {
    "BaseUrl": "https://consultas.anvisa.gov.br/api",
    "ApiKey": "${ANVISA_API_KEY}",
    "TimeoutSeconds": 30
  },
  
  "CORS": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://demo.localhost",
      "http://*.localhost",
      "https://farmacia-dev.diegolima.dev",
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
  
  // Theme farmacêutico brasileiro por tenant
  theme: {
    primary: '#2E7D32',      // Verde farmácia brasileira
    secondary: '#1976D2',    // Azul confiança
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
[RequireModule("CUSTOMERS")] // Attribute customizado
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

### Módulos Starter (R$ 149,90/mês)
- `PRODUCTS` - Produtos e medicamentos com classificação ANVISA
- `SALES` - Sistema completo de vendas farmacêuticas
- `STOCK` - Controle de estoque com lotes e validade
- `USERS` - Gestão de usuários farmacêuticos

### Módulos Professional (+R$ 100,00/mês)
- `CUSTOMERS` - CRM farmacêutico + fidelidade
- `PROMOTIONS` - Engine de descontos automáticos
- `BASIC_REPORTS` - Relatórios operacionais básicos

### Módulos Enterprise (+R$ 150,00/mês)
- `ADVANCED_REPORTS` - Analytics e dashboards executivos
- `AUDIT` - Logs compliance ANVISA/CFF
- `SUPPLIERS` - Gestão completa de fornecedores
- `MOBILE` - API para aplicativos mobile

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
                message = $"O módulo {_moduleCode} não está ativo para sua farmácia. Faça upgrade do seu plano.",
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
feat(multi-tenant): adiciona isolamento automático de dados por farmácia
fix(modulos): corrige validação de módulos pagos brasileiros  
docs(readme): atualiza documentação SAAS multi-tenant brasileiro
refactor(security): melhora autenticação JWT para farmácias
test(integration): adiciona testes com dados ANVISA reais
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

# Status de todos os serviços farmacêuticos
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

### Setup Nova Farmácia (Tenant)
```bash
# Script para criar nova farmácia com dados brasileiros
./scripts/tenant-setup.sh farmacia-brasilia-df

# Seed manual para farmácia específica  
docker-compose -f docker-compose.dev.yml exec backend \
  dotnet run --seed-tenant=farmacia-brasilia-df --seed-type=FARMACIA_COMPLETA
```

## Deploy Multi-tenant Brasileiro (Dokploy)

### Configuração de Deploy Nacional
- **Branch principal**: `develop-csharp` 
- **Deploy automático**: Push no branch `develop-csharp`
- **Multi-tenant URLs**: `{farmacia}.farmacia.seudominio.com.br`
- **API URL**: `api.farmacia.seudominio.com.br`
- **Admin URL**: `admin.farmacia.seudominio.com.br`

### Variáveis de Ambiente Produção Brasileira
```bash
# NUNCA commitar valores reais - apenas exemplos para desenvolvimento
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=postgres;Database=farmacia_saas_prod;...

# Security brasileiro
JWT__Secret=fake-jwt-secret-brasileiro-super-seguro-farmacia
JWT__Issuer=CoreAPIBrasil
JWT__Audience=FarmaciaClientsBrasil

# Multi-tenant brasileiro
MultiTenant__DefaultTenant=demo
MultiTenant__TenantDomain=farmacia.seudominio.com.br
MultiTenant__AdminDomain=admin.farmacia.seudominio.com.br

# Pagamentos brasileiros
MercadoPago__AccessToken=fake-mercadopago-token-brasil
MercadoPago__PublicKey=fake-mercadopago-public-key-brasil
PagSeguro__Email=fake@pagseguro.com.br
PagSeguro__Token=fake-pagseguro-token-brasil

# APIs brasileiras
ANVISA__ApiKey=fake-anvisa-api-key-brasil
ANVISA__BaseUrl=https://consultas.anvisa.gov.br/api
```

### Traefik Labels para Produção Brasileira
```yaml
# Multi-tenant routing automático para farmácias brasileiras
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.farmacia-api.rule=Host(`api.farmacia.seudominio.com.br`)"
  - "traefik.http.routers.farmacia-app.rule=HostRegexp(`{farmacia:[a-z0-9-]+}.farmacia.seudominio.com.br`)"
  - "traefik.http.routers.farmacia-admin.rule=Host(`admin.farmacia.seudominio.com.br`)"
  - "traefik.http.routers.farmacia-api.tls.certresolver=letsencrypt"
  - "traefik.http.services.farmacia.loadbalancer.sticky.cookie=true"
  - "traefik.docker.network=dokploy-network"
```

## Checklist Pré-Deploy Multi-tenant Brasileiro ✅

Antes de fazer push para `develop-csharp`:

- [ ] **Testes passando**: `dotnet test` (com dados farmacêuticos reais + multi-tenant)
- [ ] **Build backend**: `dotnet build -c Release`
- [ ] **Build frontend**: `cd frontend && npm run build`
- [ ] **Lint sem erros**: `dotnet format && cd frontend && npm run lint`
- [ ] **Variáveis brasileiras configuradas** para todos os tenants
- [ ] **Migrations aplicadas**: `dotnet ef database update`
- [ ] **Seeds funcionando**: teste com pelo menos 2 farmácias diferentes
- [ ] **Isolamento de dados**: validar que farmácias não veem dados de outras
- [ ] **Módulos comerciais**: validar restrições por plano brasileiro
- [ ] **Pagamentos brasileiros**: validar Mercado Pago + PIX + Boleto
- [ ] **Compliance ANVISA**: validar classificações e receituários
- [ ] **Documentação atualizada**: README.md e CLAUDE.md sincronizados

## Monitoramento Multi-tenant Brasileiro

### Health Checks por Farmácia
```bash
# Health check geral da API
curl https://api.farmacia.seudominio.com.br/health

# Health check específico por farmácia
curl -H "X-Tenant-ID: farmacia-sp" https://api.farmacia.seudominio.com.br/health/tenant

# Métricas por farmácia ativa
curl https://api.farmacia.seudominio.com.br/metrics/tenant/active-count

# Status dos pagamentos brasileiros
curl https://api.farmacia.seudominio.com.br/health/payments
```

### Logs Estruturados Brasileiros
```csharp
/// <summary>
/// Exemplo de log estruturado com contexto farmacêutico brasileiro
/// </summary>
[Microsoft.Extensions.Logging.LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Information,
    Message = "Venda realizada na farmácia {TenantId} - Valor: R$ {Valor} - Medicamentos controlados: {HasControlados}")]
public static partial void LogVendaFarmaceutica(
    this ILogger logger, 
    string tenantId, 
    decimal valor, 
    bool hasControlados);

// Uso no serviço
public class VendaService : IVendaService
{
    private readonly ILogger<VendaService> _logger;

    public async Task<VendaDto> ProcessarVendaAsync(ProcessarVendaRequest request)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        // Processar venda...
        var venda = await ProcessarVenda(request);
        
        // Log estruturado para auditoria farmacêutica
        _logger.LogVendaFarmaceutica(
            tenantId, 
            venda.ValorTotal, 
            venda.TemMedicamentosControlados());
        
        return venda.ToDto();
    }
}
```

---

## Resumo das Regras Absolutas Brasileiras

1. **SEMPRE português brasileiro** em código, documentação e comunicação
2. **SEMPRE documentar código** com XML Documentation em PT-BR
3. **SEMPRE dados farmacêuticos reais** nos testes (nunca mocks)
4. **SEMPRE isolamento por tenant** em todas as operações (farmácias isoladas)
5. **SEMPRE validação de módulos** antes de executar funcionalidades pagas
6. **SEMPRE Docker + Traefik** para desenvolvimento multi-tenant
7. **SEMPRE TestContainers + dados ANVISA reais** para testes de integração
8. **SEMPRE preços em reais brasileiros** (R$ 149,90/249,90/399,90)
9. **SEMPRE compliance farmacêutico** (ANVISA, CFF, LGPD)
10. **SEMPRE zero interrupções** - comandos automáticos sem confirmação

**Sistema SAAS Multi-tenant 100% brasileiro com máxima qualidade, segurança e compliance farmacêutica nacional!**

---

## 🇧🇷 Orgulhosamente Desenvolvido para Farmácias Brasileiras

Este sistema foi criado especificamente para o mercado farmacêutico brasileiro, respeitando todas as regulamentações nacionais e oferecendo preços acessíveis para farmácias de todos os portes.

**Tecnologia de ponta + Compliance total + Preços justos = Revolução farmacêutica brasileira**