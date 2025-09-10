namespace CoreApp.Domain.Interfaces.Services;

/// <summary>
/// Interface para serviço de validação de módulos
/// Gerencia acesso a funcionalidades baseado em planos comerciais
/// </summary>
public interface IModuleValidationService
{
    /// <summary>
    /// Verifica se um tenant tem acesso a um módulo específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigoModulo">Código do módulo (ex: PRODUCTS, SALES)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se tem acesso ao módulo</returns>
    Task<bool> HasModuleAccessAsync(string tenantId, string codigoModulo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os módulos disponíveis para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de códigos de módulos disponíveis</returns>
    Task<IEnumerable<string>> GetAvailableModulesAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida múltiplos módulos de uma vez
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigosModulos">Lista de códigos de módulos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dicionário com resultado da validação para cada módulo</returns>
    Task<Dictionary<string, bool>> ValidateMultipleModulesAsync(string tenantId, IEnumerable<string> codigosModulos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica limites de uso de um módulo para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <param name="tipoRecurso">Tipo de recurso (ex: produtos, vendas)</param>
    /// <param name="quantidadeUsada">Quantidade atualmente usada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se ainda pode usar o recurso</returns>
    Task<bool> CheckResourceLimitAsync(string tenantId, string codigoModulo, string tipoRecurso, int quantidadeUsada, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações detalhadas do plano atual do tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Informações do plano atual</returns>
    Task<ModuleValidationInfo> GetTenantPlanInfoAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se uma operação é permitida para o tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="operacao">Nome da operação</param>
    /// <param name="contexto">Contexto adicional da operação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da validação</returns>
    Task<OperationValidationResult> ValidateOperationAsync(string tenantId, string operacao, Dictionary<string, object>? contexto = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Informações de validação de módulo
/// </summary>
public class ModuleValidationInfo
{
    public string TenantId { get; set; } = string.Empty;
    public string PlanoAtual { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public List<string> ModulosAtivos { get; set; } = new();
    public Dictionary<string, int> LimitesRecursos { get; set; } = new();
    public bool PlanoAtivo { get; set; }
}

/// <summary>
/// Resultado de validação de operação
/// </summary>
public class OperationValidationResult
{
    public bool Permitido { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? ModuloNecessario { get; set; }
    public string? PlanoNecessario { get; set; }
    public Dictionary<string, object> DetalhesAdicionais { get; set; } = new();
}