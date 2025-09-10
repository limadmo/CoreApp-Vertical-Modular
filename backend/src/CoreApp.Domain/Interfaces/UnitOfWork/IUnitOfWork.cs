using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Interfaces.Repositories;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.UnitOfWork;

/// <summary>
/// Interface principal para Unit of Work com controle transacional estado da arte
/// Coordena operações entre múltiplos repositórios e verticais de negócio
/// </summary>
/// <remarks>
/// Implementa padrão UoW conforme especificado no CLAUDE.md para coordenar
/// transações entre diferentes verticais sem SaveChanges direto nos repositórios
/// </remarks>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Obtém repositório específico por tipo da entidade
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade que implementa ITenantEntity</typeparam>
    /// <returns>Repositório configurado para a entidade especificada</returns>
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class, ITenantEntity;

    /// <summary>
    /// Obtém repositório específico para entidades com suporte a verticais
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade que implementa IVerticalEntity e ITenantEntity</typeparam>
    /// <returns>Repositório com funcionalidades verticais</returns>
    IVerticalRepository<TEntity> VerticalRepository<TEntity>() 
        where TEntity : class, IVerticalEntity, ITenantEntity;

    /// <summary>
    /// Inicia uma nova transação para operações coordenadas
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representing the operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma todas as mudanças pendentes na transação atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades afetadas</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma transação atual se existir
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representing the operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverte transação atual em caso de erro
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representing the operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa operação em transação isolada com rollback automático em caso de falha
    /// </summary>
    /// <typeparam name="T">Tipo do resultado da operação</typeparam>
    /// <param name="operation">Operação a ser executada na transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa operação em transação isolada sem retorno
    /// </summary>
    /// <param name="operation">Operação a ser executada na transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representing the operation</returns>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica se existe uma transação ativa
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Obtém estatísticas da sessão atual do UoW
    /// </summary>
    /// <returns>Estatísticas de operações realizadas</returns>
    UnitOfWorkStatistics GetStatistics();

    /// <summary>
    /// Limpa cache de repositórios e força refresh
    /// </summary>
    void ClearRepositoryCache();

    /// <summary>
    /// Evento disparado quando transação é iniciada
    /// </summary>
    event EventHandler<UnitOfWorkEventArgs>? TransactionStarted;

    /// <summary>
    /// Evento disparado quando transação é confirmada
    /// </summary>
    event EventHandler<UnitOfWorkEventArgs>? TransactionCommitted;

    /// <summary>
    /// Evento disparado quando transação é revertida
    /// </summary>
    event EventHandler<UnitOfWorkEventArgs>? TransactionRolledBack;
}

/// <summary>
/// Estatísticas de operações do Unit of Work
/// </summary>
public class UnitOfWorkStatistics
{
    public int EntitiesAdded { get; set; }
    public int EntitiesModified { get; set; }
    public int EntitiesDeleted { get; set; }
    public int QueriesExecuted { get; set; }
    public int TransactionsStarted { get; set; }
    public int TransactionsCommitted { get; set; }
    public int TransactionsRolledBack { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public DateTime SessionStarted { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total de operações realizadas na sessão
    /// </summary>
    public int TotalOperations => EntitiesAdded + EntitiesModified + EntitiesDeleted;
    
    /// <summary>
    /// Indica se houve modificações pendentes
    /// </summary>
    public bool HasPendingChanges => TotalOperations > 0;
}

/// <summary>
/// Argumentos para eventos do Unit of Work
/// </summary>
public class UnitOfWorkEventArgs : EventArgs
{
    public string TransactionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}