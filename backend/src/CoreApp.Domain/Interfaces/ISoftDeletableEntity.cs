namespace CoreApp.Domain.Interfaces;

/// <summary>
/// Interface para entidades que suportam exclusão lógica (soft delete)
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Indica se a entidade foi excluída logicamente
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Data e hora da exclusão lógica
    /// </summary>
    DateTime? DataExclusao { get; set; }

    /// <summary>
    /// Usuário que fez a exclusão lógica
    /// </summary>
    string? UsuarioExclusao { get; set; }

    /// <summary>
    /// Motivo da exclusão (opcional)
    /// </summary>
    string? MotivoExclusao { get; set; }

    /// <summary>
    /// Marca a entidade como excluída logicamente
    /// </summary>
    /// <param name="usuarioId">ID do usuário que está excluindo</param>
    /// <param name="motivo">Motivo da exclusão</param>
    void MarkAsDeleted(string? usuarioId = null, string? motivo = null);

    /// <summary>
    /// Restaura a entidade excluída logicamente
    /// </summary>
    void Restore();
}