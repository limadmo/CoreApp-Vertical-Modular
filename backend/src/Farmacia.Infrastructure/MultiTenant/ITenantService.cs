namespace Farmacia.Infrastructure.MultiTenant;

/// <summary>
/// Serviço responsável por gerenciar informações do tenant atual
/// no contexto de requisições multi-tenant farmacêuticas
/// </summary>
/// <remarks>
/// Este serviço é fundamental para o isolamento de dados entre farmácias
/// e deve ser injetado em todas as operações que precisam de contexto de tenant
/// </remarks>
public interface ITenantService
{
    /// <summary>
    /// Obtém o identificador do tenant (farmácia) da requisição atual
    /// </summary>
    /// <returns>ID do tenant atual</returns>
    /// <example>farmacia-sp-centro, farmacia-rj-copacabana</example>
    string GetCurrentTenantId();

    /// <summary>
    /// Obtém o identificador do usuário da requisição atual
    /// </summary>
    /// <returns>ID do usuário logado</returns>
    /// <example>usuario-123, farmaceutico-456</example>
    string GetCurrentUserId();

    /// <summary>
    /// Verifica se existe um tenant válido no contexto atual
    /// </summary>
    /// <returns>True se tenant está configurado</returns>
    bool HasCurrentTenant();

    /// <summary>
    /// Define o tenant atual (usado em casos específicos como migrations e seeds)
    /// </summary>
    /// <param name="tenantId">Identificador do tenant</param>
    void SetCurrentTenant(string tenantId);

    /// <summary>
    /// Obtém informações detalhadas do tenant atual
    /// </summary>
    /// <returns>Informações do tenant</returns>
    TenantInfo GetCurrentTenantInfo();

    /// <summary>
    /// Verifica se o tenant atual tem acesso ao módulo comercial especificado
    /// </summary>
    /// <param name="moduleCode">Código do módulo</param>
    /// <returns>True se tem acesso ao módulo</returns>
    bool HasModuleAccess(string moduleCode);
}

/// <summary>
/// Informações detalhadas do tenant (farmácia)
/// </summary>
public class TenantInfo
{
    /// <summary>
    /// Identificador único do tenant
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome fantasia da farmácia
    /// </summary>
    public string NomeFantasia { get; set; } = string.Empty;

    /// <summary>
    /// Razão social da empresa
    /// </summary>
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ da farmácia
    /// </summary>
    public string CNPJ { get; set; } = string.Empty;

    /// <summary>
    /// Estado brasileiro onde a farmácia está localizada
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Cidade onde a farmácia está localizada
    /// </summary>
    public string Cidade { get; set; } = string.Empty;

    /// <summary>
    /// Plano comercial contratado (Starter, Professional, Enterprise)
    /// </summary>
    public string Plano { get; set; } = "Starter";

    /// <summary>
    /// Módulos comerciais ativos para esta farmácia
    /// </summary>
    public List<string> ModulosAtivos { get; set; } = new();

    /// <summary>
    /// Status da farmácia (Ativo, Suspenso, Cancelado)
    /// </summary>
    public string Status { get; set; } = "Ativo";

    /// <summary>
    /// Data de criação do tenant
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Configurações específicas da farmácia
    /// </summary>
    public Dictionary<string, string> Configuracoes { get; set; } = new();
}