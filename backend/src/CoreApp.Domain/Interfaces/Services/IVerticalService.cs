using CoreApp.Domain.Interfaces.Common;

namespace CoreApp.Domain.Interfaces.Services;

/// <summary>
/// Interface base para serviços específicos de verticais de negócio
/// Permite implementar lógicas especializadas por tipo de comércio
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade que implementa IVerticalEntity</typeparam>
public interface IVerticalService<TEntity> where TEntity : class, IVerticalEntity
{
    /// <summary>
    /// Identificador do vertical que o serviço atende
    /// </summary>
    string VerticalType { get; }

    /// <summary>
    /// Valida uma entidade conforme regras específicas do vertical
    /// </summary>
    /// <param name="entity">Entidade a ser validada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da validação</returns>
    Task<VerticalValidationResult> ValidateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa uma entidade aplicando regras específicas do vertical
    /// </summary>
    /// <param name="entity">Entidade a ser processada</param>
    /// <param name="operation">Tipo de operação (CREATE, UPDATE, DELETE)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade processada</returns>
    Task<TEntity> ProcessAsync(TEntity entity, string operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula valores específicos do vertical (preços, impostos, descontos, etc.)
    /// </summary>
    /// <param name="entity">Entidade base para cálculo</param>
    /// <param name="calculationType">Tipo de cálculo</param>
    /// <param name="parameters">Parâmetros do cálculo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado do cálculo</returns>
    Task<VerticalCalculationResult> CalculateAsync(TEntity entity, string calculationType, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriquece uma entidade com dados específicos do vertical
    /// </summary>
    /// <param name="entity">Entidade a ser enriquecida</param>
    /// <param name="enrichmentType">Tipo de enriquecimento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade enriquecida</returns>
    Task<TEntity> EnrichAsync(TEntity entity, string enrichmentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém configurações padrão para o vertical
    /// </summary>
    /// <param name="configType">Tipo de configuração</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Configurações do vertical</returns>
    Task<Dictionary<string, object>> GetDefaultConfigurationAsync(string configType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma operação é suportada pelo vertical
    /// </summary>
    /// <param name="operation">Nome da operação</param>
    /// <returns>True se a operação é suportada</returns>
    bool SupportsOperation(string operation);

    /// <summary>
    /// Obtém metadados específicos do vertical
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Metadados do vertical</returns>
    Task<VerticalMetadata> GetMetadataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de validação específica de vertical
/// </summary>
public class VerticalValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
    public string? RecommendedAction { get; set; }
}

/// <summary>
/// Resultado de cálculo específico de vertical
/// </summary>
public class VerticalCalculationResult
{
    public decimal Value { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public Dictionary<string, decimal> Breakdown { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool IsEstimate { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Metadados de um vertical de negócio
/// </summary>
public class VerticalMetadata
{
    public string VerticalType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> SupportedOperations { get; set; } = new();
    public List<string> RequiredModules { get; set; } = new();
    public Dictionary<string, object> Capabilities { get; set; } = new();
    public bool IsActive { get; set; } = true;
}