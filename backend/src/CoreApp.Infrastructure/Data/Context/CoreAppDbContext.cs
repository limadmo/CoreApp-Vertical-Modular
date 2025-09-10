using System.Linq.Expressions;
using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Archived;
using CoreApp.Domain.Entities.Configuration;
using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace CoreApp.Infrastructure.Data.Context;

/// <summary>
/// Contexto principal do Entity Framework Core para o sistema CoreApp multi-tenant
/// Implementa isolamento automático via Global Query Filters
/// </summary>
public class CoreAppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public CoreAppDbContext(DbContextOptions<CoreAppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    #region DbSets - Entidades CoreApp

    public DbSet<ProdutoEntity> Produtos { get; set; } = null!;
    public DbSet<VendaEntity> Vendas { get; set; } = null!;
    public DbSet<ItemVendaEntity> ItensVenda { get; set; } = null!;
    public DbSet<MovimentacaoEstoqueEntity> MovimentacoesEstoque { get; set; } = null!;
    public DbSet<ClienteEntity> Clientes { get; set; } = null!;
    public DbSet<UsuarioEntity> Usuarios { get; set; } = null!;
    public DbSet<CategoriaEntity> Categorias { get; set; } = null!;
    public DbSet<FornecedorEntity> Fornecedores { get; set; } = null!;
    public DbSet<LoteEntity> Lotes { get; set; } = null!;
    public DbSet<PromocaoEntity> Promocoes { get; set; } = null!;
    public DbSet<PromocaoProdutoEntity> PromocoesProdutos { get; set; } = null!;
    public DbSet<PromocaoCategoriaEntity> PromocoesCategorias { get; set; } = null!;
    public DbSet<FormaPagamentoVendaEntity> FormasPagamentoVenda { get; set; } = null!;
    public DbSet<ModuloEntity> Modulos { get; set; } = null!;
    public DbSet<PlanoComercialEntity> PlanosComerciais { get; set; } = null!;
    public DbSet<PlanoModuloEntity> PlanosModulos { get; set; } = null!;
    public DbSet<TenantPlanoEntity> TenantsPlanos { get; set; } = null!;
    public DbSet<TenantModuloEntity> TenantsModulos { get; set; } = null!;
    public DbSet<RelatorioArquivamentoEntity> RelatoriosArquivamento { get; set; } = null!;
    public DbSet<TipoMovimentacaoEntity> TiposMovimentacao { get; set; } = null!;

    // Configurações
    public DbSet<ClassificacaoAnvisaEntity> ClassificacoesAnvisa { get; set; } = null!;
    public DbSet<FormaPagamentoEntity> FormasPagamento { get; set; } = null!;
    public DbSet<StatusEstoqueEntity> StatusEstoque { get; set; } = null!;
    public DbSet<StatusPagamentoEntity> StatusPagamento { get; set; } = null!;
    public DbSet<StatusSincronizacaoEntity> StatusSincronizacao { get; set; } = null!;

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas as configurações da assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreAppDbContext).Assembly);

        // Global Query Filters para Multi-tenant
        var tenantEntities = new[]
        {
            typeof(ProdutoEntity), typeof(VendaEntity), typeof(ItemVendaEntity),
            typeof(MovimentacaoEstoqueEntity), typeof(ClienteEntity), typeof(UsuarioEntity),
            typeof(CategoriaEntity), typeof(FornecedorEntity), typeof(LoteEntity),
            typeof(PromocaoEntity), typeof(PromocaoProdutoEntity), typeof(PromocaoCategoriaEntity),
            typeof(FormaPagamentoVendaEntity), typeof(ModuloEntity), typeof(PlanoComercialEntity),
            typeof(PlanoModuloEntity), typeof(TenantPlanoEntity), typeof(TenantModuloEntity),
            typeof(RelatorioArquivamentoEntity), typeof(TipoMovimentacaoEntity),
            typeof(ClassificacaoAnvisaEntity), typeof(FormaPagamentoEntity),
            typeof(StatusEstoqueEntity), typeof(StatusPagamentoEntity), typeof(StatusSincronizacaoEntity)
        };

        foreach (var entityType in tenantEntities)
        {
            // Aplicar filtro de tenant apenas se a entidade implementa ITenantEntity
            if (typeof(ITenantEntity).IsAssignableFrom(entityType))
            {
                modelBuilder.Entity(entityType).HasQueryFilter(
                    CreateTenantFilter(entityType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private LambdaExpression CreateTenantFilter(Type entityType)
    {
        var parameterType = entityType;
        var parameter = Expression.Parameter(parameterType, "e");
        var property = Expression.Property(parameter, "TenantId");
        var tenantId = Expression.Constant(_tenantContext.GetCurrentTenantId());
        var equal = Expression.Equal(property, tenantId);
        return Expression.Lambda(equal, parameter);
    }
}