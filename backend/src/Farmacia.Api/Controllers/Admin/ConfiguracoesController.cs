using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Farmacia.Application.Services;
using Farmacia.Domain.Entities.Configuration;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.MultiTenant;

namespace Farmacia.Api.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão dinâmica de configurações farmacêuticas
/// Permite criar, editar e remover configurações sem necessidade de deploy
/// </summary>
/// <remarks>
/// Este controller implementa o padrão de configuração dinâmica exigido pelo CLAUDE.md
/// para substituir enums rígidos por configurações flexíveis em banco de dados
/// </remarks>
[ApiController]
[Route("api/admin/configuracoes")]
[Authorize(Roles = "Admin")]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<ConfiguracoesController> _logger;

    public ConfiguracoesController(
        IConfigurationService configurationService,
        ITenantService tenantService,
        ILogger<ConfiguracoesController> logger)
    {
        _configurationService = configurationService;
        _tenantService = tenantService;
        _logger = logger;
    }

    #region Classificações ANVISA

    /// <summary>
    /// Lista todas as classificações ANVISA para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    /// <returns>Lista de classificações ANVISA ativas</returns>
    [HttpGet("anvisa")]
    public async Task<ActionResult<List<ClassificacaoAnvisaEntity>>> GetClassificacoesAnvisa([FromQuery] string? tenantId = null)
    {
        var classificacoes = await _configurationService.GetAllByTenantAsync<ClassificacaoAnvisaEntity>(tenantId);
        return Ok(classificacoes);
    }

    /// <summary>
    /// Obtém uma classificação ANVISA específica por código
    /// </summary>
    /// <param name="codigo">Código da classificação (A1, B1, C1, etc.)</param>
    /// <param name="tenantId">ID do tenant (null para configuração global)</param>
    /// <returns>Classificação ANVISA encontrada</returns>
    [HttpGet("anvisa/{codigo}")]
    public async Task<ActionResult<ClassificacaoAnvisaEntity>> GetClassificacaoAnvisa(string codigo, [FromQuery] string? tenantId = null)
    {
        var classificacao = await _configurationService.GetByCodeAsync<ClassificacaoAnvisaEntity>(tenantId, codigo);
        
        if (classificacao == null)
            return NotFound($"Classificação ANVISA '{codigo}' não encontrada para tenant '{tenantId ?? "GLOBAL"}'");

        return Ok(classificacao);
    }

    /// <summary>
    /// Cria ou atualiza uma classificação ANVISA personalizada
    /// </summary>
    /// <param name="classificacao">Dados da classificação</param>
    /// <returns>Classificação salva</returns>
    [HttpPost("anvisa")]
    public async Task<ActionResult<ClassificacaoAnvisaEntity>> CreateClassificacaoAnvisa([FromBody] ClassificacaoAnvisaEntity classificacao)
    {
        try
        {
            // Validar se não é uma classificação oficial ANVISA (protegida)
            if (classificacao.IsOficialAnvisa)
            {
                return BadRequest("Não é permitido modificar classificações oficiais ANVISA");
            }

            var resultado = await _configurationService.SaveAsync(classificacao);
            
            _logger.LogInformation("Classificação ANVISA personalizada criada: {Codigo} para tenant {TenantId}", 
                classificacao.Codigo, classificacao.TenantId ?? "GLOBAL");

            return CreatedAtAction(nameof(GetClassificacaoAnvisa), 
                new { codigo = resultado.Codigo, tenantId = resultado.TenantId }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar classificação ANVISA {Codigo}", classificacao.Codigo);
            return BadRequest($"Erro ao criar classificação: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove uma classificação ANVISA personalizada
    /// </summary>
    /// <param name="id">ID da classificação</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("anvisa/{id}")]
    public async Task<ActionResult> DeleteClassificacaoAnvisa(Guid id)
    {
        try
        {
            var sucesso = await _configurationService.DeleteAsync<ClassificacaoAnvisaEntity>(id);
            
            if (!sucesso)
                return NotFound("Classificação não encontrada ou protegida contra remoção");

            _logger.LogInformation("Classificação ANVISA removida: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover classificação ANVISA {Id}", id);
            return BadRequest($"Erro ao remover classificação: {ex.Message}");
        }
    }

    #endregion

    #region Status de Estoque

    /// <summary>
    /// Lista todos os status de estoque configurados
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    [HttpGet("status-estoque")]
    public async Task<ActionResult<List<StatusEstoqueEntity>>> GetStatusEstoque([FromQuery] string? tenantId = null)
    {
        var statusList = await _configurationService.GetAllByTenantAsync<StatusEstoqueEntity>(tenantId);
        return Ok(statusList);
    }

    /// <summary>
    /// Cria ou atualiza um status de estoque
    /// </summary>
    /// <param name="status">Dados do status de estoque</param>
    [HttpPost("status-estoque")]
    public async Task<ActionResult<StatusEstoqueEntity>> CreateStatusEstoque([FromBody] StatusEstoqueEntity status)
    {
        try
        {
            var resultado = await _configurationService.SaveAsync(status);
            return CreatedAtAction(nameof(GetStatusEstoque), new { tenantId = resultado.TenantId }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao salvar status de estoque: {ex.Message}");
        }
    }

    #endregion

    #region Formas de Pagamento

    /// <summary>
    /// Lista todas as formas de pagamento configuradas
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    [HttpGet("formas-pagamento")]
    public async Task<ActionResult<List<FormaPagamentoEntity>>> GetFormasPagamento([FromQuery] string? tenantId = null)
    {
        var formas = await _configurationService.GetAllByTenantAsync<FormaPagamentoEntity>(tenantId);
        return Ok(formas);
    }

    /// <summary>
    /// Cria ou atualiza uma forma de pagamento
    /// </summary>
    /// <param name="formaPagamento">Dados da forma de pagamento</param>
    [HttpPost("formas-pagamento")]
    public async Task<ActionResult<FormaPagamentoEntity>> CreateFormaPagamento([FromBody] FormaPagamentoEntity formaPagamento)
    {
        try
        {
            var resultado = await _configurationService.SaveAsync(formaPagamento);
            return CreatedAtAction(nameof(GetFormasPagamento), new { tenantId = resultado.TenantId }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao salvar forma de pagamento: {ex.Message}");
        }
    }

    #endregion

    #region Status de Pagamento

    /// <summary>
    /// Lista todos os status de pagamento configurados
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    [HttpGet("status-pagamento")]
    public async Task<ActionResult<List<StatusPagamentoEntity>>> GetStatusPagamento([FromQuery] string? tenantId = null)
    {
        var statusList = await _configurationService.GetAllByTenantAsync<StatusPagamentoEntity>(tenantId);
        return Ok(statusList);
    }

    /// <summary>
    /// Cria ou atualiza um status de pagamento
    /// </summary>
    /// <param name="status">Dados do status de pagamento</param>
    [HttpPost("status-pagamento")]
    public async Task<ActionResult<StatusPagamentoEntity>> CreateStatusPagamento([FromBody] StatusPagamentoEntity status)
    {
        try
        {
            var resultado = await _configurationService.SaveAsync(status);
            return CreatedAtAction(nameof(GetStatusPagamento), new { tenantId = resultado.TenantId }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao salvar status de pagamento: {ex.Message}");
        }
    }

    #endregion

    #region Status de Sincronização

    /// <summary>
    /// Lista todos os status de sincronização PDV configurados
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para configurações globais)</param>
    [HttpGet("status-sincronizacao")]
    public async Task<ActionResult<List<StatusSincronizacaoEntity>>> GetStatusSincronizacao([FromQuery] string? tenantId = null)
    {
        var statusList = await _configurationService.GetAllByTenantAsync<StatusSincronizacaoEntity>(tenantId);
        return Ok(statusList);
    }

    /// <summary>
    /// Cria ou atualiza um status de sincronização
    /// </summary>
    /// <param name="status">Dados do status de sincronização</param>
    [HttpPost("status-sincronizacao")]
    public async Task<ActionResult<StatusSincronizacaoEntity>> CreateStatusSincronizacao([FromBody] StatusSincronizacaoEntity status)
    {
        try
        {
            var resultado = await _configurationService.SaveAsync(status);
            return CreatedAtAction(nameof(GetStatusSincronizacao), new { tenantId = resultado.TenantId }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao salvar status de sincronização: {ex.Message}");
        }
    }

    #endregion

    #region Operações de Cache

    /// <summary>
    /// Invalida o cache de configurações para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant (null para cache global)</param>
    /// <param name="tipo">Tipo de configuração (anvisa, estoque, pagamento, etc.)</param>
    [HttpPost("cache/invalidate")]
    public ActionResult InvalidateCache([FromQuery] string? tenantId = null, [FromQuery] string? tipo = null)
    {
        try
        {
            switch (tipo?.ToLower())
            {
                case "anvisa":
                    _configurationService.InvalidateCache<ClassificacaoAnvisaEntity>(tenantId);
                    break;
                case "estoque":
                    _configurationService.InvalidateCache<StatusEstoqueEntity>(tenantId);
                    break;
                case "pagamento":
                    _configurationService.InvalidateCache<FormaPagamentoEntity>(tenantId);
                    _configurationService.InvalidateCache<StatusPagamentoEntity>(tenantId);
                    break;
                case "sincronizacao":
                    _configurationService.InvalidateCache<StatusSincronizacaoEntity>(tenantId);
                    break;
                case null:
                case "all":
                    _configurationService.InvalidateAllCache();
                    break;
                default:
                    return BadRequest($"Tipo de cache inválido: {tipo}");
            }

            _logger.LogInformation("Cache invalidado: tipo={Tipo}, tenant={TenantId}", 
                tipo ?? "ALL", tenantId ?? "GLOBAL");

            return Ok(new { message = "Cache invalidado com sucesso", tipo, tenantId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao invalidar cache");
            return BadRequest($"Erro ao invalidar cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Recarrega seeds de configurações padrão (apenas em desenvolvimento)
    /// </summary>
    [HttpPost("seeds/reload")]
    public async Task<ActionResult> ReloadSeeds()
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return Forbid("Operação permitida apenas em ambiente de desenvolvimento");
        }

        try
        {
            var seedService = HttpContext.RequestServices.GetRequiredService<IConfigurationSeedService>();
            
            // Invalidar todo o cache antes de recarregar
            _configurationService.InvalidateAllCache();
            
            // Recarregar seeds
            await seedService.SeedClassificacoesAnvisaAsync();
            await seedService.SeedStatusEstoqueAsync();
            await seedService.SeedFormasPagamentoAsync();
            await seedService.SeedStatusPagamentoAsync();
            await seedService.SeedStatusSincronizacaoAsync();

            _logger.LogInformation("Seeds de configurações recarregados com sucesso");
            
            return Ok(new { message = "Seeds recarregados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recarregar seeds");
            return BadRequest($"Erro ao recarregar seeds: {ex.Message}");
        }
    }

    #endregion
}