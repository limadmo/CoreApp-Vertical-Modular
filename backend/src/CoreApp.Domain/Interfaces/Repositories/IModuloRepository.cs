using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de módulos do sistema
/// Fornece operações para gestão de módulos e validação de acesso
/// </summary>
public interface IModuloRepository : IBaseRepository<ModuloEntity>
{
    /// <summary>
    /// Obtém módulo por código
    /// </summary>
    /// <param name="codigo">Código do módulo (ex: PRODUCTS, SALES)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Módulo encontrado ou null</returns>
    Task<ModuloEntity?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os módulos ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos ativos</returns>
    Task<IEnumerable<ModuloEntity>> GetModulosAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém módulos por plano comercial
    /// </summary>
    /// <param name="planoId">ID do plano comercial</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos do plano</returns>
    Task<IEnumerable<ModuloEntity>> GetByPlanoIdAsync(Guid planoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um módulo está ativo para um tenant específico
    /// </summary>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o módulo está ativo para o tenant</returns>
    Task<bool> ModuloAtivoParaTenantAsync(string codigoModulo, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os módulos disponíveis para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos do tenant</returns>
    Task<IEnumerable<ModuloEntity>> GetModulosTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}