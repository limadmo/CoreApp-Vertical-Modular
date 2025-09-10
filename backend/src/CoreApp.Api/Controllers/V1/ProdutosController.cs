using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Application.Services;

namespace CoreApp.Api.Controllers.V1;

/// <summary>
/// Controller para gestão de produtos comerciais multi-tenant
/// Implementa operações CRUD com validação de módulos e isolamento por tenant
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ApiVersion("1.0")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        IProdutoService produtoService,
        ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista produtos do tenant atual com paginação
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20)</param>
    /// <param name="search">Termo de busca opcional</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de produtos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Services.ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Services.ProdutoDto>>> ListarProdutos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pageRequest = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                // Se há termo de busca, usar busca por nome
                var produtos = await _produtoService.BuscarPorNomeAsync(search, cancellationToken);
                
                // Simula paginação dos resultados de busca
                var skip = (pageNumber - 1) * pageSize;
                var produtosPaginados = produtos.Skip(skip).Take(pageSize);
                
                var resultado = new PagedResult<Application.Services.ProdutoDto>
                {
                    Items = produtosPaginados,
                    Total = produtos.Count(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)produtos.Count() / pageSize)
                };
                
                return Ok(resultado);
            }
            else
            {
                // Listagem paginada normal
                var resultado = await _produtoService.ListarProdutosAsync(pageRequest, cancellationToken);
                return Ok(resultado);
            }
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao listar produtos");
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "PRODUTOS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar produtos");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém um produto específico por ID
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Produto encontrado</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Application.Services.ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.ProdutoDto>> ObterProduto(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var produto = await _produtoService.ObterProdutoPorIdAsync(id, cancellationToken);
            
            if (produto == null)
            {
                return NotFound(new
                {
                    error = "Produto não encontrado",
                    message = $"Produto com ID {id} não foi encontrado"
                });
            }
            
            return Ok(produto);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao obter produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "PRODUTOS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Produto criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Application.Services.ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.ProdutoDto>> CriarProduto(
        [FromBody] Application.Services.CriarProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = await _produtoService.CriarProdutoAsync(request, cancellationToken);
            
            return CreatedAtAction(
                nameof(ObterProduto),
                new { id = produto.Id },
                produto);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao criar produto");
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "PRODUTOS"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos ao criar produto");
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    /// <param name="id">ID do produto a ser atualizado</param>
    /// <param name="request">Dados para atualização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Produto atualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Application.Services.ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<Application.Services.ProdutoDto>> AtualizarProduto(
        Guid id,
        [FromBody] Application.Services.AtualizarProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = await _produtoService.AtualizarProdutoAsync(id, request, cancellationToken);
            return Ok(produto);
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao atualizar produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "PRODUTOS"
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Produto não encontrado ao atualizar {ProdutoId}", id);
            return NotFound(new
            {
                error = "Produto não encontrado",
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos ao atualizar produto {ProdutoId}", id);
            return BadRequest(new
            {
                error = "Dados inválidos",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Remove um produto (exclusão lógica)
    /// </summary>
    /// <param name="id">ID do produto a ser removido</param>
    /// <param name="motivo">Motivo da remoção</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação da remoção</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult> RemoverProduto(
        Guid id,
        [FromQuery] string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var removido = await _produtoService.RemoverProdutoAsync(id, motivo, cancellationToken);
            
            if (!removido)
            {
                return NotFound(new
                {
                    error = "Produto não encontrado",
                    message = $"Produto com ID {id} não foi encontrado"
                });
            }
            
            return NoContent();
        }
        catch (ModuleNotActiveException ex)
        {
            _logger.LogWarning(ex, "Módulo não ativo ao remover produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = ex.Message,
                moduleRequired = "PRODUTOS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produto {ProdutoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Erro interno",
                message = "Erro interno do servidor"
            });
        }
    }
}

// DTOs movidos para Application.Services para evitar duplicação