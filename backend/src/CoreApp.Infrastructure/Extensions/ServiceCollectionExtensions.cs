using CoreApp.Infrastructure.Data.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreApp.Infrastructure.Extensions;

/// <summary>
/// Extensions para configuração de serviços da infraestrutura
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona e configura o sistema de seeding do banco de dados
    /// </summary>
    public static IServiceCollection AddDatabaseSeeding(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        return services;
    }

    /// <summary>
    /// Executa o seeding do banco de dados durante o startup da aplicação
    /// </summary>
    public static async Task<IHost> SeedDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        try
        {
            logger.LogInformation("Iniciando seeding automático do banco de dados");
            
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
            
            logger.LogInformation("Seeding automático concluído com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante o seeding automático do banco de dados");
            throw;
        }

        return host;
    }
}