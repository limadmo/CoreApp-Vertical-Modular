using CoreApp.Domain.Entities;

namespace CoreApp.Infrastructure.Repositories;

public interface ITenantPlanoRepository
{
    Task<TenantPlanoEntity?> GetByIdAsync(Guid id);
    Task<List<TenantPlanoEntity>> GetByTenantIdAsync(string tenantId);
    Task<TenantPlanoEntity> AddAsync(TenantPlanoEntity entity);
    Task<TenantPlanoEntity> UpdateAsync(TenantPlanoEntity entity);
    Task DeleteAsync(Guid id);
}