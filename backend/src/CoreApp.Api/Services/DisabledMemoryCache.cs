using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace CoreApp.Api.Services;

/// <summary>
/// Implementação de IMemoryCache que desabilita cache em desenvolvimento
/// Todos os métodos retornam como se não houvesse cache, forçando sempre busca na origem
/// </summary>
public class DisabledMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key)
    {
        // Retorna entrada de cache que não persiste dados
        return new DisabledCacheEntry(key);
    }

    public void Dispose()
    {
        // Nada para disposed
    }

    public void Remove(object key)
    {
        // Nada para remover
    }

    public bool TryGetValue(object key, out object? value)
    {
        // Sempre retorna false - como se não houvesse cache
        value = null;
        return false;
    }
}

/// <summary>
/// Cache entry que não persiste dados - sempre comporta como miss
/// </summary>
internal class DisabledCacheEntry : ICacheEntry
{
    public DisabledCacheEntry(object key)
    {
        Key = key;
    }

    public object Key { get; }
    
    public object? Value { get; set; }
    
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    
    public TimeSpan? SlidingExpiration { get; set; }
    
    public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();
    
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();
    
    public CacheItemPriority Priority { get; set; }
    
    public long? Size { get; set; }

    public void Dispose()
    {
        // Entry disabled - não persiste dados
    }
}