using CoreApp.Domain.Entities;

namespace CoreApp.Infrastructure.Repositories;

/// <summary>
/// Interface para repositório de módulos por tenant
/// </summary>
public interface ITenantModuloRepository
{
    Task<TenantModuloEntity?> GetByIdAsync(Guid id);
    Task<List<TenantModuloEntity>> GetByTenantIdAsync(string tenantId);
    Task AddAsync(TenantModuloEntity entity);
    Task UpdateAsync(TenantModuloEntity entity);
    Task DeleteAsync(Guid id);
}
