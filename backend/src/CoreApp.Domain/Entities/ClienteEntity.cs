using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade Cliente do sistema CoreApp
/// </summary>
public class ClienteEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Nome { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? CPF { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? Telefone { get; set; }
    
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;

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

    public bool IsValido()
    {
        return !string.IsNullOrWhiteSpace(Nome) && Ativo;
    }
}