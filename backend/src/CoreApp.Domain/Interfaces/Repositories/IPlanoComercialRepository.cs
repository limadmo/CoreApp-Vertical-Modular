using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de planos comerciais
/// Fornece operações para gestão de planos SAAS
/// </summary>
public interface IPlanoComercialRepository : IBaseRepository<PlanoComercialEntity>
{
    /// <summary>
    /// Obtém plano por código
    /// </summary>
    /// <param name="codigo">Código do plano (STARTER, PROFESSIONAL, ENTERPRISE)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Plano encontrado ou null</returns>
    Task<PlanoComercialEntity?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os planos ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de planos ativos</returns>
    Task<IEnumerable<PlanoComercialEntity>> GetPlanosAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém planos ordenados por preço
    /// </summary>
    /// <param name="crescente">True para ordem crescente, false para decrescente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de planos ordenados por preço</returns>
    Task<IEnumerable<PlanoComercialEntity>> GetPlanosOrdenadosPorPrecoAsync(bool crescente = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém módulos incluídos em um plano
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos do plano</returns>
    Task<IEnumerable<PlanoModuloEntity>> GetModulosPlanoAsync(Guid planoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um plano inclui um módulo específico
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o plano inclui o módulo</returns>
    Task<bool> PlanoIncluiModuloAsync(Guid planoId, string codigoModulo, CancellationToken cancellationToken = default);
}