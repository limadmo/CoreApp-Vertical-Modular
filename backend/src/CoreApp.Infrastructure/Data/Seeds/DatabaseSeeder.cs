using Bogus;
using CoreApp.Domain.Entities;
using CoreApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreApp.Infrastructure.Data.Seeds;

/// <summary>
/// Seeding robusto com dados brasileiros usando Bogus
/// Gera +10.000 registros realistas para desenvolvimento e testes
/// </summary>
public class DatabaseSeeder
{
    private readonly CoreAppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly Faker _faker;

    // Configurações do seeding
    private const int PRODUTOS_COUNT = 10000;
    private const int CLIENTES_COUNT = 1000;
    private const int FORNECEDORES_COUNT = 500;
    private const int CATEGORIAS_COUNT = 100;
    private const int USUARIOS_COUNT = 50;
    private const int VENDAS_COUNT = 2000;
    private const int MOVIMENTACOES_ESTOQUE_COUNT = 5000;
    
    // Batch size para performance
    private const int BATCH_SIZE = 1000;

    public DatabaseSeeder(CoreAppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configurar Faker para português brasileiro
        _faker = new Faker("pt_BR");
    }

    /// <summary>
    /// Executa o seeding completo do banco de dados
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("🌱 Iniciando seeding robusto com Bogus (dados brasileiros)");
        _logger.LogInformation("📊 Preparando para criar {Total} registros realistas", 
            PRODUTOS_COUNT + CLIENTES_COUNT + FORNECEDORES_COUNT + CATEGORIAS_COUNT + USUARIOS_COUNT + VENDAS_COUNT + MOVIMENTACOES_ESTOQUE_COUNT);

        try
        {
            // Verificar se o banco já foi populado
            if (await _context.Produtos.AnyAsync())
            {
                _logger.LogInformation("⏭️ Banco já possui dados, pulando seeding");
                return;
            }

            // Configurar tenant padrão para seeding
            const string tenantId = "seed-tenant-001";

            // Seeding sequencial para manter relacionamentos
            await SeedCategorias(tenantId);
            await SeedFornecedores(tenantId);
            await SeedClientes(tenantId);
            await SeedUsuarios(tenantId);
            await SeedProdutos(tenantId);
            await SeedMovimentacoesEstoque(tenantId);
            await SeedVendas(tenantId);

            var totalRecords = await _context.Produtos.CountAsync() + 
                             await _context.Clientes.CountAsync() + 
                             await _context.Fornecedores.CountAsync();

            _logger.LogInformation("✅ Seeding concluído! {Total} registros brasileiros criados", totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante o seeding do banco de dados");
            throw;
        }
    }

    /// <summary>
    /// Seed categorias de produtos brasileiros
    /// </summary>
    private async Task SeedCategorias(string tenantId)
    {
        _logger.LogInformation("📦 Criando categorias de produtos brasileiros...");

        var categoriasBrasileiras = new List<string>
        {
            "Medicamentos", "Cosméticos", "Higiene Pessoal", "Perfumaria", "Dermocosméticos",
            "Suplementos", "Vitaminas", "Infantil", "Equipamentos Médicos", "Ortopedia",
            "Alimentícios", "Bebidas", "Laticínios", "Carnes e Frios", "Pães e Doces",
            "Frutas e Verduras", "Cereais e Grãos", "Conservas", "Condimentos", "Temperos",
            "Material de Limpeza", "Papel e Celulose", "Utilidades Domésticas", "Eletrônicos", "Brinquedos"
        };

        var categoriasFaker = new Faker<CategoriaEntity>("pt_BR")
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.TenantId, f => tenantId)
            .RuleFor(c => c.Nome, f => f.PickRandom(categoriasBrasileiras))
            .RuleFor(c => c.Descricao, f => f.Commerce.ProductDescription())
            .RuleFor(c => c.Ativo, f => f.Random.Bool(0.95f))
            .RuleFor(c => c.DataCriacao, f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now))
            .RuleFor(c => c.DataAtualizacao, f => f.Date.Recent(365));

        var categorias = categoriasFaker.Generate(CATEGORIAS_COUNT);
        
        await _context.Categorias.AddRangeAsync(categorias);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} categorias criadas com sucesso!", categorias.Count);
    }

    /// <summary>
    /// Seed fornecedores brasileiros
    /// </summary>
    private async Task SeedFornecedores(string tenantId)
    {
        _logger.LogInformation("🏭 Criando fornecedores brasileiros...");

        var fornecedoresFaker = new Faker<FornecedorEntity>("pt_BR")
            .RuleFor(f => f.Id, f => Guid.NewGuid())
            .RuleFor(f => f.TenantId, f => tenantId)
            .RuleFor(f => f.Nome, f => f.Company.CompanyName())
            .RuleFor(f => f.Descricao, f => f.Company.Bs())
            .RuleFor(f => f.Ativo, f => f.Random.Bool(0.95f))
            .RuleFor(f => f.DataCriacao, f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now))
            .RuleFor(f => f.DataAtualizacao, f => f.Date.Recent(365));

        var fornecedores = new List<FornecedorEntity>();
        
        for (int i = 0; i < FORNECEDORES_COUNT; i += BATCH_SIZE)
        {
            var batchSize = Math.Min(BATCH_SIZE, FORNECEDORES_COUNT - i);
            var batch = fornecedoresFaker.Generate(batchSize);
            
            fornecedores.AddRange(batch);

            if (i % 100 == 0)
            {
                _logger.LogInformation("Processando fornecedores: {Current}/{Total}", i, FORNECEDORES_COUNT);
            }
        }

        await _context.Fornecedores.AddRangeAsync(fornecedores);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} fornecedores inseridos com sucesso!", fornecedores.Count);
    }

    /// <summary>
    /// Seed clientes brasileiros
    /// </summary>
    private async Task SeedClientes(string tenantId)
    {
        _logger.LogInformation("👥 Criando clientes brasileiros...");

        var clientesFaker = new Faker<ClienteEntity>("pt_BR")
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.TenantId, f => tenantId)
            .RuleFor(c => c.Nome, f => f.Person.FullName)
            .RuleFor(c => c.CPF, f => f.Random.Replace("###.###.###-##"))
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Nome))
            .RuleFor(c => c.Telefone, f => f.Phone.PhoneNumber("(##) #####-####"))
            .RuleFor(c => c.DataCadastro, f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now))
            .RuleFor(c => c.Ativo, f => f.Random.Bool(0.98f))
            .RuleFor(c => c.DataCriacao, (f, c) => c.DataCadastro)
            .RuleFor(c => c.DataAtualizacao, f => f.Date.Recent(180));

        var clientes = new List<ClienteEntity>();
        
        for (int i = 0; i < CLIENTES_COUNT; i += BATCH_SIZE)
        {
            var batchSize = Math.Min(BATCH_SIZE, CLIENTES_COUNT - i);
            var batch = clientesFaker.Generate(batchSize);
            
            clientes.AddRange(batch);

            if (i % 100 == 0)
            {
                _logger.LogInformation("Processando clientes: {Current}/{Total}", i, CLIENTES_COUNT);
            }
        }

        await _context.Clientes.AddRangeAsync(clientes);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} clientes inseridos com sucesso!", clientes.Count);
    }

    /// <summary>
    /// Seed usuários do sistema
    /// </summary>
    private async Task SeedUsuarios(string tenantId)
    {
        _logger.LogInformation("👤 Criando usuários do sistema...");

        var usuariosFaker = new Faker<UsuarioEntity>("pt_BR")
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.TenantId, f => tenantId)
            .RuleFor(u => u.Nome, f => f.Person.FullName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Nome))
            .RuleFor(u => u.Ativo, f => f.Random.Bool(0.90f))
            .RuleFor(u => u.DataCriacao, f => f.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now))
            .RuleFor(u => u.DataAtualizacao, f => f.Date.Recent(90));

        var usuarios = usuariosFaker.Generate(USUARIOS_COUNT);
        
        await _context.Usuarios.AddRangeAsync(usuarios);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} usuários criados com sucesso!", usuarios.Count);
    }

    /// <summary>
    /// Seed produtos farmacêuticos brasileiros
    /// </summary>
    private async Task SeedProdutos(string tenantId)
    {
        _logger.LogInformation("💊 Criando produtos farmacêuticos brasileiros...");

        // Buscar categorias e fornecedores para relacionamentos
        var categorias = await _context.Categorias.Where(c => c.TenantId == tenantId).ToListAsync();
        var fornecedores = await _context.Fornecedores.Where(f => f.TenantId == tenantId).ToListAsync();

        var produtosBrasileiros = new List<string>
        {
            "Dipirona 500mg", "Paracetamol 750mg", "Ibuprofeno 600mg", "Amoxicilina 500mg", "Azitromicina 500mg",
            "Omeprazol 20mg", "Losartana 50mg", "Sinvastatina 20mg", "Metformina 850mg", "Captopril 25mg",
            "Shampoo Anticaspa", "Protetor Solar FPS 60", "Vitamina D3 2000UI", "Whey Protein", "Creatina",
            "Leite Ninho 400g", "Café Pilão 500g", "Açúcar Cristal 1kg", "Arroz Tio João 5kg", "Feijão Carioca 1kg"
        };

        var produtosFaker = new Faker<ProdutoEntity>("pt_BR")
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.TenantId, f => tenantId)
            .RuleFor(p => p.Nome, f => f.PickRandom(produtosBrasileiros))
            .RuleFor(p => p.Descricao, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.CodigoBarras, f => f.Commerce.Ean13())
            .RuleFor(p => p.CodigoInterno, f => f.Random.AlphaNumeric(8))
            .RuleFor(p => p.PrecoVenda, f => f.Random.Decimal(5.00m, 500.00m))
            .RuleFor(p => p.PrecoCusto, (f, p) => p.PrecoVenda * 0.7m)
            .RuleFor(p => p.MargemLucro, (f, p) => (p.PrecoVenda - p.PrecoCusto) / p.PrecoCusto * 100)
            .RuleFor(p => p.UnidadeMedida, f => f.PickRandom("UN", "CX", "FR", "ML", "KG"))
            .RuleFor(p => p.EstoqueAtual, f => f.Random.Decimal(0, 1000))
            .RuleFor(p => p.EstoqueMinimo, f => f.Random.Decimal(5, 50))
            .RuleFor(p => p.CategoriaId, f => f.PickRandom(categorias)?.Id)
            .RuleFor(p => p.NCM, f => f.Random.Replace("####.##.##"))
            .RuleFor(p => p.CEST, f => f.Random.Replace("##.###.##"))
            .RuleFor(p => p.Origem, f => f.Random.Int(0, 8))
            .RuleFor(p => p.CST, f => f.PickRandom("000", "010", "020", "030", "040", "041", "050", "051", "060", "070"))
            .RuleFor(p => p.AliquotaICMS, f => f.PickRandom(0.00m, 7.00m, 12.00m, 18.00m, 25.00m))
            .RuleFor(p => p.VerticalType, f => "FARMACIA")
            .RuleFor(p => p.Ativo, f => f.Random.Bool(0.95f))
            .RuleFor(p => p.DataCriacao, f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now))
            .RuleFor(p => p.DataAtualizacao, f => f.Date.Recent(365));

        var produtos = new List<ProdutoEntity>();
        
        for (int i = 0; i < PRODUTOS_COUNT; i += BATCH_SIZE)
        {
            var batchSize = Math.Min(BATCH_SIZE, PRODUTOS_COUNT - i);
            var batch = produtosFaker.Generate(batchSize);
            
            produtos.AddRange(batch);

            if (i % 1000 == 0)
            {
                _logger.LogInformation("Processando produtos: {Current}/{Total}", i, PRODUTOS_COUNT);
            }
        }

        await _context.Produtos.AddRangeAsync(produtos);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} produtos inseridos com sucesso!", produtos.Count);
    }

    /// <summary>
    /// Seed movimentações de estoque
    /// </summary>
    private async Task SeedMovimentacoesEstoque(string tenantId)
    {
        _logger.LogInformation("📋 Criando movimentações de estoque...");

        var movimentacoesFaker = new Faker<MovimentacaoEstoqueEntity>("pt_BR")
            .RuleFor(m => m.Id, f => Guid.NewGuid())
            .RuleFor(m => m.TenantId, f => tenantId)
            .RuleFor(m => m.Nome, f => f.PickRandom("Entrada de Mercadoria", "Saída por Venda", "Ajuste de Estoque", "Perda/Avaria", "Transferência"))
            .RuleFor(m => m.Descricao, f => f.Lorem.Sentence(8))
            .RuleFor(m => m.Ativo, f => true)
            .RuleFor(m => m.DataCriacao, f => f.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now))
            .RuleFor(m => m.DataAtualizacao, f => f.Date.Recent(30));

        var movimentacoes = new List<MovimentacaoEstoqueEntity>();
        
        for (int i = 0; i < MOVIMENTACOES_ESTOQUE_COUNT; i += BATCH_SIZE)
        {
            var batchSize = Math.Min(BATCH_SIZE, MOVIMENTACOES_ESTOQUE_COUNT - i);
            var batch = movimentacoesFaker.Generate(batchSize);
            
            movimentacoes.AddRange(batch);

            if (i % 500 == 0)
            {
                _logger.LogInformation("Processando movimentações: {Current}/{Total}", i, MOVIMENTACOES_ESTOQUE_COUNT);
            }
        }

        await _context.MovimentacoesEstoque.AddRangeAsync(movimentacoes);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} movimentações inseridas com sucesso!", movimentacoes.Count);
    }

    /// <summary>
    /// Seed vendas brasileiras
    /// </summary>
    private async Task SeedVendas(string tenantId)
    {
        _logger.LogInformation("💰 Criando vendas brasileiras...");

        // Buscar clientes e usuários para relacionamentos
        var clientes = await _context.Clientes.Where(c => c.TenantId == tenantId).ToListAsync();
        var usuarios = await _context.Usuarios.Where(u => u.TenantId == tenantId).ToListAsync();

        var vendasFaker = new Faker<VendaEntity>("pt_BR")
            .RuleFor(v => v.Id, f => Guid.NewGuid())
            .RuleFor(v => v.TenantId, f => tenantId)
            .RuleFor(v => v.DataVenda, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now))
            .RuleFor(v => v.ValorTotal, f => f.Random.Decimal(10.00m, 2000.00m))
            .RuleFor(v => v.ValorDesconto, (f, v) => f.Random.Decimal(0, v.ValorTotal * 0.2m))
            .RuleFor(v => v.Status, f => f.PickRandom("FINALIZADA", "CANCELADA"))
            .RuleFor(v => v.DataCriacao, (f, v) => v.DataVenda)
            .RuleFor(v => v.DataAtualizacao, f => f.Date.Recent(30));

        var vendas = vendasFaker.Generate(VENDAS_COUNT);
        
        await _context.Vendas.AddRangeAsync(vendas);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} vendas criadas com sucesso!", vendas.Count);
    }
}