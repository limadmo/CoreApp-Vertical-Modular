using System.ComponentModel.DataAnnotations;

namespace CoreApp.Domain.Entities.Common;

/// <summary>
/// Interface para entidades que suportam exclusão lógica (soft delete)
/// Permite manter dados para auditoria e compliance comercial
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Indica se a entidade foi excluída logicamente
    /// </summary>
    bool Excluido { get; set; }

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    DateTime? DataExclusao { get; set; }

    /// <summary>
    /// Usuário responsável pela exclusão
    /// </summary>
    [StringLength(100)]
    string? UsuarioExclusao { get; set; }

    /// <summary>
    /// Motivo da exclusão para auditoria
    /// </summary>
    [StringLength(500)]
    string? MotivoExclusao { get; set; }

    /// <summary>
    /// Marca a entidade como excluída logicamente
    /// </summary>
    /// <param name="usuarioId">ID do usuário que está excluindo</param>
    /// <param name="motivo">Motivo da exclusão</param>
    void MarkAsDeleted(string? usuarioId = null, string? motivo = null);

    /// <summary>
    /// Restaura uma entidade excluída logicamente
    /// </summary>
    void Restore();
}