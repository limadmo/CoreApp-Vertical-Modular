using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Farmacia.Infrastructure.Extensions;

/// <summary>
/// Extensões para HttpContext facilitando acesso a informações de tenant e usuário
/// Centraliza lógica de extração de dados do contexto HTTP multi-tenant
/// </summary>
/// <remarks>
/// Estas extensões são essenciais para o sistema SAAS farmacêutico brasileiro,
/// permitindo fácil acesso ao tenant e usuário atual em qualquer ponto da aplicação
/// </remarks>
public static class HttpContextExtensions
{
    // Constantes para chaves no contexto
    private const string TENANT_ID_KEY = "TenantId";
    private const string USER_ID_KEY = "UserId";
    private const string TENANT_INFO_KEY = "TenantInfo";
    private const string USER_INFO_KEY = "UserInfo";

    #region Tenant Extensions

    /// <summary>
    /// Obtém o ID do tenant atual do contexto HTTP
    /// </summary>
    /// <param name="httpContext">Contexto HTTP atual</param>
    /// <returns>ID do tenant ou string vazia se não encontrado</returns>
    public static string GetTenantId(this HttpContext httpContext)
    {
        // Primeiro tenta obter do contexto (já processado pelo middleware)
        if (httpContext.Items.ContainsKey(TENANT_ID_KEY))
        {
            return httpContext.Items[TENANT_ID_KEY]?.ToString() ?? string.Empty;
        }

        // Tenta obter do header HTTP
        if (httpContext.Request.Headers.ContainsKey("X-Tenant-ID"))
        {
            var tenantId = httpContext.Request.Headers["X-Tenant-ID"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantId))
            {
                httpContext.Items[TENANT_ID_KEY] = tenantId;
                return tenantId;
            }
        }

        // Tenta obter do subdomínio (padrão brasileiro: farmacia.dominio.com.br)
        var host = httpContext.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var parts = host.Split('.');
            if (parts.Length >= 3) // farmacia.dominio.com.br
            {
                var tenantId = parts[0];
                if (!IsSystemSubdomain(tenantId))
                {
                    httpContext.Items[TENANT_ID_KEY] = tenantId;
                    return tenantId;
                }
            }
        }

        // Tenta obter do JWT token
        var claimsTenantId = httpContext.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(claimsTenantId))
        {
            httpContext.Items[TENANT_ID_KEY] = claimsTenantId;
            return claimsTenantId;
        }

        // Se não encontrou, retorna vazio
        return string.Empty;
    }

    /// <summary>
    /// Define o ID do tenant no contexto HTTP
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <param name="tenantId">ID do tenant</param>
    public static void SetTenantId(this HttpContext httpContext, string tenantId)
    {
        if (!string.IsNullOrEmpty(tenantId))
        {
            httpContext.Items[TENANT_ID_KEY] = tenantId;
        }
    }

    /// <summary>
    /// Verifica se há um tenant válido no contexto
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>True se tenant está definido</returns>
    public static bool HasTenant(this HttpContext httpContext)
    {
        return !string.IsNullOrEmpty(httpContext.GetTenantId());
    }

    /// <summary>
    /// Obtém informações completas do tenant atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Informações do tenant ou null</returns>
    public static TenantInfo? GetTenantInfo(this HttpContext httpContext)
    {
        if (httpContext.Items.ContainsKey(TENANT_INFO_KEY))
        {
            return httpContext.Items[TENANT_INFO_KEY] as TenantInfo;
        }

        return null;
    }

    /// <summary>
    /// Define informações completas do tenant no contexto
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <param name="tenantInfo">Informações do tenant</param>
    public static void SetTenantInfo(this HttpContext httpContext, TenantInfo tenantInfo)
    {
        httpContext.Items[TENANT_INFO_KEY] = tenantInfo;
        httpContext.Items[TENANT_ID_KEY] = tenantInfo.Id;
    }

    #endregion

    #region User Extensions

    /// <summary>
    /// Obtém o ID do usuário atual do contexto HTTP
    /// </summary>
    /// <param name="httpContext">Contexto HTTP atual</param>
    /// <returns>ID do usuário ou Guid.Empty se não encontrado</returns>
    public static Guid GetCurrentUserId(this HttpContext httpContext)
    {
        // Primeiro tenta obter do contexto
        if (httpContext.Items.ContainsKey(USER_ID_KEY))
        {
            var userIdObj = httpContext.Items[USER_ID_KEY];
            if (userIdObj is Guid userId)
                return userId;
            
            if (Guid.TryParse(userIdObj?.ToString(), out var parsedUserId))
                return parsedUserId;
        }

        // Tenta obter do JWT token
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? httpContext.User.FindFirst("user_id")?.Value 
                         ?? httpContext.User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var claimUserId))
        {
            httpContext.Items[USER_ID_KEY] = claimUserId;
            return claimUserId;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Define o ID do usuário no contexto HTTP
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <param name="userId">ID do usuário</param>
    public static void SetCurrentUserId(this HttpContext httpContext, Guid userId)
    {
        httpContext.Items[USER_ID_KEY] = userId;
    }

    /// <summary>
    /// Obtém o nome do usuário atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Nome do usuário ou string vazia</returns>
    public static string GetCurrentUserName(this HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value 
               ?? httpContext.User.FindFirst("name")?.Value 
               ?? string.Empty;
    }

    /// <summary>
    /// Obtém o email do usuário atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Email do usuário ou string vazia</returns>
    public static string GetCurrentUserEmail(this HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Email)?.Value 
               ?? httpContext.User.FindFirst("email")?.Value 
               ?? string.Empty;
    }

    /// <summary>
    /// Verifica se usuário está autenticado
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>True se usuário está autenticado</returns>
    public static bool IsAuthenticated(this HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Verifica se usuário tem role específica
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <param name="role">Nome da role</param>
    /// <returns>True se usuário tem a role</returns>
    public static bool HasRole(this HttpContext httpContext, string role)
    {
        return httpContext.User.IsInRole(role);
    }

    /// <summary>
    /// Obtém todas as roles do usuário atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Lista de roles</returns>
    public static List<string> GetCurrentUserRoles(this HttpContext httpContext)
    {
        return httpContext.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Obtém informações completas do usuário atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Informações do usuário ou null</returns>
    public static UserInfo? GetCurrentUserInfo(this HttpContext httpContext)
    {
        if (httpContext.Items.ContainsKey(USER_INFO_KEY))
        {
            return httpContext.Items[USER_INFO_KEY] as UserInfo;
        }

        return null;
    }

    /// <summary>
    /// Define informações completas do usuário no contexto
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <param name="userInfo">Informações do usuário</param>
    public static void SetCurrentUserInfo(this HttpContext httpContext, UserInfo userInfo)
    {
        httpContext.Items[USER_INFO_KEY] = userInfo;
        httpContext.Items[USER_ID_KEY] = userInfo.Id;
    }

    #endregion

    #region Request Information Extensions

    /// <summary>
    /// Obtém IP real do cliente considerando proxies/load balancers
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Endereço IP do cliente</returns>
    public static string GetClientIpAddress(this HttpContext httpContext)
    {
        // Verifica headers de proxy
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    /// <summary>
    /// Obtém User-Agent do cliente
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>User-Agent string</returns>
    public static string GetUserAgent(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Verifica se requisição é de API (vs browser)
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>True se é requisição de API</returns>
    public static bool IsApiRequest(this HttpContext httpContext)
    {
        var accept = httpContext.Request.Headers["Accept"].FirstOrDefault() ?? string.Empty;
        var userAgent = httpContext.GetUserAgent();
        
        return accept.Contains("application/json") || 
               userAgent.Contains("API") || 
               userAgent.Contains("curl") ||
               userAgent.Contains("Postman");
    }

    /// <summary>
    /// Verifica se requisição é mobile
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>True se é dispositivo mobile</returns>
    public static bool IsMobileRequest(this HttpContext httpContext)
    {
        var userAgent = httpContext.GetUserAgent().ToLower();
        
        var mobileKeywords = new[] 
        { 
            "mobile", "android", "iphone", "ipad", "windows phone",
            "blackberry", "opera mini", "mobile safari"
        };

        return mobileKeywords.Any(keyword => userAgent.Contains(keyword));
    }

    #endregion

    #region Session Extensions

    /// <summary>
    /// Obtém ID da sessão atual
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>ID da sessão</returns>
    public static string GetSessionId(this HttpContext httpContext)
    {
        // Primeiro tenta obter do JWT
        var sessionId = httpContext.User.FindFirst("session_id")?.Value;
        if (!string.IsNullOrEmpty(sessionId))
            return sessionId;

        // Se não tem no JWT, gera baseado em informações da requisição
        return GenerateSessionId(httpContext);
    }

    /// <summary>
    /// Define contexto de auditoria para logs
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>Contexto de auditoria</returns>
    public static AuditContext GetAuditContext(this HttpContext httpContext)
    {
        return new AuditContext
        {
            TenantId = httpContext.GetTenantId(),
            UserId = httpContext.GetCurrentUserId(),
            UserName = httpContext.GetCurrentUserName(),
            IpAddress = httpContext.GetClientIpAddress(),
            UserAgent = httpContext.GetUserAgent(),
            SessionId = httpContext.GetSessionId(),
            Timestamp = DateTime.UtcNow,
            RequestPath = httpContext.Request.Path,
            RequestMethod = httpContext.Request.Method
        };
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Verifica se subdomínio é do sistema (não é tenant)
    /// </summary>
    /// <param name="subdomain">Subdomínio a verificar</param>
    /// <returns>True se é subdomínio do sistema</returns>
    private static bool IsSystemSubdomain(string subdomain)
    {
        var systemSubdomains = new[]
        {
            "www", "api", "admin", "app", "mail", "ftp", "static",
            "cdn", "assets", "docs", "help", "support", "status"
        };

        return systemSubdomains.Contains(subdomain.ToLower());
    }

    /// <summary>
    /// Gera ID de sessão baseado em informações da requisição
    /// </summary>
    /// <param name="httpContext">Contexto HTTP</param>
    /// <returns>ID único da sessão</returns>
    private static string GenerateSessionId(HttpContext httpContext)
    {
        var userId = httpContext.GetCurrentUserId();
        var tenantId = httpContext.GetTenantId();
        var ipAddress = httpContext.GetClientIpAddress();
        var userAgent = httpContext.GetUserAgent();
        
        var sessionData = $"{userId}|{tenantId}|{ipAddress}|{userAgent}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(sessionData));
        
        return Convert.ToBase64String(hash)[..16]; // Primeiros 16 caracteres
    }

    #endregion
}

/// <summary>
/// Informações básicas do tenant para contexto HTTP
/// </summary>
public class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<string> ActiveModules { get; set; } = new List<string>();
}

/// <summary>
/// Informações básicas do usuário para contexto HTTP
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Contexto de auditoria para logs estruturados
/// </summary>
public class AuditContext
{
    public string TenantId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
}