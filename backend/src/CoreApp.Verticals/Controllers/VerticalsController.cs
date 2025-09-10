using CoreApp.Verticals.Common;
using CoreApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using CoreApp.Domain.Interfaces.Services;

namespace CoreApp.Verticals.Controllers;

/// <summary>
/// Controller para gerenciamento das verticais de negócio
/// Permite ativação, desativação e consulta de verticais por tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VerticalsController : ControllerBase
{
    private readonly IVerticalManager _verticalManager;
    private readonly IModuleValidationService _moduleValidationService;
    private readonly ILogger<VerticalsController> _logger;

    public VerticalsController(
        IVerticalManager verticalManager,
        IModuleValidationService moduleValidationService,
        ILogger<VerticalsController> logger)
    {
        _verticalManager = verticalManager;
        _moduleValidationService = moduleValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as verticais disponíveis no sistema
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<AvailableVertical>>> GetAvailableVerticals()
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            var availableModules = await _moduleValidationService.GetAvailableModulesAsync(tenantId);
            var availableVerticals = await _verticalManager.GetAvailableVerticalsAsync(tenantId, availableModules);

            return Ok(availableVerticals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar verticais disponíveis");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as verticais ativas para o tenant atual
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<VerticalInfo>>> GetActiveVerticals()
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            var activeVerticals = await _verticalManager.GetActiveVerticalsAsync(tenantId);

            var verticalInfos = activeVerticals.Select(v => new VerticalInfo
            {
                Name = v.VerticalName,
                Version = v.Version,
                Description = v.Description,
                RequiredModules = v.RequiredModules.ToList(),
                OptionalModules = v.OptionalModules.ToList()
            });

            return Ok(verticalInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar verticais ativas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma vertical específica
    /// </summary>
    /// <param name="verticalName">Nome da vertical</param>
    [HttpGet("{verticalName}")]
    public ActionResult<VerticalInfo> GetVertical(string verticalName)
    {
        try
        {
            var vertical = _verticalManager.GetVertical(verticalName);
            
            if (vertical == null)
            {
                return NotFound(new { error = $"Vertical '{verticalName}' não encontrada" });
            }

            var verticalInfo = new VerticalInfo
            {
                Name = vertical.VerticalName,
                Version = vertical.Version,
                Description = vertical.Description,
                RequiredModules = vertical.RequiredModules.ToList(),
                OptionalModules = vertical.OptionalModules.ToList(),
                DefaultConfiguration = vertical.DefaultConfigurations
            };

            return Ok(verticalInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter detalhes da vertical {VerticalName}", verticalName);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Ativa uma vertical para o tenant atual
    /// </summary>
    /// <param name="verticalName">Nome da vertical a ativar</param>
    /// <param name="request">Configurações para ativação</param>
    [HttpPost("{verticalName}/activate")]
    public async Task<ActionResult<VerticalActivationResult>> ActivateVertical(
        string verticalName, 
        [FromBody] ActivateVerticalRequest? request = null)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            var configuration = request?.Configuration ?? new Dictionary<string, object>();

            var result = await _verticalManager.ActivateVerticalAsync(tenantId, verticalName, configuration);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Vertical {VerticalName} ativada com sucesso para tenant {TenantId}",
                    verticalName, tenantId);
                
                return Ok(result);
            }
            else
            {
                _logger.LogWarning(
                    "Falha na ativação da vertical {VerticalName} para tenant {TenantId}: {Message}",
                    verticalName, tenantId, result.Message);
                
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Erro ao ativar vertical {VerticalName} para tenant {TenantId}",
                verticalName, HttpContext.GetTenantId());
            
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Desativa uma vertical para o tenant atual
    /// </summary>
    /// <param name="verticalName">Nome da vertical a desativar</param>
    [HttpPost("{verticalName}/deactivate")]
    public async Task<ActionResult<VerticalActivationResult>> DeactivateVertical(string verticalName)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            var result = await _verticalManager.DeactivateVerticalAsync(tenantId, verticalName);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Vertical {VerticalName} desativada com sucesso para tenant {TenantId}",
                    verticalName, tenantId);
                
                return Ok(result);
            }
            else
            {
                _logger.LogWarning(
                    "Falha na desativação da vertical {VerticalName} para tenant {TenantId}: {Message}",
                    verticalName, tenantId, result.Message);
                
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao desativar vertical {VerticalName} para tenant {TenantId}",
                verticalName, HttpContext.GetTenantId());
            
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Valida propriedades verticais para uma entidade
    /// </summary>
    /// <param name="verticalName">Nome da vertical</param>
    /// <param name="request">Dados para validação</param>
    [HttpPost("{verticalName}/validate")]
    public async Task<ActionResult<VerticalPropertiesValidationResult>> ValidateVerticalProperties(
        string verticalName,
        [FromBody] ValidateVerticalPropertiesRequest request)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            
            var result = await _verticalManager.ValidateVerticalPropertiesAsync(
                tenantId, verticalName, request.EntityType, request.Properties);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao validar propriedades da vertical {VerticalName} para tenant {TenantId}",
                verticalName, HttpContext.GetTenantId());
            
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}

#region DTOs

/// <summary>
/// Informações básicas de uma vertical para API
/// </summary>
public class VerticalInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredModules { get; set; } = new();
    public List<string> OptionalModules { get; set; } = new();
    public Dictionary<string, object> DefaultConfiguration { get; set; } = new();
}

/// <summary>
/// Request para ativação de vertical
/// </summary>
public class ActivateVerticalRequest
{
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Request para validação de propriedades verticais
/// </summary>
public class ValidateVerticalPropertiesRequest
{
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

#endregion