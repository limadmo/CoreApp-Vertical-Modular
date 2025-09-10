using System.ComponentModel.DataAnnotations;
using CoreApp.Domain.Entities.Base;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade Categoria para sistema CoreApp multi-tenant
/// Implementa padrões SOLID e compliance brasileiro
/// </summary>
public class CategoriaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do tenant (loja/comércio) para isolamento de dados
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Nome/descrição principal da Categoria
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada opcional
    /// </summary>
    [StringLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Indica se o registro está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;

    // Implementação de ISoftDeletableEntity
    /// <summary>
    /// Indica se o registro foi excluído logicamente
    /// </summary>
    public bool Excluido { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DataExclusao { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão
    /// </summary>
    [StringLength(100)]
    public string? UsuarioExclusao { get; set; }

    /// <summary>
    /// Motivo da exclusão
    /// </summary>
    [StringLength(500)]
    public string? MotivoExclusao { get; set; }

    /// <summary>
    /// Marca o registro como excluído logicamente
    /// </summary>
    /// <param name="usuarioId">ID do usuário que está excluindo</param>
    /// <param name="motivo">Motivo da exclusão</param>
    public void MarkAsDeleted(string? usuarioId = null, string? motivo = null)
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        UsuarioExclusao = usuarioId;
        MotivoExclusao = motivo;
    }

    /// <summary>
    /// Restaura um registro excluído logicamente
    /// </summary>
    public void Restore()
    {
        Excluido = false;
        DataExclusao = null;
        UsuarioExclusao = null;
        MotivoExclusao = null;
    }

    // Implementação de IArchivableEntity
    /// <summary>
    /// Indica se o registro foi arquivado
    /// </summary>
    public bool Arquivado { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? DataArquivamento { get; set; }

    /// <summary>
    /// Data da última movimentação/alteração
    /// </summary>
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Atualiza a data da última movimentação
    /// </summary>
    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
    }
}
