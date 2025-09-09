using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Farmacia.Infrastructure.Configuration;

/// <summary>
/// Configuração do Redis para cache distribuído no sistema farmacêutico brasileiro
/// Implementa cache de alta performance para módulos, configurações e sessões
/// </summary>
/// <remarks>
/// O Redis é essencial para o sistema SAAS multi-tenant, fornecendo:
/// - Cache de módulos ativos por farmácia (sub-milissegundo)
/// - Cache de configurações dinâmicas
/// - Sessões de usuários distribuídas
/// - Blacklist de tokens JWT
/// </remarks>
public static class RedisConfiguration
{
    /// <summary>
    /// Configuração principal do Redis para o sistema farmacêutico
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();

        // Configura StackExchange.Redis
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<IConnectionMultiplexer>>();
            
            try
            {
                var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                
                // Configurações de produção para sistema farmacêutico brasileiro
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectRetry = redisOptions.ConnectRetry;
                configurationOptions.ConnectTimeout = redisOptions.ConnectTimeoutMs;
                configurationOptions.SyncTimeout = redisOptions.SyncTimeoutMs;
                configurationOptions.AsyncTimeout = redisOptions.AsyncTimeoutMs;
                configurationOptions.KeepAlive = redisOptions.KeepAliveInterval;
                configurationOptions.DefaultDatabase = redisOptions.DefaultDatabase;
                configurationOptions.ClientName = $"Farmacia-{Environment.MachineName}";
                
                // Log de eventos Redis
                configurationOptions.SocketManager = SocketManager.ThreadPool;
                
                var connection = ConnectionMultiplexer.Connect(configurationOptions);
                
                // Eventos de monitoramento
                connection.ConnectionFailed += (sender, e) =>
                    logger.LogError("Redis connection failed: {EndPoint} - {FailureType}", 
                        e.EndPoint, e.FailureType);
                
                connection.ConnectionRestored += (sender, e) =>
                    logger.LogInformation("Redis connection restored: {EndPoint}", e.EndPoint);
                
                connection.InternalError += (sender, e) =>
                    logger.LogError(e.Exception, "Redis internal error: {Origin}", e.Origin);

                logger.LogInformation("Redis conectado com sucesso - Endpoint: {Endpoint}", 
                    redisConnectionString);
                
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao conectar com Redis: {ConnectionString}", 
                    redisConnectionString);
                throw;
            }
        });

        // Configura IDistributedCache usando Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = redisOptions.InstanceName;
            
            // Configurações específicas para farmácias brasileiras
            options.ConfigurationOptions = configurationOptions =>
            {
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectRetry = redisOptions.ConnectRetry;
                configurationOptions.DefaultDatabase = redisOptions.DefaultDatabase;
                configurationOptions.ClientName = $"Farmacia-Cache-{Environment.MachineName}";
            };
        });

        // Registra serviços customizados de cache
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IModuleCacheService, ModuleCacheService>();
        services.AddSingleton<IConfigurationCacheService, ConfigurationCacheService>();
        services.AddSingleton<ISessionCacheService, SessionCacheService>();
        
        // Health check para Redis
        services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>("redis", 
                tags: new[] { "ready", "cache", "redis" });
    }
}

/// <summary>
/// Opções de configuração do Redis
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Nome da instância Redis (usado como prefixo)
    /// </summary>
    public string InstanceName { get; set; } = "Farmacia";

    /// <summary>
    /// Database padrão do Redis
    /// </summary>
    public int DefaultDatabase { get; set; } = 0;

    /// <summary>
    /// Tentativas de reconexão
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Timeout de conexão em milissegundos
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Timeout síncrono em milissegundos
    /// </summary>
    public int SyncTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Timeout assíncrono em milissegundos
    /// </summary>
    public int AsyncTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Intervalo de keep-alive em segundos
    /// </summary>
    public int KeepAliveInterval { get; set; } = 60;

    /// <summary>
    /// Tempo padrão de expiração do cache em minutos
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Se deve usar compressão nos dados
    /// </summary>
    public bool UseCompression { get; set; } = true;

    /// <summary>
    /// Prefixo para chaves do sistema farmacêutico
    /// </summary>
    public string KeyPrefix { get; set; } = "farmacia";
}

/// <summary>
/// Interface para serviços de cache Redis específicos do sistema farmacêutico
/// </summary>
public interface IRedisCacheService
{
    /// <summary>
    /// Obtém valor do cache com tipo específico
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Define valor no cache com expiração
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    
    /// <summary>
    /// Remove chave do cache
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Remove múltiplas chaves por padrão
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
    
    /// <summary>
    /// Verifica se chave existe
    /// </summary>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Incrementa contador atomicamente
    /// </summary>
    Task<long> IncrementAsync(string key, long increment = 1, TimeSpan? expiration = null);
    
    /// <summary>
    /// Obtém múltiplas chaves de uma vez
    /// </summary>
    Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class;
}

/// <summary>
/// Interface para cache específico de módulos
/// </summary>
public interface IModuleCacheService
{
    /// <summary>
    /// Cache dos módulos ativos de um tenant
    /// </summary>
    Task<List<string>?> GetActiveModulesAsync(string tenantId);
    
    /// <summary>
    /// Define módulos ativos no cache
    /// </summary>
    Task SetActiveModulesAsync(string tenantId, List<string> modules);
    
    /// <summary>
    /// Limpa cache de módulos de um tenant
    /// </summary>
    Task ClearTenantModulesAsync(string tenantId);
    
    /// <summary>
    /// Limpa cache de todos os tenants
    /// </summary>
    Task ClearAllModuleCacheAsync();
}

/// <summary>
/// Interface para cache de configurações dinâmicas
/// </summary>
public interface IConfigurationCacheService
{
    /// <summary>
    /// Obtém configuração por tenant e tipo
    /// </summary>
    Task<T?> GetTenantConfigAsync<T>(string tenantId, string configType) where T : class;
    
    /// <summary>
    /// Define configuração por tenant
    /// </summary>
    Task SetTenantConfigAsync<T>(string tenantId, string configType, T config) where T : class;
    
    /// <summary>
    /// Obtém configuração global
    /// </summary>
    Task<T?> GetGlobalConfigAsync<T>(string configType) where T : class;
    
    /// <summary>
    /// Define configuração global
    /// </summary>
    Task SetGlobalConfigAsync<T>(string configType, T config) where T : class;
}

/// <summary>
/// Interface para cache de sessões
/// </summary>
public interface ISessionCacheService
{
    /// <summary>
    /// Armazena sessão de usuário
    /// </summary>
    Task SetUserSessionAsync(string sessionId, UserSessionData session, TimeSpan? expiration = null);
    
    /// <summary>
    /// Obtém sessão de usuário
    /// </summary>
    Task<UserSessionData?> GetUserSessionAsync(string sessionId);
    
    /// <summary>
    /// Remove sessão
    /// </summary>
    Task RemoveUserSessionAsync(string sessionId);
    
    /// <summary>
    /// Adiciona token à blacklist
    /// </summary>
    Task BlacklistTokenAsync(string tokenId, TimeSpan expiration);
    
    /// <summary>
    /// Verifica se token está na blacklist
    /// </summary>
    Task<bool> IsTokenBlacklistedAsync(string tokenId);
}

/// <summary>
/// Dados de sessão do usuário para cache
/// </summary>
public class UserSessionData
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> ActiveModules { get; set; } = new List<string>();
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// Health check para Redis
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            var key = "health:check:" + Guid.NewGuid();
            var value = DateTime.UtcNow.ToString("O");
            
            await database.StringSetAsync(key, value, TimeSpan.FromSeconds(30));
            var retrieved = await database.StringGetAsync(key);
            await database.KeyDeleteAsync(key);
            
            if (retrieved == value)
            {
                return HealthCheckResult.Healthy("Redis está funcionando corretamente");
            }
            
            return HealthCheckResult.Unhealthy("Redis não conseguiu armazenar/recuperar dados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no health check do Redis");
            return HealthCheckResult.Unhealthy("Erro na conexão com Redis", ex);
        }
    }
}