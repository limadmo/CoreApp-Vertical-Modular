using Farmacia.Application.Services;
using Farmacia.Domain.Enums;
using Farmacia.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace Farmacia.Api.Controllers.V1;

/// <summary>
/// Controller para gerenciamento de estoque farmacêutico brasileiro
/// Inclui operações offline-first e controle FEFO para lotes
/// </summary>
/// <remarks>
/// Este controller implementa operações de estoque com suporte offline,
/// sincronização automática e compliance farmacêutico ANVISA completo
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/estoque")]
[Tags("Estoque Farmacêutico")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<EstoqueController> _logger;

    public EstoqueController(
        IEstoqueService estoqueService,
        ITenantService tenantService,
        ILogger<EstoqueController> logger)
    {
        _estoqueService = estoqueService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Registra uma nova movimentação de estoque
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Movimentação registrada</returns>
    /// <response code="201">Movimentação registrada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    /// <response code="409">Estoque insuficiente para saída</response>
    [HttpPost("movimentacoes")]
    [ProducesResponseType(typeof(MovimentacaoEstoqueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovimentacaoEstoqueResponse>> RegistrarMovimentacao(
        [FromBody] RegistrarMovimentacaoApiRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceRequest = new RegistrarMovimentacaoRequest
            {
                ProdutoId = request.ProdutoId,
                Tipo = request.Tipo,
                Quantidade = request.Quantidade,
                Motivo = request.Motivo,
                Observacoes = request.Observacoes,
                ClienteTimestamp = request.ClienteTimestamp
            };

            var movimentacao = await _estoqueService.RegistrarMovimentacaoAsync(serviceRequest, cancellationToken);

            var response = new MovimentacaoEstoqueResponse
            {
                Id = movimentacao.Id,
                ProdutoId = movimentacao.ProdutoId,
                Tipo = movimentacao.Tipo,
                Quantidade = movimentacao.Quantidade,
                QuantidadeAnterior = movimentacao.QuantidadeAnterior,
                QuantidadeAtual = movimentacao.QuantidadeAtual,
                Motivo = movimentacao.Motivo,
                Observacoes = movimentacao.Observacoes,
                UsuarioId = movimentacao.UsuarioId,
                DataCriacao = movimentacao.DataCriacao
            };

            return CreatedAtAction(
                nameof(BuscarMovimentacao),
                new { id = movimentacao.Id },
                response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Estoque insuficiente"))
        {
            return Conflict(new
            {
                error = "Estoque insuficiente",
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Lista movimentações de estoque com filtros
    /// </summary>
    /// <param name="produtoId">ID do produto (opcional)</param>
    /// <param name="tipo">Tipo de movimentação (opcional)</param>
    /// <param name="usuarioId">ID do usuário (opcional)</param>
    /// <param name="dataInicio">Data de início (opcional)</param>
    /// <param name="dataFim">Data de fim (opcional)</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="size">Tamanho da página (padrão: 20, máximo: 100)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de movimentações</returns>
    /// <response code="200">Lista de movimentações retornada com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpGet("movimentacoes")]
    [ProducesResponseType(typeof(PagedResponse<MovimentacaoEstoqueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<PagedResponse<MovimentacaoEstoqueResponse>>> ListarMovimentacoes(
        [FromQuery] Guid? produtoId = null,
        [FromQuery] TipoMovimentacao? tipo = null,
        [FromQuery] string? usuarioId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar parâmetros
            page = Math.Max(1, page);
            size = Math.Min(100, Math.Max(1, size));

            var request = new ListarMovimentacoesRequest
            {
                ProdutoId = produtoId,
                Tipo = tipo,
                UsuarioId = usuarioId,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Page = page,
                Size = size
            };

            var resultado = await _estoqueService.ListarMovimentacoesAsync(request, cancellationToken);

            var response = new PagedResponse<MovimentacaoEstoqueResponse>
            {
                Items = resultado.Items.Select(m => new MovimentacaoEstoqueResponse
                {
                    Id = m.Id,
                    ProdutoId = m.ProdutoId,
                    Tipo = m.Tipo,
                    Quantidade = m.Quantidade,
                    QuantidadeAnterior = m.QuantidadeAnterior,
                    QuantidadeAtual = m.QuantidadeAtual,
                    Motivo = m.Motivo,
                    Observacoes = m.Observacoes,
                    UsuarioId = m.UsuarioId,
                    DataCriacao = m.DataCriacao
                }).ToList(),
                Page = resultado.Page,
                Size = resultado.Size,
                Total = resultado.Total,
                Pages = resultado.Pages,
                HasNext = resultado.HasNext,
                HasPrevious = resultado.HasPrevious
            };

            // Log para auditoria
            _logger.LogInformation(
                "Consulta movimentações: Tenant={TenantId}, Produto={ProdutoId}, Tipo={Tipo}, Page={Page}",
                _tenantService.GetCurrentTenantId(), produtoId, tipo, page);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
    }

    /// <summary>
    /// Busca movimentação por ID
    /// </summary>
    /// <param name="id">ID da movimentação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Movimentação encontrada</returns>
    /// <response code="200">Movimentação encontrada</response>
    /// <response code="404">Movimentação não encontrada</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpGet("movimentacoes/{id:guid}")]
    [ProducesResponseType(typeof(MovimentacaoEstoqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<MovimentacaoEstoqueResponse>> BuscarMovimentacao(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implementar busca por ID no serviço
            return NotFound(new
            {
                error = "Movimentação não encontrada",
                message = $"Movimentação {id} não foi encontrada"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
    }

    /// <summary>
    /// Obtém resumo do estoque
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resumo do estoque por produto</returns>
    /// <response code="200">Resumo retornado com sucesso</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(IEnumerable<EstoqueResumoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<IEnumerable<EstoqueResumoResponse>>> ObterResumoEstoque(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resumos = await _estoqueService.ObterResumoEstoqueAsync(cancellationToken);

            var response = resumos.Select(r => new EstoqueResumoResponse
            {
                ProdutoId = r.ProdutoId,
                QuantidadeAtual = r.QuantidadeAtual,
                UltimaMovimentacao = r.UltimaMovimentacao,
                Status = r.Status
            });

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
    }

    /// <summary>
    /// Lista produtos com estoque baixo
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de produtos com estoque baixo</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpGet("alertas/estoque-baixo")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoEstoqueBaixoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<IEnumerable<ProdutoEstoqueBaixoResponse>>> ListarProdutosEstoqueBaixo(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _estoqueService.ListarProdutosEstoqueBaixoAsync(cancellationToken);

            var response = produtos.Select(p => new ProdutoEstoqueBaixoResponse
            {
                ProdutoId = p.ProdutoId,
                NomeProduto = p.NomeProduto,
                EstoqueAtual = p.EstoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                Status = p.Status
            });

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
    }

    /// <summary>
    /// Obtém dashboard do estoque
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas do dashboard</returns>
    /// <response code="200">Dashboard retornado com sucesso</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardEstoqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<DashboardEstoqueResponse>> ObterDashboardEstoque(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dashboard = await _estoqueService.ObterDashboardEstoqueAsync(cancellationToken);

            var response = new DashboardEstoqueResponse
            {
                TotalProdutos = dashboard.TotalProdutos,
                ProdutosEstoqueBaixo = dashboard.ProdutosEstoqueBaixo,
                ProdutosZerados = dashboard.ProdutosZerados,
                MovimentacoesHoje = dashboard.MovimentacoesHoje,
                MovimentacoesSemana = dashboard.MovimentacoesSemana
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
    }

    /// <summary>
    /// Sincroniza movimentações offline
    /// </summary>
    /// <param name="request">Lista de movimentações offline</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da sincronização</returns>
    /// <response code="200">Sincronização concluída</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="402">Módulo STOCK não ativo</response>
    [HttpPost("sincronizar")]
    [ProducesResponseType(typeof(SincronizacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<SincronizacaoResponse>> SincronizarMovimentacoesOffline(
        [FromBody] SincronizarMovimentacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Movimentacoes?.Any() != true)
            {
                return BadRequest(new
                {
                    error = "Dados inválidos",
                    message = "Lista de movimentações não pode estar vazia"
                });
            }

            var movimentacoesDto = request.Movimentacoes.Select(m => new MovimentacaoOfflineDto
            {
                Id = m.Id,
                ProdutoId = m.ProdutoId,
                Tipo = m.Tipo,
                Quantidade = m.Quantidade,
                Motivo = m.Motivo,
                Observacoes = m.Observacoes,
                ClienteTimestamp = m.ClienteTimestamp,
                HashIntegridade = m.HashIntegridade
            });

            var resultado = await _estoqueService.SincronizarMovimentacoesOfflineAsync(movimentacoesDto, cancellationToken);

            var response = new SincronizacaoResponse
            {
                Processadas = resultado.Processadas,
                Sucessos = resultado.Sucessos,
                Erros = resultado.Erros,
                Conflitos = resultado.Conflitos,
                Detalhes = resultado.Detalhes.Select(d => new SincronizacaoDetalheResponse
                {
                    Id = d.Id,
                    Tipo = d.Tipo,
                    Status = d.Status,
                    Erro = d.Erro
                }).ToList()
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "STOCK"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
    }
}

#region DTOs de Request e Response

/// <summary>
/// Request para registrar movimentação via API
/// </summary>
public class RegistrarMovimentacaoApiRequest
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Tipo da movimentação
    /// </summary>
    public TipoMovimentacao Tipo { get; set; }

    /// <summary>
    /// Quantidade movimentada
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Motivo da movimentação (obrigatório para auditoria)
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Timestamp do cliente (para operações offline)
    /// </summary>
    public DateTime? ClienteTimestamp { get; set; }
}

/// <summary>
/// Response para movimentação de estoque
/// </summary>
public class MovimentacaoEstoqueResponse
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public decimal Quantidade { get; set; }
    public decimal QuantidadeAnterior { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// Response para resumo de estoque
/// </summary>
public class EstoqueResumoResponse
{
    public Guid ProdutoId { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public DateTime? UltimaMovimentacao { get; set; }
    public StatusEstoque Status { get; set; }
}

/// <summary>
/// Response para produto com estoque baixo
/// </summary>
public class ProdutoEstoqueBaixoResponse
{
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public decimal EstoqueAtual { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public StatusEstoque Status { get; set; }
}

/// <summary>
/// Response para dashboard de estoque
/// </summary>
public class DashboardEstoqueResponse
{
    public int TotalProdutos { get; set; }
    public int ProdutosEstoqueBaixo { get; set; }
    public int ProdutosZerados { get; set; }
    public int MovimentacoesHoje { get; set; }
    public int MovimentacoesSemana { get; set; }
}

/// <summary>
/// Request para sincronizar movimentações offline
/// </summary>
public class SincronizarMovimentacoesRequest
{
    public List<MovimentacaoOfflineRequest> Movimentacoes { get; set; } = new();
}

/// <summary>
/// Request para movimentação offline
/// </summary>
public class MovimentacaoOfflineRequest
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public decimal Quantidade { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime ClienteTimestamp { get; set; }
    public string? HashIntegridade { get; set; }
}

/// <summary>
/// Response para sincronização
/// </summary>
public class SincronizacaoResponse
{
    public int Processadas { get; set; }
    public int Sucessos { get; set; }
    public int Erros { get; set; }
    public int Conflitos { get; set; }
    public List<SincronizacaoDetalheResponse> Detalhes { get; set; } = new();
}

/// <summary>
/// Response para detalhe de sincronização
/// </summary>
public class SincronizacaoDetalheResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public StatusSincronizacao Status { get; set; }
    public string? Erro { get; set; }
}

/// <summary>
/// Response paginada
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public int Pages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

#endregion