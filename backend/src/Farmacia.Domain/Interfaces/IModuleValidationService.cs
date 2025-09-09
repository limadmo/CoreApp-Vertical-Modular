using Farmacia.Domain.Entities;

namespace Farmacia.Domain.Interfaces;

/// <summary>
/// Interface para serviço de validação de módulos comerciais brasileiros
/// Gerencia quais módulos estão ativos para cada farmácia baseado no plano contratado
/// </summary>
/// <remarks>
/// Este serviço é crítico para o controle de acesso às funcionalidades pagas
/// do sistema SAAS farmacêutico brasileiro
/// </remarks>
public interface IModuleValidationService
{
    /// <summary>
    /// Verifica se tenant tem módulo específico ativo
    /// </summary>
    /// <param name="tenantId">ID do tenant (farmácia)</param>
    /// <param name="moduleCode">Código do módulo</param>
    /// <returns>True se módulo está ativo</returns>
    Task<bool> HasActiveModuleAsync(string tenantId, string moduleCode);

    /// <summary>
    /// Verifica múltiplos módulos de uma vez
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduleCodes">Lista de códigos de módulos</param>
    /// <returns>Dicionário com status de cada módulo</returns>
    Task<Dictionary<string, bool>> HasActiveModulesAsync(string tenantId, IEnumerable<string> moduleCodes);

    /// <summary>
    /// Obtém todos os módulos ativos para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de códigos dos módulos ativos</returns>
    Task<List<string>> GetActiveModulesAsync(string tenantId);

    /// <summary>
    /// Obtém informações detalhadas dos módulos ativos
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de módulos com informações completas</returns>
    Task<List<ModuloEntity>> GetActiveModuleDetailsAsync(string tenantId);

    /// <summary>
    /// Valida se tenant pode acessar módulo (lança exceção se não pode)
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduleCode">Código do módulo</param>
    /// <exception cref="ModuleNotActiveException">Quando módulo não está ativo</exception>
    Task ValidateModuleAsync(string tenantId, string moduleCode);

    /// <summary>
    /// Obtém informações do plano comercial ativo do tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Informações do plano ativo ou null se não encontrado</returns>
    Task<TenantPlanoEntity?> GetActivePlanAsync(string tenantId);

    /// <summary>
    /// Verifica se tenant está dentro dos limites do plano contratado
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Informações sobre limites e uso atual</returns>
    Task<PlanLimitsValidationResult> ValidatePlanLimitsAsync(string tenantId);

    /// <summary>
    /// Ativa módulo específico para tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduleCode">Código do módulo</param>
    /// <param name="activatedBy">Usuário que está ativando</param>
    /// <param name="reason">Motivo da ativação</param>
    Task ActivateModuleAsync(string tenantId, string moduleCode, string activatedBy, string? reason = null);

    /// <summary>
    /// Desativa módulo específico para tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduleCode">Código do módulo</param>
    /// <param name="deactivatedBy">Usuário que está desativando</param>
    /// <param name="reason">Motivo da desativação</param>
    Task DeactivateModuleAsync(string tenantId, string moduleCode, string deactivatedBy, string reason);

    /// <summary>
    /// Atualiza cache dos módulos ativos para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    Task RefreshCacheAsync(string tenantId);

    /// <summary>
    /// Limpa cache de todos os módulos de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    Task ClearCacheAsync(string tenantId);

    /// <summary>
    /// Obtém estatísticas de uso de módulos para relatórios
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Estatísticas de uso</returns>
    Task<ModuleUsageStatistics> GetUsageStatisticsAsync(string tenantId);
}

/// <summary>
/// Resultado da validação de limites do plano comercial
/// </summary>
public class PlanLimitsValidationResult
{
    /// <summary>
    /// Se tenant está dentro dos limites
    /// </summary>
    public bool IsWithinLimits { get; set; } = true;

    /// <summary>
    /// Plano comercial ativo
    /// </summary>
    public PlanoComercialEntity? ActivePlan { get; set; }

    /// <summary>
    /// Limites violados
    /// </summary>
    public List<PlanLimitViolation> Violations { get; set; } = new List<PlanLimitViolation>();

    /// <summary>
    /// Uso atual dos recursos
    /// </summary>
    public Dictionary<string, int> CurrentUsage { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Limites máximos do plano
    /// </summary>
    public Dictionary<string, int> PlanLimits { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Violação de limite do plano
/// </summary>
public class PlanLimitViolation
{
    /// <summary>
    /// Recurso que excedeu o limite
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Limite máximo permitido
    /// </summary>
    public int MaxLimit { get; set; }

    /// <summary>
    /// Uso atual
    /// </summary>
    public int CurrentUsage { get; set; }

    /// <summary>
    /// Quantidade excedida
    /// </summary>
    public int ExceededBy { get; set; }

    /// <summary>
    /// Mensagem de erro amigável
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Estatísticas de uso de módulos por tenant
/// </summary>
public class ModuleUsageStatistics
{
    /// <summary>
    /// ID do tenant
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Total de módulos disponíveis
    /// </summary>
    public int TotalModulesAvailable { get; set; }

    /// <summary>
    /// Total de módulos ativos
    /// </summary>
    public int TotalModulesActive { get; set; }

    /// <summary>
    /// Percentual de utilização
    /// </summary>
    public decimal UsagePercentage { get; set; }

    /// <summary>
    /// Módulos mais utilizados
    /// </summary>
    public List<ModuleUsageStat> MostUsedModules { get; set; } = new List<ModuleUsageStat>();

    /// <summary>
    /// Data da última atualização das estatísticas
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Estatística de uso de um módulo específico
/// </summary>
public class ModuleUsageStat
{
    /// <summary>
    /// Código do módulo
    /// </summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Nome do módulo
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// Categoria do módulo
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Se está ativo
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Data da última utilização
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Número de acessos no mês atual
    /// </summary>
    public int MonthlyAccesses { get; set; }

    /// <summary>
    /// Dados de configuração específica (JSON)
    /// </summary>
    public string? SpecificConfiguration { get; set; }
}

/// <summary>
/// Exceção lançada quando tenant tenta acessar módulo não ativo
/// </summary>
public class ModuleNotActiveException : Exception
{
    /// <summary>
    /// Código do módulo que causou a exceção
    /// </summary>
    public string ModuleCode { get; }

    /// <summary>
    /// ID do tenant
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// URL para upgrade de plano
    /// </summary>
    public string? UpgradeUrl { get; set; }

    public ModuleNotActiveException(string moduleCode, string tenantId, string message) 
        : base(message)
    {
        ModuleCode = moduleCode;
        TenantId = tenantId;
    }

    public ModuleNotActiveException(string moduleCode, string tenantId, string message, Exception innerException) 
        : base(message, innerException)
    {
        ModuleCode = moduleCode;
        TenantId = tenantId;
    }
}

/// <summary>
/// Exceção lançada quando tenant excede limites do plano
/// </summary>
public class PlanLimitExceededException : Exception
{
    /// <summary>
    /// Recurso que excedeu o limite
    /// </summary>
    public string Resource { get; }

    /// <summary>
    /// ID do tenant
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Limite máximo permitido
    /// </summary>
    public int MaxLimit { get; }

    /// <summary>
    /// Uso atual
    /// </summary>
    public int CurrentUsage { get; }

    /// <summary>
    /// URL para upgrade de plano
    /// </summary>
    public string? UpgradeUrl { get; set; }

    public PlanLimitExceededException(string resource, string tenantId, int maxLimit, int currentUsage, string message) 
        : base(message)
    {
        Resource = resource;
        TenantId = tenantId;
        MaxLimit = maxLimit;
        CurrentUsage = currentUsage;
    }
}