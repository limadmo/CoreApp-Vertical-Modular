using CoreApp.Domain.Entities;

namespace CoreApp.Infrastructure.Repositories;

public interface IModuloRepository
{
    Task<ModuloEntity?> GetByIdAsync(Guid id);
    Task<List<ModuloEntity>> GetAllAsync();
    Task<ModuloEntity> AddAsync(ModuloEntity entity);
    Task<ModuloEntity> UpdateAsync(ModuloEntity entity);
    Task DeleteAsync(Guid id);
}