using Microsoft.EntityFrameworkCore;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.Data;

namespace Farmacia.Infrastructure.Repositories;

/// <summary>
/// Repositório base para operações comuns de CRUD
/// Implementa padrões standard para sistema farmacêutico multi-tenant brasileiro
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade</typeparam>
/// <remarks>
/// Este repositório base fornece operações CRUD padrão que são herdadas
/// por todos os repositórios específicos do sistema
/// </remarks>
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Obtém entidade por ID
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Adiciona nova entidade
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    /// <summary>
    /// Atualiza entidade existente
    /// </summary>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Remove entidade por ID
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    /// <summary>
    /// Remove entidade
    /// </summary>
    public virtual async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Verifica se entidade existe
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    /// <summary>
    /// Conta total de registros
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// <summary>
    /// Obtém entidades paginadas
    /// </summary>
    public virtual async Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Salva alterações no contexto
    /// </summary>
    protected async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Inicia transação
    /// </summary>
    protected async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
}