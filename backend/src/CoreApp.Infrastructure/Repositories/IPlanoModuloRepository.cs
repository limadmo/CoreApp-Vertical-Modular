using CoreApp.Domain.Entities;

namespace CoreApp.Infrastructure.Repositories;

public interface IPlanoModuloRepository
{
    Task<PlanoModuloEntity?> GetByIdAsync(Guid id);
    Task<List<PlanoModuloEntity>> GetAllAsync();
    Task<PlanoModuloEntity> AddAsync(PlanoModuloEntity entity);
    Task<PlanoModuloEntity> UpdateAsync(PlanoModuloEntity entity);
    Task DeleteAsync(Guid id);
}