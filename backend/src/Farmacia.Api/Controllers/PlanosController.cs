using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Farmacia.Application.Services;
using Farmacia.Infrastructure.Extensions;
using Farmacia.Infrastructure.Middleware;

namespace Farmacia.Api.Controllers;

/// <summary>
/// Controller para gerenciar planos comerciais do sistema farmacêutico brasileiro
/// Fornece endpoints para consulta, contratação e gerenciamento de planos SAAS
/// </summary>
/// <remarks>
/// Este controller gerencia todas as operações relacionadas aos planos comerciais,
/// incluindo Starter (R$149,90), Professional (R$249,90) e Enterprise (R$399,90)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanosController : ControllerBase
{
    private readonly IPlanManagementService _planService;
    private readonly ILogger<PlanosController> _logger;

    public PlanosController(
        IPlanManagementService planService,
        ILogger<PlanosController> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os planos comerciais disponíveis para contratação
    /// </summary>
    /// <returns>Lista de planos disponíveis com preços em reais</returns>
    /// <response code="200">Lista de planos retornada com sucesso</response>
    [HttpGet("disponiveis")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AvailablePlan>>> ObterPlanosDisponiveis()
    {
        try
        {
            var tenantId = HttpContext.HasTenant() ? HttpContext.GetTenantId() : null;
            var planos = await _planService.GetAvailablePlansAsync(tenantId);
            
            _logger.LogInformation("Planos disponíveis consultados - Total: {Count}", planos.Count);
            
            return Ok(planos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter planos disponíveis");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém o status do plano atual da farmácia logada
    /// </summary>
    /// <returns>Informações detalhadas do plano ativo</returns>
    /// <response code="200">Status do plano retornado com sucesso</response>
    /// <response code="404">Farmácia não possui plano ativo</response>
    [HttpGet("status")]
    public async Task<ActionResult<CurrentPlanStatus>> ObterStatusPlanoAtual()
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { message = "Farmácia não identificada" });
            }

            var status = await _planService.GetCurrentPlanStatusAsync(tenantId);
            if (status == null)
            {
                return NotFound(new { message = "Nenhum plano ativo encontrado para esta farmácia" });
            }

            _logger.LogDebug("Status do plano consultado para tenant {TenantId} - Plano: {PlanName}", 
                tenantId, status.PlanName);

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do plano para tenant {TenantId}", 
                HttpContext.GetTenantId());
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Contrata um novo plano para a farmácia
    /// </summary>
    /// <param name="request">Dados da contratação</param>
    /// <returns>Resultado da contratação</returns>
    /// <response code="201">Plano contratado com sucesso</response>
    /// <response code="400">Dados inválidos ou farmácia já possui plano ativo</response>
    [HttpPost("contratar")]
    [RequireModule("ADMIN")] // Apenas admins podem contratar planos
    public async Task<ActionResult<PlanContractResult>> ContratarPlano([FromBody] ContractPlanRequest request)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { message = "Farmácia não identificada" });
            }

            // Define tenant e usuário automaticamente
            request.TenantId = tenantId;
            request.ContractedBy = HttpContext.GetCurrentUserName();

            var resultado = await _planService.ContractPlanAsync(request);
            
            if (resultado.Success)
            {
                _logger.LogInformation("Plano {PlanCode} contratado com sucesso para tenant {TenantId} por {User}", 
                    request.PlanCode, tenantId, request.ContractedBy);
                
                return CreatedAtAction(
                    nameof(ObterStatusPlanoAtual), 
                    new { }, 
                    resultado);
            }

            return BadRequest(new { message = resultado.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na contratação do plano para tenant {TenantId}", 
                HttpContext.GetTenantId());
            return StatusCode(500, new { message = "Erro interno na contratação" });
        }
    }

    /// <summary>
    /// Faz upgrade do plano atual para um plano superior
    /// </summary>
    /// <param name="request">Dados do upgrade</param>
    /// <returns>Resultado do upgrade</returns>
    /// <response code="200">Upgrade realizado com sucesso</response>
    /// <response code="400">Dados inválidos ou upgrade não permitido</response>
    [HttpPut("upgrade")]
    [RequireModule("ADMIN")]
    public async Task<ActionResult<PlanUpgradeResult>> FazerUpgrade([FromBody] UpgradePlanRequest request)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { message = "Farmácia não identificada" });
            }

            // Define tenant e usuário automaticamente
            request.TenantId = tenantId;
            request.UpgradedBy = HttpContext.GetCurrentUserName();

            var resultado = await _planService.UpgradePlanAsync(request);
            
            if (resultado.Success)
            {
                _logger.LogInformation("Upgrade realizado com sucesso para tenant {TenantId} - Novo plano: {NewPlan}", 
                    tenantId, resultado.NewPlan?.Nome);
                
                return Ok(resultado);
            }

            return BadRequest(new { message = resultado.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upgrade do plano para tenant {TenantId}", 
                HttpContext.GetTenantId());
            return StatusCode(500, new { message = "Erro interno no upgrade" });
        }
    }

    /// <summary>
    /// Cancela o plano ativo da farmácia
    /// </summary>
    /// <param name="request">Dados do cancelamento</param>
    /// <returns>Resultado do cancelamento</returns>
    /// <response code="200">Plano cancelado com sucesso</response>
    /// <response code="400">Dados inválidos ou nenhum plano ativo</response>
    [HttpDelete("cancelar")]
    [RequireModule("ADMIN")]
    public async Task<ActionResult<PlanCancellationResult>> CancelarPlano([FromBody] CancelPlanRequest request)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { message = "Farmácia não identificada" });
            }

            // Define tenant e usuário automaticamente
            request.TenantId = tenantId;
            request.CancelledBy = HttpContext.GetCurrentUserName();

            var resultado = await _planService.CancelPlanAsync(request);
            
            if (resultado.Success)
            {
                _logger.LogWarning("Plano cancelado para tenant {TenantId} por {User} - Motivo: {Reason}", 
                    tenantId, request.CancelledBy, request.Reason);
                
                return Ok(resultado);
            }

            return BadRequest(new { message = resultado.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no cancelamento do plano para tenant {TenantId}", 
                HttpContext.GetTenantId());
            return StatusCode(500, new { message = "Erro interno no cancelamento" });
        }
    }

    /// <summary>
    /// Endpoint administrativo para processar renovações automáticas
    /// </summary>
    /// <param name="diasAntecipadio">Dias de antecipação para renovação</param>
    /// <returns>Resultado das renovações processadas</returns>
    /// <response code="200">Renovações processadas com sucesso</response>
    [HttpPost("admin/renovacoes-automaticas")]
    [RequireEnterprise("ADMIN")] // Apenas plano Enterprise pode executar operações administrativas
    public async Task<ActionResult<List<PlanRenewalResult>>> ProcessarRenovacoesAutomaticas([FromQuery] int diasAntecipadio = 3)
    {
        try
        {
            _logger.LogInformation("Processando renovações automáticas - Dias antecipados: {Days}", diasAntecipadio);
            
            var resultados = await _planService.ProcessAutoRenewalsAsync(diasAntecipadio);
            
            var sucessos = resultados.Count(r => r.Success);
            var erros = resultados.Count(r => !r.Success);
            
            _logger.LogInformation("Renovações automáticas processadas - Sucessos: {Success}, Erros: {Errors}", 
                sucessos, erros);
            
            return Ok(new 
            { 
                message = $"Processamento concluído: {sucessos} sucessos, {erros} erros",
                results = resultados 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento de renovações automáticas");
            return StatusCode(500, new { message = "Erro interno no processamento" });
        }
    }

    /// <summary>
    /// Obtém histórico de mudanças de plano da farmácia
    /// </summary>
    /// <returns>Lista de mudanças ordenadas por data</returns>
    /// <response code="200">Histórico retornado com sucesso</response>
    [HttpGet("historico")]
    [RequireProfessional("REPORTS")] // Histórico disponível apenas para Professional+
    public async Task<ActionResult> ObterHistoricoPlanos()
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { message = "Farmácia não identificada" });
            }

            // Por enquanto retorna resposta simples - seria implementado no service
            _logger.LogDebug("Histórico de planos consultado para tenant {TenantId}", tenantId);
            
            return Ok(new 
            { 
                message = "Histórico de planos - funcionalidade em desenvolvimento",
                tenantId = tenantId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de planos para tenant {TenantId}", 
                HttpContext.GetTenantId());
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Simula contratação de plano (para testes de integração)
    /// </summary>
    /// <param name="planCode">Código do plano para simular</param>
    /// <returns>Simulação de contratação</returns>
    /// <response code="200">Simulação executada com sucesso</response>
    [HttpPost("simular/{planCode}")]
    [RequireModuleBeta("SIMULATION")] // Funcionalidade em beta
    public async Task<ActionResult> SimularContratacao(string planCode)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            var userName = HttpContext.GetCurrentUserName();
            
            _logger.LogDebug("Simulação de contratação - Plano: {PlanCode}, Tenant: {TenantId}, User: {User}", 
                planCode, tenantId, userName);
            
            // Simulação básica - retorna dados mockados
            var simulacao = new
            {
                planCode = planCode.ToUpper(),
                tenantId = tenantId,
                userName = userName,
                estimatedValue = planCode.ToUpper() switch
                {
                    "STARTER" => 149.90m,
                    "PROFESSIONAL" => 249.90m,
                    "ENTERPRISE" => 399.90m,
                    _ => 0m
                },
                currency = "BRL",
                paymentTypes = new[] { "MENSAL", "ANUAL" },
                trialAvailable = !string.IsNullOrEmpty(tenantId),
                message = "Esta é uma simulação para testes de integração"
            };
            
            return Ok(simulacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na simulação de contratação");
            return StatusCode(500, new { message = "Erro interno na simulação" });
        }
    }
}