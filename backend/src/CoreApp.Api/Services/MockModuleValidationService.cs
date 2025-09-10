using CoreApp.Domain.Interfaces.Services;

namespace CoreApp.Api.Services;

/// <summary>
/// Implementação mock do serviço de validação de módulos para desenvolvimento
/// </summary>
public class MockModuleValidationService : IModuleValidationService
{
    public Task<bool> HasModuleAccessAsync(string tenantId, string codigoModulo, CancellationToken cancellationToken = default)
    {
        // Para desenvolvimento, sempre permitir acesso
        return Task.FromResult(true);
    }

    public Task<IEnumerable<string>> GetAvailableModulesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // Simular módulos disponíveis para desenvolvimento
        var modules = new[] { "PRODUCTS", "SALES", "STOCK", "CUSTOMERS", "REPORTS" };
        return Task.FromResult<IEnumerable<string>>(modules);
    }

    public Task<Dictionary<string, bool>> ValidateMultipleModulesAsync(string tenantId, IEnumerable<string> codigosModulos, CancellationToken cancellationToken = default)
    {
        // Para desenvolvimento, permitir todos os módulos
        var result = codigosModulos.ToDictionary(m => m, m => true);
        return Task.FromResult(result);
    }

    public Task<bool> CheckResourceLimitAsync(string tenantId, string codigoModulo, string tipoRecurso, int quantidadeUsada, CancellationToken cancellationToken = default)
    {
        // Para desenvolvimento, sempre dentro do limite
        return Task.FromResult(true);
    }

    public Task<ModuleValidationInfo> GetTenantPlanInfoAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        // Simular plano enterprise para desenvolvimento
        var planInfo = new ModuleValidationInfo
        {
            TenantId = tenantId,
            PlanoAtual = "Enterprise",
            DataVencimento = DateTime.UtcNow.AddYears(1),
            ModulosAtivos = new List<string> { "PRODUCTS", "SALES", "STOCK", "CUSTOMERS", "REPORTS" },
            PlanoAtivo = true,
            LimitesRecursos = new Dictionary<string, int>() // Sem limites no Enterprise
        };

        return Task.FromResult(planInfo);
    }

    public Task<OperationValidationResult> ValidateOperationAsync(string tenantId, string operacao, Dictionary<string, object>? contexto = null, CancellationToken cancellationToken = default)
    {
        // Para desenvolvimento, sempre permitir operações
        var result = new OperationValidationResult
        {
            Permitido = true,
            Motivo = "Operação permitida (desenvolvimento)"
        };

        return Task.FromResult(result);
    }
}