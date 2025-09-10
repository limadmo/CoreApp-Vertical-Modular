using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface para repositórios especializados em verticais de negócio
/// Estende funcionalidades básicas com operações específicas por vertical
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade que implementa IVerticalEntity e ITenantEntity</typeparam>
public interface IVerticalRepository<TEntity> : IBaseRepository<TEntity> 
    where TEntity : class, IVerticalEntity, ITenantEntity
{
    /// <summary>
    /// Obtém entidades filtradas por tipo de vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical (ex: "PADARIA", "FARMACIA")</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades do vertical especificado</returns>
    Task<IEnumerable<TEntity>> GetByVerticalTypeAsync(string verticalType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém entidades por propriedade vertical específica
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="propertyName">Nome da propriedade vertical</param>
    /// <param name="propertyValue">Valor da propriedade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades que possuem a propriedade com o valor especificado</returns>
    Task<IEnumerable<TEntity>> GetByVerticalPropertyAsync(string verticalType, string propertyName, object propertyValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades usando query específica do vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="verticalQuery">Query específica do vertical em formato JSON</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades que atendem a query</returns>
    Task<IEnumerable<TEntity>> SearchByVerticalQueryAsync(string verticalType, string verticalQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas agregadas por vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="aggregationType">Tipo de agregação (COUNT, SUM, AVG, etc.)</param>
    /// <param name="propertyName">Nome da propriedade para agregação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da agregação</returns>
    Task<VerticalAggregationResult> GetVerticalAggregationAsync(string verticalType, string aggregationType, string? propertyName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida consistência das propriedades verticais
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da validação</returns>
    Task<VerticalConsistencyResult> ValidateVerticalConsistencyAsync(string verticalType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Migra entidades para nova versão do schema vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="fromVersion">Versão origem</param>
    /// <param name="toVersion">Versão destino</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da migração</returns>
    Task<VerticalMigrationResult> MigrateVerticalSchemaAsync(string verticalType, string fromVersion, string toVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém entidades que requerem processamento específico do vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="processingType">Tipo de processamento necessário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades que requerem processamento</returns>
    Task<IEnumerable<TEntity>> GetPendingVerticalProcessingAsync(string verticalType, string processingType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa operação em lote específica do vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="operation">Operação a ser executada</param>
    /// <param name="entities">Entidades alvo da operação</param>
    /// <param name="parameters">Parâmetros da operação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação em lote</returns>
    Task<VerticalBatchResult> ExecuteVerticalBatchOperationAsync(string verticalType, string operation, IEnumerable<TEntity> entities, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém métricas de desempenho específicas do vertical
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="metricsType">Tipo de métricas</param>
    /// <param name="dateRange">Período para cálculo das métricas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Métricas do vertical</returns>
    Task<VerticalMetrics> GetVerticalMetricsAsync(string verticalType, string metricsType, DateRange dateRange, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de agregação vertical
/// </summary>
public class VerticalAggregationResult
{
    public string VerticalType { get; set; } = string.Empty;
    public string AggregationType { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public object Value { get; set; } = new();
    public int Count { get; set; }
    public Dictionary<string, object> Breakdown { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resultado de validação de consistência vertical
/// </summary>
public class VerticalConsistencyResult
{
    public string VerticalType { get; set; } = string.Empty;
    public bool IsConsistent { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public int TotalEntities { get; set; }
    public int ValidEntities { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Resultado de migração de schema vertical
/// </summary>
public class VerticalMigrationResult
{
    public string VerticalType { get; set; } = string.Empty;
    public string FromVersion { get; set; } = string.Empty;
    public string ToVersion { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int MigratedEntities { get; set; }
    public int FailedEntities { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime MigratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resultado de operação em lote vertical
/// </summary>
public class VerticalBatchResult
{
    public string VerticalType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ProcessedEntities { get; set; }
    public int SuccessfulEntities { get; set; }
    public int FailedEntities { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Results { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Métricas específicas de vertical
/// </summary>
public class VerticalMetrics
{
    public string VerticalType { get; set; } = string.Empty;
    public string MetricsType { get; set; } = string.Empty;
    public DateRange DateRange { get; set; } = new();
    public Dictionary<string, decimal> Values { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Período de tempo para métricas
/// </summary>
public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public DateRange() { }
    
    public DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
    
    public TimeSpan Duration => EndDate - StartDate;
    public bool IsValid => EndDate >= StartDate;
}