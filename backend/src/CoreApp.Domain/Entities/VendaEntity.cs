using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade Venda do sistema CoreApp
/// </summary>
public class VendaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public Guid ClienteId { get; set; }
    
    [Required]
    public decimal ValorTotal { get; set; }
    
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    
    [StringLength(50)]
    public string Status { get; set; } = "PENDENTE";

    // Implementação de ISoftDeletableEntity
    public bool Excluido { get; set; } = false;
    public DateTime? DataExclusao { get; set; }
    [StringLength(100)]
    public string? UsuarioExclusao { get; set; }
    [StringLength(500)]
    public string? MotivoExclusao { get; set; }

    public void MarkAsDeleted(string? usuarioId = null, string? motivo = null)
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        UsuarioExclusao = usuarioId;
        MotivoExclusao = motivo;
    }

    public void Restore()
    {
        Excluido = false;
        DataExclusao = null;
        UsuarioExclusao = null;
        MotivoExclusao = null;
    }

    // Implementação de IArchivableEntity
    public bool Arquivado { get; set; } = false;
    public DateTime? DataArquivamento { get; set; }
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
    }

    public bool IsConcluida()
    {
        return Status == "CONCLUIDA";
    }
}