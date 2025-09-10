namespace CoreApp.Verticals.Common;

/// <summary>
/// Interface para gerenciamento centralizado de todas as verticais do sistema
/// Responsável por descobrir, registrar e gerenciar verticais dinamicamente
/// </summary>
public interface IVerticalManager
{
    /// <summary>
    /// Obtém todas as verticais registradas no sistema
    /// </summary>
    IEnumerable<IVerticalModule> GetAllVerticals();
    
    /// <summary>
    /// Obtém uma vertical específica por nome
    /// </summary>
    /// <param name="verticalName">Nome da vertical</param>
    IVerticalModule? GetVertical(string verticalName);
    
    /// <summary>
    /// Obtém todas as verticais ativas para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    Task<IEnumerable<IVerticalModule>> GetActiveVerticalsAsync(string tenantId);
    
    /// <summary>
    /// Obtém todas as verticais disponíveis para ativação em um tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="availableModules">Módulos disponíveis no plano do tenant</param>
    Task<IEnumerable<AvailableVertical>> GetAvailableVerticalsAsync(string tenantId, IEnumerable<string> availableModules);
    
    /// <summary>
    /// Ativa uma vertical para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="verticalName">Nome da vertical a ativar</param>
    /// <param name="configuration">Configurações específicas</param>
    Task<VerticalActivationResult> ActivateVerticalAsync(string tenantId, string verticalName, Dictionary<string, object>? configuration = null);
    
    /// <summary>
    /// Desativa uma vertical para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="verticalName">Nome da vertical a desativar</param>
    Task<VerticalActivationResult> DeactivateVerticalAsync(string tenantId, string verticalName);
    
    /// <summary>
    /// Valida propriedades verticais para uma entidade específica
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="verticalName">Nome da vertical</param>
    /// <param name="entityType">Tipo da entidade</param>
    /// <param name="properties">Propriedades a validar</param>
    Task<VerticalPropertiesValidationResult> ValidateVerticalPropertiesAsync(string tenantId, string verticalName, string entityType, Dictionary<string, object> properties);
    
    /// <summary>
    /// Registra uma nova vertical dinamicamente
    /// </summary>
    /// <param name="vertical">Instância da vertical</param>
    void RegisterVertical(IVerticalModule vertical);
    
    /// <summary>
    /// Remove registro de uma vertical
    /// </summary>
    /// <param name="verticalName">Nome da vertical</param>
    void UnregisterVertical(string verticalName);
}

/// <summary>
/// Representa uma vertical disponível para ativação
/// </summary>
public class AvailableVertical
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CanActivate { get; set; }
    public List<string> MissingModules { get; set; } = new();
    public List<string> AvailableOptionalModules { get; set; } = new();
    public Dictionary<string, object> DefaultConfiguration { get; set; } = new();
}