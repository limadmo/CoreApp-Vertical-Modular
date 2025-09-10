using CoreApp.Domain.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreApp.Infrastructure.Data.Context;

/// <summary>
/// Factory para criação do DbContext em design time (migrations)
/// </summary>
public class CoreAppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CoreAppDbContext>
{
    public CoreAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreAppDbContext>();
        
        // Connection string para container Docker
        var connectionString = "Host=postgres;Port=5432;Database=coreapp_saas_development;Username=admin;Password=dev123456;Include Error Detail=true";
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly("CoreApp.Infrastructure");
            options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        });

        // Configurações adicionais para development
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();

        // Mock do TenantContext para design time
        var tenantContext = new DesignTimeTenantContext();

        return new CoreAppDbContext(optionsBuilder.Options, tenantContext);
    }
}

/// <summary>
/// Mock do ITenantContext para uso durante migrations
/// </summary>
internal class DesignTimeTenantContext : ITenantContext
{
    public string GetCurrentTenantId() => "design-time-tenant";
    public string GetCurrentUserId() => "design-time-user";
    public void SetCurrentTenant(string tenantId) { }
    public void SetCurrentUser(string userId) { }
    public bool HasTenant() => true;
    public bool HasUser() => true;
}