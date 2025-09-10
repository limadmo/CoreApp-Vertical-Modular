using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de relação tenant-plano
/// Fornece operações para gestão de planos contratados por tenant
/// </summary>
public interface ITenantPlanoRepository : IBaseRepository<TenantPlanoEntity>
{
    /// <summary>
    /// Obtém o plano ativo atual de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Plano ativo do tenant ou null</returns>
    Task<TenantPlanoEntity?> GetPlanoAtivoTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém histórico de planos de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de planos do tenant</returns>
    Task<IEnumerable<TenantPlanoEntity>> GetHistoricoPlanosTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os tenants de um plano específico
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="apenasAtivos">Se deve retornar apenas contratos ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de tenants que usam o plano</returns>
    Task<IEnumerable<TenantPlanoEntity>> GetTenantsPorPlanoAsync(Guid planoId, bool apenasAtivos = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um tenant tem um plano ativo
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o tenant tem plano ativo</returns>
    Task<bool> TenantTemPlanoAtivoAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém planos próximos ao vencimento
    /// </summary>
    /// <param name="diasAntecedencia">Número de dias de antecedência</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de planos próximos ao vencimento</returns>
    Task<IEnumerable<TenantPlanoEntity>> GetPlanosProximosVencimentoAsync(int diasAntecedencia = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa um plano para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="planoId">ID do plano</param>
    /// <param name="dataInicio">Data de início do plano</param>
    /// <param name="dataFim">Data de fim do plano</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relação tenant-plano criada</returns>
    Task<TenantPlanoEntity> AtivarPlanoTenantAsync(string tenantId, Guid planoId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa o plano atual de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="motivoCancelamento">Motivo do cancelamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se desativado com sucesso</returns>
    Task<bool> DesativarPlanoTenantAsync(string tenantId, string? motivoCancelamento = null, CancellationToken cancellationToken = default);
}