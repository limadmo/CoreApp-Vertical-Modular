using CoreApp.Domain.Interfaces.Repositories;
using CoreApp.Domain.Entities.Common;
using CoreApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreApp.Infrastructure.Repositories.Base;

/// <summary>
/// Implementação base para todos os repositórios do sistema CoreApp
/// Fornece operações CRUD básicas com suporte multi-tenant
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade que implementa ITenantEntity</typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, ITenantEntity
{
    protected readonly CoreAppDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected readonly ILogger Logger;

    public BaseRepository(CoreAppDbContext context, ILogger logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        DbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Obtém uma entidade por ID considerando o tenant atual
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Buscando entidade {EntityType} por ID: {Id}", typeof(TEntity).Name, id);
            return await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar entidade {EntityType} por ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Obtém todas as entidades do tenant atual com paginação
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Listando entidades {EntityType} - Página: {Page}, Tamanho: {Size}", 
                typeof(TEntity).Name, pageNumber, pageSize);

            var skip = (pageNumber - 1) * pageSize;
            
            return await DbSet
                .AsNoTracking()
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao listar entidades {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Conta o total de registros do tenant atual
    /// </summary>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Contando entidades {EntityType}", typeof(TEntity).Name);
            return await DbSet.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao contar entidades {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            Logger.LogDebug("Adicionando entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, entity.Id);
            
            var entry = await DbSet.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao adicionar entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            Logger.LogDebug("Atualizando entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, entity.Id);
            
            Context.Entry(entity).State = EntityState.Modified;
            
            await Task.CompletedTask;
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao atualizar entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Remove uma entidade por ID (exclusão lógica se implementa ISoftDeletableEntity)
    /// </summary>
    public virtual async Task<bool> DeleteAsync(
        Guid id, 
        string? usuarioId = null, 
        string? motivo = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Removendo entidade {EntityType} - ID: {Id}, Usuário: {UsuarioId}", 
                typeof(TEntity).Name, id, usuarioId);

            var entity = await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (entity == null)
                return false;

            // Se implementa soft delete, aplica exclusão lógica
            if (entity is ISoftDeletableEntity softDeletableEntity)
            {
                softDeletableEntity.MarkAsDeleted(usuarioId, motivo);
                
                Context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                // Exclusão física
                DbSet.Remove(entity);
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao remover entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma entidade com o ID especificado
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando existência de entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, id);
            return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar existência de entidade {EntityType} - ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    /// <summary>
    /// VIOLAÇÃO DO PADRÃO UOW - Este método não deve ser usado
    /// O Unit of Work deve coordenar todas as operações de SaveChanges
    /// </summary>
    [Obsolete("Use IUnitOfWork.CommitAsync() em vez de SaveChangesAsync direto no repositório", true)]
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Este método viola o padrão Unit of Work
        // Mantido apenas para compatibilidade com a interface
        Logger.LogWarning("SaveChangesAsync chamado diretamente no repositório - USE IUnitOfWork.CommitAsync()");
        throw new InvalidOperationException("Use IUnitOfWork.CommitAsync() em vez de SaveChangesAsync direto no repositório");
    }
}