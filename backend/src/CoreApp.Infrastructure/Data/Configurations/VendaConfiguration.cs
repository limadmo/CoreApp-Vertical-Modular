using CoreApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApp.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade VendaEntity
/// </summary>
public class VendaConfiguration : IEntityTypeConfiguration<VendaEntity>
{
    public void Configure(EntityTypeBuilder<VendaEntity> builder)
    {
        builder.ToTable("Vendas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.NumeroVenda)
            .IsRequired();

        builder.Property(v => v.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.DataVenda)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Valores monetários
        builder.Property(v => v.ValorProdutos)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.ValorDesconto)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(v => v.ValorImpostos)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(v => v.ValorTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.ValorPago)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(v => v.ValorTroco)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        // Status e tipo
        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("PENDENTE");

        builder.Property(v => v.TipoVenda)
            .HasMaxLength(20)
            .HasDefaultValue("BALCAO");

        // Campos opcionais
        builder.Property(v => v.Observacoes)
            .HasMaxLength(1000);

        builder.Property(v => v.NumeroNFCe)
            .HasMaxLength(50);

        builder.Property(v => v.ChaveNFCe)
            .HasMaxLength(100);

        builder.Property(v => v.MotivoCancelamento)
            .HasMaxLength(500);

        // Configurações soft delete
        builder.Property(v => v.Excluido)
            .HasDefaultValue(false);

        builder.Property(v => v.MotivoExclusao)
            .HasMaxLength(500);

        builder.Property(v => v.UsuarioExclusao)
            .HasMaxLength(100);

        // Configurações archivable
        builder.Property(v => v.Arquivado)
            .HasDefaultValue(false);

        builder.Property(v => v.UltimaMovimentacao)
            .HasDefaultValueSql("NOW()");

        // Relacionamentos
        builder.HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.UsuarioVenda)
            .WithMany()
            .HasForeignKey(v => v.UsuarioVendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Itens)
            .WithOne(i => i.Venda)
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.FormasPagamento)
            .WithOne()
            .HasForeignKey("VendaId")
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(v => new { v.TenantId, v.NumeroVenda })
            .IsUnique()
            .HasDatabaseName("IX_Vendas_TenantId_NumeroVenda");

        builder.HasIndex(v => new { v.TenantId, v.DataVenda })
            .HasDatabaseName("IX_Vendas_TenantId_DataVenda");

        builder.HasIndex(v => new { v.TenantId, v.Status })
            .HasDatabaseName("IX_Vendas_TenantId_Status");

        builder.HasIndex(v => new { v.TenantId, v.ClienteId })
            .HasDatabaseName("IX_Vendas_TenantId_ClienteId");

        builder.HasIndex(v => new { v.TenantId, v.UsuarioVendaId })
            .HasDatabaseName("IX_Vendas_TenantId_UsuarioVendaId");

        builder.HasIndex(v => new { v.TenantId, v.Excluido })
            .HasDatabaseName("IX_Vendas_TenantId_Excluido");

        // Query Filter para soft delete
        builder.HasQueryFilter(v => !v.Excluido);
    }
}