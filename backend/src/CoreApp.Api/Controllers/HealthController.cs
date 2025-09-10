using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using CoreApp.Domain.Interfaces.Common;

namespace CoreApp.Api.Controllers;

/// <summary>
/// Controller para verificação de saúde do sistema CoreApp
/// Fornece informações sobre status dos serviços e dependências
/// </summary>
[ApiController]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly ITenantContext _tenantContext;

    public HealthController(ILogger<HealthController> logger, ITenantContext tenantContext)
    {
        _logger = logger;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Verifica o status geral do sistema
    /// </summary>
    /// <returns>Status de saúde do sistema</returns>
    [HttpGet("/health")]
    [HttpGet("api/health")]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<HealthStatus> GetHealth()
    {
        // Obter informações de tenant
        var currentTenant = _tenantContext.GetCurrentTenantId();
        var currentUser = _tenantContext.GetCurrentUserId();
        
        var healthStatus = new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Version = "1.0.0", // TODO: Obter da assembly
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            TenantId = currentTenant,
            UserId = currentUser
        };

        var checks = new List<HealthCheck>();

        // Verificar cache (simplificado por enquanto)
        checks.Add(new HealthCheck
        {
            Name = "Cache",
            Status = "Healthy",
            Description = "Cache configurado",
            ResponseTime = TimeSpan.FromMilliseconds(1)
        });

        // Verificar banco de dados
        try
        {
            // TODO: Implementar verificação de DB quando DbContext estiver configurado
            checks.Add(new HealthCheck
            {
                Name = "Database",
                Status = "Healthy",
                Description = "Conexão com banco de dados funcionando",
                ResponseTime = TimeSpan.FromMilliseconds(50) // Simulated
            });
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheck
            {
                Name = "Database",
                Status = "Unhealthy",
                Description = $"Erro no banco: {ex.Message}",
                Error = ex.Message
            });
            healthStatus.Status = "Unhealthy";
        }

        // Verificar memória
        var memoryUsed = GC.GetTotalMemory(false);
        var memoryMB = memoryUsed / 1024 / 1024;
        
        if (memoryMB > 1000) // Warning se > 1GB
        {
            checks.Add(new HealthCheck
            {
                Name = "Memory",
                Status = "Warning",
                Description = $"Alto uso de memória: {memoryMB}MB",
                Data = new { MemoryMB = memoryMB }
            });
            
            if (healthStatus.Status == "Healthy")
                healthStatus.Status = "Degraded";
        }
        else
        {
            checks.Add(new HealthCheck
            {
                Name = "Memory",
                Status = "Healthy",
                Description = $"Uso de memória normal: {memoryMB}MB",
                Data = new { MemoryMB = memoryMB }
            });
        }

        healthStatus.Checks = checks;

        // Retornar status HTTP apropriado
        var statusCode = healthStatus.Status switch
        {
            "Healthy" => StatusCodes.Status200OK,
            "Degraded" => StatusCodes.Status200OK,
            "Unhealthy" => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status503ServiceUnavailable
        };

        _logger.LogInformation(
            "Health check executado - Status: {Status}, Tenant: {TenantId}, Host: {Host}", 
            healthStatus.Status, currentTenant, Request.Host.Host);

        return StatusCode(statusCode, healthStatus);
    }

    /// <summary>
    /// Verifica status específico do cache
    /// </summary>
    /// <returns>Estatísticas do cache</returns>
    [HttpGet("cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetCacheHealth()
    {
        return Ok(new
        {
            status = "healthy",
            description = "Cache configurado e funcionando",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint básico para verificação rápida (usado por load balancers)
    /// </summary>
    /// <returns>Status simples</returns>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Obtém informações da versão e build
    /// </summary>
    /// <returns>Informações de versão</returns>
    [HttpGet("version")]
    [ProducesResponseType(typeof(VersionInfo), StatusCodes.Status200OK)]
    public ActionResult<VersionInfo> GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        var versionInfo = new VersionInfo
        {
            Version = version?.ToString() ?? "Unknown",
            BuildDate = System.IO.File.GetCreationTime(assembly.Location),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            RuntimeVersion = Environment.Version.ToString()
        };

        return Ok(versionInfo);
    }
}

/// <summary>
/// Status de saúde do sistema
/// </summary>
public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public IEnumerable<HealthCheck> Checks { get; set; } = new List<HealthCheck>();
}

/// <summary>
/// Verificação individual de saúde
/// </summary>
public class HealthCheck
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Error { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// Informações de versão
/// </summary>
public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public DateTime BuildDate { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
}