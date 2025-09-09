using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Farmacia.Infrastructure.MultiTenant;

/// <summary>
/// Implementação do serviço de multi-tenancy para farmácias brasileiras
/// Extrai informações de tenant de headers HTTP e subdomínios
/// </summary>
/// <remarks>
/// Este serviço suporta múltiplas estratégias de identificação de tenant:
/// 1. Header X-Tenant-ID
/// 2. Subdomínio (farmacia-sp.localhost)
/// 3. Claim do JWT token
/// </remarks>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly string _defaultTenant;
    private string? _currentTenantOverride;

    public TenantService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _defaultTenant = _configuration["MultiTenant:DefaultTenant"] ?? "demo";
    }

    /// <summary>
    /// Obtém o identificador do tenant atual usando múltiplas estratégias
    /// </summary>
    /// <returns>ID do tenant atual</returns>
    public string GetCurrentTenantId()
    {
        // 1. Verificar se foi definido manualmente (para migrations/seeds)
        if (!string.IsNullOrEmpty(_currentTenantOverride))
        {
            return _currentTenantOverride;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return _defaultTenant;
        }

        // 2. Estratégia: Header X-Tenant-ID (prioritário)
        var tenantFromHeader = ExtractTenantFromHeader(httpContext);
        if (!string.IsNullOrEmpty(tenantFromHeader))
        {
            return NormalizeTenantId(tenantFromHeader);
        }

        // 3. Estratégia: Subdomínio do hostname
        var tenantFromSubdomain = ExtractTenantFromSubdomain(httpContext);
        if (!string.IsNullOrEmpty(tenantFromSubdomain))
        {
            return NormalizeTenantId(tenantFromSubdomain);
        }

        // 4. Estratégia: Claim do JWT token
        var tenantFromToken = ExtractTenantFromToken(httpContext);
        if (!string.IsNullOrEmpty(tenantFromToken))
        {
            return NormalizeTenantId(tenantFromToken);
        }

        // 5. Fallback: tenant padrão
        return _defaultTenant;
    }

    /// <summary>
    /// Obtém o ID do usuário atual do token JWT
    /// </summary>
    /// <returns>ID do usuário logado</returns>
    public string GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Tentar obter do claim padrão
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.User.FindFirst("sub")?.Value
                      ?? httpContext.User.FindFirst("user_id")?.Value
                      ?? httpContext.User.FindFirst("uid")?.Value;

            return userId ?? "system";
        }

        return "system";
    }

    /// <summary>
    /// Verifica se existe um tenant válido no contexto atual
    /// </summary>
    /// <returns>True se tenant está configurado</returns>
    public bool HasCurrentTenant()
    {
        var tenantId = GetCurrentTenantId();
        return !string.IsNullOrEmpty(tenantId) && tenantId != _defaultTenant;
    }

    /// <summary>
    /// Define o tenant atual manualmente (para migrations/seeds)
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    public void SetCurrentTenant(string tenantId)
    {
        _currentTenantOverride = NormalizeTenantId(tenantId);
    }

    /// <summary>
    /// Obtém informações detalhadas do tenant atual
    /// </summary>
    /// <returns>Informações do tenant</returns>
    public TenantInfo GetCurrentTenantInfo()
    {
        var tenantId = GetCurrentTenantId();
        
        // TODO: Implementar cache Redis para informações de tenant
        // TODO: Buscar dados reais do banco de dados
        
        return new TenantInfo
        {
            Id = tenantId,
            NomeFantasia = GetTenantDisplayName(tenantId),
            RazaoSocial = $"Farmácia {GetTenantDisplayName(tenantId)} LTDA",
            CNPJ = "00.000.000/0001-00", // TODO: Buscar do banco
            Estado = ExtractStateFromTenantId(tenantId),
            Cidade = ExtractCityFromTenantId(tenantId),
            Plano = "Starter", // TODO: Buscar do banco
            ModulosAtivos = GetActiveModulesForTenant(tenantId),
            Status = "Ativo",
            DataCriacao = DateTime.UtcNow.AddMonths(-3), // TODO: Buscar do banco
            Configuracoes = new Dictionary<string, string>()
        };
    }

    /// <summary>
    /// Verifica se o tenant atual tem acesso ao módulo comercial
    /// </summary>
    /// <param name="moduleCode">Código do módulo</param>
    /// <returns>True se tem acesso</returns>
    public bool HasModuleAccess(string moduleCode)
    {
        var tenantInfo = GetCurrentTenantInfo();
        
        // Enterprise tem acesso a todos os módulos
        if (tenantInfo.Plano == "Enterprise")
        {
            return true;
        }

        // Verificar se o módulo está na lista de módulos ativos
        return tenantInfo.ModulosAtivos.Contains(moduleCode.ToUpperInvariant());
    }

    #region Métodos Privados de Extração de Tenant

    /// <summary>
    /// Extrai tenant do header X-Tenant-ID
    /// </summary>
    /// <param name="httpContext">Contexto HTTP atual</param>
    /// <returns>ID do tenant ou null</returns>
    private string? ExtractTenantFromHeader(HttpContext httpContext)
    {
        // Verificar header X-Tenant-ID
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            return tenantHeader.FirstOrDefault();
        }

        // Verificar header alternativo Tenant-ID
        if (httpContext.Request.Headers.TryGetValue("Tenant-ID", out var altTenantHeader))
        {
            return altTenantHeader.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Extrai tenant do subdomínio (farmacia-sp.localhost)
    /// </summary>
    /// <param name="httpContext">Contexto HTTP atual</param>
    /// <returns>ID do tenant ou null</returns>
    private string? ExtractTenantFromSubdomain(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host.ToLowerInvariant();
        var tenantDomain = _configuration["MultiTenant:TenantDomain"] ?? "localhost";

        // Se não é um subdomínio do domínio configurado, retornar null
        if (!host.EndsWith($".{tenantDomain}"))
        {
            return null;
        }

        // Extrair a primeira parte do subdomínio
        var subdomain = host.Replace($".{tenantDomain}", "");
        
        // Validar se não é um subdomínio reservado
        var reservedSubdomains = new[] { "www", "api", "admin", "app", "mail", "logs", "traefik" };
        if (reservedSubdomains.Contains(subdomain))
        {
            return null;
        }

        return subdomain;
    }

    /// <summary>
    /// Extrai tenant do token JWT
    /// </summary>
    /// <param name="httpContext">Contexto HTTP atual</param>
    /// <returns>ID do tenant ou null</returns>
    private string? ExtractTenantFromToken(HttpContext httpContext)
    {
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.FindFirst("tenant_id")?.Value
                ?? httpContext.User.FindFirst("tenant")?.Value;
        }

        return null;
    }

    /// <summary>
    /// Normaliza o ID do tenant para padrão consistente
    /// </summary>
    /// <param name="tenantId">ID do tenant bruto</param>
    /// <returns>ID normalizado</returns>
    private string NormalizeTenantId(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return _defaultTenant;
        }

        // Converter para minúsculo e remover espaços
        var normalized = tenantId.Trim().ToLowerInvariant();
        
        // Substituir espaços e caracteres especiais por hífens
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9-]", "-");
        
        // Remover hífens duplicados
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"-+", "-");
        
        // Remover hífens no início e fim
        normalized = normalized.Trim('-');

        return string.IsNullOrEmpty(normalized) ? _defaultTenant : normalized;
    }

    /// <summary>
    /// Obtém nome amigável do tenant para exibição
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Nome para exibição</returns>
    private string GetTenantDisplayName(string tenantId)
    {
        return tenantId switch
        {
            "demo" => "Farmácia Demo",
            "farmacia-sp" => "Farmácia São Paulo",
            "farmacia-rj" => "Farmácia Rio de Janeiro",
            "farmacia-mg" => "Farmácia Minas Gerais",
            _ => $"Farmácia {tenantId.Replace("-", " ").ToTitleCase()}"
        };
    }

    /// <summary>
    /// Extrai estado do ID do tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Código do estado</returns>
    private string ExtractStateFromTenantId(string tenantId)
    {
        if (tenantId.Contains("-sp")) return "SP";
        if (tenantId.Contains("-rj")) return "RJ";
        if (tenantId.Contains("-mg")) return "MG";
        if (tenantId.Contains("-rs")) return "RS";
        if (tenantId.Contains("-pr")) return "PR";
        
        return "SP"; // Padrão São Paulo
    }

    /// <summary>
    /// Extrai cidade do ID do tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Nome da cidade</returns>
    private string ExtractCityFromTenantId(string tenantId)
    {
        if (tenantId.Contains("sao-paulo") || tenantId.Contains("-sp")) return "São Paulo";
        if (tenantId.Contains("rio-de-janeiro") || tenantId.Contains("-rj")) return "Rio de Janeiro";
        if (tenantId.Contains("belo-horizonte") || tenantId.Contains("-mg")) return "Belo Horizonte";
        
        return "São Paulo"; // Padrão
    }

    /// <summary>
    /// Obtém módulos ativos para o tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <returns>Lista de módulos ativos</returns>
    private List<string> GetActiveModulesForTenant(string tenantId)
    {
        // TODO: Buscar do banco de dados baseado no plano contratado
        return new List<string> { "PRODUCTS", "SALES", "STOCK", "USERS" }; // Plano Starter padrão
    }

    #endregion
}

/// <summary>
/// Extensões de string para o serviço de tenant
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converte string para Title Case
    /// </summary>
    /// <param name="input">String de entrada</param>
    /// <returns>String em Title Case</returns>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..].ToLowerInvariant();
            }
        }

        return string.Join(' ', words);
    }
}