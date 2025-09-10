using Microsoft.AspNetCore.Builder;
using CoreApp.Infrastructure.Data.Context;
using CoreApp.Infrastructure.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApp.Infrastructure;

/// <summary>
/// Configuração de injeção de dependência para infraestrutura
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona serviços de infraestrutura
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<CoreAppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Multi-tenant
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantService, TenantService>();

        return services;
    }
}
