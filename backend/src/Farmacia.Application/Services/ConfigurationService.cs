using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Farmacia.Infrastructure.Data.Context;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço genérico para gerenciar configurações dinâmicas farmacêuticas brasileiras
/// Utiliza cache em memória para performance e fallback automático para PostgreSQL
/// </summary>
/// <remarks>
/// Este serviço substitui o sistema de enums rígidos por configurações flexíveis que permitem:
/// - Cache em memória nativo do .NET (IMemoryCache)
/// - Sistema hierárquico: Global → Tenant → Personalizado
/// - Alterações sem necessidade de deploy
/// - Performance adequada com cache de 30 minutos
/// - Fallback automático para banco quando cache expira
/// </remarks>
public class ConfigurationService : IConfigurationService
{
    private readonly IMemoryCache _cache;
    private readonly FarmaciaDbContext _context;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ConfigurationService(
        IMemoryCache cache,
        FarmaciaDbContext context,
        ILogger<ConfigurationService> logger)
    {
        _cache = cache;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtém uma configuração específica por código e tenant
    /// </summary>
    /// <typeparam name="T">Tipo da entidade de configuração</typeparam>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    /// <param name="codigo">Código da configuração</param>
    /// <returns>Configuração encontrada ou null</returns>
    public async Task<T?> GetByCodeAsync<T>(string? tenantId, string codigo) 
        where T : class, ITenantEntity
    {
        var cacheKey = BuildCacheKey<T>(tenantId, codigo);
        
        // 1. Tenta obter do cache em memória
        if (_cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            _logger.LogDebug("Configuração {Type}:{TenantId}:{Codigo} obtida do cache", 
                typeof(T).Name, tenantId ?? "GLOBAL", codigo);
            return cachedValue;
        }

        // 2. Busca no banco de dados
        try
        {
            var configuration = await LoadFromDatabaseAsync<T>(tenantId, codigo);
            
            // 3. Cacheia o resultado (mesmo se null)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheExpiration,
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, configuration, cacheOptions);
            
            _logger.LogDebug("Configuração {Type}:{TenantId}:{Codigo} carregada do banco e cacheada", 
                typeof(T).Name, tenantId ?? "GLOBAL", codigo);
            
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar configuração {Type}:{TenantId}:{Codigo}", 
                typeof(T).Name, tenantId ?? "GLOBAL", codigo);
            return null;
        }
    }

    /// <summary>
    /// Obtém todas as configurações ativas de um tipo para um tenant
    /// </summary>
    /// <typeparam name="T">Tipo da entidade de configuração</typeparam>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    /// <returns>Lista de configurações ativas</returns>
    public async Task<List<T>> GetAllByTenantAsync<T>(string? tenantId) 
        where T : class, ITenantEntity
    {
        var cacheKey = BuildCacheKeyForAll<T>(tenantId);
        
        // 1. Tenta obter do cache em memória
        if (_cache.TryGetValue(cacheKey, out List<T>? cachedList))
        {
            _logger.LogDebug("Lista de configurações {Type}:{TenantId} obtida do cache", 
                typeof(T).Name, tenantId ?? "GLOBAL");
            return cachedList ?? new List<T>();
        }

        // 2. Busca no banco de dados
        try
        {
            var configurations = await LoadAllFromDatabaseAsync<T>(tenantId);
            
            // 3. Cacheia o resultado
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheExpiration,
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, configurations, cacheOptions);
            
            _logger.LogDebug("Lista de configurações {Type}:{TenantId} carregada do banco e cacheada ({Count} itens)", 
                typeof(T).Name, tenantId ?? "GLOBAL", configurations.Count);
            
            return configurations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar configurações {Type}:{TenantId}", 
                typeof(T).Name, tenantId ?? "GLOBAL");
            return new List<T>();
        }
    }

    /// <summary>
    /// Cria ou atualiza uma configuração
    /// </summary>
    /// <typeparam name="T">Tipo da entidade de configuração</typeparam>
    /// <param name="configuration">Configuração a ser salva</param>
    /// <returns>Configuração salva</returns>
    public async Task<T> SaveAsync<T>(T configuration) 
        where T : class, ITenantEntity
    {
        try
        {
            var dbSet = GetDbSet<T>();
            
            // Verifica se já existe
            var existing = await dbSet.FirstOrDefaultAsync(c => c.Id == configuration.Id);
            
            if (existing != null)
            {
                // Atualiza existente
                _context.Entry(existing).CurrentValues.SetValues(configuration);
            }
            else
            {
                // Adiciona novo
                await dbSet.AddAsync(configuration);
            }
            
            await _context.SaveChangesAsync();
            
            // Invalida cache relacionado
            InvalidateCache<T>(configuration.TenantId, GetCodigoFromEntity(configuration));
            
            _logger.LogInformation("Configuração {Type}:{TenantId}:{Codigo} salva com sucesso", 
                typeof(T).Name, configuration.TenantId ?? "GLOBAL", GetCodigoFromEntity(configuration));
            
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configuração {Type}:{TenantId}", 
                typeof(T).Name, configuration.TenantId ?? "GLOBAL");
            throw;
        }
    }

    /// <summary>
    /// Remove uma configuração por ID
    /// </summary>
    /// <typeparam name="T">Tipo da entidade de configuração</typeparam>
    /// <param name="id">ID da configuração</param>
    /// <returns>True se removida com sucesso</returns>
    public async Task<bool> DeleteAsync<T>(Guid id) 
        where T : class, ITenantEntity
    {
        try
        {
            var dbSet = GetDbSet<T>();
            var configuration = await dbSet.FirstOrDefaultAsync(c => c.Id == id);
            
            if (configuration == null)
                return false;
            
            // Verifica se pode ser removida
            if (!CanBeDeleted(configuration))
            {
                _logger.LogWarning("Tentativa de remover configuração protegida {Type}:{Id}", 
                    typeof(T).Name, id);
                return false;
            }
            
            dbSet.Remove(configuration);
            await _context.SaveChangesAsync();
            
            // Invalida cache relacionado
            InvalidateCache<T>(configuration.TenantId, GetCodigoFromEntity(configuration));
            
            _logger.LogInformation("Configuração {Type}:{Id} removida com sucesso", 
                typeof(T).Name, id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover configuração {Type}:{Id}", 
                typeof(T).Name, id);
            return false;
        }
    }

    /// <summary>
    /// Invalida cache para uma configuração específica
    /// </summary>
    /// <typeparam name="T">Tipo da entidade de configuração</typeparam>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigo">Código da configuração</param>
    public void InvalidateCache<T>(string? tenantId, string? codigo = null) 
        where T : class, ITenantEntity
    {
        if (!string.IsNullOrEmpty(codigo))
        {
            var cacheKey = BuildCacheKey<T>(tenantId, codigo);
            _cache.Remove(cacheKey);
        }
        
        // Sempre invalida a lista completa
        var cacheKeyAll = BuildCacheKeyForAll<T>(tenantId);
        _cache.Remove(cacheKeyAll);
        
        _logger.LogDebug("Cache invalidado para {Type}:{TenantId}:{Codigo}", 
            typeof(T).Name, tenantId ?? "GLOBAL", codigo ?? "ALL");
    }

    /// <summary>
    /// Invalida todo o cache
    /// </summary>
    public void InvalidateAllCache()
    {
        // IMemoryCache não tem método Clear(), mas podemos usar reflection ou recrear
        // Para simplicidade, vamos usar uma abordagem mais simples
        if (_cache is MemoryCache memoryCache)
        {
            // Reflection para limpar o cache
            var field = typeof(MemoryCache).GetField("_entries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is IDictionary entries)
            {
                entries.Clear();
            }
        }
        
        _logger.LogInformation("Todo o cache de configurações foi invalidado");
    }

    /// <summary>
    /// Carrega configuração do banco de dados com fallback hierárquico
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigo">Código da configuração</param>
    /// <returns>Configuração encontrada ou null</returns>
    private async Task<T?> LoadFromDatabaseAsync<T>(string? tenantId, string codigo)
        where T : class, ITenantEntity
    {
        var dbSet = GetDbSet<T>();
        var query = dbSet.AsQueryable();
        
        // 1. Primeiro tenta buscar configuração específica do tenant
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantConfig = await query
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && 
                                        GetCodigoFromEntity(c) == codigo);
            
            if (tenantConfig != null)
                return tenantConfig;
        }
        
        // 2. Fallback para configuração global
        var globalConfig = await query
            .FirstOrDefaultAsync(c => c.TenantId == null && 
                                    GetCodigoFromEntity(c) == codigo);
        
        return globalConfig;
    }

    /// <summary>
    /// Carrega todas as configurações do banco de dados para um tenant
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de configurações</returns>
    private async Task<List<T>> LoadAllFromDatabaseAsync<T>(string? tenantId)
        where T : class, ITenantEntity
    {
        var dbSet = GetDbSet<T>();
        var query = dbSet.AsQueryable();
        
        if (string.IsNullOrEmpty(tenantId))
        {
            // Busca apenas configurações globais
            return await query
                .Where(c => c.TenantId == null)
                .Where(c => IsActiveConfiguration(c))
                .OrderBy(c => GetOrdemFromEntity(c))
                .ToListAsync();
        }
        else
        {
            // Busca configurações do tenant + globais (com tenant tendo prioridade)
            var tenantConfigs = await query
                .Where(c => c.TenantId == tenantId)
                .Where(c => IsActiveConfiguration(c))
                .ToListAsync();
            
            var globalConfigs = await query
                .Where(c => c.TenantId == null)
                .Where(c => IsActiveConfiguration(c))
                .ToListAsync();
            
            // Merge: tenant configs override global configs with same code
            var result = new List<T>();
            var tenantCodes = tenantConfigs.Select(GetCodigoFromEntity).ToHashSet();
            
            // Adiciona configs do tenant
            result.AddRange(tenantConfigs);
            
            // Adiciona configs globais que não foram sobrescritas
            result.AddRange(globalConfigs.Where(g => !tenantCodes.Contains(GetCodigoFromEntity(g))));
            
            return result.OrderBy(GetOrdemFromEntity).ToList();
        }
    }

    /// <summary>
    /// Obtém o DbSet apropriado para o tipo
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <returns>DbSet da entidade</returns>
    private DbSet<T> GetDbSet<T>() where T : class, ITenantEntity
    {
        var typeName = typeof(T).Name;
        
        return typeName switch
        {
            "ClassificacaoAnvisaEntity" => (DbSet<T>)(object)_context.ClassificacoesAnvisa,
            "StatusEstoqueEntity" => (DbSet<T>)(object)_context.StatusEstoque,
            "FormaPagamentoEntity" => (DbSet<T>)(object)_context.FormasPagamento,
            "StatusPagamentoEntity" => (DbSet<T>)(object)_context.StatusPagamento,
            "StatusSincronizacaoEntity" => (DbSet<T>)(object)_context.StatusSincronizacao,
            "TipoMovimentacaoEntity" => (DbSet<T>)(object)_context.TiposMovimentacao,
            _ => throw new NotSupportedException($"Tipo {typeName} não suportado pelo ConfigurationService")
        };
    }

    /// <summary>
    /// Constrói chave de cache para uma configuração específica
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigo">Código da configuração</param>
    /// <returns>Chave do cache</returns>
    private static string BuildCacheKey<T>(string? tenantId, string codigo) where T : class
    {
        return $"config:{typeof(T).Name}:{tenantId ?? "GLOBAL"}:{codigo}";
    }

    /// <summary>
    /// Constrói chave de cache para lista de configurações
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Chave do cache</returns>
    private static string BuildCacheKeyForAll<T>(string? tenantId) where T : class
    {
        return $"config:{typeof(T).Name}:{tenantId ?? "GLOBAL"}:ALL";
    }

    /// <summary>
    /// Obtém o código de uma entidade de configuração usando reflection
    /// </summary>
    /// <param name="entity">Entidade de configuração</param>
    /// <returns>Código da configuração</returns>
    private static string GetCodigoFromEntity<T>(T entity) where T : ITenantEntity
    {
        var property = typeof(T).GetProperty("Codigo");
        return property?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Obtém a ordem de uma entidade de configuração usando reflection
    /// </summary>
    /// <param name="entity">Entidade de configuração</param>
    /// <returns>Ordem da configuração</returns>
    private static int GetOrdemFromEntity<T>(T entity) where T : ITenantEntity
    {
        var property = typeof(T).GetProperty("Ordem");
        return property?.GetValue(entity) as int? ?? 0;
    }

    /// <summary>
    /// Verifica se uma configuração está ativa usando reflection
    /// </summary>
    /// <param name="entity">Entidade de configuração</param>
    /// <returns>True se ativa</returns>
    private static bool IsActiveConfiguration<T>(T entity) where T : ITenantEntity
    {
        var property = typeof(T).GetProperty("Ativo");
        return property?.GetValue(entity) as bool? ?? true;
    }

    /// <summary>
    /// Verifica se uma configuração pode ser removida
    /// </summary>
    /// <param name="entity">Entidade de configuração</param>
    /// <returns>True se pode ser removida</returns>
    private static bool CanBeDeleted<T>(T entity) where T : ITenantEntity
    {
        // Verifica propriedades que indicam proteção
        var isSistemaProperty = typeof(T).GetProperty("IsSistema");
        var isOficialAnvisaProperty = typeof(T).GetProperty("IsOficialAnvisa");
        
        if (isSistemaProperty?.GetValue(entity) as bool? == true)
            return false;
            
        if (isOficialAnvisaProperty?.GetValue(entity) as bool? == true)
            return false;
        
        return true;
    }
}

/// <summary>
/// Interface para o serviço de configuração
/// </summary>
public interface IConfigurationService
{
    Task<T?> GetByCodeAsync<T>(string? tenantId, string codigo) where T : class, ITenantEntity;
    Task<List<T>> GetAllByTenantAsync<T>(string? tenantId) where T : class, ITenantEntity;
    Task<T> SaveAsync<T>(T configuration) where T : class, ITenantEntity;
    Task<bool> DeleteAsync<T>(Guid id) where T : class, ITenantEntity;
    void InvalidateCache<T>(string? tenantId, string? codigo = null) where T : class, ITenantEntity;
    void InvalidateAllCache();
}