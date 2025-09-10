using CoreApp.Verticals.Common;
using CoreApp.Verticals.Padaria;
using CoreApp.Verticals.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

namespace CoreApp.Verticals.Extensions;

/// <summary>
/// Extensões para configuração e registro automático do sistema de verticais
/// Fornece métodos fluentes para integração com o container de DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona o sistema completo de verticais ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços do ASP.NET Core</param>
    /// <returns>ServiceCollection para chamadas fluentes</returns>
    public static IServiceCollection AddVerticalSystem(this IServiceCollection services)
    {
        // Registra o registry de verticais como singleton (para manter estado global)
        services.AddSingleton<IVerticalRegistry, VerticalRegistry>();
        
        // Registra o gerenciador de verticais como scoped (para consumir outros serviços scoped)
        services.AddScoped<IVerticalManager, VerticalManager>();
        
        // Registra todas as verticais disponíveis
        services.AddVerticals();
        
        return services;
    }
    
    /// <summary>
    /// Registra todas as verticais implementadas no sistema
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>ServiceCollection para chamadas fluentes</returns>
    public static IServiceCollection AddVerticals(this IServiceCollection services)
    {
        // Registra a vertical Padaria
        services.AddTransient<PadariaModule>();
        
        // TODO: Adicionar outras verticais aqui conforme forem implementadas
        // services.AddTransient<FarmaciaModule>();
        // services.AddTransient<RestauranteModule>();
        // services.AddTransient<AutoPecasModule>();
        
        return services;
    }
    
    /// <summary>
    /// Configura e registra automaticamente todas as verticais no VerticalManager
    /// </summary>
    /// <param name="app">Application builder configurado</param>
    /// <returns>Application builder para chamadas fluentes</returns>
    public static IApplicationBuilder ConfigureVerticals(this IApplicationBuilder app)
    {
        // Cria um scope para resolver serviços scoped
        using var scope = app.ApplicationServices.CreateScope();
        var verticalManager = scope.ServiceProvider.GetRequiredService<IVerticalManager>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IVerticalManager>>();
        
        try
        {
            // Registra a vertical Padaria
            var padariaModule = scope.ServiceProvider.GetRequiredService<PadariaModule>();
            verticalManager.RegisterVertical(padariaModule);
            
            // TODO: Registrar outras verticais aqui
            
            logger.LogInformation("Sistema de verticais configurado com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao configurar sistema de verticais");
            throw;
        }
        
        return app;
    }
    
    /// <summary>
    /// Adiciona middleware para interceptação automática de propriedades verticais
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>ServiceCollection para chamadas fluentes</returns>
    public static IServiceCollection AddVerticalMiddleware(this IServiceCollection services)
    {
        // Middleware não deve ser registrado no container de DI
        // O ASP.NET Core gerencia automaticamente via UseMiddleware<T>()
        return services;
    }
    
    /// <summary>
    /// Configura o pipeline de middleware para interceptação de verticais
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder para chamadas fluentes</returns>
    public static IApplicationBuilder UseVerticalInterception(this IApplicationBuilder app)
    {
        return app.UseMiddleware<VerticalInterceptionMiddleware>();
    }
}