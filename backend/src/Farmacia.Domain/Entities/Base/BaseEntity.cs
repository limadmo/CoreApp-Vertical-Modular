using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities.Base;

/// <summary>
/// Classe base para todas as entidades do sistema
/// Fornece propriedades comuns de auditoria e controle
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Data de criação da entidade
    /// </summary>
    [Required]
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }

    /// <summary>
    /// Identificador do usuário que criou a entidade
    /// </summary>
    [StringLength(100)]
    public string? UsuarioCriacao { get; set; }

    /// <summary>
    /// Identificador do usuário que fez a última atualização
    /// </summary>
    [StringLength(100)]
    public string? UsuarioAtualizacao { get; set; }

    /// <summary>
    /// Versão da entidade para controle de concorrência otimística
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Atualiza os campos de auditoria antes de salvar
    /// </summary>
    /// <param name="usuarioId">ID do usuário que está fazendo a operação</param>
    /// <param name="isUpdate">Se é uma operação de update (true) ou create (false)</param>
    public virtual void UpdateAuditFields(string? usuarioId = null, bool isUpdate = false)
    {
        if (isUpdate)
        {
            DataAtualizacao = DateTime.UtcNow;
            UsuarioAtualizacao = usuarioId;
        }
        else
        {
            DataCriacao = DateTime.UtcNow;
            UsuarioCriacao = usuarioId;
        }
    }
}