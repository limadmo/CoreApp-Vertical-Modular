using Farmacia.Domain.Entities;
using Farmacia.Infrastructure.Data.Context;
using Farmacia.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Api.Controllers.V1;

/// <summary>
/// Controller para gerenciamento de produtos farmacêuticos brasileiros
/// Inclui classificação ANVISA e compliance farmacêutico completo
/// </summary>
/// <remarks>
/// Este controller implementa isolamento automático multi-tenant
/// e validação de módulos comerciais por plano contratado
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/produtos")]
[Tags("Produtos Farmacêuticos")]
public class ProdutosController : ControllerBase
{
    private readonly FarmaciaDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        FarmaciaDbContext context,
        ITenantService tenantService,
        ILogger<ProdutosController> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os produtos da farmácia atual
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="size">Tamanho da página (padrão: 20, máximo: 100)</param>
    /// <param name="search">Termo de busca (nome ou princípio ativo)</param>
    /// <param name="controlado">Filtrar apenas medicamentos controlados</param>
    /// <param name="ativo">Filtrar apenas produtos ativos</param>
    /// <returns>Lista paginada de produtos</returns>
    /// <response code="200">Lista de produtos retornada com sucesso</response>
    /// <response code="401">Não autorizado - token inválido</response>
    /// <response code="403">Módulo PRODUCTS não ativo para esta farmácia</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProdutoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<ProdutoResponse>>> ListarProdutos(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? controlado = null,
        [FromQuery] bool? ativo = null)
    {
        // Validar módulo comercial
        if (!_tenantService.HasModuleAccess("PRODUCTS"))
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                message = "O módulo PRODUCTS não está ativo para sua farmácia. Faça upgrade do seu plano.",
                moduleRequired = "PRODUCTS",
                tenantInfo = _tenantService.GetCurrentTenantInfo()
            });
        }

        // Validar parâmetros de paginação
        page = Math.Max(1, page);
        size = Math.Min(100, Math.Max(1, size));

        // Construir query com filtros automáticos multi-tenant
        var query = _context.Produtos.AsQueryable();

        // Aplicar filtros opcionais
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p => 
                p.Nome.Contains(search) || 
                (p.PrincipioAtivo != null && p.PrincipioAtivo.Contains(search)) ||
                (p.CodigoBarras != null && p.CodigoBarras.Contains(search)));
        }

        if (controlado.HasValue)
        {
            if (controlado.Value)
            {
                query = query.Where(p => p.ClassificacaoAnvisa >= Domain.Enums.ClassificacaoAnvisa.A1);
            }
            else
            {
                query = query.Where(p => p.ClassificacaoAnvisa < Domain.Enums.ClassificacaoAnvisa.A1);
            }
        }

        if (ativo.HasValue)
        {
            query = query.Where(p => p.Ativo == ativo.Value);
        }

        // Contar total de registros
        var totalItems = await query.CountAsync();

        // Aplicar paginação e buscar dados
        var produtos = await query
            .OrderBy(p => p.Nome)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProdutoResponse
            {
                Id = p.Id,
                Nome = p.Nome,
                CodigoBarras = p.CodigoBarras,
                RegistroAnvisa = p.RegistroAnvisa,
                PrincipioAtivo = p.PrincipioAtivo,
                Concentracao = p.Concentracao,
                FormaFarmaceutica = p.FormaFarmaceutica,
                Laboratorio = p.Laboratorio,
                ClassificacaoAnvisa = p.ClassificacaoAnvisa.ToString(),
                IsControlado = p.ClassificacaoAnvisa >= Domain.Enums.ClassificacaoAnvisa.A1,
                TipoReceitaNecessaria = p.ClassificacaoAnvisa switch
                {
                    Domain.Enums.ClassificacaoAnvisa.A1 or Domain.Enums.ClassificacaoAnvisa.A2 or Domain.Enums.ClassificacaoAnvisa.A3 => "Receita Especial Azul (2 vias)",
                    Domain.Enums.ClassificacaoAnvisa.B1 or Domain.Enums.ClassificacaoAnvisa.B2 => "Receita Especial Branca (2 vias)",
                    Domain.Enums.ClassificacaoAnvisa.C1 or Domain.Enums.ClassificacaoAnvisa.C5 => "Receita de Controle Especial Branca (2 vias)",
                    Domain.Enums.ClassificacaoAnvisa.C2 or Domain.Enums.ClassificacaoAnvisa.C3 or Domain.Enums.ClassificacaoAnvisa.C4 => "Receita de Controle Especial Branca",
                    Domain.Enums.ClassificacaoAnvisa.SUJEITO_PRESCRICAO => "Receita Médica Simples",
                    _ => null
                },
                PrecoCusto = p.PrecoCusto,
                PrecoVenda = p.PrecoVenda,
                MargemLucro = p.MargemLucro,
                EstoqueAtual = p.EstoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                Localizacao = p.Localizacao,
                Ativo = p.Ativo,
                DataCriacao = p.DataCriacao,
                DataAtualizacao = p.DataAtualizacao
            })
            .ToListAsync();

        // Montar resposta paginada
        var response = new PagedResponse<ProdutoResponse>
        {
            Items = produtos,
            Page = page,
            Size = size,
            Total = totalItems,
            Pages = (int)Math.Ceiling(totalItems / (double)size),
            HasNext = page * size < totalItems,
            HasPrevious = page > 1
        };

        // Log da operação para auditoria
        _logger.LogInformation(
            "Listagem de produtos: Tenant={TenantId}, Page={Page}, Size={Size}, Total={Total}, Search={Search}",
            _tenantService.GetCurrentTenantId(),
            page,
            size,
            totalItems,
            search ?? "");

        return Ok(response);
    }

    /// <summary>
    /// Busca produto por código de barras
    /// </summary>
    /// <param name="codigoBarras">Código EAN-13 do produto</param>
    /// <returns>Produto encontrado</returns>
    /// <response code="200">Produto encontrado</response>
    /// <response code="404">Produto não encontrado</response>
    [HttpGet("barras/{codigoBarras}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> BuscarPorCodigoBarras(string codigoBarras)
    {
        if (!_tenantService.HasModuleAccess("PRODUCTS"))
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                moduleRequired = "PRODUCTS"
            });
        }

        var produto = await _context.Produtos
            .Where(p => p.CodigoBarras == codigoBarras)
            .Select(p => new ProdutoResponse
            {
                Id = p.Id,
                Nome = p.Nome,
                CodigoBarras = p.CodigoBarras,
                RegistroAnvisa = p.RegistroAnvisa,
                PrincipioAtivo = p.PrincipioAtivo,
                ClassificacaoAnvisa = p.ClassificacaoAnvisa.ToString(),
                IsControlado = p.ClassificacaoAnvisa >= Domain.Enums.ClassificacaoAnvisa.A1,
                PrecoVenda = p.PrecoVenda,
                EstoqueAtual = p.EstoqueAtual,
                Ativo = p.Ativo
            })
            .FirstOrDefaultAsync();

        if (produto == null)
        {
            _logger.LogWarning(
                "Produto não encontrado: CodigoBarras={CodigoBarras}, Tenant={TenantId}",
                codigoBarras,
                _tenantService.GetCurrentTenantId());

            return NotFound(new
            {
                error = "Produto não encontrado",
                message = $"Nenhum produto encontrado com código de barras {codigoBarras}"
            });
        }

        return Ok(produto);
    }

    /// <summary>
    /// Cria um novo produto farmacêutico
    /// </summary>
    /// <param name="request">Dados do produto</param>
    /// <returns>Produto criado</returns>
    /// <response code="201">Produto criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="409">Produto com código de barras já existe</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> CriarProduto([FromBody] CriarProdutoRequest request)
    {
        if (!_tenantService.HasModuleAccess("PRODUCTS"))
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                error = "Módulo não ativo",
                moduleRequired = "PRODUCTS"
            });
        }

        // Verificar se código de barras já existe
        if (!string.IsNullOrEmpty(request.CodigoBarras))
        {
            var existingProduct = await _context.Produtos
                .AnyAsync(p => p.CodigoBarras == request.CodigoBarras);

            if (existingProduct)
            {
                return Conflict(new
                {
                    error = "Código de barras já existe",
                    message = $"Já existe um produto com código de barras {request.CodigoBarras}"
                });
            }
        }

        // Criar entidade
        var produto = new ProdutoEntity
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            CodigoBarras = request.CodigoBarras?.Trim(),
            RegistroAnvisa = request.RegistroAnvisa?.Trim(),
            PrincipioAtivo = request.PrincipioAtivo?.Trim(),
            Concentracao = request.Concentracao?.Trim(),
            FormaFarmaceutica = request.FormaFarmaceutica?.Trim(),
            Laboratorio = request.Laboratorio?.Trim(),
            ClassificacaoAnvisa = Enum.Parse<Domain.Enums.ClassificacaoAnvisa>(request.ClassificacaoAnvisa),
            PrecoCusto = request.PrecoCusto,
            PrecoVenda = request.PrecoVenda,
            MargemLucro = request.MargemLucro,
            EstoqueAtual = request.EstoqueInicial,
            EstoqueMinimo = request.EstoqueMinimo,
            EstoqueMaximo = request.EstoqueMaximo,
            Localizacao = request.Localizacao?.Trim(),
            Observacoes = request.Observacoes?.Trim(),
            Ativo = true,
            CriadoPor = _tenantService.GetCurrentUserId(),
            AtualizadoPor = _tenantService.GetCurrentUserId()
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        var response = new ProdutoResponse
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CodigoBarras = produto.CodigoBarras,
            ClassificacaoAnvisa = produto.ClassificacaoAnvisa.ToString(),
            IsControlado = produto.IsControlado,
            PrecoVenda = produto.PrecoVenda,
            EstoqueAtual = produto.EstoqueAtual,
            Ativo = produto.Ativo,
            DataCriacao = produto.DataCriacao
        };

        _logger.LogInformation(
            "Produto criado: Id={ProductId}, Nome={Nome}, Tenant={TenantId}",
            produto.Id,
            produto.Nome,
            _tenantService.GetCurrentTenantId());

        return CreatedAtAction(
            nameof(BuscarPorCodigoBarras),
            new { codigoBarras = produto.CodigoBarras },
            response);
    }
}

#region DTOs de Resposta

/// <summary>
/// Resposta paginada genérica
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

/// <summary>
/// DTO de resposta para produto farmacêutico
/// </summary>
public class ProdutoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string? RegistroAnvisa { get; set; }
    public string? PrincipioAtivo { get; set; }
    public string? Concentracao { get; set; }
    public string? FormaFarmaceutica { get; set; }
    public string? Laboratorio { get; set; }
    public string ClassificacaoAnvisa { get; set; } = string.Empty;
    public bool IsControlado { get; set; }
    public string? TipoReceitaNecessaria { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal MargemLucro { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public string? Localizacao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}

/// <summary>
/// DTO de requisição para criar produto
/// </summary>
public class CriarProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string? RegistroAnvisa { get; set; }
    public string? PrincipioAtivo { get; set; }
    public string? Concentracao { get; set; }
    public string? FormaFarmaceutica { get; set; }
    public string? Laboratorio { get; set; }
    public string ClassificacaoAnvisa { get; set; } = "ISENTO_PRESCRICAO";
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal MargemLucro { get; set; }
    public int EstoqueInicial { get; set; }
    public int EstoqueMinimo { get; set; } = 10;
    public int EstoqueMaximo { get; set; } = 100;
    public string? Localizacao { get; set; }
    public string? Observacoes { get; set; }
}

#endregion