using CoreApp.Domain.Entities;
using CoreApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreApp.Infrastructure.Repositories;

public class ModuloRepository : IModuloRepository
{
    private readonly CoreAppDbContext _context;

    public ModuloRepository(CoreAppDbContext context)
    {
        _context = context;
    }

    public async Task<ModuloEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Modulos.FindAsync(id);
    }

    public async Task<List<ModuloEntity>> GetAllAsync()
    {
        return await _context.Modulos.ToListAsync();
    }

    public async Task<ModuloEntity> AddAsync(ModuloEntity entity)
    {
        _context.Modulos.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ModuloEntity> UpdateAsync(ModuloEntity entity)
    {
        _context.Modulos.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Modulos.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}