using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de relação tenant-módulo
/// Fornece operações para gestão de módulos específicos por tenant
/// </summary>
public interface ITenantModuloRepository : IBaseRepository<TenantModuloEntity>
{
    /// <summary>
    /// Obtém todos os módulos ativos de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos ativos do tenant</returns>
    Task<IEnumerable<TenantModuloEntity>> GetModulosAtivosTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um tenant tem acesso a um módulo específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o tenant tem acesso ao módulo</returns>
    Task<bool> TenantTemAcessoModuloAsync(string tenantId, string codigoModulo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém tenants que usam um módulo específico
    /// </summary>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="apenasAtivos">Se deve retornar apenas acessos ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de tenants que usam o módulo</returns>
    Task<IEnumerable<TenantModuloEntity>> GetTenantsPorModuloAsync(string codigoModulo, bool apenasAtivos = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa um módulo para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <param name="dataInicio">Data de início do acesso</param>
    /// <param name="dataFim">Data de fim do acesso (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relação tenant-módulo criada</returns>
    Task<TenantModuloEntity> AtivarModuloTenantAsync(string tenantId, Guid moduloId, DateTime dataInicio, DateTime? dataFim = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa um módulo para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="motivoDesativacao">Motivo da desativação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se desativado com sucesso</returns>
    Task<bool> DesativarModuloTenantAsync(string tenantId, string codigoModulo, string? motivoDesativacao = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sincroniza módulos de um tenant baseado no seu plano atual
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de módulos sincronizados</returns>
    Task<IEnumerable<TenantModuloEntity>> SincronizarModulosPorPlanoAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas de uso de módulos por tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas de uso dos módulos</returns>
    Task<Dictionary<string, object>> GetEstatisticasUsoModulosAsync(string tenantId, CancellationToken cancellationToken = default);
}