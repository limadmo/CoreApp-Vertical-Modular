using CoreApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApp.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade ProdutoEntity
/// </summary>
public class ProdutoConfiguration : IEntityTypeConfiguration<ProdutoEntity>
{
    public void Configure(EntityTypeBuilder<ProdutoEntity> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.CodigoBarras)
            .HasMaxLength(20);

        builder.Property(p => p.CodigoInterno)
            .HasMaxLength(50);

        builder.Property(p => p.PrecoVenda)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PrecoCusto)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.MargemLucro)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(p => p.EstoqueAtual)
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.Property(p => p.EstoqueMinimo)
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.Property(p => p.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("UN");

        builder.Property(p => p.NCM)
            .HasMaxLength(20);

        builder.Property(p => p.CEST)
            .HasMaxLength(20);

        builder.Property(p => p.CST)
            .HasMaxLength(5);

        builder.Property(p => p.AliquotaICMS)
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        // Configurações Vertical
        builder.Property(p => p.VerticalType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("GENERICO");

        builder.Property(p => p.VerticalProperties)
            .HasColumnType("jsonb");

        builder.Property(p => p.VerticalConfiguration)
            .HasColumnType("jsonb");

        builder.Property(p => p.VerticalSchemaVersion)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("1.0");

        builder.Property(p => p.VerticalActive)
            .HasDefaultValue(true);

        // Configurações soft delete
        builder.Property(p => p.Excluido)
            .HasDefaultValue(false);

        builder.Property(p => p.MotivoExclusao)
            .HasMaxLength(500);

        builder.Property(p => p.UsuarioExclusao)
            .HasMaxLength(100);

        // Configurações archivable
        builder.Property(p => p.Arquivado)
            .HasDefaultValue(false);

        builder.Property(p => p.UltimaMovimentacao)
            .HasDefaultValueSql("NOW()");

        // Relacionamentos
        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(p => new { p.TenantId, p.CodigoBarras })
            .HasDatabaseName("IX_Produtos_TenantId_CodigoBarras");

        builder.HasIndex(p => new { p.TenantId, p.CodigoInterno })
            .HasDatabaseName("IX_Produtos_TenantId_CodigoInterno");

        builder.HasIndex(p => new { p.TenantId, p.Nome })
            .HasDatabaseName("IX_Produtos_TenantId_Nome");

        builder.HasIndex(p => new { p.TenantId, p.VerticalType })
            .HasDatabaseName("IX_Produtos_TenantId_VerticalType");

        builder.HasIndex(p => new { p.TenantId, p.Ativo })
            .HasDatabaseName("IX_Produtos_TenantId_Ativo");

        builder.HasIndex(p => new { p.TenantId, p.Excluido })
            .HasDatabaseName("IX_Produtos_TenantId_Excluido");

        // Query Filter para soft delete
        builder.HasQueryFilter(p => !p.Excluido);
    }
}