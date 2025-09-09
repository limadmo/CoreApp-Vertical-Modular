using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.Extensions;

namespace Farmacia.Infrastructure.Middleware;

/// <summary>
/// Attribute de autorização que valida se tenant tem módulo comercial ativo
/// Implementa controle de acesso baseado nos planos contratados pelas farmácias
/// </summary>
/// <remarks>
/// Este attribute é essencial para o modelo SAAS brasileiro, garantindo que
/// apenas farmácias com planos apropriados acessem funcionalidades específicas
/// </remarks>
/// <example>
/// <code>
/// [HttpPost("clientes")]
/// [RequireModule("CUSTOMERS")]
/// public async Task&lt;ActionResult&gt; CriarCliente([FromBody] CriarClienteRequest request)
/// {
///     // Código do endpoint protegido por módulo
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireModuleAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Código do módulo necessário para acessar o recurso
    /// </summary>
    public string ModuleCode { get; }

    /// <summary>
    /// Mensagem customizada de erro (opcional)
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// URL para upgrade de plano (opcional)
    /// </summary>
    public string? UpgradeUrl { get; set; }

    /// <summary>
    /// Se deve permitir acesso durante período de teste
    /// </summary>
    public bool AllowDuringTrial { get; set; } = false;

    /// <summary>
    /// Nível de prioridade da validação (1-10, sendo 10 crítico)
    /// </summary>
    public int Priority { get; set; } = 5;

    public RequireModuleAttribute(string moduleCode)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
            throw new ArgumentException("Código do módulo não pode ser vazio", nameof(moduleCode));

        ModuleCode = moduleCode.ToUpper();
        UpgradeUrl = "/upgrade-plan";
    }

    /// <summary>
    /// Executa validação de autorização baseada em módulos
    /// </summary>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            // Obtém serviços necessários
            var moduleService = context.HttpContext.RequestServices
                .GetRequiredService<IModuleValidationService>();
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireModuleAttribute>>();

            // Obtém tenant ID do contexto
            var tenantId = context.HttpContext.GetTenantId();
            
            if (string.IsNullOrEmpty(tenantId))
            {
                logger?.LogWarning("TenantId não encontrado no contexto da requisição");
                context.Result = CreateUnauthorizedResult("Farmácia não identificada");
                return;
            }

            // Valida módulo de forma assíncrona
            var hasModule = Task.Run(async () => 
                await moduleService.HasActiveModuleAsync(tenantId, ModuleCode)).Result;

            if (!hasModule)
            {
                // Se permite durante teste, verifica se está em período de teste
                if (AllowDuringTrial)
                {
                    var planInfo = Task.Run(async () => 
                        await moduleService.GetActivePlanAsync(tenantId)).Result;
                    
                    if (planInfo?.PeriodoTeste == true && planInfo.DiasTesteRestantes > 0)
                    {
                        logger?.LogDebug("Acesso liberado durante período de teste para módulo {ModuleCode} - Tenant: {TenantId}", 
                            ModuleCode, tenantId);
                        return; // Permite acesso durante teste
                    }
                }

                // Registra tentativa de acesso negado
                logger?.LogWarning("Acesso negado ao módulo {ModuleCode} para tenant {TenantId} - Endpoint: {Endpoint}", 
                    ModuleCode, tenantId, context.HttpContext.Request.Path);

                context.Result = CreateModuleNotActiveResult(tenantId);
                return;
            }

            // Log de acesso autorizado
            logger?.LogDebug("Acesso autorizado ao módulo {ModuleCode} para tenant {TenantId}", 
                ModuleCode, tenantId);

        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireModuleAttribute>>();
            
            logger?.LogError(ex, "Erro na validação do módulo {ModuleCode} para tenant", ModuleCode);
            
            // Em caso de erro, nega acesso por segurança
            context.Result = CreateErrorResult();
        }
    }

    /// <summary>
    /// Cria resposta para módulo não ativo
    /// </summary>
    private ObjectResult CreateModuleNotActiveResult(string tenantId)
    {
        var message = CustomMessage ?? 
            $"O módulo '{ModuleCode}' não está ativo para sua farmácia. " +
            $"Faça upgrade do seu plano para acessar esta funcionalidade.";

        var response = new
        {
            error = "MODULE_NOT_ACTIVE",
            message = message,
            details = new
            {
                moduleRequired = ModuleCode,
                tenantId = tenantId,
                upgradeUrl = UpgradeUrl,
                priority = Priority,
                allowDuringTrial = AllowDuringTrial,
                timestamp = DateTime.UtcNow,
                supportContact = "suporte@farmacia.com.br"
            },
            links = new
            {
                upgrade = UpgradeUrl,
                plans = "/plans",
                support = "/support"
            }
        };

        return new ObjectResult(response)
        {
            StatusCode = 402 // Payment Required
        };
    }

    /// <summary>
    /// Cria resposta para tenant não identificado
    /// </summary>
    private ObjectResult CreateUnauthorizedResult(string message)
    {
        var response = new
        {
            error = "TENANT_NOT_IDENTIFIED",
            message = message,
            details = new
            {
                timestamp = DateTime.UtcNow,
                required = "Valid tenant identification"
            }
        };

        return new ObjectResult(response)
        {
            StatusCode = 401 // Unauthorized
        };
    }

    /// <summary>
    /// Cria resposta para erro interno
    /// </summary>
    private ObjectResult CreateErrorResult()
    {
        var response = new
        {
            error = "MODULE_VALIDATION_ERROR",
            message = "Erro interno na validação de módulos. Tente novamente em alguns instantes.",
            details = new
            {
                timestamp = DateTime.UtcNow,
                supportContact = "suporte@farmacia.com.br"
            }
        };

        return new ObjectResult(response)
        {
            StatusCode = 500 // Internal Server Error
        };
    }
}

/// <summary>
/// Attribute específico para funcionalidades do plano Professional
/// Atalho para módulos que requerem plano Professional ou superior
/// </summary>
public class RequireProfessionalAttribute : RequireModuleAttribute
{
    public RequireProfessionalAttribute(string moduleCode) : base(moduleCode)
    {
        CustomMessage = "Esta funcionalidade está disponível apenas para planos Professional e Enterprise. " +
                       "Faça upgrade para acessar recursos avançados.";
        Priority = 7;
    }
}

/// <summary>
/// Attribute específico para funcionalidades do plano Enterprise
/// Atalho para módulos que requerem plano Enterprise
/// </summary>
public class RequireEnterpriseAttribute : RequireModuleAttribute
{
    public RequireEnterpriseAttribute(string moduleCode) : base(moduleCode)
    {
        CustomMessage = "Esta funcionalidade está disponível apenas para o plano Enterprise. " +
                       "Faça upgrade para o plano mais completo.";
        Priority = 9;
    }
}

/// <summary>
/// Attribute para funcionalidades em beta que permitem teste
/// Permite acesso durante período de teste mesmo sem o módulo ativo
/// </summary>
public class RequireModuleBetaAttribute : RequireModuleAttribute
{
    public RequireModuleBetaAttribute(string moduleCode) : base(moduleCode)
    {
        AllowDuringTrial = true;
        CustomMessage = "Esta funcionalidade está em beta. Durante o período de teste, " +
                       "você pode experimentar gratuitamente.";
        Priority = 3;
    }
}

/// <summary>
/// Attribute para múltiplos módulos (OR logic)
/// Permite acesso se pelo menos um dos módulos estiver ativo
/// </summary>
/// <example>
/// <code>
/// [RequireAnyModule("BASIC_REPORTS", "ADVANCED_REPORTS")]
/// public async Task&lt;ActionResult&gt; GerarRelatorio()
/// {
///     // Acessível com qualquer módulo de relatórios
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireAnyModuleAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Lista de módulos (qualquer um serve)
    /// </summary>
    public string[] ModuleCodes { get; }

    /// <summary>
    /// Mensagem customizada de erro
    /// </summary>
    public string? CustomMessage { get; set; }

    public RequireAnyModuleAttribute(params string[] moduleCodes)
    {
        if (moduleCodes == null || moduleCodes.Length == 0)
            throw new ArgumentException("Deve especificar pelo menos um módulo", nameof(moduleCodes));

        ModuleCodes = moduleCodes.Select(m => m.ToUpper()).ToArray();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            var moduleService = context.HttpContext.RequestServices
                .GetRequiredService<IModuleValidationService>();
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireAnyModuleAttribute>>();

            var tenantId = context.HttpContext.GetTenantId();
            
            if (string.IsNullOrEmpty(tenantId))
            {
                context.Result = CreateUnauthorizedResult("Farmácia não identificada");
                return;
            }

            // Verifica se tem qualquer um dos módulos
            var hasAnyModule = Task.Run(async () =>
            {
                var moduleStatuses = await moduleService.HasActiveModulesAsync(tenantId, ModuleCodes);
                return moduleStatuses.Values.Any(active => active);
            }).Result;

            if (!hasAnyModule)
            {
                var message = CustomMessage ?? 
                    $"Esta funcionalidade requer um dos seguintes módulos: {string.Join(", ", ModuleCodes)}";

                logger?.LogWarning("Acesso negado - nenhum dos módulos {ModuleCodes} ativo para tenant {TenantId}", 
                    string.Join(",", ModuleCodes), tenantId);

                context.Result = CreateModuleNotActiveResult(tenantId, message);
                return;
            }

            logger?.LogDebug("Acesso autorizado com um dos módulos {ModuleCodes} para tenant {TenantId}", 
                string.Join(",", ModuleCodes), tenantId);

        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireAnyModuleAttribute>>();
            
            logger?.LogError(ex, "Erro na validação de múltiplos módulos para tenant");
            context.Result = CreateErrorResult();
        }
    }

    private ObjectResult CreateUnauthorizedResult(string message)
    {
        return new ObjectResult(new { error = "TENANT_NOT_IDENTIFIED", message })
        {
            StatusCode = 401
        };
    }

    private ObjectResult CreateModuleNotActiveResult(string tenantId, string message)
    {
        return new ObjectResult(new
        {
            error = "MODULES_NOT_ACTIVE",
            message = message,
            details = new
            {
                modulesRequired = ModuleCodes,
                tenantId = tenantId,
                upgradeUrl = "/upgrade-plan"
            }
        })
        {
            StatusCode = 402
        };
    }

    private ObjectResult CreateErrorResult()
    {
        return new ObjectResult(new
        {
            error = "MODULE_VALIDATION_ERROR",
            message = "Erro interno na validação de módulos"
        })
        {
            StatusCode = 500
        };
    }
}