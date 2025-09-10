using CoreApp.Domain.Interfaces.Common;
using Microsoft.AspNetCore.Http;

namespace CoreApp.Infrastructure.Services;

/// <summary>
/// Implementação básica do contexto de tenant
/// Obtém informações do tenant atual via header HTTP ou subdomain
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _currentTenantId;
    private string? _currentUserId;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obtém o ID do tenant atual do header ou subdomain
    /// </summary>
    public string GetCurrentTenantId()
    {
        if (!string.IsNullOrEmpty(_currentTenantId))
            return _currentTenantId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "default"; // Fallback para testes

        // Tentar obter do header X-Tenant-ID
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            _currentTenantId = tenantHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(_currentTenantId))
                return _currentTenantId;
        }

        // Tentar obter do subdomain
        var host = httpContext.Request.Host.Host;
        if (!string.IsNullOrEmpty(host))
        {
            // Para desenvolvimento local com .localhost
            if (host.EndsWith(".api.localhost"))
            {
                var subdomain = host.Replace(".api.localhost", "");
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "api")
                {
                    _currentTenantId = subdomain.ToLowerInvariant();
                    return _currentTenantId;
                }
            }
            
            // Para produção com domínio real
            if (!host.StartsWith("127.0.0.1") && !host.Equals("localhost"))
            {
                var parts = host.Split('.');
                if (parts.Length >= 3) // ex: loja123.api.coreapp.com.br
                {
                    var subdomain = parts[0];
                    if (!string.IsNullOrEmpty(subdomain) && subdomain != "api" && subdomain != "www")
                    {
                        _currentTenantId = subdomain.ToLowerInvariant();
                        return _currentTenantId;
                    }
                }
            }
        }

        // Fallback: tenant padrão para desenvolvimento
        _currentTenantId = "demo";
        return _currentTenantId;
    }

    /// <summary>
    /// Obtém o ID do usuário atual do claims ou header
    /// </summary>
    public string GetCurrentUserId()
    {
        if (!string.IsNullOrEmpty(_currentUserId))
            return _currentUserId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Tentar obter do claim NameIdentifier
            _currentUserId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(_currentUserId))
                return _currentUserId;

            // Tentar obter do claim personalizado
            _currentUserId = httpContext.User.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(_currentUserId))
                return _currentUserId;
        }

        // Tentar obter do header para desenvolvimento
        if (httpContext?.Request.Headers.TryGetValue("X-User-ID", out var userHeader) == true)
        {
            _currentUserId = userHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(_currentUserId))
                return _currentUserId;
        }

        // Fallback: usuário de sistema para operações automáticas
        _currentUserId = "system";
        return _currentUserId;
    }

    /// <summary>
    /// Define o tenant atual (usado para override em testes ou situações especiais)
    /// </summary>
    public void SetCurrentTenant(string tenantId)
    {
        _currentTenantId = tenantId;
    }

    /// <summary>
    /// Define o usuário atual (usado para override em testes ou situações especiais)
    /// </summary>
    public void SetCurrentUser(string userId)
    {
        _currentUserId = userId;
    }

    /// <summary>
    /// Verifica se há um tenant definido
    /// </summary>
    public bool HasTenant()
    {
        var tenantId = GetCurrentTenantId();
        return !string.IsNullOrEmpty(tenantId) && tenantId != "default";
    }

    /// <summary>
    /// Verifica se há um usuário definido
    /// </summary>
    public bool HasUser()
    {
        var userId = GetCurrentUserId();
        return !string.IsNullOrEmpty(userId) && userId != "system";
    }
}