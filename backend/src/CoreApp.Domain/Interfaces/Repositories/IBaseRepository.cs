using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface base para todos os repositórios do sistema CoreApp
/// Fornece operações CRUD básicas com suporte multi-tenant
/// </summary>
/// <typeparam name="T">Tipo da entidade que implementa ITenantEntity</typeparam>
public interface IBaseRepository<T> where T : class, ITenantEntity
{
    /// <summary>
    /// Obtém uma entidade por ID considerando o tenant atual
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as entidades do tenant atual com paginação
    /// </summary>
    /// <param name="pageNumber">Número da página (baseado em 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de entidades</returns>
    Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta o total de registros do tenant atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Total de registros</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade adicionada</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    /// <param name="entity">Entidade a ser atualizada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade atualizada</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma entidade por ID (exclusão lógica se implementa ISoftDeletableEntity)
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="usuarioId">ID do usuário que está excluindo</param>
    /// <param name="motivo">Motivo da exclusão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se removido com sucesso</returns>
    Task<bool> DeleteAsync(Guid id, string? usuarioId = null, string? motivo = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe uma entidade com o ID especificado
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva todas as mudanças pendentes
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades afetadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}