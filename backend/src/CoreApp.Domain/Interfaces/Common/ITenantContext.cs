namespace CoreApp.Domain.Interfaces.Common;

/// <summary>
/// Interface para contexto de tenant atual
/// Fornece informações sobre o tenant ativo na requisição
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Obtém o ID do tenant atual
    /// </summary>
    /// <returns>ID do tenant atual</returns>
    string GetCurrentTenantId();

    /// <summary>
    /// Obtém o ID do usuário atual
    /// </summary>
    /// <returns>ID do usuário atual</returns>
    string GetCurrentUserId();

    /// <summary>
    /// Define o tenant atual
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    void SetCurrentTenant(string tenantId);

    /// <summary>
    /// Define o usuário atual
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    void SetCurrentUser(string userId);

    /// <summary>
    /// Verifica se há um tenant definido
    /// </summary>
    /// <returns>True se há tenant ativo</returns>
    bool HasTenant();

    /// <summary>
    /// Verifica se há um usuário definido
    /// </summary>
    /// <returns>True se há usuário ativo</returns>
    bool HasUser();
}