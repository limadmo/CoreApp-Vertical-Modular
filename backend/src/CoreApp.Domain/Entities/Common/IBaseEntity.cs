namespace CoreApp.Domain.Entities.Common;

/// <summary>
/// Interface base para todas as entidades do sistema CoreApp
/// Garante que todas as entidades tenham identificador único
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    Guid Id { get; set; }
}