using Farmacia.Infrastructure.Data.Context;
using Farmacia.Infrastructure.MultiTenant;
using Farmacia.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacia.Infrastructure;

/// <summary>
/// Configuração de injeção de dependência para a camada de infraestrutura
/// Registra todos os serviços relacionados a EF Core, Cache em memória, Multi-tenant e APIs externas
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona todos os serviços de infraestrutura ao container de DI
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Container configurado</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Registrar serviços multi-tenant
        AddMultiTenantServices(services);
        
        // Registrar Entity Framework Core
        AddEntityFramework(services, configuration);
        
        // Registrar serviços de cache em memória
        AddCacheServices(services);
        
        // Registrar APIs externas brasileiras
        AddBrazilianExternalServices(services, configuration);

        return services;
    }

    /// <summary>
    /// Registra serviços relacionados a multi-tenancy
    /// </summary>
    /// <param name="services">Container de serviços</param>
    private static void AddMultiTenantServices(IServiceCollection services)
    {
        // HttpContextAccessor necessário para obter informações da requisição
        services.AddHttpContextAccessor();
        
        // Serviço principal de multi-tenancy
        services.AddScoped<ITenantService, TenantService>();
    }

    /// <summary>
    /// Configura Entity Framework Core com PostgreSQL e otimizações multi-tenant
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    private static void AddEntityFramework(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<FarmaciaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Configurações específicas do PostgreSQL
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                // Configurar timezone brasileiro
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // Configurações de desenvolvimento
            if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }

            // Configurações de produção
            else
            {
                options.EnableServiceProviderCaching();
                options.EnableSensitiveDataLogging(false);
            }
        });

        // Health checks para PostgreSQL
        services.AddHealthChecks()
            .AddNpgSql(connectionString!, name: "postgresql", tags: new[] { "db", "postgresql" });
    }

    /// <summary>
    /// Registra serviços de cache em memória com IMemoryCache
    /// </summary>
    /// <param name="services">Container de serviços</param>
    private static void AddCacheServices(IServiceCollection services)
    {
        // Registrar IMemoryCache nativo do .NET
        services.AddMemoryCache(options =>
        {
            // Configurações de cache em memória otimizadas
            options.SizeLimit = 1000; // Máximo 1000 entradas
            options.CompactionPercentage = 0.25; // Remove 25% quando atinge limite
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Verifica expiração a cada 5 minutos
        });
        
        // Registrar serviço de configuração dinâmica
        services.AddScoped<IConfigurationService, ConfigurationService>();
        
        // Registrar serviço de seed de configurações
        services.AddScoped<IConfigurationSeedService, ConfigurationSeedService>();
    }

    /// <summary>
    /// Registra serviços para APIs externas brasileiras (ANVISA, gateways de pagamento)
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    private static void AddBrazilianExternalServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configurar HttpClient para APIs externas
        services.AddHttpClient("ANVISA", client =>
        {
            var anvisaBaseUrl = configuration["ANVISA:BaseUrl"] ?? "https://consultas.anvisa.gov.br/api";
            var anvisaTimeout = configuration.GetValue<int>("ANVISA:TimeoutSeconds", 30);

            client.BaseAddress = new Uri(anvisaBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(anvisaTimeout);
            
            // Headers padrão para API ANVISA
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Farmacia-SAAS/3.0 (+https://farmacia.com.br)");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            // API Key se configurada
            var anvisaApiKey = configuration["ANVISA:ApiKey"];
            if (!string.IsNullOrEmpty(anvisaApiKey) && anvisaApiKey != "fake-anvisa-key")
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {anvisaApiKey}");
            }
        });

        // HttpClient para Mercado Pago
        services.AddHttpClient("MercadoPago", client =>
        {
            client.BaseAddress = new Uri("https://api.mercadopago.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            
            var mpAccessToken = configuration["PagamentosBrasileiros:MercadoPago:AccessToken"];
            if (!string.IsNullOrEmpty(mpAccessToken) && mpAccessToken != "fake-mercadopago-token")
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {mpAccessToken}");
            }
        });

        // HttpClient para PagSeguro
        services.AddHttpClient("PagSeguro", client =>
        {
            client.BaseAddress = new Uri("https://ws.pagseguro.uol.com.br/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // HttpClient genérico para outras integrações
        services.AddHttpClient("Default", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Farmacia-SAAS/3.0 (+https://farmacia.com.br)");
        });

        // TODO: Registrar serviços específicos das APIs
        // services.AddScoped<IAnvisaService, AnvisaService>();
        // services.AddScoped<IMercadoPagoService, MercadoPagoService>();
        // services.AddScoped<IPagSeguroService, PagSeguroService>();
        // services.AddScoped<IPIXService, PIXService>();
    }

    /// <summary>
    /// Configura migrations e seeds automáticos para desenvolvimento
    /// </summary>
    /// <param name="app">Aplicação configurada</param>
    /// <returns>Aplicação com migrations aplicadas</returns>
    public static async Task<WebApplication> ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<FarmaciaDbContext>();
            
            // Aplicar migrations pendentes
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                Console.WriteLine("🔄 Aplicando migrations pendentes...");
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Migrations aplicadas com sucesso");
            }
            
            // Verificar conectividade
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                Console.WriteLine("✅ Conexão com PostgreSQL estabelecida");
                
                // Aplicar seeds de configuração após conectar
                await ApplyConfigurationSeedsAsync(scope.ServiceProvider);
            }
            else
            {
                Console.WriteLine("❌ Falha na conexão com PostgreSQL");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao aplicar migrations: {ex.Message}");
            
            // Em desenvolvimento, não falhar por problema de banco
            if (app.Environment.IsDevelopment())
            {
                Console.WriteLine("⚠️ Continuando execução em modo desenvolvimento");
            }
            else
            {
                throw;
            }
        }

        return app;
    }

    /// <summary>
    /// Aplica seeds de configurações ANVISA e padrões brasileiros
    /// </summary>
    /// <param name="serviceProvider">Provider de serviços</param>
    private static async Task ApplyConfigurationSeedsAsync(IServiceProvider serviceProvider)
    {
        try
        {
            Console.WriteLine("🌱 Aplicando seeds de configurações farmacêuticas...");
            
            var seedService = serviceProvider.GetRequiredService<IConfigurationSeedService>();
            
            // Seed das classificações ANVISA (A1-C5)
            await seedService.SeedClassificacoesAnvisaAsync();
            Console.WriteLine("✅ Classificações ANVISA aplicadas");
            
            // Seed das configurações padrão brasileiras
            await seedService.SeedStatusEstoqueAsync();
            Console.WriteLine("✅ Status de estoque configurados");
            
            await seedService.SeedFormasPagamentoAsync();
            Console.WriteLine("✅ Formas de pagamento brasileiras configuradas");
            
            await seedService.SeedStatusPagamentoAsync();
            Console.WriteLine("✅ Status de pagamento configurados");
            
            await seedService.SeedStatusSincronizacaoAsync();
            Console.WriteLine("✅ Status de sincronização PDV configurados");
            
            Console.WriteLine("🌱 Seeds de configurações aplicados com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao aplicar seeds de configurações: {ex.Message}");
            // Não falhar a aplicação por problema de seed
        }
    }
}