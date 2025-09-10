using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;

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
    private readonly IModuleValidationService _moduleValidation;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        IModuleValidationService moduleValidation,
        ITenantContext tenantContext,
        ILogger<ProdutosController> logger)
    {
        _moduleValidation = moduleValidation;
        _tenantContext = tenantContext;
        _logger = logger;
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
    [ProducesResponseType(typeof(PagedResult<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ProdutoDto>>> ListarProdutos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            // Validar acesso ao módulo de produtos
            if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "PRODUCTS", cancellationToken))
            {
                return StatusCode(StatusCodes.Status402PaymentRequired, new
                {
                    error = "Módulo não ativo",
                    message = "O módulo de produtos não está ativo para sua loja. Faça upgrade do seu plano.",
                    moduleRequired = "PRODUCTS"
                });
            }

            // TODO: Implementar busca de produtos quando serviços de aplicação estiverem prontos
            var result = new PagedResult<ProdutoDto>
            {
                Items = new List<ProdutoDto>(),
                TotalItems = 0,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = 0
            };

            _logger.LogInformation("Produtos listados para tenant {TenantId} - Página {Page}", tenantId, pageNumber);
            return Ok(result);
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
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<ProdutoDto>> ObterProduto(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            // Validar acesso ao módulo
            if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "PRODUCTS", cancellationToken))
            {
                return StatusCode(StatusCodes.Status402PaymentRequired, new
                {
                    error = "Módulo não ativo",
                    message = "O módulo de produtos não está ativo para sua loja.",
                    moduleRequired = "PRODUCTS"
                });
            }

            // TODO: Implementar busca por ID quando serviços estiverem prontos
            return NotFound(new
            {
                error = "Produto não encontrado",
                message = $"Produto com ID {id} não foi encontrado"
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
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<ProdutoDto>> CriarProduto(
        [FromBody] CriarProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            // Validar acesso ao módulo
            if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "PRODUCTS", cancellationToken))
            {
                return StatusCode(StatusCodes.Status402PaymentRequired, new
                {
                    error = "Módulo não ativo",
                    message = "O módulo de produtos não está ativo para sua loja.",
                    moduleRequired = "PRODUCTS"
                });
            }

            // Validar operação específica
            var validationResult = await _moduleValidation.ValidateOperationAsync(
                tenantId, 
                "CRIAR_PRODUTO", 
                new Dictionary<string, object> { ["tipoRecurso"] = "produtos", ["quantidade"] = 1 },
                cancellationToken);

            if (!validationResult.Permitido)
            {
                return StatusCode(StatusCodes.Status402PaymentRequired, new
                {
                    error = "Limite excedido",
                    message = validationResult.Motivo,
                    moduleRequired = validationResult.ModuloNecessario,
                    planRequired = validationResult.PlanoNecessario
                });
            }

            // TODO: Implementar criação quando serviços estiverem prontos
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Não implementado",
                message = "Funcionalidade em desenvolvimento"
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
}

/// <summary>
/// Resultado paginado genérico
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// DTO de produto para API
/// </summary>
public class ProdutoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal? PrecoCusto { get; set; }
    public string? Categoria { get; set; }
    public int? EstoqueAtual { get; set; }
    public int? EstoqueMinimo { get; set; }
    public DateTime? DataValidade { get; set; }
    public string? Marca { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// Request para criação de produto
/// </summary>
public class CriarProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal? PrecoCusto { get; set; }
    public Guid? CategoriaId { get; set; }
    public int? EstoqueMinimo { get; set; }
    public DateTime? DataValidade { get; set; }
    public string? Marca { get; set; }
}