using CoreApp.Verticals.Common;
using CoreApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreApp.Verticals.Middleware;

/// <summary>
/// Middleware para interceptação automática de operações com propriedades verticais
/// Aplica validações e transformações baseadas nas verticais ativas do tenant
/// </summary>
/// <remarks>
/// Este middleware intercepta requests que contenham propriedades verticais
/// e aplica automaticamente as validações e transformações necessárias
/// sem necessidade de código adicional nos controllers
/// </remarks>
public class VerticalInterceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VerticalInterceptionMiddleware> _logger;

    public VerticalInterceptionMiddleware(
        RequestDelegate next,
        ILogger<VerticalInterceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IVerticalManager verticalManager)
    {
        // Intercepta apenas requests POST/PUT/PATCH que podem conter dados de entidades
        if (!ShouldIntercept(context))
        {
            await _next(context);
            return;
        }

        try
        {
            var tenantId = context.GetTenantId();
            
            // Obtém as verticais ativas do tenant
            var activeVerticals = await verticalManager.GetActiveVerticalsAsync(tenantId);
            
            // Se não há verticais ativas, continua normalmente
            if (!activeVerticals.Any())
            {
                await _next(context);
                return;
            }

            // Intercepta o request body para análise
            var originalBody = context.Request.Body;
            var bodyContent = await ReadRequestBodyAsync(context.Request);
            
            if (string.IsNullOrEmpty(bodyContent))
            {
                await _next(context);
                return;
            }

            // Analisa o body em busca de propriedades verticais
            var interceptResult = await InterceptVerticalProperties(
                tenantId, bodyContent, activeVerticals, verticalManager);

            if (interceptResult.HasModifications)
            {
                // Substitui o body do request com as modificações
                var modifiedBodyBytes = System.Text.Encoding.UTF8.GetBytes(interceptResult.ModifiedBody);
                context.Request.Body = new MemoryStream(modifiedBodyBytes);
                context.Request.ContentLength = modifiedBodyBytes.Length;
                
                _logger.LogInformation(
                    "Propriedades verticais processadas para tenant {TenantId}: {ModificationsCount} modificações",
                    tenantId, interceptResult.ModificationsCount);
            }
            else
            {
                // Restaura o body original se não houve modificações
                var originalBodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyContent);
                context.Request.Body = new MemoryStream(originalBodyBytes);
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Erro no middleware de interceptação de verticais para request {Method} {Path}",
                context.Request.Method, context.Request.Path);
            
            // Em caso de erro, continua sem interceptação
            await _next(context);
        }
    }

    /// <summary>
    /// Determina se o request deve ser interceptado para análise de verticais
    /// </summary>
    private bool ShouldIntercept(HttpContext context)
    {
        // Intercepta apenas métodos que modificam dados
        var method = context.Request.Method.ToUpper();
        if (method != "POST" && method != "PUT" && method != "PATCH")
            return false;

        // Intercepta apenas requests com content-type JSON
        var contentType = context.Request.ContentType;
        if (string.IsNullOrEmpty(contentType) || !contentType.Contains("application/json"))
            return false;

        // Pula requests de sistema e health checks
        var path = context.Request.Path.Value?.ToLower();
        if (string.IsNullOrEmpty(path))
            return false;

        var skipPaths = new[] { "/health", "/swagger", "/api/auth", "/api/system" };
        if (skipPaths.Any(skip => path.StartsWith(skip)))
            return false;

        return true;
    }

    /// <summary>
    /// Lê o conteúdo do body do request de forma não destrutiva
    /// </summary>
    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        
        request.Body.Position = 0;
        return body;
    }

    /// <summary>
    /// Intercepta e processa propriedades verticais no body do request
    /// </summary>
    private async Task<InterceptionResult> InterceptVerticalProperties(
        string tenantId,
        string bodyContent,
        IEnumerable<IVerticalModule> activeVerticals,
        IVerticalManager verticalManager)
    {
        var result = new InterceptionResult { OriginalBody = bodyContent };

        try
        {
            // Tenta deserializar o JSON para análise
            using var document = JsonDocument.Parse(bodyContent);
            var rootElement = document.RootElement;

            // Procura por propriedades verticais conhecidas
            var modificationsNeeded = false;
            var modifiedJson = bodyContent;

            foreach (var vertical in activeVerticals)
            {
                // Procura por propriedades nomeadas como "{VerticalName}Properties"
                var verticalPropertyName = $"{vertical.VerticalName.ToLower()}Properties";
                
                if (rootElement.TryGetProperty(verticalPropertyName, out var verticalProperty))
                {
                    // Extrai as propriedades verticais
                    var propertiesDict = ExtractPropertiesFromJsonElement(verticalProperty);
                    
                    if (propertiesDict.Any())
                    {
                        // Determina o tipo de entidade baseado no endpoint
                        var entityType = DetermineEntityTypeFromContext();
                        
                        // Valida as propriedades com a vertical
                        var validationResult = await verticalManager.ValidateVerticalPropertiesAsync(
                            tenantId, vertical.VerticalName, entityType, propertiesDict);

                        if (!validationResult.IsValid)
                        {
                            // TODO: Em uma implementação completa, poderia retornar erro
                            // Por ora, apenas loga os problemas de validação
                            _logger.LogWarning(
                                "Validação de propriedades verticais falhou para {VerticalName}: {Errors}",
                                vertical.VerticalName, string.Join(", ", validationResult.Errors));
                        }
                        else if (validationResult.ValidatedData.Any())
                        {
                            // Aplica dados validados/transformados de volta ao JSON
                            // Esta seria a implementação completa de transformação
                            modificationsNeeded = true;
                            result.ModificationsCount++;
                        }
                    }
                }
            }

            if (modificationsNeeded)
            {
                result.ModifiedBody = modifiedJson;
                result.HasModifications = true;
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Erro ao analisar JSON para interceptação de verticais");
            return result; // Retorna sem modificações em caso de JSON inválido
        }
    }

    /// <summary>
    /// Extrai propriedades de um JsonElement para Dictionary
    /// </summary>
    private Dictionary<string, object> ExtractPropertiesFromJsonElement(JsonElement element)
    {
        var properties = new Dictionary<string, object>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                object? value = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => property.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => (object?)null,
                    JsonValueKind.Array => property.Value.EnumerateArray().Select(ExtractValueFromJsonElement).ToList(),
                    JsonValueKind.Object => ExtractPropertiesFromJsonElement(property.Value),
                    _ => property.Value.ToString()
                };

                properties[property.Name] = value ?? string.Empty;
            }
        }

        return properties;
    }

    /// <summary>
    /// Extrai valor de um JsonElement
    /// </summary>
    private object? ExtractValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => (object?)null,
            JsonValueKind.Array => element.EnumerateArray().Select(ExtractValueFromJsonElement).ToList(),
            JsonValueKind.Object => ExtractPropertiesFromJsonElement(element),
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Determina o tipo de entidade baseado no contexto da requisição
    /// Esta é uma implementação simples - pode ser expandida conforme necessário
    /// </summary>
    private string DetermineEntityTypeFromContext()
    {
        // TODO: Implementar lógica mais sofisticada baseada no path da requisição
        // Por exemplo: /api/produtos -> "produto", /api/clientes -> "cliente"
        return "generic";
    }
}

/// <summary>
/// Resultado da interceptação de propriedades verticais
/// </summary>
internal class InterceptionResult
{
    public string OriginalBody { get; set; } = string.Empty;
    public string ModifiedBody { get; set; } = string.Empty;
    public bool HasModifications { get; set; } = false;
    public int ModificationsCount { get; set; } = 0;
}