using Farmacia.Domain.Entities;

namespace Farmacia.Domain.Interfaces;

/// <summary>
/// Interface do repositório de módulos do sistema farmacêutico brasileiro
/// Gerencia módulos funcionais disponíveis no sistema SAAS
/// </summary>
/// <remarks>
/// Este repositório é responsável por gerenciar os módulos que podem ser
/// ativados/desativados conforme os planos comerciais contratados
/// </remarks>
public interface IModuloRepository : IBaseRepository<ModuloEntity>
{
    /// <summary>
    /// Obtém módulo por código único
    /// </summary>
    /// <param name="codigo">Código do módulo</param>
    /// <returns>Módulo encontrado ou null</returns>
    Task<ModuloEntity?> GetByCodeAsync(string codigo);

    /// <summary>
    /// Obtém múltiplos módulos por códigos
    /// </summary>
    /// <param name="codigos">Lista de códigos</param>
    /// <returns>Lista de módulos encontrados</returns>
    Task<List<ModuloEntity>> GetByCodesAsync(IEnumerable<string> codigos);

    /// <summary>
    /// Obtém módulos por categoria
    /// </summary>
    /// <param name="categoria">Categoria dos módulos</param>
    /// <returns>Lista de módulos da categoria</returns>
    Task<List<ModuloEntity>> GetByCategoryAsync(string categoria);

    /// <summary>
    /// Obtém todos os módulos ativos
    /// </summary>
    /// <returns>Lista de módulos ativos</returns>
    Task<List<ModuloEntity>> GetActiveAsync();

    /// <summary>
    /// Obtém módulos essenciais (não podem ser desabilitados)
    /// </summary>
    /// <returns>Lista de módulos essenciais</returns>
    Task<List<ModuloEntity>> GetEssentialAsync();

    /// <summary>
    /// Conta total de módulos ativos
    /// </summary>
    /// <returns>Número de módulos ativos</returns>
    Task<int> CountActiveAsync();

    /// <summary>
    /// Verifica se código de módulo já existe
    /// </summary>
    /// <param name="codigo">Código a verificar</param>
    /// <param name="excludeId">ID a excluir da verificação (para updates)</param>
    /// <returns>True se código já existe</returns>
    Task<bool> ExistsCodeAsync(string codigo, Guid? excludeId = null);

    /// <summary>
    /// Obtém módulos ordenados por categoria e ordem de exibição
    /// </summary>
    /// <returns>Lista de módulos ordenados</returns>
    Task<List<ModuloEntity>> GetOrderedByCategoryAsync();

    /// <summary>
    /// Busca módulos por texto (nome ou descrição)
    /// </summary>
    /// <param name="searchTerm">Termo de busca</param>
    /// <returns>Lista de módulos encontrados</returns>
    Task<List<ModuloEntity>> SearchAsync(string searchTerm);
}

/// <summary>
/// Interface do repositório de planos comerciais
/// Gerencia os planos disponíveis para contratação pelas farmácias
/// </summary>
public interface IPlanoComercialRepository : IBaseRepository<PlanoComercialEntity>
{
    /// <summary>
    /// Obtém plano por código único
    /// </summary>
    /// <param name="codigo">Código do plano</param>
    /// <returns>Plano encontrado ou null</returns>
    Task<PlanoComercialEntity?> GetByCodeAsync(string codigo);

    /// <summary>
    /// Obtém planos ativos para contratação
    /// </summary>
    /// <returns>Lista de planos disponíveis</returns>
    Task<List<PlanoComercialEntity>> GetActiveForContractAsync();

    /// <summary>
    /// Obtém planos ordenados por preço
    /// </summary>
    /// <param name="ascending">True para ordem crescente</param>
    /// <returns>Lista de planos ordenados por preço</returns>
    Task<List<PlanoComercialEntity>> GetOrderedByPriceAsync(bool ascending = true);

    /// <summary>
    /// Obtém plano com seus módulos incluídos
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <returns>Plano com módulos carregados</returns>
    Task<PlanoComercialEntity?> GetWithModulesAsync(Guid planoId);

    /// <summary>
    /// Obtém planos promocionais válidos
    /// </summary>
    /// <returns>Lista de planos promocionais ativos</returns>
    Task<List<PlanoComercialEntity>> GetActivePromotionalAsync();

    /// <summary>
    /// Verifica se código de plano já existe
    /// </summary>
    /// <param name="codigo">Código a verificar</param>
    /// <param name="excludeId">ID a excluir da verificação</param>
    /// <returns>True se código já existe</returns>
    Task<bool> ExistsCodeAsync(string codigo, Guid? excludeId = null);
}

/// <summary>
/// Interface do repositório de associações entre planos e módulos
/// </summary>
public interface IPlanoModuloRepository : IBaseRepository<PlanoModuloEntity>
{
    /// <summary>
    /// Obtém associações por plano
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <returns>Lista de associações ativas</returns>
    Task<List<PlanoModuloEntity>> GetByPlanoAsync(Guid planoId);

    /// <summary>
    /// Obtém planos que incluem um módulo específico
    /// </summary>
    /// <param name="moduloId">ID do módulo</param>
    /// <returns>Lista de associações ativas</returns>
    Task<List<PlanoModuloEntity>> GetByModuloAsync(Guid moduloId);

    /// <summary>
    /// Obtém associação específica entre plano e módulo
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <returns>Associação encontrada ou null</returns>
    Task<PlanoModuloEntity?> GetByPlanoAndModuloAsync(Guid planoId, Guid moduloId);

    /// <summary>
    /// Remove todos os módulos de um plano
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    Task RemoveAllByPlanoAsync(Guid planoId);

    /// <summary>
    /// Remove associação específica
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="moduloId">ID do módulo</param>
    Task RemoveByPlanoAndModuloAsync(Guid planoId, Guid moduloId);
}

/// <summary>
/// Interface do repositório de contratações de planos pelos tenants
/// </summary>
public interface ITenantPlanoRepository : IBaseRepository<TenantPlanoEntity>
{
    /// <summary>
    /// Obtém contratação ativa de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Contratação ativa ou null</returns>
    Task<TenantPlanoEntity?> GetActiveByTenantAsync(string tenantId);

    /// <summary>
    /// Obtém histórico de contratações de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de contratações ordenadas por data</returns>
    Task<List<TenantPlanoEntity>> GetHistoryByTenantAsync(string tenantId);

    /// <summary>
    /// Obtém contratações que vencem em X dias
    /// </summary>
    /// <param name="days">Número de dias para vencimento</param>
    /// <returns>Lista de contratações próximas do vencimento</returns>
    Task<List<TenantPlanoEntity>> GetExpiringInDaysAsync(int days);

    /// <summary>
    /// Obtém contratações vencidas
    /// </summary>
    /// <returns>Lista de contratações vencidas</returns>
    Task<List<TenantPlanoEntity>> GetExpiredAsync();

    /// <summary>
    /// Obtém contratações em período de teste
    /// </summary>
    /// <returns>Lista de contratações em teste</returns>
    Task<List<TenantPlanoEntity>> GetTrialPeriodsAsync();

    /// <summary>
    /// Obtém contratações por plano
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <returns>Lista de contratações do plano</returns>
    Task<List<TenantPlanoEntity>> GetByPlanoAsync(Guid planoId);

    /// <summary>
    /// Conta contratações ativas por plano
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <returns>Número de contratações ativas</returns>
    Task<int> CountActiveByPlanoAsync(Guid planoId);

    /// <summary>
    /// Obtém contratação com informações do plano carregadas
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Contratação com plano carregado</returns>
    Task<TenantPlanoEntity?> GetWithPlanByTenantAsync(string tenantId);

    /// <summary>
    /// Cancela todas as contratações ativas de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="usuario">Usuário que cancelou</param>
    Task CancelAllActiveByTenantAsync(string tenantId, string motivo, string usuario);
}

/// <summary>
/// Interface do repositório de módulos específicos dos tenants
/// </summary>
public interface ITenantModuloRepository : IBaseRepository<TenantModuloEntity>
{
    /// <summary>
    /// Obtém módulos ativos de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de módulos ativos</returns>
    Task<List<TenantModuloEntity>> GetActiveByTenantAsync(string tenantId);

    /// <summary>
    /// Obtém todos os módulos de um tenant (ativos e inativos)
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de todos os módulos do tenant</returns>
    Task<List<TenantModuloEntity>> GetAllByTenantAsync(string tenantId);

    /// <summary>
    /// Obtém associação específica entre tenant e módulo
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <returns>Associação encontrada ou null</returns>
    Task<TenantModuloEntity?> GetByTenantAndModuleAsync(string tenantId, Guid moduloId);

    /// <summary>
    /// Obtém tenants que têm um módulo específico ativo
    /// </summary>
    /// <param name="moduloId">ID do módulo</param>
    /// <returns>Lista de associações ativas</returns>
    Task<List<TenantModuloEntity>> GetActiveByModuleAsync(Guid moduloId);

    /// <summary>
    /// Conta quantos tenants têm um módulo ativo
    /// </summary>
    /// <param name="moduloId">ID do módulo</param>
    /// <returns>Número de tenants com módulo ativo</returns>
    Task<int> CountActiveByModuleAsync(Guid moduloId);

    /// <summary>
    /// Remove todos os módulos de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    Task RemoveAllByTenantAsync(string tenantId);

    /// <summary>
    /// Ativa/desativa módulo específico para tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <param name="ativo">Se deve estar ativo</param>
    /// <param name="usuario">Usuário que fez a alteração</param>
    /// <param name="motivo">Motivo da alteração</param>
    Task SetModuleStatusAsync(string tenantId, Guid moduloId, bool ativo, string usuario, string? motivo = null);

    /// <summary>
    /// Sincroniza módulos do tenant com módulos do plano contratado
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="modulosPlano">Lista de códigos dos módulos do plano</param>
    /// <param name="usuario">Usuário que está sincronizando</param>
    Task SyncWithPlanModulesAsync(string tenantId, List<string> modulosPlano, string usuario);
}

/// <summary>
/// Interface base para todos os repositórios
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Obtém entidade por ID
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    /// <returns>Lista de todas as entidades</returns>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// Adiciona nova entidade
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada</param>
    /// <returns>Entidade adicionada</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Atualiza entidade existente
    /// </summary>
    /// <param name="entity">Entidade a ser atualizada</param>
    /// <returns>Entidade atualizada</returns>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Remove entidade por ID
    /// </summary>
    /// <param name="id">ID da entidade</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Remove entidade
    /// </summary>
    /// <param name="entity">Entidade a ser removida</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Verifica se entidade existe
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Conta total de registros
    /// </summary>
    /// <returns>Número total de registros</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Obtém entidades paginadas
    /// </summary>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Lista paginada de entidades</returns>
    Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize);
}