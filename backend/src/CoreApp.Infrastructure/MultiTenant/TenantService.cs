using Microsoft.AspNetCore.Http;

namespace CoreApp.Infrastructure.MultiTenant;

/// <summary>
/// Implementação do serviço de tenant multi-tenant para comércios brasileiros
/// Gerencia contexto de tenant via HttpContext
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _currentTenantId;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obtém o ID do tenant atual do header HTTP X-Tenant-ID
    /// </summary>
    /// <returns>ID do tenant ou "default" se não especificado</returns>
    public string? GetCurrentTenantId()
    {
        if (_currentTenantId != null)
            return _currentTenantId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request?.Headers != null)
        {
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                return tenantId.FirstOrDefault() ?? "default";
            }
        }

        return "default";
    }

    /// <summary>
    /// Define o tenant atual (usado principalmente em testes)
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    public void SetCurrentTenant(string tenantId)
    {
        _currentTenantId = tenantId;
    }

    /// <summary>
    /// Verifica se há um tenant válido no contexto
    /// </summary>
    /// <returns>True se há tenant ativo</returns>
    public bool HasCurrentTenant()
    {
        var tenantId = GetCurrentTenantId();
        return !string.IsNullOrEmpty(tenantId) && tenantId != "default";
    }
}
