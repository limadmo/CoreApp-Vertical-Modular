using CoreApp.Domain.Interfaces.Common;

namespace CoreApp.Domain.Interfaces.Services;

/// <summary>
/// Serviço principal para composição e orquestração de verticais de negócio
/// Coordena a interação entre diferentes verticais e o sistema base
/// </summary>
public interface IVerticalCompositionService
{
    /// <summary>
    /// Registra um novo vertical no sistema
    /// </summary>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="metadata">Metadados do vertical</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se registrado com sucesso</returns>
    Task<bool> RegisterVerticalAsync(string verticalType, VerticalMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os verticais registrados no sistema
    /// </summary>
    /// <param name="activeOnly">Se deve retornar apenas verticais ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de verticais disponíveis</returns>
    Task<IEnumerable<VerticalMetadata>> GetAvailableVerticalsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém verticais ativos para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de verticais ativos para o tenant</returns>
    Task<IEnumerable<string>> GetTenantVerticalsAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa um vertical para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="configuration">Configuração específica do vertical para o tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se ativado com sucesso</returns>
    Task<bool> ActivateVerticalForTenantAsync(string tenantId, string verticalType, Dictionary<string, object>? configuration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa um vertical para um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <param name="reason">Motivo da desativação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se desativado com sucesso</returns>
    Task<bool> DeactivateVerticalForTenantAsync(string tenantId, string verticalType, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compõe uma entidade com funcionalidades de múltiplos verticais
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade</typeparam>
    /// <param name="entity">Entidade base</param>
    /// <param name="verticalTypes">Tipos de verticais a serem aplicados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade com composições aplicadas</returns>
    Task<TEntity> ComposeEntityAsync<TEntity>(TEntity entity, IEnumerable<string> verticalTypes, CancellationToken cancellationToken = default) where TEntity : class, IVerticalEntity;

    /// <summary>
    /// Executa uma operação usando serviços de múltiplos verticais
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade</typeparam>
    /// <param name="entity">Entidade alvo</param>
    /// <param name="operation">Operação a ser executada</param>
    /// <param name="verticalTypes">Verticais que devem processar a operação</param>
    /// <param name="parameters">Parâmetros da operação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da composição da operação</returns>
    Task<VerticalCompositionResult> ExecuteComposedOperationAsync<TEntity>(TEntity entity, string operation, IEnumerable<string> verticalTypes, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default) where TEntity : class, IVerticalEntity;

    /// <summary>
    /// Resolve qual vertical deve processar uma entidade baseado em suas propriedades
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade</typeparam>
    /// <param name="entity">Entidade a ser analisada</param>
    /// <param name="operation">Operação que será executada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de verticais que devem processar a entidade</returns>
    Task<IEnumerable<string>> ResolveVerticalsForEntityAsync<TEntity>(TEntity entity, string operation, CancellationToken cancellationToken = default) where TEntity : class, IVerticalEntity;

    /// <summary>
    /// Valida se uma configuração de verticais é compatível
    /// </summary>
    /// <param name="verticalTypes">Tipos de verticais</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da validação de compatibilidade</returns>
    Task<VerticalCompatibilityResult> ValidateVerticalCompatibilityAsync(IEnumerable<string> verticalTypes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém configuração padrão para uma combinação de verticais
    /// </summary>
    /// <param name="verticalTypes">Tipos de verticais</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Configuração composta dos verticais</returns>
    Task<Dictionary<string, object>> GetComposedConfigurationAsync(IEnumerable<string> verticalTypes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa migração de dados para novos verticais
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="fromVerticals">Verticais origem</param>
    /// <param name="toVerticals">Verticais destino</param>
    /// <param name="migrationOptions">Opções de migração</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da migração</returns>
    Task<Domain.Interfaces.Repositories.VerticalMigrationResult> MigrateVerticalDataAsync(string tenantId, IEnumerable<string> fromVerticals, IEnumerable<string> toVerticals, Dictionary<string, object>? migrationOptions = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de uma operação composta por múltiplos verticais
/// </summary>
public class VerticalCompositionResult
{
    public bool Success { get; set; }
    public string Operation { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public Dictionary<string, List<string>> ErrorsByVertical { get; set; } = new();
    public Dictionary<string, List<string>> WarningsByVertical { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica se todos os verticais processaram com sucesso
    /// </summary>
    public bool AllVerticalsSucceeded => !ErrorsByVertical.Any(x => x.Value.Any());
    
    /// <summary>
    /// Obtém resultado específico de um vertical
    /// </summary>
    /// <typeparam name="T">Tipo do resultado</typeparam>
    /// <param name="verticalType">Tipo do vertical</param>
    /// <returns>Resultado do vertical ou default(T)</returns>
    public T? GetVerticalResult<T>(string verticalType)
    {
        if (Results.TryGetValue(verticalType, out var result) && result is T typedResult)
            return typedResult;
        return default(T);
    }
}

/// <summary>
/// Resultado de validação de compatibilidade entre verticais
/// </summary>
public class VerticalCompatibilityResult
{
    public bool IsCompatible { get; set; }
    public List<string> Conflicts { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
    public List<string> RequiredDependencies { get; set; } = new();
    public List<string> OptionalDependencies { get; set; } = new();
}