using CoreApp.Domain.Entities;

namespace CoreApp.Infrastructure.Repositories;

public interface IPlanoComercialRepository
{
    Task<PlanoComercialEntity?> GetByIdAsync(Guid id);
    Task<List<PlanoComercialEntity>> GetAllAsync();
    Task<PlanoComercialEntity> AddAsync(PlanoComercialEntity entity);
    Task<PlanoComercialEntity> UpdateAsync(PlanoComercialEntity entity);
    Task DeleteAsync(Guid id);
}