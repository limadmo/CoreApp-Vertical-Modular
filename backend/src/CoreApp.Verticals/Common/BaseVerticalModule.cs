using CoreApp.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CoreApp.Verticals.Common;

/// <summary>
/// Classe base abstrata para implementação de módulos verticais
/// Fornece funcionalidades comuns e padrões para todas as verticais
/// </summary>
/// <remarks>
/// Esta classe implementa os padrões básicos de validação, configuração e gerenciamento
/// de estado que todas as verticais precisam, evitando duplicação de código
/// </remarks>
public abstract class BaseVerticalModule : IVerticalModule
{
    protected readonly ILogger<BaseVerticalModule> Logger;
    protected readonly IModuleValidationService ModuleValidationService;

    protected BaseVerticalModule(
        ILogger<BaseVerticalModule> logger,
        IModuleValidationService moduleValidationService)
    {
        Logger = logger;
        ModuleValidationService = moduleValidationService;
    }

    public abstract string VerticalName { get; }
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract IEnumerable<string> RequiredModules { get; }
    public abstract IEnumerable<string> OptionalModules { get; }
    public abstract Dictionary<string, object> DefaultConfigurations { get; }

    /// <summary>
    /// Implementação padrão de validação baseada nos módulos necessários
    /// Pode ser sobrescrita pelas verticais filhas para validações específicas
    /// </summary>
    public virtual async Task<bool> CanActivateAsync(string tenantId, IEnumerable<string> availableModules)
    {
        try
        {
            var availableModulesList = availableModules.ToList();
            
            // Verifica se todos os módulos obrigatórios estão disponíveis
            foreach (var requiredModule in RequiredModules)
            {
                var hasModule = await ModuleValidationService.HasModuleAccessAsync(tenantId, requiredModule);
                if (!hasModule)
                {
                    Logger.LogWarning(
                        "Vertical {VerticalName} não pode ser ativada para tenant {TenantId}: módulo {ModuleName} não está disponível",
                        VerticalName, tenantId, requiredModule);
                    return false;
                }
            }

            // Log dos módulos opcionais disponíveis
            var optionalAvailable = OptionalModules.Where(optional => availableModulesList.Contains(optional)).ToList();
            if (optionalAvailable.Any())
            {
                Logger.LogInformation(
                    "Vertical {VerticalName} para tenant {TenantId} terá funcionalidades extras: {OptionalModules}",
                    VerticalName, tenantId, string.Join(", ", optionalAvailable));
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Erro ao validar ativação da vertical {VerticalName} para tenant {TenantId}",
                VerticalName, tenantId);
            return false;
        }
    }

    /// <summary>
    /// Implementação padrão de ativação
    /// Executa validações e chama método específico da vertical
    /// </summary>
    public virtual async Task<VerticalActivationResult> ActivateAsync(string tenantId, Dictionary<string, object>? configuration = null)
    {
        var result = new VerticalActivationResult();

        try
        {
            // Validação prévia
            var tenantModules = await ModuleValidationService.GetAvailableModulesAsync(tenantId);
            var canActivate = await CanActivateAsync(tenantId, tenantModules);

            if (!canActivate)
            {
                result.Success = false;
                result.Message = $"Vertical {VerticalName} não pode ser ativada para o tenant {tenantId}";
                result.Errors.Add("Módulos obrigatórios não disponíveis no plano atual");
                return result;
            }

            // Mescla configurações padrão com as específicas
            var finalConfiguration = new Dictionary<string, object>(DefaultConfigurations);
            if (configuration != null)
            {
                foreach (var kvp in configuration)
                {
                    finalConfiguration[kvp.Key] = kvp.Value;
                }
            }

            // Chama implementação específica da vertical
            var specificResult = await ActivateSpecificAsync(tenantId, finalConfiguration);
            
            if (specificResult.Success)
            {
                Logger.LogInformation(
                    "Vertical {VerticalName} ativada com sucesso para tenant {TenantId}",
                    VerticalName, tenantId);
            }

            return specificResult;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro ao ativar vertical {VerticalName} para tenant {TenantId}",
                VerticalName, tenantId);

            result.Success = false;
            result.Message = "Erro interno na ativação da vertical";
            result.Errors.Add($"Exceção: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Implementação padrão de desativação
    /// </summary>
    public virtual async Task<VerticalActivationResult> DeactivateAsync(string tenantId)
    {
        try
        {
            var result = await DeactivateSpecificAsync(tenantId);
            
            if (result.Success)
            {
                Logger.LogInformation(
                    "Vertical {VerticalName} desativada com sucesso para tenant {TenantId}",
                    VerticalName, tenantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro ao desativar vertical {VerticalName} para tenant {TenantId}",
                VerticalName, tenantId);

            return new VerticalActivationResult
            {
                Success = false,
                Message = "Erro interno na desativação da vertical",
                Errors = { $"Exceção: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Implementação padrão de validação de propriedades
    /// Pode ser sobrescrita pelas verticais específicas
    /// </summary>
    public virtual async Task<VerticalPropertiesValidationResult> ValidatePropertiesAsync(string entityType, Dictionary<string, object> properties)
    {
        // Implementação base - apenas verifica se as propriedades existem
        var result = new VerticalPropertiesValidationResult
        {
            IsValid = true,
            ValidatedData = new Dictionary<string, object>(properties)
        };

        // Validações específicas devem ser implementadas nas classes filhas
        await Task.CompletedTask;
        
        return result;
    }

    /// <summary>
    /// Método abstrato para implementação específica de ativação
    /// Cada vertical deve implementar sua lógica específica aqui
    /// </summary>
    protected abstract Task<VerticalActivationResult> ActivateSpecificAsync(string tenantId, Dictionary<string, object> configuration);

    /// <summary>
    /// Método abstrato para implementação específica de desativação
    /// </summary>
    protected abstract Task<VerticalActivationResult> DeactivateSpecificAsync(string tenantId);
}