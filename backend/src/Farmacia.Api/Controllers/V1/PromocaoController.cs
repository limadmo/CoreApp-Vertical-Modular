using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Farmacia.Application.Services;
using Farmacia.Domain.Entities;
using Farmacia.Infrastructure.MultiTenant;
using Farmacia.Infrastructure.Middleware;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Api.Controllers.V1;

/// <summary>
/// Controller REST para gerenciamento de promoções farmacêuticas brasileiras
/// Implementa padrões REST completos com isolamento multi-tenant automático
/// </summary>
/// <remarks>
/// Esta API implementa controle completo de promoções para farmácias brasileiras,
/// incluindo descontos, combos, cashback e validações específicas para medicamentos
/// controlados conforme regulamentações do setor.
/// </remarks>
[ApiController]
[Route("api/v1/promocoes")]
[Authorize]
[RequireModule("PROMOTIONS")]
public class PromocaoController : ControllerBase
{
    private readonly IPromocaoService _promocaoService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<PromocaoController> _logger;

    public PromocaoController(
        IPromocaoService promocaoService,
        ITenantService tenantService,
        ILogger<PromocaoController> logger)
    {
        _promocaoService = promocaoService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as promoções da farmácia com paginação
    /// </summary>
    /// <param name="status">Filtrar por status (opcional)</param>
    /// <param name="tipo">Filtrar por tipo (opcional)</param>
    /// <param name="ativas">Mostrar apenas promoções ativas (default: false)</param>
    /// <param name="page">Página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20)</param>
    /// <returns>Lista paginada de promoções</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PromocaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PromocaoResponseDto>>> ListarPromocoes(
        [FromQuery] StatusPromocao? status = null,
        [FromQuery] TipoPromocao? tipo = null,
        [FromQuery] bool ativas = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var filtro = new PromocaoFiltroDto
            {
                Status = status,
                Tipo = tipo,
                ApenasAtivas = ativas,
                Page = page,
                PageSize = Math.Min(pageSize, 100) // Máximo 100 itens por página
            };

            var promocoes = await _promocaoService.ListarPromocoesAsync(filtro);

            _logger.LogInformation(
                "Listagem de promoções solicitada - Tenant: {TenantId}, Filtros: {Status}, {Tipo}, Ativas: {Ativas}",
                tenantId, status, tipo, ativas);

            return Ok(promocoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar promoções");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma promoção específica por ID
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <returns>Dados da promoção</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromocaoResponseDto>> ObterPromocao(Guid id)
    {
        try
        {
            var promocao = await _promocaoService.ObterPromocaoPorIdAsync(id);
            
            if (promocao == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            return Ok(promocao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria uma nova promoção farmacêutica
    /// </summary>
    /// <param name="request">Dados da promoção a ser criada</param>
    /// <returns>Promoção criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromocaoResponseDto>> CriarPromocao([FromBody] CriarPromocaoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validações específicas brasileiras
            if (request.AplicavelMedicamentosControlados && 
                (request.PercentualDesconto > 10 || request.ValorDesconto > 50))
            {
                ModelState.AddModelError("Desconto", 
                    "Descontos para medicamentos controlados não podem exceder 10% ou R$ 50,00");
                return BadRequest(ModelState);
            }

            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var promocao = await _promocaoService.CriarPromocaoAsync(request, usuarioId);

            _logger.LogInformation(
                "Promoção criada - ID: {PromocaoId}, Nome: {Nome}, Tenant: {TenantId}",
                promocao.Id, promocao.Nome, _tenantService.GetCurrentTenantId());

            return CreatedAtAction(
                nameof(ObterPromocao), 
                new { id = promocao.Id }, 
                promocao);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar promoção");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma promoção existente
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="request">Dados atualizados da promoção</param>
    /// <returns>Promoção atualizada</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromocaoResponseDto>> AtualizarPromocao(
        Guid id, 
        [FromBody] AtualizarPromocaoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var promocao = await _promocaoService.AtualizarPromocaoAsync(id, request, usuarioId);

            if (promocao == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Promoção atualizada - ID: {PromocaoId}, Tenant: {TenantId}",
                id, _tenantService.GetCurrentTenantId());

            return Ok(promocao);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Ativa uma promoção
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <returns>Promoção ativada</returns>
    [HttpPost("{id:guid}/ativar")]
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromocaoResponseDto>> AtivarPromocao(Guid id)
    {
        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var promocao = await _promocaoService.AtivarPromocaoAsync(id, usuarioId);

            if (promocao == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Promoção ativada - ID: {PromocaoId}, Tenant: {TenantId}",
                id, _tenantService.GetCurrentTenantId());

            return Ok(promocao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Desativa uma promoção
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="motivo">Motivo da desativação</param>
    /// <returns>Promoção desativada</returns>
    [HttpPost("{id:guid}/desativar")]
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromocaoResponseDto>> DesativarPromocao(
        Guid id, 
        [FromBody] DesativarPromocaoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var promocao = await _promocaoService.DesativarPromocaoAsync(id, request.Motivo, usuarioId);

            if (promocao == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Promoção desativada - ID: {PromocaoId}, Motivo: {Motivo}, Tenant: {TenantId}",
                id, request.Motivo, _tenantService.GetCurrentTenantId());

            return Ok(promocao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Aprova uma promoção pendente
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <returns>Promoção aprovada</returns>
    [HttpPost("{id:guid}/aprovar")]
    [RequireModule("ADVANCED_PROMOTIONS")] // Requer módulo avançado
    [ProducesResponseType(typeof(PromocaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<PromocaoResponseDto>> AprovarPromocao(Guid id)
    {
        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var promocao = await _promocaoService.AprovarPromocaoAsync(id, usuarioId);

            if (promocao == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Promoção aprovada - ID: {PromocaoId}, Tenant: {TenantId}",
                id, _tenantService.GetCurrentTenantId());

            return Ok(promocao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aprovar promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove uma promoção (soft delete)
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="motivo">Motivo da remoção</param>
    /// <returns>Confirmação da remoção</returns>
    [HttpDelete("{id:guid}")]
    [RequireModule("ADVANCED_PROMOTIONS")] // Requer módulo avançado
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult> RemoverPromocao(
        Guid id, 
        [FromQuery, Required] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest(new { error = "Motivo da remoção é obrigatório" });
        }

        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var sucesso = await _promocaoService.RemoverPromocaoAsync(id, motivo, usuarioId);

            if (!sucesso)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Promoção removida - ID: {PromocaoId}, Motivo: {Motivo}, Tenant: {TenantId}",
                id, motivo, _tenantService.GetCurrentTenantId());

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém produtos aplicáveis a uma promoção específica
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="page">Página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20)</param>
    /// <returns>Lista de produtos da promoção</returns>
    [HttpGet("{id:guid}/produtos")]
    [ProducesResponseType(typeof(PagedResult<ProdutoPromocaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ProdutoPromocaoDto>>> ObterProdutosPromocao(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var produtos = await _promocaoService.ObterProdutosPromocaoAsync(id, page, pageSize);

            if (produtos == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos da promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Adiciona produtos a uma promoção
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="request">Lista de produtos a adicionar</param>
    /// <returns>Confirmação da adição</returns>
    [HttpPost("{id:guid}/produtos")]
    [RequireModule("ADVANCED_PROMOTIONS")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> AdicionarProdutosPromocao(
        Guid id,
        [FromBody] AdicionarProdutosPromocaoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioId = Guid.Parse(_tenantService.GetCurrentUserId());
            var sucesso = await _promocaoService.AdicionarProdutosPromocaoAsync(id, request.Produtos, usuarioId);

            if (!sucesso)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            _logger.LogInformation(
                "Produtos adicionados à promoção - ID: {PromocaoId}, Produtos: {ProdutoCount}, Tenant: {TenantId}",
                id, request.Produtos.Count, _tenantService.GetCurrentTenantId());

            return Ok(new { message = "Produtos adicionados com sucesso" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar produtos à promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Calcula desconto de uma promoção para um valor específico
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <param name="valor">Valor para calcular o desconto</param>
    /// <returns>Valor do desconto calculado</returns>
    [HttpGet("{id:guid}/calcular-desconto")]
    [ProducesResponseType(typeof(CalculoDescontoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CalculoDescontoResponseDto>> CalcularDesconto(
        Guid id,
        [FromQuery] decimal valor)
    {
        if (valor <= 0)
        {
            return BadRequest(new { error = "Valor deve ser positivo" });
        }

        try
        {
            var resultado = await _promocaoService.CalcularDescontoAsync(id, valor);

            if (resultado == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular desconto da promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estatísticas de uso de uma promoção
    /// </summary>
    /// <param name="id">ID da promoção</param>
    /// <returns>Estatísticas da promoção</returns>
    [HttpGet("{id:guid}/estatisticas")]
    [RequireModule("REPORTS")]
    [ProducesResponseType(typeof(EstatisticasPromocaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<EstatisticasPromocaoDto>> ObterEstatisticas(Guid id)
    {
        try
        {
            var estatisticas = await _promocaoService.ObterEstatisticasPromocaoAsync(id);

            if (estatisticas == null)
            {
                return NotFound(new { error = "Promoção não encontrada" });
            }

            return Ok(estatisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas da promoção {PromocaoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}

#region DTOs

/// <summary>
/// DTO para filtros de consulta de promoções
/// </summary>
public class PromocaoFiltroDto
{
    public StatusPromocao? Status { get; set; }
    public TipoPromocao? Tipo { get; set; }
    public bool ApenasAtivas { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para resposta de promoção
/// </summary>
public class PromocaoResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Codigo { get; set; }
    public TipoPromocao Tipo { get; set; }
    public StatusPromocao Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorMinimo { get; set; }
    public decimal? ValorMaximoDesconto { get; set; }
    public int QuantidadeMinima { get; set; }
    public int? LimiteUso { get; set; }
    public int ContadorUso { get; set; }
    public bool AplicavelMedicamentosControlados { get; set; }
    public string? TextoCliente { get; set; }
    public int DiasRestantes { get; set; }
    public bool PodeSerAplicada { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para criação de promoção
/// </summary>
public class CriarPromocaoRequestDto
{
    [Required, StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [StringLength(50)]
    public string? Codigo { get; set; }

    [Required]
    public TipoPromocao Tipo { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Range(0, 999999.99)]
    public decimal ValorDesconto { get; set; }

    [Range(0, 100)]
    public decimal PercentualDesconto { get; set; }

    [Range(0, 999999.99)]
    public decimal ValorMinimo { get; set; }

    [Range(0, 999999.99)]
    public decimal? ValorMaximoDesconto { get; set; }

    [Range(1, 9999)]
    public int QuantidadeMinima { get; set; } = 1;

    [Range(1, 999999)]
    public int? LimiteUso { get; set; }

    public bool AplicavelMedicamentosControlados { get; set; } = true;

    [StringLength(500)]
    public string? TextoCliente { get; set; }

    public List<Guid> ProdutoIds { get; set; } = new();
}

/// <summary>
/// DTO para atualização de promoção
/// </summary>
public class AtualizarPromocaoRequestDto : CriarPromocaoRequestDto
{
}

/// <summary>
/// DTO para desativação de promoção
/// </summary>
public class DesativarPromocaoRequestDto
{
    [Required, StringLength(500)]
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// DTO para adicionar produtos à promoção
/// </summary>
public class AdicionarProdutosPromocaoRequestDto
{
    [Required]
    public List<AdicionarProdutoPromocaoDto> Produtos { get; set; } = new();
}

/// <summary>
/// DTO para produto da promoção
/// </summary>
public class AdicionarProdutoPromocaoDto
{
    [Required]
    public Guid ProdutoId { get; set; }

    [Range(0, 999999.99)]
    public decimal? ValorDescontoEspecifico { get; set; }

    [Range(0, 100)]
    public decimal? PercentualDescontoEspecifico { get; set; }

    [Range(1, 9999)]
    public int QuantidadeMinimaEspecifica { get; set; } = 1;
}

/// <summary>
/// DTO para resposta de produto da promoção
/// </summary>
public class ProdutoPromocaoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal? ValorDescontoEspecifico { get; set; }
    public decimal? PercentualDescontoEspecifico { get; set; }
    public int QuantidadeMinimaEspecifica { get; set; }
}

/// <summary>
/// DTO para resultado de cálculo de desconto
/// </summary>
public class CalculoDescontoResponseDto
{
    public decimal ValorOriginal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal PercentualDesconto { get; set; }
    public bool DescontoAplicado { get; set; }
    public string? MotivoNaoAplicacao { get; set; }
}

/// <summary>
/// DTO para estatísticas da promoção
/// </summary>
public class EstatisticasPromocaoDto
{
    public int TotalUsos { get; set; }
    public decimal ValorTotalDescontos { get; set; }
    public decimal TicketMedioComPromocao { get; set; }
    public int VendasComPromocao { get; set; }
    public Dictionary<string, int> UsosPorDia { get; set; } = new();
    public List<ProdutoMaisVendidoPromocaoDto> ProdutosMaisVendidos { get; set; } = new();
}

/// <summary>
/// DTO para produto mais vendido na promoção
/// </summary>
public class ProdutoMaisVendidoPromocaoDto
{
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}

/// <summary>
/// DTO para resultado paginado
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

#endregion