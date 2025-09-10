using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Interfaces.UnitOfWork;
using CoreApp.Domain.Interfaces.Repositories;
using CoreApp.Domain.Entities.Common;
using CoreApp.Infrastructure.Data.Context;
using CoreApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CoreApp.Infrastructure.UnitOfWork;

/// <summary>
/// Implementação estado da arte do Unit of Work para coordenação transacional completa
/// Gerencia repositórios, transações e operações entre múltiplos verticais de negócio
/// </summary>
/// <remarks>
/// Implementa padrão UoW conforme CLAUDE.md para evitar SaveChanges direto nos repositórios
/// e coordenar operações entre diferentes verticais com controle transacional robusto
/// </remarks>
public class UnitOfWork : IUnitOfWork
{
    private readonly CoreAppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly Stopwatch _stopwatch;
    private readonly UnitOfWorkStatistics _statistics;
    
    // Cache thread-safe para repositórios
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private readonly ConcurrentDictionary<Type, object> _verticalRepositories = new();
    
    // Controle de transação
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        CoreAppDbContext context, 
        ILogger<UnitOfWork> logger,
        ITenantContext tenantContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _stopwatch = Stopwatch.StartNew();
        _statistics = new UnitOfWorkStatistics();

        _logger.LogDebug("UnitOfWork inicializado para tenant: {TenantId}", 
            _tenantContext.GetCurrentTenantId());
    }

    /// <summary>
    /// Obtém repositório base com cache automático
    /// </summary>
    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class, ITenantEntity
    {
        var entityType = typeof(TEntity);
        
        return (IBaseRepository<TEntity>)_repositories.GetOrAdd(entityType, _ =>
        {
            _logger.LogDebug("Criando repositório base para entidade: {EntityType}", entityType.Name);
            return new BaseRepository<TEntity>(_context, _logger);
        });
    }

    /// <summary>
    /// Obtém repositório vertical com cache automático
    /// </summary>
    public IVerticalRepository<TEntity> VerticalRepository<TEntity>() 
        where TEntity : class, IVerticalEntity, ITenantEntity
    {
        var entityType = typeof(TEntity);
        
        return (IVerticalRepository<TEntity>)_verticalRepositories.GetOrAdd(entityType, _ =>
        {
            _logger.LogDebug("Criando repositório vertical para entidade: {EntityType}", entityType.Name);
            return new VerticalRepository<TEntity>(_context, _logger, _tenantContext);
        });
    }

    /// <summary>
    /// Inicia nova transação com isolamento adequado
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            _logger.LogWarning("Tentativa de iniciar transação quando já existe uma ativa");
            return;
        }

        try
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _statistics.TransactionsStarted++;
            
            var transactionId = _currentTransaction.TransactionId.ToString();
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            _logger.LogInformation("Transação iniciada - ID: {TransactionId}, Tenant: {TenantId}", 
                transactionId, tenantId);

            TransactionStarted?.Invoke(this, new UnitOfWorkEventArgs
            {
                TransactionId = transactionId,
                TenantId = tenantId,
                Metadata = { ["Type"] = "BeginTransaction" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar transação para tenant: {TenantId}", 
                _tenantContext.GetCurrentTenantId());
            throw;
        }
    }

    /// <summary>
    /// Confirma mudanças pendentes com auditoria completa
    /// </summary>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Captura estatísticas antes do commit
            var changeTracker = _context.ChangeTracker;
            var addedEntities = changeTracker.Entries().Count(e => e.State == EntityState.Added);
            var modifiedEntities = changeTracker.Entries().Count(e => e.State == EntityState.Modified);
            var deletedEntities = changeTracker.Entries().Count(e => e.State == EntityState.Deleted);

            var tenantId = _tenantContext.GetCurrentTenantId();
            
            _logger.LogInformation(
                "Iniciando commit - Tenant: {TenantId}, Adicionadas: {Added}, Modificadas: {Modified}, Removidas: {Deleted}",
                tenantId, addedEntities, modifiedEntities, deletedEntities);

            var affectedRows = await _context.SaveChangesAsync(cancellationToken);

            // Atualiza estatísticas
            _statistics.EntitiesAdded += addedEntities;
            _statistics.EntitiesModified += modifiedEntities;
            _statistics.EntitiesDeleted += deletedEntities;
            _statistics.QueriesExecuted++;

            _logger.LogInformation("Commit concluído - {AffectedRows} linhas afetadas para tenant: {TenantId}", 
                affectedRows, tenantId);

            return affectedRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante commit para tenant: {TenantId}", 
                _tenantContext.GetCurrentTenantId());
            throw;
        }
    }

    /// <summary>
    /// Confirma transação atual se existir
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("Tentativa de commit de transação quando nenhuma está ativa");
            return;
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            _statistics.TransactionsCommitted++;
            
            var transactionId = _currentTransaction.TransactionId.ToString();
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            _logger.LogInformation("Transação confirmada - ID: {TransactionId}, Tenant: {TenantId}", 
                transactionId, tenantId);

            TransactionCommitted?.Invoke(this, new UnitOfWorkEventArgs
            {
                TransactionId = transactionId,
                TenantId = tenantId,
                Metadata = { ["Type"] = "CommitTransaction" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar transação para tenant: {TenantId}", 
                _tenantContext.GetCurrentTenantId());
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Reverte transação com logging detalhado
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("Tentativa de rollback quando nenhuma transação está ativa");
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _statistics.TransactionsRolledBack++;
            
            var transactionId = _currentTransaction.TransactionId.ToString();
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            _logger.LogWarning("Transação revertida - ID: {TransactionId}, Tenant: {TenantId}", 
                transactionId, tenantId);

            TransactionRolledBack?.Invoke(this, new UnitOfWorkEventArgs
            {
                TransactionId = transactionId,
                TenantId = tenantId,
                Metadata = { ["Type"] = "RollbackTransaction" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante rollback para tenant: {TenantId}", 
                _tenantContext.GetCurrentTenantId());
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Executa operação em transação isolada com rollback automático
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        var hadActiveTransaction = HasActiveTransaction;
        
        if (!hadActiveTransaction)
            await BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation();
            
            if (!hadActiveTransaction)
            {
                await CommitAsync(cancellationToken);
                await CommitTransactionAsync(cancellationToken);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante execução em transação para tenant: {TenantId}", 
                _tenantContext.GetCurrentTenantId());
            
            if (!hadActiveTransaction)
                await RollbackTransactionAsync(cancellationToken);
            
            throw;
        }
    }

    /// <summary>
    /// Executa operação em transação isolada sem retorno
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Verifica se existe transação ativa
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Obtém estatísticas atualizadas da sessão
    /// </summary>
    public UnitOfWorkStatistics GetStatistics()
    {
        _statistics.TotalExecutionTime = _stopwatch.Elapsed;
        return _statistics;
    }

    /// <summary>
    /// Limpa cache de repositórios
    /// </summary>
    public void ClearRepositoryCache()
    {
        _repositories.Clear();
        _verticalRepositories.Clear();
        _logger.LogDebug("Cache de repositórios limpo para tenant: {TenantId}", 
            _tenantContext.GetCurrentTenantId());
    }

    /// <summary>
    /// Eventos de transação
    /// </summary>
    public event EventHandler<UnitOfWorkEventArgs>? TransactionStarted;
    public event EventHandler<UnitOfWorkEventArgs>? TransactionCommitted;
    public event EventHandler<UnitOfWorkEventArgs>? TransactionRolledBack;

    /// <summary>
    /// Libera transação atual
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Dispose com cleanup completo
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            var stats = GetStatistics();
            _logger.LogInformation(
                "UnitOfWork finalizado - Tenant: {TenantId}, Operações: {Operations}, Tempo: {Duration}ms",
                _tenantContext.GetCurrentTenantId(), 
                stats.TotalOperations, 
                stats.TotalExecutionTime.TotalMilliseconds);

            if (_currentTransaction != null)
            {
                _logger.LogWarning("Transação ativa durante dispose - executando rollback automático");
                _currentTransaction.Rollback();
                _currentTransaction.Dispose();
            }

            _stopwatch.Stop();
            _repositories.Clear();
            _verticalRepositories.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante dispose do UnitOfWork");
        }
        finally
        {
            _disposed = true;
        }
    }
}