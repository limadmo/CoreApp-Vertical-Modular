using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Interfaces.UnitOfWork;
using CoreApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CoreApp.Application.Services;

/// <summary>
/// Serviço principal para composição e orquestração de verticais de negócio brasileiros
/// Coordena a interação entre diferentes verticais (Padaria, Farmácia, Supermercado, etc.)
/// </summary>
/// <remarks>
/// Implementa arquitetura de composição conforme CLAUDE.md:
/// - Composição ao invés de herança complexa (SOLID)
/// - Suporte a múltiplos verticais simultâneos
/// - Coordenação transacional via Unit of Work
/// - Cache de serviços verticais para performance
/// </remarks>
public class VerticalCompositionService : IVerticalCompositionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VerticalCompositionService> _logger;

    // Cache thread-safe para serviços verticais registrados
    private readonly ConcurrentDictionary<string, VerticalMetadata> _registeredVerticals = new();
    private readonly ConcurrentDictionary<string, object> _verticalServices = new();

    public VerticalCompositionService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IServiceProvider serviceProvider,
        ILogger<VerticalCompositionService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra um novo vertical no sistema de composição
    /// </summary>
    public async Task<bool> RegisterVerticalAsync(string verticalType, VerticalMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("Tipo de vertical não pode ser vazio", nameof(verticalType));
        
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        try
        {
            _logger.LogInformation("Registrando vertical: {VerticalType} - {Name} v{Version}", 
                verticalType, metadata.Name, metadata.Version);

            // Valida metadados do vertical
            if (!ValidateVerticalMetadata(metadata))
            {
                _logger.LogError("Metadados inválidos para vertical {VerticalType}", verticalType);
                return false;
            }

            // Registra vertical no cache
            _registeredVerticals.AddOrUpdate(verticalType.ToUpper(), metadata, (key, oldValue) => metadata);

            // TODO: Persistir registro no banco quando entidades estiverem implementadas
            // await _unitOfWork.Repository<VerticalRegistryEntity>().AddAsync(verticalRegistry, cancellationToken);
            // await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Vertical {VerticalType} registrado com sucesso", verticalType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar vertical {VerticalType}", verticalType);
            return false;
        }
    }

    /// <summary>
    /// Obtém todos os verticais registrados no sistema
    /// </summary>
    public async Task<IEnumerable<VerticalMetadata>> GetAvailableVerticalsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Obtendo verticais disponíveis - ActiveOnly: {ActiveOnly}", activeOnly);

            // TODO: Quando banco estiver implementado, buscar do repositório
            // var verticais = await _unitOfWork.Repository<VerticalRegistryEntity>()
            //     .GetAllAsync(cancellationToken);

            var verticais = _registeredVerticals.Values.AsEnumerable();

            if (activeOnly)
            {
                verticais = verticais.Where(v => v.IsActive);
            }

            await Task.CompletedTask; // Remove quando implementar acesso ao banco
            return verticais;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter verticais disponíveis");
            throw;
        }
    }

    /// <summary>
    /// Obtém verticais ativos para um tenant específico
    /// </summary>
    public async Task<IEnumerable<string>> GetTenantVerticalsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

        try
        {
            _logger.LogDebug("Obtendo verticais ativos para tenant {TenantId}", tenantId);

            // TODO: Implementar busca no banco quando entidades estiverem prontas
            // var tenantVerticals = await _unitOfWork.Repository<TenantVerticalEntity>()
            //     .GetByTenantIdAsync(tenantId, cancellationToken);

            // Por enquanto, simula baseado no tipo de tenant para desenvolvimento
            var verticais = SimulateTenantVerticals(tenantId);

            _logger.LogInformation("Verticais ativos para tenant {TenantId}: {Verticais}", 
                tenantId, string.Join(", ", verticais));

            await Task.CompletedTask;
            return verticais;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter verticais do tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Ativa um vertical para um tenant
    /// </summary>
    public async Task<bool> ActivateVerticalForTenantAsync(
        string tenantId, 
        string verticalType, 
        Dictionary<string, object>? configuration = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));
        
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("VerticalType não pode ser vazio", nameof(verticalType));

        try
        {
            _logger.LogInformation("Ativando vertical {VerticalType} para tenant {TenantId}", verticalType, tenantId);

            // Verifica se o vertical está registrado
            if (!_registeredVerticals.ContainsKey(verticalType.ToUpper()))
            {
                _logger.LogError("Vertical {VerticalType} não está registrado", verticalType);
                return false;
            }

            // Executa em transação para garantir consistência
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // TODO: Implementar persistência quando entidades estiverem prontas
                // var tenantVertical = new TenantVerticalEntity
                // {
                //     TenantId = tenantId,
                //     VerticalType = verticalType,
                //     Configuration = JsonSerializer.Serialize(configuration ?? new Dictionary<string, object>()),
                //     IsActive = true,
                //     ActivatedAt = DateTime.UtcNow
                // };
                // 
                // await _unitOfWork.Repository<TenantVerticalEntity>().AddAsync(tenantVertical, cancellationToken);

                _logger.LogInformation("Vertical {VerticalType} ativado para tenant {TenantId}", verticalType, tenantId);
                return true;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar vertical {VerticalType} para tenant {TenantId}", verticalType, tenantId);
            return false;
        }
    }

    /// <summary>
    /// Desativa um vertical para um tenant
    /// </summary>
    public async Task<bool> DeactivateVerticalForTenantAsync(
        string tenantId, 
        string verticalType, 
        string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));
        
        if (string.IsNullOrWhiteSpace(verticalType))
            throw new ArgumentException("VerticalType não pode ser vazio", nameof(verticalType));

        try
        {
            _logger.LogInformation("Desativando vertical {VerticalType} para tenant {TenantId} - Motivo: {Reason}", 
                verticalType, tenantId, reason);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // TODO: Implementar soft delete quando entidades estiverem prontas
                // var tenantVertical = await _unitOfWork.Repository<TenantVerticalEntity>()
                //     .GetByTenantAndVerticalAsync(tenantId, verticalType, cancellationToken);
                // 
                // if (tenantVertical != null)
                // {
                //     tenantVertical.IsActive = false;
                //     tenantVertical.DeactivatedAt = DateTime.UtcNow;
                //     tenantVertical.DeactivationReason = reason;
                //     
                //     await _unitOfWork.Repository<TenantVerticalEntity>().UpdateAsync(tenantVertical, cancellationToken);
                // }

                _logger.LogInformation("Vertical {VerticalType} desativado para tenant {TenantId}", verticalType, tenantId);
                return true;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar vertical {VerticalType} para tenant {TenantId}", verticalType, tenantId);
            return false;
        }
    }

    /// <summary>
    /// Compõe uma entidade com funcionalidades de múltiplos verticais
    /// </summary>
    public async Task<TEntity> ComposeEntityAsync<TEntity>(
        TEntity entity, 
        IEnumerable<string> verticalTypes, 
        CancellationToken cancellationToken = default) 
        where TEntity : class, IVerticalEntity
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var verticalTypesList = verticalTypes?.ToList() ?? new List<string>();
        
        if (!verticalTypesList.Any())
        {
            _logger.LogDebug("Nenhum vertical especificado para composição");
            return entity;
        }

        try
        {
            _logger.LogDebug("Compondo entidade {EntityType} com verticais: {Verticais}", 
                typeof(TEntity).Name, string.Join(", ", verticalTypesList));

            foreach (var verticalType in verticalTypesList)
            {
                try
                {
                    var service = GetVerticalService<TEntity>(verticalType);
                    if (service != null)
                    {
                        entity = await service.ProcessAsync(entity, "COMPOSE", cancellationToken);
                        _logger.LogDebug("Entidade processada pelo vertical {VerticalType}", verticalType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar entidade no vertical {VerticalType}", verticalType);
                    // Continua processamento nos outros verticais
                }
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante composição de entidade com verticais");
            throw;
        }
    }

    /// <summary>
    /// Executa uma operação usando serviços de múltiplos verticais
    /// </summary>
    public async Task<VerticalCompositionResult> ExecuteComposedOperationAsync<TEntity>(
        TEntity entity, 
        string operation, 
        IEnumerable<string> verticalTypes, 
        Dictionary<string, object>? parameters = null, 
        CancellationToken cancellationToken = default) 
        where TEntity : class, IVerticalEntity
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operação não pode ser vazia", nameof(operation));

        var result = new VerticalCompositionResult
        {
            Operation = operation,
            ExecutedAt = DateTime.UtcNow
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var verticalTypesList = verticalTypes?.ToList() ?? new List<string>();

        try
        {
            _logger.LogInformation("Executando operação composta {Operation} com verticais: {Verticais}", 
                operation, string.Join(", ", verticalTypesList));

            foreach (var verticalType in verticalTypesList)
            {
                try
                {
                    var service = GetVerticalService<TEntity>(verticalType);
                    if (service != null && service.SupportsOperation(operation))
                    {
                        var processedEntity = await service.ProcessAsync(entity, operation, cancellationToken);
                        result.Results[verticalType] = processedEntity;
                        
                        _logger.LogDebug("Operação {Operation} executada com sucesso no vertical {VerticalType}", 
                            operation, verticalType);
                    }
                    else
                    {
                        result.WarningsByVertical[verticalType] = new List<string> 
                        { 
                            $"Vertical não suporta operação {operation}" 
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar operação {Operation} no vertical {VerticalType}", 
                        operation, verticalType);
                    
                    result.ErrorsByVertical[verticalType] = new List<string> { ex.Message };
                }
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.Success = result.AllVerticalsSucceeded;

            _logger.LogInformation("Operação composta {Operation} concluída - Sucesso: {Success}, Duração: {Duration}ms", 
                operation, result.Success, result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.Success = false;
            
            _logger.LogError(ex, "Erro durante execução de operação composta {Operation}", operation);
            throw;
        }
    }

    /// <summary>
    /// Resolve qual vertical deve processar uma entidade baseado em suas propriedades
    /// </summary>
    public async Task<IEnumerable<string>> ResolveVerticalsForEntityAsync<TEntity>(
        TEntity entity, 
        string operation, 
        CancellationToken cancellationToken = default) 
        where TEntity : class, IVerticalEntity
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _logger.LogDebug("Resolvendo verticais para entidade {EntityType} - Operação: {Operation}", 
                typeof(TEntity).Name, operation);

            var applicableVerticals = new List<string>();

            // Se a entidade já tem um vertical definido, prioriza ele
            if (!string.IsNullOrWhiteSpace(entity.VerticalType))
            {
                applicableVerticals.Add(entity.VerticalType);
            }

            // Analisa propriedades verticais para determinar outros verticais aplicáveis
            var additionalVerticals = await AnalyzeEntityPropertiesForVerticals(entity, operation, cancellationToken);
            applicableVerticals.AddRange(additionalVerticals);

            // Remove duplicatas e retorna
            var result = applicableVerticals.Distinct().ToList();
            
            _logger.LogDebug("Verticais resolvidos para entidade: {Verticais}", string.Join(", ", result));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resolver verticais para entidade");
            throw;
        }
    }

    /// <summary>
    /// Valida se uma configuração de verticais é compatível
    /// </summary>
    public async Task<VerticalCompatibilityResult> ValidateVerticalCompatibilityAsync(
        IEnumerable<string> verticalTypes, 
        CancellationToken cancellationToken = default)
    {
        var result = new VerticalCompatibilityResult { IsCompatible = true };
        var verticalTypesList = verticalTypes?.ToList() ?? new List<string>();

        try
        {
            _logger.LogDebug("Validando compatibilidade entre verticais: {Verticais}", 
                string.Join(", ", verticalTypesList));

            // Verifica se todos os verticais estão registrados
            foreach (var verticalType in verticalTypesList)
            {
                if (!_registeredVerticals.ContainsKey(verticalType.ToUpper()))
                {
                    result.IsCompatible = false;
                    result.Conflicts.Add($"Vertical {verticalType} não está registrado");
                }
            }

            // Analisa dependências e conflitos
            await AnalyzeVerticalDependencies(verticalTypesList, result, cancellationToken);

            if (result.IsCompatible)
            {
                _logger.LogInformation("Configuração de verticais é compatível");
            }
            else
            {
                _logger.LogWarning("Configuração de verticais tem conflitos: {Conflicts}", 
                    string.Join(", ", result.Conflicts));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar compatibilidade de verticais");
            throw;
        }
    }

    /// <summary>
    /// Obtém configuração padrão para uma combinação de verticais
    /// </summary>
    public async Task<Dictionary<string, object>> GetComposedConfigurationAsync(
        IEnumerable<string> verticalTypes, 
        CancellationToken cancellationToken = default)
    {
        var configuration = new Dictionary<string, object>();
        var verticalTypesList = verticalTypes?.ToList() ?? new List<string>();

        try
        {
            _logger.LogDebug("Obtendo configuração composta para verticais: {Verticais}", 
                string.Join(", ", verticalTypesList));

            foreach (var verticalType in verticalTypesList)
            {
                if (_registeredVerticals.TryGetValue(verticalType.ToUpper(), out var metadata))
                {
                    // Mescla configurações padrão de cada vertical
                    foreach (var capability in metadata.Capabilities)
                    {
                        configuration[$"{verticalType}_{capability.Key}"] = capability.Value;
                    }
                }
            }

            await Task.CompletedTask;
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração composta");
            throw;
        }
    }

    /// <summary>
    /// Executa migração de dados para novos verticais
    /// </summary>
    public async Task<Domain.Interfaces.Repositories.VerticalMigrationResult> MigrateVerticalDataAsync(
        string tenantId, 
        IEnumerable<string> fromVerticals, 
        IEnumerable<string> toVerticals, 
        Dictionary<string, object>? migrationOptions = null, 
        CancellationToken cancellationToken = default)
    {
        var result = new Domain.Interfaces.Repositories.VerticalMigrationResult
        {
            VerticalType = "MIGRATION",
            FromVersion = string.Join(",", fromVerticals),
            ToVersion = string.Join(",", toVerticals)
        };

        try
        {
            _logger.LogInformation("Iniciando migração de dados verticais para tenant {TenantId} - De: [{From}] Para: [{To}]", 
                tenantId, result.FromVersion, result.ToVersion);

            // TODO: Implementar lógica real de migração quando entidades estiverem prontas
            await Task.Delay(100, cancellationToken); // Simula processamento

            result.Success = true;
            result.MigratedEntities = 0; // TODO: Contar entidades migradas
            
            _logger.LogInformation("Migração de dados verticais concluída para tenant {TenantId}", tenantId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante migração de dados verticais para tenant {TenantId}", tenantId);
            result.Success = false;
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    #region Métodos Auxiliares Privados

    /// <summary>
    /// Valida metadados do vertical
    /// </summary>
    private static bool ValidateVerticalMetadata(VerticalMetadata metadata)
    {
        return !string.IsNullOrWhiteSpace(metadata.VerticalType) &&
               !string.IsNullOrWhiteSpace(metadata.Name) &&
               !string.IsNullOrWhiteSpace(metadata.Version);
    }

    /// <summary>
    /// Simula verticais do tenant para desenvolvimento
    /// </summary>
    private static List<string> SimulateTenantVerticals(string tenantId)
    {
        return tenantId.ToLower() switch
        {
            var id when id.Contains("padaria") => new List<string> { "PADARIA", "ALIMENTICIO" },
            var id when id.Contains("farmacia") => new List<string> { "FARMACIA", "MEDICAMENTOS" },
            var id when id.Contains("supermercado") => new List<string> { "SUPERMERCADO", "ALIMENTICIO", "BEBIDAS" },
            _ => new List<string> { "GENERICO" }
        };
    }

    /// <summary>
    /// Obtém serviço vertical específico
    /// </summary>
    private IVerticalService<TEntity>? GetVerticalService<TEntity>(string verticalType) 
        where TEntity : class, IVerticalEntity
    {
        try
        {
            // Tenta obter do cache primeiro
            var cacheKey = $"{verticalType}_{typeof(TEntity).Name}";
            
            if (_verticalServices.TryGetValue(cacheKey, out var cachedService))
            {
                return cachedService as IVerticalService<TEntity>;
            }

            // Tenta resolver via DI
            var service = _serviceProvider.GetService<IVerticalService<TEntity>>();
            
            if (service != null)
            {
                _verticalServices.TryAdd(cacheKey, service);
            }

            return service;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter serviço vertical {VerticalType} para {EntityType}", 
                verticalType, typeof(TEntity).Name);
            return null;
        }
    }

    /// <summary>
    /// Analisa propriedades da entidade para determinar verticais aplicáveis
    /// </summary>
    private async Task<List<string>> AnalyzeEntityPropertiesForVerticals<TEntity>(
        TEntity entity, 
        string operation, 
        CancellationToken cancellationToken) 
        where TEntity : class, IVerticalEntity
    {
        var applicableVerticals = new List<string>();

        try
        {
            // Analisa propriedades verticais JSON
            if (!string.IsNullOrWhiteSpace(entity.VerticalProperties))
            {
                var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VerticalProperties);
                
                if (properties != null)
                {
                    // Lógica para determinar verticais baseado nas propriedades
                    if (properties.ContainsKey("temGluten") || properties.ContainsKey("dataValidade"))
                    {
                        applicableVerticals.Add("ALIMENTICIO");
                    }
                    
                    if (properties.ContainsKey("principioAtivo") || properties.ContainsKey("dosagem"))
                    {
                        applicableVerticals.Add("MEDICAMENTOS");
                    }
                }
            }

            await Task.CompletedTask;
            return applicableVerticals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar propriedades da entidade para verticais");
            return applicableVerticals;
        }
    }

    /// <summary>
    /// Analisa dependências e conflitos entre verticais
    /// </summary>
    private async Task AnalyzeVerticalDependencies(
        List<string> verticalTypes, 
        VerticalCompatibilityResult result, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Analisa conflitos conhecidos
            if (verticalTypes.Contains("FARMACIA") && verticalTypes.Contains("BEBIDAS_ALCOOLICAS"))
            {
                result.Conflicts.Add("Farmácia não pode vender bebidas alcoólicas conforme regulamentação ANVISA");
                result.IsCompatible = false;
            }

            // Analisa dependências
            foreach (var verticalType in verticalTypes)
            {
                if (_registeredVerticals.TryGetValue(verticalType.ToUpper(), out var metadata))
                {
                    result.RequiredDependencies.AddRange(metadata.RequiredModules);
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar dependências de verticais");
            throw;
        }
    }

    #endregion
}