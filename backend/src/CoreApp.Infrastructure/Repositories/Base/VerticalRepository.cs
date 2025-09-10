using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Interfaces.Repositories;
using CoreApp.Domain.Entities.Common;
using CoreApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreApp.Infrastructure.Repositories.Base;

/// <summary>
/// Implementação do repositório para entidades com suporte a verticais de negócio
/// Estende funcionalidades básicas com operações específicas por vertical
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade que implementa IVerticalEntity e ITenantEntity</typeparam>
public class VerticalRepository<TEntity> : BaseRepository<TEntity>, IVerticalRepository<TEntity>
    where TEntity : class, IVerticalEntity, ITenantEntity
{
    private readonly ITenantContext _tenantContext;

    public VerticalRepository(
        CoreAppDbContext context, 
        ILogger logger,
        ITenantContext tenantContext) 
        : base(context, logger)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <summary>
    /// Obtém entidades filtradas por tipo de vertical
    /// </summary>
    public async Task<IEnumerable<TEntity>> GetByVerticalTypeAsync(string verticalType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug("Buscando entidades por vertical - Tipo: {VerticalType}, Tenant: {TenantId}", 
                verticalType, tenantId);

            return await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar entidades por tipo vertical: {VerticalType}", verticalType);
            throw;
        }
    }

    /// <summary>
    /// Obtém entidades por propriedade vertical específica
    /// </summary>
    public async Task<IEnumerable<TEntity>> GetByVerticalPropertyAsync(
        string verticalType, 
        string propertyName, 
        object propertyValue, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Nome da propriedade não pode ser vazio", nameof(propertyName));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug(
                "Buscando entidades por propriedade vertical - Tipo: {VerticalType}, Propriedade: {PropertyName}, Tenant: {TenantId}", 
                verticalType, propertyName, tenantId);

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Filtra por propriedade vertical em memória (otimização futura: usar SQL JSON)
            return entities.Where(entity =>
            {
                try
                {
                    if (string.IsNullOrEmpty(entity.VerticalProperties))
                        return false;

                    var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VerticalProperties);
                    
                    if (properties?.TryGetValue(propertyName, out var value) == true)
                    {
                        return value?.ToString() == propertyValue?.ToString();
                    }
                }
                catch (JsonException ex)
                {
                    Logger.LogWarning(ex, "Erro ao deserializar propriedades verticais da entidade {EntityId}", entity.Id);
                }
                
                return false;
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Erro ao buscar entidades por propriedade vertical: {VerticalType}.{PropertyName}", 
                verticalType, propertyName);
            throw;
        }
    }

    /// <summary>
    /// Busca entidades usando query específica do vertical em formato JSON
    /// </summary>
    public async Task<IEnumerable<TEntity>> SearchByVerticalQueryAsync(
        string verticalType, 
        string verticalQuery, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(verticalQuery))
            throw new ArgumentException("Query vertical não pode ser vazia", nameof(verticalQuery));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug("Executando query vertical - Tipo: {VerticalType}, Tenant: {TenantId}", 
                verticalType, tenantId);

            // Parse da query JSON
            var queryConditions = JsonSerializer.Deserialize<Dictionary<string, object>>(verticalQuery);
            if (queryConditions == null)
                throw new ArgumentException("Query vertical inválida", nameof(verticalQuery));

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Aplica filtros em memória
            return entities.Where(entity => MatchesVerticalQuery(entity, queryConditions));
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Erro ao fazer parse da query vertical: {VerticalQuery}", verticalQuery);
            throw new ArgumentException("Query vertical com formato inválido", nameof(verticalQuery), ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro durante busca por query vertical: {VerticalType}", verticalType);
            throw;
        }
    }

    /// <summary>
    /// Obtém estatísticas agregadas por vertical
    /// </summary>
    public async Task<VerticalAggregationResult> GetVerticalAggregationAsync(
        string verticalType, 
        string aggregationType, 
        string? propertyName = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(aggregationType))
            throw new ArgumentException("Tipo de agregação não pode ser vazio", nameof(aggregationType));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug(
                "Calculando agregação vertical - Tipo: {VerticalType}, Agregação: {AggregationType}, Tenant: {TenantId}", 
                verticalType, aggregationType, tenantId);

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new VerticalAggregationResult
            {
                VerticalType = verticalType,
                AggregationType = aggregationType,
                PropertyName = propertyName,
                Count = entities.Count
            };

            switch (aggregationType.ToUpper())
            {
                case "COUNT":
                    result.Value = entities.Count;
                    break;
                    
                case "SUM":
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        result.Value = CalculateNumericAggregation(entities, propertyName, values => values.Sum());
                    }
                    break;
                    
                case "AVG":
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        result.Value = CalculateNumericAggregation(entities, propertyName, values => values.Average());
                    }
                    break;
                    
                case "MIN":
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        result.Value = CalculateNumericAggregation(entities, propertyName, values => values.Min());
                    }
                    break;
                    
                case "MAX":
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        result.Value = CalculateNumericAggregation(entities, propertyName, values => values.Max());
                    }
                    break;
                    
                default:
                    throw new ArgumentException($"Tipo de agregação não suportado: {aggregationType}", nameof(aggregationType));
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Erro durante agregação vertical: {VerticalType}.{AggregationType}", 
                verticalType, aggregationType);
            throw;
        }
    }

    /// <summary>
    /// Valida consistência das propriedades verticais
    /// </summary>
    public async Task<VerticalConsistencyResult> ValidateVerticalConsistencyAsync(
        string verticalType, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug("Validando consistência vertical - Tipo: {VerticalType}, Tenant: {TenantId}", 
                verticalType, tenantId);

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new VerticalConsistencyResult
            {
                VerticalType = verticalType,
                TotalEntities = entities.Count,
                IsConsistent = true
            };

            var validEntities = 0;

            foreach (var entity in entities)
            {
                try
                {
                    if (entity.ValidateVerticalProperties())
                    {
                        validEntities++;
                    }
                    else
                    {
                        result.Issues.Add($"Entidade {entity.Id}: Propriedades verticais inválidas");
                        result.IsConsistent = false;
                    }
                }
                catch (Exception ex)
                {
                    result.Issues.Add($"Entidade {entity.Id}: Erro de validação - {ex.Message}");
                    result.IsConsistent = false;
                }
            }

            result.ValidEntities = validEntities;

            if (!result.IsConsistent)
            {
                result.Recommendations.Add("Revisar propriedades verticais das entidades com problemas");
                result.Recommendations.Add("Considerar migração para nova versão do schema vertical");
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro durante validação de consistência vertical: {VerticalType}", verticalType);
            throw;
        }
    }

    /// <summary>
    /// Migra entidades para nova versão do schema vertical
    /// </summary>
    public async Task<VerticalMigrationResult> MigrateVerticalSchemaAsync(
        string verticalType, 
        string fromVersion, 
        string toVersion, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));

        var result = new VerticalMigrationResult
        {
            VerticalType = verticalType,
            FromVersion = fromVersion,
            ToVersion = toVersion
        };

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogInformation(
                "Iniciando migração de schema vertical - Tipo: {VerticalType}, De: {FromVersion}, Para: {ToVersion}, Tenant: {TenantId}", 
                verticalType, fromVersion, toVersion, tenantId);

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalSchemaVersion == fromVersion)
                .ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                try
                {
                    // Implementar lógica de migração específica por versão
                    MigrateEntitySchema(entity, fromVersion, toVersion);
                    entity.VerticalSchemaVersion = toVersion;
                    
                    result.MigratedEntities++;
                }
                catch (Exception ex)
                {
                    result.FailedEntities++;
                    result.Errors.Add($"Entidade {entity.Id}: {ex.Message}");
                    Logger.LogError(ex, "Erro ao migrar entidade {EntityId}", entity.Id);
                }
            }

            if (result.MigratedEntities > 0)
            {
                await Context.SaveChangesAsync(cancellationToken);
            }

            result.Success = result.FailedEntities == 0;
            
            Logger.LogInformation(
                "Migração concluída - Sucesso: {Success}, Migradas: {Migrated}, Falhas: {Failed}", 
                result.Success, result.MigratedEntities, result.FailedEntities);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro durante migração de schema vertical: {VerticalType}", verticalType);
            result.Success = false;
            result.Errors.Add($"Erro geral: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Obtém entidades que requerem processamento específico do vertical
    /// </summary>
    public async Task<IEnumerable<TEntity>> GetPendingVerticalProcessingAsync(
        string verticalType, 
        string processingType, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(processingType))
            throw new ArgumentException("Tipo de processamento não pode ser vazio", nameof(processingType));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug(
                "Buscando entidades para processamento - Tipo: {VerticalType}, Processamento: {ProcessingType}, Tenant: {TenantId}", 
                verticalType, processingType, tenantId);

            var entities = await DbSet
                .Where(e => e.VerticalType == verticalType && e.VerticalActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Filtrar entidades que precisam do processamento específico
            return entities.Where(entity => RequiresProcessing(entity, processingType));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Erro ao buscar entidades para processamento: {VerticalType}.{ProcessingType}", 
                verticalType, processingType);
            throw;
        }
    }

    /// <summary>
    /// Executa operação em lote específica do vertical
    /// </summary>
    public async Task<VerticalBatchResult> ExecuteVerticalBatchOperationAsync(
        string verticalType, 
        string operation, 
        IEnumerable<TEntity> entities, 
        Dictionary<string, object>? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operação não pode ser vazia", nameof(operation));

        var entitiesList = entities?.ToList() ?? new List<TEntity>();
        
        var result = new VerticalBatchResult
        {
            VerticalType = verticalType,
            Operation = operation,
            ProcessedEntities = entitiesList.Count
        };

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogInformation(
                "Executando operação em lote - Tipo: {VerticalType}, Operação: {Operation}, Entidades: {Count}, Tenant: {TenantId}", 
                verticalType, operation, entitiesList.Count, tenantId);

            foreach (var entity in entitiesList)
            {
                try
                {
                    await ExecuteVerticalOperation(entity, operation, parameters, cancellationToken);
                    result.SuccessfulEntities++;
                }
                catch (Exception ex)
                {
                    result.FailedEntities++;
                    result.Errors.Add($"Entidade {entity.Id}: {ex.Message}");
                    Logger.LogError(ex, "Erro ao executar operação em entidade {EntityId}", entity.Id);
                }
            }

            if (result.SuccessfulEntities > 0)
            {
                await Context.SaveChangesAsync(cancellationToken);
            }

            result.Success = result.FailedEntities == 0;
            
            Logger.LogInformation(
                "Operação em lote concluída - Sucesso: {Success}, Processadas: {Successful}, Falhas: {Failed}", 
                result.Success, result.SuccessfulEntities, result.FailedEntities);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro durante operação em lote: {VerticalType}.{Operation}", verticalType, operation);
            result.Success = false;
            result.Errors.Add($"Erro geral: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Obtém métricas de desempenho específicas do vertical
    /// </summary>
    public async Task<VerticalMetrics> GetVerticalMetricsAsync(
        string verticalType, 
        string metricsType, 
        DateRange dateRange, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (string.IsNullOrWhiteSpace(metricsType))
            throw new ArgumentException("Tipo de métricas não pode ser vazio", nameof(metricsType));

        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            Logger.LogDebug(
                "Calculando métricas verticais - Tipo: {VerticalType}, Métricas: {MetricsType}, Tenant: {TenantId}", 
                verticalType, metricsType, tenantId);

            var metrics = new VerticalMetrics
            {
                VerticalType = verticalType,
                MetricsType = metricsType,
                DateRange = dateRange
            };

            // Implementar cálculo de métricas específicas por tipo
            await CalculateVerticalMetrics(metrics, cancellationToken);

            return metrics;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Erro ao calcular métricas verticais: {VerticalType}.{MetricsType}", 
                verticalType, metricsType);
            throw;
        }
    }

    #region Métodos Auxiliares Privados

    /// <summary>
    /// Verifica se entidade atende condições da query vertical
    /// </summary>
    private bool MatchesVerticalQuery(TEntity entity, Dictionary<string, object> queryConditions)
    {
        try
        {
            if (string.IsNullOrEmpty(entity.VerticalProperties))
                return false;

            var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VerticalProperties);
            if (properties == null)
                return false;

            return queryConditions.All(condition =>
                properties.TryGetValue(condition.Key, out var value) &&
                value?.ToString() == condition.Value?.ToString());
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Calcula agregação numérica para propriedade vertical
    /// </summary>
    private decimal CalculateNumericAggregation(
        IEnumerable<TEntity> entities, 
        string propertyName, 
        Func<IEnumerable<decimal>, decimal> aggregation)
    {
        var values = new List<decimal>();

        foreach (var entity in entities)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.VerticalProperties))
                {
                    var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VerticalProperties);
                    
                    if (properties?.TryGetValue(propertyName, out var value) == true &&
                        decimal.TryParse(value?.ToString(), out var numericValue))
                    {
                        values.Add(numericValue);
                    }
                }
            }
            catch (JsonException)
            {
                // Ignora entidades com propriedades inválidas
            }
        }

        return values.Any() ? aggregation(values) : 0;
    }

    /// <summary>
    /// Migra schema de uma entidade específica
    /// </summary>
    private void MigrateEntitySchema(TEntity entity, string fromVersion, string toVersion)
    {
        // Implementar lógicas específicas de migração por versão
        // Este é um exemplo básico - deve ser expandido conforme necessário
        
        if (fromVersion == "1.0" && toVersion == "1.1")
        {
            // Exemplo de migração: adicionar nova propriedade com valor padrão
            if (!string.IsNullOrEmpty(entity.VerticalProperties))
            {
                var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VerticalProperties);
                if (properties != null && !properties.ContainsKey("versaoMigrada"))
                {
                    properties["versaoMigrada"] = true;
                    entity.VerticalProperties = JsonSerializer.Serialize(properties);
                }
            }
        }
    }

    /// <summary>
    /// Verifica se entidade requer processamento específico
    /// </summary>
    private bool RequiresProcessing(TEntity entity, string processingType)
    {
        // Implementar lógica específica por tipo de processamento
        try
        {
            return entity.GetVerticalProperty<bool>($"requiresProcessing_{processingType}") == true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executa operação vertical específica em uma entidade
    /// </summary>
    private async Task ExecuteVerticalOperation(
        TEntity entity, 
        string operation, 
        Dictionary<string, object>? parameters, 
        CancellationToken cancellationToken)
    {
        // Implementar operações específicas por tipo
        switch (operation.ToUpper())
        {
            case "REFRESH_PROPERTIES":
                // Atualizar propriedades verticais
                break;
                
            case "VALIDATE_SCHEMA":
                // Validar schema das propriedades
                if (!entity.ValidateVerticalProperties())
                    throw new InvalidOperationException("Schema inválido");
                break;
                
            case "UPDATE_VERSION":
                // Atualizar versão do schema
                if (parameters?.TryGetValue("version", out var version) == true)
                    entity.VerticalSchemaVersion = version.ToString() ?? entity.VerticalSchemaVersion;
                break;
                
            default:
                throw new NotSupportedException($"Operação não suportada: {operation}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Calcula métricas específicas do vertical
    /// </summary>
    private async Task CalculateVerticalMetrics(VerticalMetrics metrics, CancellationToken cancellationToken)
    {
        // Implementar cálculo de métricas específicas
        var entities = await DbSet
            .Where(e => e.VerticalType == metrics.VerticalType && e.VerticalActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        switch (metrics.MetricsType.ToUpper())
        {
            case "USAGE":
                metrics.Values["totalEntities"] = entities.Count;
                metrics.Values["activeEntities"] = entities.Count(e => e.VerticalActive);
                break;
                
            case "PERFORMANCE":
                // Calcular métricas de performance
                break;
                
            default:
                throw new NotSupportedException($"Tipo de métrica não suportado: {metrics.MetricsType}");
        }
    }

    #endregion
}