namespace CoreApp.Verticals.Common;

/// <summary>
/// Interface base para todos os módulos verticais de negócio
/// Permite extensibilidade dinâmica do sistema através de composição
/// </summary>
/// <remarks>
/// Cada vertical (Padaria, Farmácia, Restaurante, etc.) deve implementar esta interface
/// para se integrar ao sistema principal CoreApp de forma padronizada
/// </remarks>
public interface IVerticalModule
{
    /// <summary>
    /// Nome único identificador da vertical
    /// </summary>
    /// <example>"Padaria", "Farmacia", "Restaurante"</example>
    string VerticalName { get; }
    
    /// <summary>
    /// Versão da implementação da vertical
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Descrição comercial da vertical para apresentação
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Lista de módulos CoreApp necessários para esta vertical funcionar
    /// </summary>
    /// <example>["PRODUCTS", "SALES", "STOCK"]</example>
    IEnumerable<string> RequiredModules { get; }
    
    /// <summary>
    /// Lista de módulos CoreApp opcionais que melhoram a funcionalidade
    /// </summary>
    /// <example>["CUSTOMERS", "PROMOTIONS", "ADVANCED_REPORTS"]</example>
    IEnumerable<string> OptionalModules { get; }
    
    /// <summary>
    /// Configurações específicas da vertical
    /// </summary>
    Dictionary<string, object> DefaultConfigurations { get; }
    
    /// <summary>
    /// Valida se a vertical pode ser ativada para o tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant (loja)</param>
    /// <param name="availableModules">Módulos disponíveis no plano do tenant</param>
    /// <returns>True se pode ser ativada</returns>
    Task<bool> CanActivateAsync(string tenantId, IEnumerable<string> availableModules);
    
    /// <summary>
    /// Inicializa a vertical para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="configuration">Configurações específicas do tenant</param>
    /// <returns>Resultado da inicialização</returns>
    Task<VerticalActivationResult> ActivateAsync(string tenantId, Dictionary<string, object>? configuration = null);
    
    /// <summary>
    /// Remove a vertical de um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Resultado da desativação</returns>
    Task<VerticalActivationResult> DeactivateAsync(string tenantId);
    
    /// <summary>
    /// Valida propriedades específicas da vertical para uma entidade
    /// </summary>
    /// <param name="entityType">Tipo da entidade</param>
    /// <param name="properties">Propriedades a validar</param>
    /// <returns>Resultado da validação</returns>
    Task<VerticalPropertiesValidationResult> ValidatePropertiesAsync(string entityType, Dictionary<string, object> properties);
}

/// <summary>
/// Resultado da ativação/desativação de uma vertical
/// </summary>
public class VerticalActivationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ResultData { get; set; } = new();
}

/// <summary>
/// Resultado da validação de propriedades verticais
/// </summary>
public class VerticalPropertiesValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidatedData { get; set; } = new();
}