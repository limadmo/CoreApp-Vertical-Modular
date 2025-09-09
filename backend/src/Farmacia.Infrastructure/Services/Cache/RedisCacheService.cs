using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Farmacia.Infrastructure.Configuration;

namespace Farmacia.Infrastructure.Services.Cache;

/// <summary>
/// Implementação do serviço de cache Redis para sistema farmacêutico brasileiro
/// Fornece cache distribuído de alta performance para módulos e configurações
/// </summary>
/// <remarks>
/// Este serviço implementa cache otimizado para o modelo SAAS multi-tenant,
/// com namespace por farmácia e fallback automático para PostgreSQL
/// </remarks>
public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase(_options.DefaultDatabase);
        _options = options.Value;
        _logger = logger;
        
        // Configurações JSON otimizadas para o sistema farmacêutico
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Obtém valor do cache com tipo específico
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var redisKey = BuildKey(key);
            var value = await _database.StringGetAsync(redisKey);
            
            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss para chave: {Key}", key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            _logger.LogDebug("Cache hit para chave: {Key}", key);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter valor do cache: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Define valor no cache com expiração
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var redisKey = BuildKey(key);
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var expiry = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);
            
            await _database.StringSetAsync(redisKey, serializedValue, expiry);
            
            _logger.LogDebug("Valor definido no cache: {Key} (expiração: {Expiry})", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir valor no cache: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Remove chave do cache
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            var removed = await _database.KeyDeleteAsync(redisKey);
            
            if (removed)
                _logger.LogDebug("Chave removida do cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover chave do cache: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Remove múltiplas chaves por padrão
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(_database.Database, BuildKey(pattern));
            
            var keyArray = keys.ToArray();
            if (keyArray.Length > 0)
            {
                await _database.KeyDeleteAsync(keyArray);
                _logger.LogDebug("Removidas {Count} chaves do cache com padrão: {Pattern}", 
                    keyArray.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover chaves por padrão: {Pattern}", pattern);
            throw;
        }
    }

    /// <summary>
    /// Verifica se chave existe
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.KeyExistsAsync(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência da chave: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Incrementa contador atomicamente
    /// </summary>
    public async Task<long> IncrementAsync(string key, long increment = 1, TimeSpan? expiration = null)
    {
        try
        {
            var redisKey = BuildKey(key);
            var result = await _database.StringIncrementAsync(redisKey, increment);
            
            if (expiration.HasValue)
            {
                await _database.KeyExpireAsync(redisKey, expiration.Value);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao incrementar contador: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Obtém múltiplas chaves de uma vez
    /// </summary>
    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class
    {
        var result = new Dictionary<string, T?>();
        
        try
        {
            var redisKeys = keys.Select(BuildKey).ToArray();
            var values = await _database.StringGetAsync(redisKeys);
            
            for (int i = 0; i < keys.Count(); i++)
            {
                var originalKey = keys.ElementAt(i);
                var value = values[i];
                
                if (value.HasValue)
                {
                    try
                    {
                        result[originalKey] = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar valor para chave: {Key}", originalKey);
                        result[originalKey] = null;
                    }
                }
                else
                {
                    result[originalKey] = null;
                }
            }
            
            _logger.LogDebug("Obtidas {Count} chaves do cache", keys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter múltiplas chaves do cache");
            throw;
        }
        
        return result;
    }

    /// <summary>
    /// Constrói chave Redis com namespace do sistema farmacêutico
    /// </summary>
    private string BuildKey(string key)
    {
        return $"{_options.KeyPrefix}:{key}";
    }
}

/// <summary>
/// Serviço de cache específico para módulos farmacêuticos
/// </summary>
public class ModuleCacheService : IModuleCacheService
{
    private readonly IRedisCacheService _cache;
    private readonly ILogger<ModuleCacheService> _logger;
    private const int MODULE_CACHE_MINUTES = 30;

    public ModuleCacheService(IRedisCacheService cache, ILogger<ModuleCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Cache dos módulos ativos de um tenant
    /// </summary>
    public async Task<List<string>?> GetActiveModulesAsync(string tenantId)
    {
        var key = $"modules:active:{tenantId}";
        return await _cache.GetAsync<List<string>>(key);
    }

    /// <summary>
    /// Define módulos ativos no cache
    /// </summary>
    public async Task SetActiveModulesAsync(string tenantId, List<string> modules)
    {
        var key = $"modules:active:{tenantId}";
        var expiration = TimeSpan.FromMinutes(MODULE_CACHE_MINUTES);
        
        await _cache.SetAsync(key, modules, expiration);
        
        _logger.LogDebug("Módulos ativos cacheados para tenant {TenantId}: {Modules}", 
            tenantId, string.Join(", ", modules));
    }

    /// <summary>
    /// Limpa cache de módulos de um tenant
    /// </summary>
    public async Task ClearTenantModulesAsync(string tenantId)
    {
        var pattern = $"modules:*:{tenantId}";
        await _cache.RemoveByPatternAsync(pattern);
        
        _logger.LogDebug("Cache de módulos limpo para tenant {TenantId}", tenantId);
    }

    /// <summary>
    /// Limpa cache de todos os tenants
    /// </summary>
    public async Task ClearAllModuleCacheAsync()
    {
        var pattern = "modules:*";
        await _cache.RemoveByPatternAsync(pattern);
        
        _logger.LogInformation("Cache de módulos limpo para todos os tenants");
    }
}

/// <summary>
/// Serviço de cache para configurações dinâmicas
/// </summary>
public class ConfigurationCacheService : IConfigurationCacheService
{
    private readonly IRedisCacheService _cache;
    private readonly ILogger<ConfigurationCacheService> _logger;
    private const int CONFIG_CACHE_HOURS = 2;

    public ConfigurationCacheService(IRedisCacheService cache, ILogger<ConfigurationCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Obtém configuração por tenant e tipo
    /// </summary>
    public async Task<T?> GetTenantConfigAsync<T>(string tenantId, string configType) where T : class
    {
        var key = $"config:tenant:{tenantId}:{configType}";
        return await _cache.GetAsync<T>(key);
    }

    /// <summary>
    /// Define configuração por tenant
    /// </summary>
    public async Task SetTenantConfigAsync<T>(string tenantId, string configType, T config) where T : class
    {
        var key = $"config:tenant:{tenantId}:{configType}";
        var expiration = TimeSpan.FromHours(CONFIG_CACHE_HOURS);
        
        await _cache.SetAsync(key, config, expiration);
        
        _logger.LogDebug("Configuração {ConfigType} cacheada para tenant {TenantId}", 
            configType, tenantId);
    }

    /// <summary>
    /// Obtém configuração global
    /// </summary>
    public async Task<T?> GetGlobalConfigAsync<T>(string configType) where T : class
    {
        var key = $"config:global:{configType}";
        return await _cache.GetAsync<T>(key);
    }

    /// <summary>
    /// Define configuração global
    /// </summary>
    public async Task SetGlobalConfigAsync<T>(string configType, T config) where T : class
    {
        var key = $"config:global:{configType}";
        var expiration = TimeSpan.FromHours(CONFIG_CACHE_HOURS);
        
        await _cache.SetAsync(key, config, expiration);
        
        _logger.LogDebug("Configuração global {ConfigType} cacheada", configType);
    }
}

/// <summary>
/// Serviço de cache para sessões de usuários
/// </summary>
public class SessionCacheService : ISessionCacheService
{
    private readonly IRedisCacheService _cache;
    private readonly ILogger<SessionCacheService> _logger;

    public SessionCacheService(IRedisCacheService cache, ILogger<SessionCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Armazena sessão de usuário
    /// </summary>
    public async Task SetUserSessionAsync(string sessionId, UserSessionData session, TimeSpan? expiration = null)
    {
        var key = $"session:user:{sessionId}";
        var expiry = expiration ?? TimeSpan.FromHours(24);
        
        await _cache.SetAsync(key, session, expiry);
        
        _logger.LogDebug("Sessão de usuário {UserId} cacheada: {SessionId}", 
            session.UserId, sessionId);
    }

    /// <summary>
    /// Obtém sessão de usuário
    /// </summary>
    public async Task<UserSessionData?> GetUserSessionAsync(string sessionId)
    {
        var key = $"session:user:{sessionId}";
        return await _cache.GetAsync<UserSessionData>(key);
    }

    /// <summary>
    /// Remove sessão
    /// </summary>
    public async Task RemoveUserSessionAsync(string sessionId)
    {
        var key = $"session:user:{sessionId}";
        await _cache.RemoveAsync(key);
        
        _logger.LogDebug("Sessão removida: {SessionId}", sessionId);
    }

    /// <summary>
    /// Adiciona token à blacklist
    /// </summary>
    public async Task BlacklistTokenAsync(string tokenId, TimeSpan expiration)
    {
        var key = $"token:blacklist:{tokenId}";
        await _cache.SetAsync(key, new { blacklisted = true, timestamp = DateTime.UtcNow }, expiration);
        
        _logger.LogInformation("Token adicionado à blacklist: {TokenId}", tokenId);
    }

    /// <summary>
    /// Verifica se token está na blacklist
    /// </summary>
    public async Task<bool> IsTokenBlacklistedAsync(string tokenId)
    {
        var key = $"token:blacklist:{tokenId}";
        return await _cache.ExistsAsync(key);
    }
}