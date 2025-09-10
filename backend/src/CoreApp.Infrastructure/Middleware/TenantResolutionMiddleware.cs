using CoreApp.Domain.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace CoreApp.Infrastructure.Middleware;

/// <summary>
/// Middleware para resolução automática de tenant multi-tenant
/// Intercepta requests e resolve o tenant baseado em header ou subdomínio
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        try
        {
            // Resolve o tenant antes de prosseguir
            var resolvedTenant = ResolveTenant(context);
            
            // Define no contexto para uso posterior
            if (!string.IsNullOrEmpty(resolvedTenant) && resolvedTenant != "default")
            {
                tenantContext.SetCurrentTenant(resolvedTenant);
                
                _logger.LogDebug(
                    "Tenant resolvido: {TenantId} para request {Method} {Path}",
                    resolvedTenant, context.Request.Method, context.Request.Path);
            }
            
            // Adiciona header de resposta para debug
            context.Response.Headers["X-Resolved-Tenant"] = resolvedTenant;
            
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Erro na resolução de tenant para request {Method} {Path}",
                context.Request.Method, context.Request.Path);
            
            // Continue sem tenant em caso de erro
            await _next(context);
        }
    }

    /// <summary>
    /// Resolve o tenant baseado no request HTTP
    /// Ordem de prioridade: Header X-Tenant-ID > Subdomínio > Default
    /// </summary>
    private string ResolveTenant(HttpContext context)
    {
        // 1. Tentar obter do header X-Tenant-ID (prioridade alta)
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            var headerValue = tenantHeader.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                _logger.LogDebug("Tenant resolvido via header X-Tenant-ID: {TenantId}", headerValue);
                return headerValue.Trim().ToLowerInvariant();
            }
        }

        // 2. Tentar obter do subdomínio
        var host = context.Request.Host.Host;
        if (!string.IsNullOrEmpty(host))
        {
            // Para desenvolvimento local com .localhost
            if (host.EndsWith(".api.localhost"))
            {
                var subdomain = host.Replace(".api.localhost", "");
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "api")
                {
                    _logger.LogDebug("Tenant resolvido via subdomínio localhost: {TenantId}", subdomain);
                    return subdomain.Trim().ToLowerInvariant();
                }
            }
            
            // Para produção com domínio real
            var parts = host.Split('.');
            if (parts.Length >= 3) // ex: loja123.api.coreapp.com.br
            {
                var subdomain = parts[0];
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "api" && subdomain != "www")
                {
                    _logger.LogDebug("Tenant resolvido via subdomínio produção: {TenantId}", subdomain);
                    return subdomain.Trim().ToLowerInvariant();
                }
            }
        }

        // 3. Verificar se é request para tenant específico via path
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path))
        {
            var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length > 0 && pathSegments[0] == "tenant" && pathSegments.Length > 1)
            {
                var pathTenant = pathSegments[1];
                _logger.LogDebug("Tenant resolvido via path: {TenantId}", pathTenant);
                return pathTenant.Trim().ToLowerInvariant();
            }
        }

        // 4. Fallback: tenant demo para desenvolvimento
        _logger.LogDebug("Usando tenant padrão: demo");
        return "demo";
    }
}

/// <summary>
/// Extensões para configuração do middleware de tenant
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Adiciona o middleware de resolução de tenant ao pipeline
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}