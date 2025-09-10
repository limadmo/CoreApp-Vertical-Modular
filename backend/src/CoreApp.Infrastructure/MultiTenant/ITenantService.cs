namespace CoreApp.Infrastructure.MultiTenant;

/// <summary>
/// Interface para serviço de tenant (loja/comércio) multi-tenant
/// Gerencia contexto de tenant para isolamento de dados
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Obtém o ID do tenant atual
    /// </summary>
    /// <returns>ID do tenant ou null se não houver tenant ativo</returns>
    string? GetCurrentTenantId();

    /// <summary>
    /// Define o tenant atual para o contexto
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    void SetCurrentTenant(string tenantId);

    /// <summary>
    /// Verifica se há um tenant ativo no contexto atual
    /// </summary>
    /// <returns>True se há tenant ativo</returns>
    bool HasCurrentTenant();
}
