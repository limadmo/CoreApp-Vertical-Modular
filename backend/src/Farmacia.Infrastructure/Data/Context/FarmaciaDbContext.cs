using Farmacia.Domain.Entities;
using Farmacia.Domain.Entities.Archived;
using Farmacia.Domain.Entities.Configuration;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Farmacia.Application.Services;

namespace Farmacia.Infrastructure.Data.Context;

/// <summary>
/// Contexto principal do Entity Framework Core para o sistema farmacêutico brasileiro
/// Implementa isolamento automático multi-tenant via Global Query Filters
/// </summary>
/// <remarks>
/// Este contexto garante que todas as queries sejam automaticamente filtradas
/// por tenant (farmácia), proporcionando isolamento total de dados
/// </remarks>
public class FarmaciaDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public FarmaciaDbContext(DbContextOptions<FarmaciaDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    #region DbSets - Entidades Farmacêuticas

    /// <summary>
    /// Produtos farmacêuticos com classificação ANVISA
    /// </summary>
    public DbSet<ProdutoEntity> Produtos { get; set; }

    /// <summary>
    /// Vendas farmacêuticas com compliance brasileiro
    /// </summary>
    public DbSet<VendaEntity> Vendas { get; set; } = null!;

    /// <summary>
    /// Itens de venda farmacêutica
    /// </summary>
    public DbSet<ItemVendaEntity> ItensVenda { get; set; } = null!;

    /// <summary>
    /// Movimentações de estoque farmacêutico
    /// </summary>
    public DbSet<MovimentacaoEstoqueEntity> MovimentacoesEstoque { get; set; } = null!;

    /// <summary>
    /// Lotes de produtos farmacêuticos
    /// </summary>
    public DbSet<LoteEntity> Lotes { get; set; } = null!;

    /// <summary>
    /// Tipos de movimentação configuráveis
    /// </summary>
    public DbSet<TipoMovimentacaoEntity> TiposMovimentacao { get; set; } = null!;

    /// <summary>
    /// Classificações ANVISA configuráveis (apenas listas controladas)
    /// </summary>
    public DbSet<ClassificacaoAnvisaEntity> ClassificacoesAnvisa { get; set; } = null!;

    /// <summary>
    /// Status de estoque configuráveis
    /// </summary>
    public DbSet<StatusEstoqueEntity> StatusEstoque { get; set; } = null!;

    /// <summary>
    /// Formas de pagamento configuráveis
    /// </summary>
    public DbSet<FormaPagamentoEntity> FormasPagamento { get; set; } = null!;

    /// <summary>
    /// Status de pagamento configuráveis
    /// </summary>
    public DbSet<StatusPagamentoEntity> StatusPagamento { get; set; } = null!;

    /// <summary>
    /// Status de sincronização offline configuráveis
    /// </summary>
    public DbSet<StatusSincronizacaoEntity> StatusSincronizacao { get; set; } = null!;

    /// <summary>
    /// Promoções farmacêuticas
    /// </summary>
    public DbSet<PromocaoEntity> Promocoes { get; set; } = null!;

    /// <summary>
    /// Produtos incluídos em promoções
    /// </summary>
    public DbSet<PromocaoProdutoEntity> PromocoesProdutos { get; set; } = null!;

    /// <summary>
    /// Categorias incluídas em promoções
    /// </summary>
    public DbSet<PromocaoCategoriaEntity> PromocoesCategorias { get; set; } = null!;

    /// <summary>
    /// Formas de pagamento de vendas
    /// </summary>
    public DbSet<FormaPagamentoVendaEntity> FormasPagamentoVenda { get; set; } = null!;

    /// <summary>
    /// Clientes farmacêuticos
    /// </summary>
    public DbSet<ClienteEntity> Clientes { get; set; } = null!;

    /// <summary>
    /// Fornecedores farmacêuticos
    /// </summary>
    public DbSet<FornecedorEntity> Fornecedores { get; set; } = null!;

    /// <summary>
    /// Usuários do sistema multi-tenant
    /// </summary>
    public DbSet<UsuarioEntity> Usuarios { get; set; } = null!;

    /// <summary>
    /// Categorias de produtos
    /// </summary>
    public DbSet<CategoriaEntity> Categorias { get; set; } = null!;

    #endregion

    #region DbSets - Tabelas de Arquivo (_log)

    /// <summary>
    /// Vendas arquivadas com mais de 7 anos - Tabela: vendas_log
    /// Mantém histórico para compliance fiscal brasileiro
    /// </summary>
    public DbSet<VendaArquivada> VendasArquivadas { get; set; }

    /// <summary>
    /// Movimentações de estoque arquivadas com mais de 5 anos - Tabela: estoque_movimentacoes_log
    /// Preserva rastreabilidade farmacêutica e compliance ANVISA
    /// </summary>
    public DbSet<MovimentacaoEstoqueArquivada> MovimentacoesEstoqueArquivadas { get; set; }

    /// <summary>
    /// Clientes arquivados com mais de 10 anos - Tabela: clientes_log
    /// Compliance LGPD e relacionamento comercial histórico
    /// </summary>
    public DbSet<ClienteArquivado> ClientesArquivados { get; set; }

    /// <summary>
    /// Fornecedores arquivados com mais de 5 anos - Tabela: fornecedores_log
    /// Histórico comercial e fiscal
    /// </summary>
    public DbSet<FornecedorArquivado> FornecedoresArquivados { get; set; }

    /// <summary>
    /// Relatórios de execução de arquivamento - Para auditoria do processo
    /// </summary>
    public DbSet<RelatorioArquivamentoEntity> RelatoriosArquivamento { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações de entidades
        ApplyEntityConfigurations(modelBuilder);

        // Aplicar Global Query Filters para isolamento multi-tenant
        ApplyGlobalQueryFilters(modelBuilder);

        // Configurar índices para performance multi-tenant
        ConfigureMultiTenantIndexes(modelBuilder);

        // Configurar convenções brasileiras
        ConfigureBrazilianConventions(modelBuilder);
    }

    /// <summary>
    /// Aplica configurações específicas das entidades farmacêuticas
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // Configuração da entidade Produto
        modelBuilder.Entity<ProdutoEntity>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(e => e.Id);

            // Índices para performance
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("IX_Produtos_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.CodigoBarras })
                  .HasDatabaseName("IX_Produtos_TenantId_CodigoBarras")
                  .IsUnique()
                  .HasFilter("\"CodigoBarras\" IS NOT NULL");

            entity.HasIndex(e => new { e.TenantId, e.Nome })
                  .HasDatabaseName("IX_Produtos_TenantId_Nome");

            entity.HasIndex(e => e.RegistroAnvisa)
                  .HasDatabaseName("IX_Produtos_RegistroAnvisa");

            entity.HasIndex(e => e.ClassificacaoAnvisa)
                  .HasDatabaseName("IX_Produtos_ClassificacaoAnvisa");

            // Propriedades com configurações específicas
            entity.Property(e => e.PrecoCusto)
                  .HasPrecision(10, 2);

            entity.Property(e => e.PrecoVenda)
                  .HasPrecision(10, 2);

            entity.Property(e => e.MargemLucro)
                  .HasPrecision(5, 2);

            // Campos obrigatórios
            entity.Property(e => e.TenantId)
                  .IsRequired();

            entity.Property(e => e.Nome)
                  .IsRequired();

            // Configurar enum como string para melhor legibilidade
            entity.Property(e => e.ClassificacaoAnvisa)
                  .HasConversion<string>();

            // Campos calculados (não mapeados)
            entity.Ignore(e => e.IsControlado);
            entity.Ignore(e => e.TipoReceitaNecessaria);
        });

        // Configurações das entidades arquivadas (_log)
        ConfigurarEntidadesArquivadas(modelBuilder);

        // Configuração da entidade de relatórios de arquivamento
        ConfigurarRelatoriosArquivamento(modelBuilder);
    }

    /// <summary>
    /// Configura entidades arquivadas (tabelas _log) para compliance farmacêutico
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ConfigurarEntidadesArquivadas(ModelBuilder modelBuilder)
    {
        // Configuração VendaArquivada
        modelBuilder.Entity<VendaArquivada>(entity =>
        {
            entity.ToTable("vendas_log");
            entity.HasKey(e => e.Id);

            // Índices para consultas de auditoria fiscal
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("IX_VendasArquivadas_TenantId");

            entity.HasIndex(e => e.OriginalId)
                  .HasDatabaseName("IX_VendasArquivadas_OriginalId");

            entity.HasIndex(e => e.DataArquivamento)
                  .HasDatabaseName("IX_VendasArquivadas_DataArquivamento");

            entity.HasIndex(e => e.DataVenda)
                  .HasDatabaseName("IX_VendasArquivadas_DataVenda");

            entity.HasIndex(e => e.CpfCnpjCliente)
                  .HasDatabaseName("IX_VendasArquivadas_CpfCnpj")
                  .HasFilter("\"CpfCnpjCliente\" IS NOT NULL");

            entity.HasIndex(e => e.NumeroNotaFiscal)
                  .HasDatabaseName("IX_VendasArquivadas_NotaFiscal")
                  .HasFilter("\"NumeroNotaFiscal\" IS NOT NULL");

            // Propriedades com precision
            entity.Property(e => e.ValorTotal)
                  .HasPrecision(18, 2);

            // Campos obrigatórios
            entity.Property(e => e.TenantId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.UsuarioDeletou)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.MotivoArquivamento)
                  .IsRequired()
                  .HasMaxLength(500);
        });

        // Configuração MovimentacaoEstoqueArquivada
        modelBuilder.Entity<MovimentacaoEstoqueArquivada>(entity =>
        {
            entity.ToTable("estoque_movimentacoes_log");
            entity.HasKey(e => e.Id);

            // Índices para rastreabilidade farmacêutica
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_TenantId");

            entity.HasIndex(e => e.OriginalId)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_OriginalId");

            entity.HasIndex(e => e.DataArquivamento)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_DataArquivamento");

            entity.HasIndex(e => e.ProdutoId)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_ProdutoId");

            entity.HasIndex(e => e.DataMovimentacao)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_DataMovimentacao");

            entity.HasIndex(e => e.Lote)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_Lote")
                  .HasFilter("\"Lote\" IS NOT NULL");

            entity.HasIndex(e => e.EraControlado)
                  .HasDatabaseName("IX_MovimentacoesArquivadas_EraControlado");

            // Propriedades com precision para quantities
            entity.Property(e => e.Quantidade)
                  .HasPrecision(18, 3);

            entity.Property(e => e.SaldoAnterior)
                  .HasPrecision(18, 3);

            entity.Property(e => e.SaldoAtual)
                  .HasPrecision(18, 3);

            // Campos obrigatórios
            entity.Property(e => e.NomeProduto)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.TipoMovimentacao)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.NomeUsuario)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        // Configuração ClienteArquivado
        modelBuilder.Entity<ClienteArquivado>(entity =>
        {
            entity.ToTable("clientes_log");
            entity.HasKey(e => e.Id);

            // Índices para compliance LGPD e consultas
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("IX_ClientesArquivados_TenantId");

            entity.HasIndex(e => e.OriginalId)
                  .HasDatabaseName("IX_ClientesArquivados_OriginalId");

            entity.HasIndex(e => e.DataArquivamento)
                  .HasDatabaseName("IX_ClientesArquivados_DataArquivamento");

            entity.HasIndex(e => e.CpfCnpj)
                  .HasDatabaseName("IX_ClientesArquivados_CpfCnpj");

            entity.HasIndex(e => e.DataCadastro)
                  .HasDatabaseName("IX_ClientesArquivados_DataCadastro");

            entity.HasIndex(e => e.UtilizavaMedicamentosControlados)
                  .HasDatabaseName("IX_ClientesArquivados_MedicamentosControlados");

            // Propriedades com precision
            entity.Property(e => e.ValorTotalCompras)
                  .HasPrecision(18, 2);

            // Campos obrigatórios
            entity.Property(e => e.CpfCnpj)
                  .IsRequired()
                  .HasMaxLength(18);

            entity.Property(e => e.Nome)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.StatusCliente)
                  .IsRequired()
                  .HasMaxLength(50);
        });

        // Configuração FornecedorArquivado
        modelBuilder.Entity<FornecedorArquivado>(entity =>
        {
            entity.ToTable("fornecedores_log");
            entity.HasKey(e => e.Id);

            // Índices para auditoria comercial
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("IX_FornecedoresArquivados_TenantId");

            entity.HasIndex(e => e.OriginalId)
                  .HasDatabaseName("IX_FornecedoresArquivados_OriginalId");

            entity.HasIndex(e => e.DataArquivamento)
                  .HasDatabaseName("IX_FornecedoresArquivados_DataArquivamento");

            entity.HasIndex(e => e.Cnpj)
                  .HasDatabaseName("IX_FornecedoresArquivados_Cnpj");

            entity.HasIndex(e => e.DataCadastro)
                  .HasDatabaseName("IX_FornecedoresArquivados_DataCadastro");

            entity.HasIndex(e => e.ForneciaControlados)
                  .HasDatabaseName("IX_FornecedoresArquivados_ForneciaControlados");

            // Propriedades com precision
            entity.Property(e => e.ValorTotalCompras)
                  .HasPrecision(18, 2);

            entity.Property(e => e.AvaliacaoMedia)
                  .HasPrecision(3, 2);

            // Campos obrigatórios
            entity.Property(e => e.Cnpj)
                  .IsRequired()
                  .HasMaxLength(18);

            entity.Property(e => e.RazaoSocial)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.StatusFornecedor)
                  .IsRequired()
                  .HasMaxLength(50);
        });
    }

    /// <summary>
    /// Configura entidade de relatórios de arquivamento
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ConfigurarRelatoriosArquivamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RelatorioArquivamentoEntity>(entity =>
        {
            entity.ToTable("relatorios_arquivamento");
            entity.HasKey(e => e.Id);

            // Índices para consultas de auditoria
            entity.HasIndex(e => e.DataExecucao)
                  .HasDatabaseName("IX_RelatoriosArquivamento_DataExecucao");

            entity.HasIndex(e => e.IntegridadeVerificada)
                  .HasDatabaseName("IX_RelatoriosArquivamento_IntegridadeVerificada");

            // Campos obrigatórios
            entity.Property(e => e.Detalhes)
                  .IsRequired()
                  .HasColumnType("text"); // JSON no PostgreSQL
        });
    }

    /// <summary>
    /// Aplica Global Query Filters para isolamento automático multi-tenant
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Aplicar filtro global para todas as entidades que implementam ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Criar expressão: entity => entity.TenantId == _tenantService.GetCurrentTenantId()
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var tenantProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var currentTenant = Expression.Constant(_tenantService.GetCurrentTenantId());
                var filter = Expression.Equal(tenantProperty, currentTenant);
                var lambda = Expression.Lambda(filter, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Configura índices otimizados para consultas multi-tenant
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ConfigureMultiTenantIndexes(ModelBuilder modelBuilder)
    {
        // Para todas as entidades multi-tenant, criar índice no TenantId
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);
                
                // Índice principal no TenantId se não existe
                var tenantIndexName = $"IX_{entityType.GetTableName()}_TenantId";
                if (!entity.Metadata.GetIndexes().Any(i => i.GetDatabaseName() == tenantIndexName))
                {
                    entity.HasIndex(nameof(ITenantEntity.TenantId))
                          .HasDatabaseName(tenantIndexName);
                }
            }
        }
    }

    /// <summary>
    /// Configura convenções específicas para o mercado brasileiro
    /// </summary>
    /// <param name="modelBuilder">Builder do modelo EF Core</param>
    private void ConfigureBrazilianConventions(ModelBuilder modelBuilder)
    {
        // Configurar collation brasileira para campos de texto
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    property.SetCollation("pt_BR.utf8");
                }
            }
        }

        // Configurar timezone brasileiro para campos DateTime
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    // PostgreSQL: usar timestamptz para timezone awareness
                    property.SetColumnType("timestamptz");
                }
            }
        }
    }

    /// <summary>
    /// Override SaveChanges para aplicar regras multi-tenant e auditoria
    /// </summary>
    public override int SaveChanges()
    {
        ApplyTenantToNewEntities();
        ApplyAuditTrail();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync para aplicar regras multi-tenant e auditoria
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantToNewEntities();
        ApplyAuditTrail();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Aplica automaticamente o TenantId para novas entidades
    /// </summary>
    private void ApplyTenantToNewEntities()
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Aplicar TenantId automaticamente para novas entidades
                if (string.IsNullOrEmpty(entry.Entity.TenantId))
                {
                    entry.Entity.TenantId = currentTenantId;
                }
                // Validar se o TenantId corresponde ao tenant atual
                else if (entry.Entity.TenantId != currentTenantId)
                {
                    throw new InvalidOperationException(
                        $"Tentativa de criar entidade para tenant '{entry.Entity.TenantId}' " +
                        $"no contexto do tenant '{currentTenantId}'. " +
                        $"Possível tentativa de vazamento de dados entre farmácias.");
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // Prevenir alteração do TenantId
                if (entry.Property(nameof(ITenantEntity.TenantId)).IsModified)
                {
                    throw new InvalidOperationException(
                        $"TenantId não pode ser alterado. Tentativa de alterar de " +
                        $"'{entry.Property(nameof(ITenantEntity.TenantId)).OriginalValue}' para " +
                        $"'{entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue}'.");
                }
            }
        }
    }

    /// <summary>
    /// Aplica trilha de auditoria automática para entidades modificadas
    /// </summary>
    private void ApplyAuditTrail()
    {
        var currentUserId = _tenantService.GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Para entidades com campos de auditoria de criação
                    if (entry.Properties.Any(p => p.Metadata.Name == "DataCriacao"))
                    {
                        entry.Property("DataCriacao").CurrentValue = now;
                        entry.Property("DataAtualizacao").CurrentValue = now;
                    }
                    
                    if (entry.Properties.Any(p => p.Metadata.Name == "CriadoPor"))
                    {
                        entry.Property("CriadoPor").CurrentValue = currentUserId;
                        entry.Property("AtualizadoPor").CurrentValue = currentUserId;
                    }
                    break;

                case EntityState.Modified:
                    // Para entidades com campos de auditoria de atualização
                    if (entry.Properties.Any(p => p.Metadata.Name == "DataAtualizacao"))
                    {
                        entry.Property("DataAtualizacao").CurrentValue = now;
                        // Não alterar DataCriacao
                        entry.Property("DataCriacao").IsModified = false;
                    }
                    
                    if (entry.Properties.Any(p => p.Metadata.Name == "AtualizadoPor"))
                    {
                        entry.Property("AtualizadoPor").CurrentValue = currentUserId;
                        // Não alterar CriadoPor
                        if (entry.Properties.Any(p => p.Metadata.Name == "CriadoPor"))
                        {
                            entry.Property("CriadoPor").IsModified = false;
                        }
                    }
                    break;
            }
        }
    }
}