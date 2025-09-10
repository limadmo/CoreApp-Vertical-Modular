using CoreApp.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace CoreApp.Verticals.Common;

/// <summary>
/// Implementação do gerenciador centralizado de verticais
/// Gerencia descoberta automática, registro e ativação de verticais de forma thread-safe
/// </summary>
/// <remarks>
/// Este serviço é responsável por orquestrar todas as verticais do sistema,
/// garantindo isolamento por tenant e validação adequada de módulos
/// </remarks>
public class VerticalManager : IVerticalManager
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _tenantActiveVerticals = new();
    private readonly ILogger<VerticalManager> _logger;
    private readonly IModuleValidationService _moduleValidationService;
    private readonly IVerticalRegistry _verticalRegistry;
    private readonly IServiceProvider _serviceProvider;

    public VerticalManager(
        ILogger<VerticalManager> logger,
        IModuleValidationService moduleValidationService,
        IVerticalRegistry verticalRegistry,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _moduleValidationService = moduleValidationService;
        _verticalRegistry = verticalRegistry;
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IVerticalModule> GetAllVerticals()
    {
        return _verticalRegistry.GetAllVerticals();
    }

    public IVerticalModule? GetVertical(string verticalName)
    {
        return _verticalRegistry.GetVertical(verticalName);
    }

    public async Task<IEnumerable<IVerticalModule>> GetActiveVerticalsAsync(string tenantId)
    {
        if (!_tenantActiveVerticals.TryGetValue(tenantId, out var activeVerticalNames))
        {
            return Enumerable.Empty<IVerticalModule>();
        }

        var activeVerticals = new List<IVerticalModule>();
        
        foreach (var verticalName in activeVerticalNames)
        {
            var vertical = _verticalRegistry.GetVertical(verticalName);
            if (vertical != null)
            {
                activeVerticals.Add(vertical);
            }
        }

        await Task.CompletedTask;
        return activeVerticals;
    }

    public async Task<IEnumerable<AvailableVertical>> GetAvailableVerticalsAsync(string tenantId, IEnumerable<string> availableModules)
    {
        var availableModulesList = availableModules.ToList();
        var availableVerticals = new List<AvailableVertical>();

        foreach (var vertical in _verticalRegistry.GetAllVerticals())
        {
            try
            {
                var canActivate = await vertical.CanActivateAsync(tenantId, availableModulesList);
                var missingModules = vertical.RequiredModules.Where(req => !availableModulesList.Contains(req)).ToList();
                var availableOptional = vertical.OptionalModules.Where(opt => availableModulesList.Contains(opt)).ToList();

                availableVerticals.Add(new AvailableVertical
                {
                    Name = vertical.VerticalName,
                    Version = vertical.Version,
                    Description = vertical.Description,
                    CanActivate = canActivate,
                    MissingModules = missingModules,
                    AvailableOptionalModules = availableOptional,
                    DefaultConfiguration = vertical.DefaultConfigurations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao avaliar disponibilidade da vertical {VerticalName} para tenant {TenantId}",
                    vertical.VerticalName, tenantId);
            }
        }

        return availableVerticals;
    }

    public async Task<VerticalActivationResult> ActivateVerticalAsync(string tenantId, string verticalName, Dictionary<string, object>? configuration = null)
    {
        try
        {
            var vertical = _verticalRegistry.GetVertical(verticalName);
            if (vertical == null)
            {
                return new VerticalActivationResult
                {
                    Success = false,
                    Message = $"Vertical '{verticalName}' não encontrada",
                    Errors = { $"Vertical '{verticalName}' não está registrada no sistema" }
                };
            }

            // Verifica se já está ativa
            var activeVerticals = _tenantActiveVerticals.GetOrAdd(tenantId, _ => new HashSet<string>());
            if (activeVerticals.Contains(verticalName))
            {
                return new VerticalActivationResult
                {
                    Success = false,
                    Message = $"Vertical '{verticalName}' já está ativa para o tenant",
                    Warnings = { $"Vertical '{verticalName}' já estava ativa" }
                };
            }

            // Tenta ativar a vertical
            var activationResult = await vertical.ActivateAsync(tenantId, configuration);

            if (activationResult.Success)
            {
                // Adiciona à lista de verticais ativas do tenant
                activeVerticals.Add(verticalName);
                
                _logger.LogInformation(
                    "Vertical {VerticalName} ativada com sucesso para tenant {TenantId}",
                    verticalName, tenantId);
            }
            else
            {
                _logger.LogWarning(
                    "Falha na ativação da vertical {VerticalName} para tenant {TenantId}: {Message}",
                    verticalName, tenantId, activationResult.Message);
            }

            return activationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao ativar vertical {VerticalName} para tenant {TenantId}",
                verticalName, tenantId);

            return new VerticalActivationResult
            {
                Success = false,
                Message = "Erro interno na ativação da vertical",
                Errors = { $"Exceção: {ex.Message}" }
            };
        }
    }

    public async Task<VerticalActivationResult> DeactivateVerticalAsync(string tenantId, string verticalName)
    {
        try
        {
            var vertical = _verticalRegistry.GetVertical(verticalName);
            if (vertical == null)
            {
                return new VerticalActivationResult
                {
                    Success = false,
                    Message = $"Vertical '{verticalName}' não encontrada",
                    Errors = { $"Vertical '{verticalName}' não está registrada no sistema" }
                };
            }

            // Verifica se está ativa
            if (!_tenantActiveVerticals.TryGetValue(tenantId, out var activeVerticals) || 
                !activeVerticals.Contains(verticalName))
            {
                return new VerticalActivationResult
                {
                    Success = false,
                    Message = $"Vertical '{verticalName}' não está ativa para o tenant",
                    Warnings = { $"Vertical '{verticalName}' já estava inativa" }
                };
            }

            // Tenta desativar a vertical
            var deactivationResult = await vertical.DeactivateAsync(tenantId);

            if (deactivationResult.Success)
            {
                // Remove da lista de verticais ativas do tenant
                activeVerticals.Remove(verticalName);
                
                _logger.LogInformation(
                    "Vertical {VerticalName} desativada com sucesso para tenant {TenantId}",
                    verticalName, tenantId);
            }
            else
            {
                _logger.LogWarning(
                    "Falha na desativação da vertical {VerticalName} para tenant {TenantId}: {Message}",
                    verticalName, tenantId, deactivationResult.Message);
            }

            return deactivationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao desativar vertical {VerticalName} para tenant {TenantId}",
                verticalName, tenantId);

            return new VerticalActivationResult
            {
                Success = false,
                Message = "Erro interno na desativação da vertical",
                Errors = { $"Exceção: {ex.Message}" }
            };
        }
    }

    public async Task<VerticalPropertiesValidationResult> ValidateVerticalPropertiesAsync(string tenantId, string verticalName, string entityType, Dictionary<string, object> properties)
    {
        try
        {
            var vertical = _verticalRegistry.GetVertical(verticalName);
            if (vertical == null)
            {
                return new VerticalPropertiesValidationResult
                {
                    IsValid = false,
                    Errors = { $"Vertical '{verticalName}' não encontrada" }
                };
            }

            // Verifica se a vertical está ativa para o tenant
            if (!_tenantActiveVerticals.TryGetValue(tenantId, out var activeVerticals) || 
                !activeVerticals.Contains(verticalName))
            {
                return new VerticalPropertiesValidationResult
                {
                    IsValid = false,
                    Errors = { $"Vertical '{verticalName}' não está ativa para o tenant" }
                };
            }

            return await vertical.ValidatePropertiesAsync(entityType, properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao validar propriedades da vertical {VerticalName} para tenant {TenantId}",
                verticalName, tenantId);

            return new VerticalPropertiesValidationResult
            {
                IsValid = false,
                Errors = { $"Erro interno na validação: {ex.Message}" }
            };
        }
    }

    public void RegisterVertical(IVerticalModule vertical)
    {
        if (vertical == null)
            throw new ArgumentNullException(nameof(vertical));

        if (string.IsNullOrWhiteSpace(vertical.VerticalName))
            throw new ArgumentException("Nome da vertical não pode ser vazio", nameof(vertical));

        _verticalRegistry.RegisterVertical(vertical);
        _logger.LogInformation(
            "Vertical {VerticalName} v{Version} registrada com sucesso",
            vertical.VerticalName, vertical.Version);
    }

    public void UnregisterVertical(string verticalName)
    {
        if (string.IsNullOrWhiteSpace(verticalName))
            throw new ArgumentException("Nome da vertical não pode ser vazio", nameof(verticalName));

        _verticalRegistry.UnregisterVertical(verticalName);
        
        // Remove de todos os tenants ativos
        foreach (var tenantVerticals in _tenantActiveVerticals.Values)
        {
            tenantVerticals.Remove(verticalName);
        }

        _logger.LogInformation(
            "Vertical {VerticalName} removida do sistema",
            verticalName);
    }
}