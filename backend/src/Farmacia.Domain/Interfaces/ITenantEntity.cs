namespace Farmacia.Domain.Interfaces;

/// <summary>
/// Interface base para todas as entidades multi-tenant farmacêuticas
/// Garante isolamento automático de dados entre farmácias
/// </summary>
/// <remarks>
/// Todas as entidades que implementam esta interface são automaticamente
/// filtradas por tenant através do EF Core Global Query Filters
/// </remarks>
public interface ITenantEntity
{
    /// <summary>
    /// Identificador único do tenant (farmácia)
    /// Usado para isolamento automático de dados
    /// </summary>
    /// <example>farmacia-sp-centro, farmacia-rj-copacabana</example>
    string TenantId { get; set; }
}