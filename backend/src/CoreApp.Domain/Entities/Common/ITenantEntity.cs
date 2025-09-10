using System.ComponentModel.DataAnnotations;

namespace CoreApp.Domain.Entities.Common;

/// <summary>
/// Interface para entidades que pertencem a um tenant específico
/// Garante isolamento de dados entre diferentes comércios/lojas
/// </summary>
public interface ITenantEntity : IBaseEntity
{
    /// <summary>
    /// Identificador do tenant (loja/comércio) ao qual a entidade pertence
    /// </summary>
    [Required]
    [StringLength(100)]
    string TenantId { get; set; }
}