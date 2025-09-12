using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Application.Services;

namespace CoreApp.Api.Controllers.V1;

/// <summary>
/// Controller para gestão de vendas comerciais multi-tenant
/// Implementa fluxo completo de vendas com validação de módulos e isolamento por tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // TODO: Remover em produção - Temporário para desenvolvimento
[ApiVersion("1.0")]
public class VendasController : ControllerBase
{
    private readonly IVendaService _vendaService;
    private readonly ILogger<VendasController> _logger;

    public VendasController(
        IVendaService vendaService,
        ILogger<VendasController> logger)
    {
        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista vendas do tenant atual com paginação
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20)</param>
    /// <param name="status">Filtrar por status da venda</param>
    /// <param name="dataInicio">Data inicial do período</param>
    /// <param name="dataFim">Data final do período</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de vendas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Services.VendaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Services.VendaDto>>> ListarVendas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pageRequest = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var resultado = await _vendaService.ListarVendasAsync(pageRequest, cancellationToken);
            return Ok(resultado);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao listar vendas");
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar vendas");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém uma venda específica por ID
    /// </summary>
    /// <param name="id">ID da venda</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda encontrada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Application.Services.VendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.VendaDto>> ObterVenda(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var venda = await _vendaService.ObterVendaPorIdAsync(id, cancellationToken);
            
            if (venda == null)
            {
                return NotFound(new
                {
                    error = "Venda não encontrada",
                    message = $"Venda com ID {id} não foi encontrada"
                });
            }
            
            return Ok(venda);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao obter venda {VendaId}", id);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter venda {VendaId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Inicia uma nova venda
    /// </summary>
    /// <param name="request">Dados iniciais da venda</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Application.Services.VendaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.VendaDto>> IniciarVenda(
        [FromBody] Application.Services.IniciarVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venda = await _vendaService.IniciarVendaAsync(request, cancellationToken);
            
            return CreatedAtAction(
                nameof(ObterVenda),
                new { id = venda.Id },
                venda);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao iniciar venda");
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos ao iniciar venda");
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar venda");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Adiciona item à venda
    /// </summary>
    /// <param name="vendaId">ID da venda</param>
    /// <param name="request">Dados do item a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda atualizada</returns>
    [HttpPost("{vendaId:guid}/itens")]
    [ProducesResponseType(typeof(Application.Services.VendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.VendaDto>> AdicionarItem(
        Guid vendaId,
        [FromBody] Application.Services.AdicionarItemVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venda = await _vendaService.AdicionarItemVendaAsync(vendaId, request, cancellationToken);
            return Ok(venda);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao adicionar item à venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Venda não encontrada ao adicionar item {VendaId}", vendaId);
            return NotFound(new
            {
                error = "Venda não encontrada",
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos ao adicionar item à venda {VendaId}", vendaId);
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar item à venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Remove item da venda
    /// </summary>
    /// <param name="vendaId">ID da venda</param>
    /// <param name="itemId">ID do item a ser removido</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda atualizada</returns>
    [HttpDelete("{vendaId:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(typeof(Application.Services.VendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.VendaDto>> RemoverItem(
        Guid vendaId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var venda = await _vendaService.RemoverItemVendaAsync(vendaId, itemId, cancellationToken);
            return Ok(venda);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao remover item da venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Venda ou item não encontrado ao remover item {VendaId}/{ItemId}", vendaId, itemId);
            return NotFound(new
            {
                error = "Venda ou item não encontrado",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item da venda {VendaId}/{ItemId}", vendaId, itemId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Finaliza a venda com pagamento
    /// </summary>
    /// <param name="vendaId">ID da venda</param>
    /// <param name="request">Dados do pagamento e finalização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda finalizada</returns>
    [HttpPut("{vendaId:guid}/finalizar")]
    [ProducesResponseType(typeof(Application.Services.VendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.VendaDto>> FinalizarVenda(
        Guid vendaId,
        [FromBody] Application.Services.FinalizarVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venda = await _vendaService.FinalizarVendaAsync(vendaId, request, cancellationToken);
            return Ok(venda);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao finalizar venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Venda não encontrada ao finalizar {VendaId}", vendaId);
            return NotFound(new
            {
                error = "Venda não encontrada",
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos ao finalizar venda {VendaId}", vendaId);
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida ao finalizar venda {VendaId}", vendaId);
            return BadRequest(new
            {
                error = "Operação inválida",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Cancela uma venda
    /// </summary>
    /// <param name="vendaId">ID da venda</param>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação do cancelamento</returns>
    [HttpPut("{vendaId:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult> CancelarVenda(
        Guid vendaId,
        [FromQuery] string motivo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return BadRequest(new
                {
                    error = "Motivo obrigatório",
                    message = "É necessário informar o motivo do cancelamento"
                });
            }

            var cancelado = await _vendaService.CancelarVendaAsync(vendaId, motivo, cancellationToken);
            
            if (!cancelado)
            {
                return NotFound(new
                {
                    error = "Venda não encontrada",
                    message = $"Venda com ID {vendaId} não foi encontrada"
                });
            }

            return Ok(new
            {
                message = "Venda cancelada com sucesso",
                vendaId = vendaId,
                motivo = motivo
            });
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao cancelar venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida ao cancelar venda {VendaId}", vendaId);
            return BadRequest(new
            {
                error = "Operação inválida",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar venda {VendaId}", vendaId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Busca vendas por período
    /// </summary>
    /// <param name="dataInicio">Data inicial do período</param>
    /// <param name="dataFim">Data final do período</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas do período</returns>
    [HttpGet("periodo")]
    [ProducesResponseType(typeof(IEnumerable<Application.Services.VendaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<IEnumerable<Application.Services.VendaDto>>> BuscarVendasPorPeriodo(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dataInicio > dataFim)
            {
                return BadRequest(new
                {
                    error = "Período inválido",
                    message = "Data inicial não pode ser maior que data final"
                });
            }

            var vendas = await _vendaService.BuscarVendasPorPeriodoAsync(dataInicio, dataFim, cancellationToken);
            return Ok(vendas);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao buscar vendas por período");
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "VENDAS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por período");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }
}