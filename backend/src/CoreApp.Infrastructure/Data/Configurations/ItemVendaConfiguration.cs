using CoreApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApp.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade ItemVendaEntity
/// </summary>
public class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVendaEntity>
{
    public void Configure(EntityTypeBuilder<ItemVendaEntity> builder)
    {
        builder.ToTable("ItensVenda");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.VendaId)
            .IsRequired();

        builder.Property(i => i.ProdutoId)
            .IsRequired();

        builder.Property(i => i.Quantidade)
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.Property(i => i.PrecoUnitario)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.ValorTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.ValorDesconto)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(i => i.PercentualDesconto)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(i => i.NomeProduto)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.CodigoProduto)
            .HasMaxLength(50);

        builder.Property(i => i.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("UN");

        builder.Property(i => i.Observacoes)
            .HasMaxLength(500);

        // Relacionamentos
        builder.HasOne(i => i.Venda)
            .WithMany(v => v.Itens)
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(i => new { i.TenantId, i.VendaId })
            .HasDatabaseName("IX_ItensVenda_TenantId_VendaId");

        builder.HasIndex(i => new { i.TenantId, i.ProdutoId })
            .HasDatabaseName("IX_ItensVenda_TenantId_ProdutoId");

        builder.HasIndex(i => new { i.VendaId, i.ProdutoId })
            .HasDatabaseName("IX_ItensVenda_VendaId_ProdutoId");
    }
}