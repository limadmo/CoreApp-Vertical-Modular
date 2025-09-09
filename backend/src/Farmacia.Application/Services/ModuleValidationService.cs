using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço de validação de módulos comerciais para sistema farmacêutico brasileiro
/// Controla acesso às funcionalidades baseado nos planos contratados
/// </summary>
/// <remarks>
/// Este serviço implementa cache Redis para alta performance na validação de módulos,
/// essencial para o funcionamento do sistema SAAS multi-tenant
/// </remarks>
public class ModuleValidationService : IModuleValidationService
{
    private readonly IModuloRepository _moduloRepository;
    private readonly ITenantPlanoRepository _tenantPlanoRepository;
    private readonly ITenantModuloRepository _tenantModuloRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ModuleValidationService> _logger;
    
    private const int CACHE_EXPIRATION_MINUTES = 30;
    private const string ACTIVE_MODULES_CACHE_KEY = "tenant:{0}:active-modules";
    private const string PLAN_INFO_CACHE_KEY = "tenant:{0}:plan-info";
    private const string USAGE_STATS_CACHE_KEY = "tenant:{0}:usage-stats";

    public ModuleValidationService(
        IModuloRepository moduloRepository,
        ITenantPlanoRepository tenantPlanoRepository,
        ITenantModuloRepository tenantModuloRepository,
        IDistributedCache cache,
        ILogger<ModuleValidationService> logger)
    {
        _moduloRepository = moduloRepository;
        _tenantPlanoRepository = tenantPlanoRepository;
        _tenantModuloRepository = tenantModuloRepository;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Verifica se tenant tem módulo específico ativo
    /// </summary>
    public async Task<bool> HasActiveModuleAsync(string tenantId, string moduleCode)
    {
        try
        {
            // Primeiro tenta obter do cache
            var activeModules = await GetActiveModulesFromCacheAsync(tenantId);
            if (activeModules != null)
            {
                return activeModules.Contains(moduleCode);
            }

            // Se não está em cache, busca do banco e atualiza cache
            activeModules = await LoadAndCacheActiveModulesAsync(tenantId);
            return activeModules.Contains(moduleCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar módulo ativo {ModuleCode} para tenant {TenantId}", 
                moduleCode, tenantId);
            
            // Em caso de erro, busca direto do banco sem cache
            return await HasActiveModuleFromDatabaseAsync(tenantId, moduleCode);
        }
    }

    /// <summary>
    /// Verifica múltiplos módulos de uma vez
    /// </summary>
    public async Task<Dictionary<string, bool>> HasActiveModulesAsync(string tenantId, IEnumerable<string> moduleCodes)
    {
        var result = new Dictionary<string, bool>();
        var activeModules = await GetActiveModulesAsync(tenantId);

        foreach (var moduleCode in moduleCodes)
        {
            result[moduleCode] = activeModules.Contains(moduleCode);
        }

        return result;
    }

    /// <summary>
    /// Obtém todos os módulos ativos para um tenant
    /// </summary>
    public async Task<List<string>> GetActiveModulesAsync(string tenantId)
    {
        try
        {
            // Primeiro tenta obter do cache
            var activeModules = await GetActiveModulesFromCacheAsync(tenantId);
            if (activeModules != null)
            {
                return activeModules;
            }

            // Se não está em cache, carrega do banco
            return await LoadAndCacheActiveModulesAsync(tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter módulos ativos para tenant {TenantId}", tenantId);
            
            // Em caso de erro, busca direto do banco
            return await GetActiveModulesFromDatabaseAsync(tenantId);
        }
    }

    /// <summary>
    /// Obtém informações detalhadas dos módulos ativos
    /// </summary>
    public async Task<List<ModuloEntity>> GetActiveModuleDetailsAsync(string tenantId)
    {
        var activeModuleCodes = await GetActiveModulesAsync(tenantId);
        
        if (!activeModuleCodes.Any())
            return new List<ModuloEntity>();

        return await _moduloRepository.GetByCodesAsync(activeModuleCodes);
    }

    /// <summary>
    /// Valida se tenant pode acessar módulo (lança exceção se não pode)
    /// </summary>
    public async Task ValidateModuleAsync(string tenantId, string moduleCode)
    {
        var hasModule = await HasActiveModuleAsync(tenantId, moduleCode);
        
        if (!hasModule)
        {
            var planInfo = await GetActivePlanAsync(tenantId);
            var planName = planInfo?.Plano?.Nome ?? "Nenhum";
            
            var message = $"O módulo '{moduleCode}' não está ativo para sua farmácia. " +
                         $"Plano atual: {planName}. " +
                         $"Faça upgrade do seu plano para acessar esta funcionalidade.";

            var exception = new ModuleNotActiveException(moduleCode, tenantId, message)
            {
                UpgradeUrl = "/upgrade-plan"
            };

            _logger.LogWarning("Acesso negado ao módulo {ModuleCode} para tenant {TenantId} - Plano: {Plan}", 
                moduleCode, tenantId, planName);
                
            throw exception;
        }

        _logger.LogDebug("Acesso autorizado ao módulo {ModuleCode} para tenant {TenantId}", 
            moduleCode, tenantId);
    }

    /// <summary>
    /// Obtém informações do plano comercial ativo do tenant
    /// </summary>
    public async Task<TenantPlanoEntity?> GetActivePlanAsync(string tenantId)
    {
        try
        {
            // Primeiro tenta obter do cache
            var cacheKey = string.Format(PLAN_INFO_CACHE_KEY, tenantId);
            var cachedPlan = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedPlan))
            {
                return JsonSerializer.Deserialize<TenantPlanoEntity>(cachedPlan);
            }

            // Se não está em cache, busca do banco
            var activePlan = await _tenantPlanoRepository.GetActiveByTenantAsync(tenantId);
            
            if (activePlan != null)
            {
                // Adiciona ao cache
                var serializedPlan = JsonSerializer.Serialize(activePlan);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES)
                };
                
                await _cache.SetStringAsync(cacheKey, serializedPlan, cacheOptions);
            }

            return activePlan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter plano ativo para tenant {TenantId}", tenantId);
            return null;
        }
    }

    /// <summary>
    /// Verifica se tenant está dentro dos limites do plano contratado
    /// </summary>
    public async Task<PlanLimitsValidationResult> ValidatePlanLimitsAsync(string tenantId)
    {
        var result = new PlanLimitsValidationResult();

        try
        {
            var activePlan = await GetActivePlanAsync(tenantId);
            if (activePlan?.Plano == null)
            {
                result.IsWithinLimits = false;
                result.Violations.Add(new PlanLimitViolation
                {
                    Resource = "PLANO",
                    Message = "Nenhum plano ativo encontrado para esta farmácia"
                });
                return result;
            }

            result.ActivePlan = activePlan.Plano;

            // Verifica limites específicos do plano
            await ValidateUserLimitAsync(tenantId, activePlan.Plano, result);
            await ValidateProductLimitAsync(tenantId, activePlan.Plano, result);
            await ValidateCustomerLimitAsync(tenantId, activePlan.Plano, result);
            await ValidateStorageLimitAsync(tenantId, activePlan.Plano, result);

            result.IsWithinLimits = !result.Violations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar limites do plano para tenant {TenantId}", tenantId);
            result.IsWithinLimits = false;
            result.Violations.Add(new PlanLimitViolation
            {
                Resource = "SYSTEM",
                Message = "Erro interno ao validar limites do plano"
            });
        }

        return result;
    }

    /// <summary>
    /// Ativa módulo específico para tenant
    /// </summary>
    public async Task ActivateModuleAsync(string tenantId, string moduleCode, string activatedBy, string? reason = null)
    {
        try
        {
            var modulo = await _moduloRepository.GetByCodeAsync(moduleCode);
            if (modulo == null)
                throw new ArgumentException($"Módulo '{moduleCode}' não encontrado");

            // Verifica se já existe registro
            var existing = await _tenantModuloRepository.GetByTenantAndModuleAsync(tenantId, modulo.Id);
            
            if (existing != null)
            {
                existing.Ativar(activatedBy, reason);
                await _tenantModuloRepository.UpdateAsync(existing);
            }
            else
            {
                var tenantModulo = TenantModuloEntity.CriarNova(tenantId, modulo.Id, activatedBy);
                tenantModulo.Motivo = reason;
                await _tenantModuloRepository.AddAsync(tenantModulo);
            }

            // Limpa cache
            await ClearCacheAsync(tenantId);

            _logger.LogInformation("Módulo {ModuleCode} ativado para tenant {TenantId} por {User}", 
                moduleCode, tenantId, activatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar módulo {ModuleCode} para tenant {TenantId}", 
                moduleCode, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Desativa módulo específico para tenant
    /// </summary>
    public async Task DeactivateModuleAsync(string tenantId, string moduleCode, string deactivatedBy, string reason)
    {
        try
        {
            var modulo = await _moduloRepository.GetByCodeAsync(moduleCode);
            if (modulo == null)
                throw new ArgumentException($"Módulo '{moduleCode}' não encontrado");

            var tenantModulo = await _tenantModuloRepository.GetByTenantAndModuleAsync(tenantId, modulo.Id);
            if (tenantModulo != null)
            {
                tenantModulo.Desativar(deactivatedBy, reason);
                await _tenantModuloRepository.UpdateAsync(tenantModulo);

                // Limpa cache
                await ClearCacheAsync(tenantId);

                _logger.LogInformation("Módulo {ModuleCode} desativado para tenant {TenantId} por {User} - Motivo: {Reason}", 
                    moduleCode, tenantId, deactivatedBy, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar módulo {ModuleCode} para tenant {TenantId}", 
                moduleCode, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Atualiza cache dos módulos ativos para um tenant
    /// </summary>
    public async Task RefreshCacheAsync(string tenantId)
    {
        try
        {
            await ClearCacheAsync(tenantId);
            await LoadAndCacheActiveModulesAsync(tenantId);
            
            _logger.LogDebug("Cache de módulos atualizado para tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cache para tenant {TenantId}", tenantId);
        }
    }

    /// <summary>
    /// Limpa cache de todos os módulos de um tenant
    /// </summary>
    public async Task ClearCacheAsync(string tenantId)
    {
        try
        {
            var keys = new[]
            {
                string.Format(ACTIVE_MODULES_CACHE_KEY, tenantId),
                string.Format(PLAN_INFO_CACHE_KEY, tenantId),
                string.Format(USAGE_STATS_CACHE_KEY, tenantId)
            };

            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar cache para tenant {TenantId}", tenantId);
        }
    }

    /// <summary>
    /// Obtém estatísticas de uso de módulos para relatórios
    /// </summary>
    public async Task<ModuleUsageStatistics> GetUsageStatisticsAsync(string tenantId)
    {
        try
        {
            // Primeiro tenta obter do cache
            var cacheKey = string.Format(USAGE_STATS_CACHE_KEY, tenantId);
            var cachedStats = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedStats))
            {
                return JsonSerializer.Deserialize<ModuleUsageStatistics>(cachedStats) 
                       ?? new ModuleUsageStatistics();
            }

            // Calcula estatísticas
            var stats = await CalculateUsageStatisticsAsync(tenantId);
            
            // Adiciona ao cache
            var serializedStats = JsonSerializer.Serialize(stats);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Cache por 1 hora
            };
            
            await _cache.SetStringAsync(cacheKey, serializedStats, cacheOptions);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de uso para tenant {TenantId}", tenantId);
            return new ModuleUsageStatistics { TenantId = tenantId };
        }
    }

    #region Private Methods

    /// <summary>
    /// Obtém módulos ativos do cache
    /// </summary>
    private async Task<List<string>?> GetActiveModulesFromCacheAsync(string tenantId)
    {
        try
        {
            var cacheKey = string.Format(ACTIVE_MODULES_CACHE_KEY, tenantId);
            var cachedModules = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedModules))
            {
                return JsonSerializer.Deserialize<List<string>>(cachedModules);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao obter módulos do cache para tenant {TenantId}", tenantId);
            return null;
        }
    }

    /// <summary>
    /// Carrega módulos ativos do banco e atualiza cache
    /// </summary>
    private async Task<List<string>> LoadAndCacheActiveModulesAsync(string tenantId)
    {
        var activeModules = await GetActiveModulesFromDatabaseAsync(tenantId);

        try
        {
            // Adiciona ao cache
            var cacheKey = string.Format(ACTIVE_MODULES_CACHE_KEY, tenantId);
            var serializedModules = JsonSerializer.Serialize(activeModules);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES)
            };

            await _cache.SetStringAsync(cacheKey, serializedModules, cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao adicionar módulos ao cache para tenant {TenantId}", tenantId);
        }

        return activeModules;
    }

    /// <summary>
    /// Obtém módulos ativos direto do banco de dados
    /// </summary>
    private async Task<List<string>> GetActiveModulesFromDatabaseAsync(string tenantId)
    {
        // Primeiro obtém o plano ativo
        var activePlan = await _tenantPlanoRepository.GetActiveByTenantAsync(tenantId);
        if (activePlan?.Plano == null)
        {
            _logger.LogWarning("Nenhum plano ativo encontrado para tenant {TenantId}", tenantId);
            return new List<string>();
        }

        // Obtém módulos do plano
        var planModules = activePlan.Plano.ObterCodigosModulos();

        // Obtém customizações específicas do tenant
        var tenantModules = await _tenantModuloRepository.GetActiveByTenantAsync(tenantId);
        var customModules = tenantModules
            .Where(tm => tm.Modulo != null)
            .Select(tm => tm.Modulo!.Codigo)
            .ToList();

        // Combina módulos do plano com customizações
        var allModules = new HashSet<string>(planModules);
        foreach (var customModule in customModules)
        {
            allModules.Add(customModule);
        }

        // Remove módulos desativados especificamente
        var deactivatedModules = tenantModules
            .Where(tm => !tm.Ativo && tm.Modulo != null)
            .Select(tm => tm.Modulo!.Codigo)
            .ToList();

        foreach (var deactivated in deactivatedModules)
        {
            allModules.Remove(deactivated);
        }

        return allModules.ToList();
    }

    /// <summary>
    /// Verifica módulo ativo direto do banco (fallback)
    /// </summary>
    private async Task<bool> HasActiveModuleFromDatabaseAsync(string tenantId, string moduleCode)
    {
        var activeModules = await GetActiveModulesFromDatabaseAsync(tenantId);
        return activeModules.Contains(moduleCode);
    }

    /// <summary>
    /// Valida limite de usuários
    /// </summary>
    private async Task ValidateUserLimitAsync(string tenantId, PlanoComercialEntity plano, PlanLimitsValidationResult result)
    {
        // Implementação seria obtida de um repository de usuários
        // Por enquanto, exemplo básico
        var currentUsers = 5; // await _usuarioRepository.CountByTenantAsync(tenantId);
        
        result.CurrentUsage["usuarios"] = currentUsers;
        result.PlanLimits["usuarios"] = plano.MaximoUsuarios;

        if (currentUsers > plano.MaximoUsuarios)
        {
            result.Violations.Add(new PlanLimitViolation
            {
                Resource = "usuarios",
                MaxLimit = plano.MaximoUsuarios,
                CurrentUsage = currentUsers,
                ExceededBy = currentUsers - plano.MaximoUsuarios,
                Message = $"Limite de usuários excedido: {currentUsers}/{plano.MaximoUsuarios}"
            });
        }
    }

    /// <summary>
    /// Valida limite de produtos
    /// </summary>
    private async Task ValidateProductLimitAsync(string tenantId, PlanoComercialEntity plano, PlanLimitsValidationResult result)
    {
        // Implementação seria obtida de um repository de produtos
        var currentProducts = 1500; // await _produtoRepository.CountByTenantAsync(tenantId);
        
        result.CurrentUsage["produtos"] = currentProducts;
        result.PlanLimits["produtos"] = plano.MaximoProdutos;

        if (currentProducts > plano.MaximoProdutos)
        {
            result.Violations.Add(new PlanLimitViolation
            {
                Resource = "produtos",
                MaxLimit = plano.MaximoProdutos,
                CurrentUsage = currentProducts,
                ExceededBy = currentProducts - plano.MaximoProdutos,
                Message = $"Limite de produtos excedido: {currentProducts}/{plano.MaximoProdutos}"
            });
        }
    }

    /// <summary>
    /// Valida limite de clientes
    /// </summary>
    private async Task ValidateCustomerLimitAsync(string tenantId, PlanoComercialEntity plano, PlanLimitsValidationResult result)
    {
        // Implementação seria obtida de um repository de clientes
        var currentCustomers = 800; // await _clienteRepository.CountByTenantAsync(tenantId);
        
        result.CurrentUsage["clientes"] = currentCustomers;
        result.PlanLimits["clientes"] = plano.MaximoClientes;

        if (currentCustomers > plano.MaximoClientes)
        {
            result.Violations.Add(new PlanLimitViolation
            {
                Resource = "clientes",
                MaxLimit = plano.MaximoClientes,
                CurrentUsage = currentCustomers,
                ExceededBy = currentCustomers - plano.MaximoClientes,
                Message = $"Limite de clientes excedido: {currentCustomers}/{plano.MaximoClientes}"
            });
        }
    }

    /// <summary>
    /// Valida limite de armazenamento
    /// </summary>
    private async Task ValidateStorageLimitAsync(string tenantId, PlanoComercialEntity plano, PlanLimitsValidationResult result)
    {
        // Implementação seria calculada baseada em arquivos/dados armazenados
        var currentStorageGB = 3; // await CalculateStorageUsageAsync(tenantId);
        
        result.CurrentUsage["armazenamento"] = currentStorageGB;
        result.PlanLimits["armazenamento"] = plano.EspacoArmazenamentoGB;

        if (currentStorageGB > plano.EspacoArmazenamentoGB)
        {
            result.Violations.Add(new PlanLimitViolation
            {
                Resource = "armazenamento",
                MaxLimit = plano.EspacoArmazenamentoGB,
                CurrentUsage = currentStorageGB,
                ExceededBy = currentStorageGB - plano.EspacoArmazenamentoGB,
                Message = $"Limite de armazenamento excedido: {currentStorageGB}GB/{plano.EspacoArmazenamentoGB}GB"
            });
        }
    }

    /// <summary>
    /// Calcula estatísticas de uso de módulos
    /// </summary>
    private async Task<ModuleUsageStatistics> CalculateUsageStatisticsAsync(string tenantId)
    {
        var stats = new ModuleUsageStatistics
        {
            TenantId = tenantId,
            LastUpdated = DateTime.UtcNow
        };

        // Obtém módulos ativos
        var activeModules = await GetActiveModulesAsync(tenantId);
        var moduleDetails = await GetActiveModuleDetailsAsync(tenantId);

        stats.TotalModulesActive = activeModules.Count;
        stats.TotalModulesAvailable = await _moduloRepository.CountActiveAsync();
        
        if (stats.TotalModulesAvailable > 0)
        {
            stats.UsagePercentage = Math.Round(
                (decimal)stats.TotalModulesActive / stats.TotalModulesAvailable * 100, 2);
        }

        // Cria estatísticas por módulo
        foreach (var module in moduleDetails)
        {
            stats.MostUsedModules.Add(new ModuleUsageStat
            {
                ModuleCode = module.Codigo,
                ModuleName = module.Nome,
                Category = module.Categoria,
                IsActive = true,
                MonthlyAccesses = 0, // Seria calculado de logs de acesso
                LastUsed = DateTime.UtcNow // Seria obtido de logs
            });
        }

        return stats;
    }

    #endregion
}