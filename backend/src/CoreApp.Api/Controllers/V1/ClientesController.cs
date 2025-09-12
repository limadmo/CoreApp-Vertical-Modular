using Microsoft.AspNetCore.Mvc;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreApp.Infrastructure.Data.Context;
using CoreApp.Infrastructure.Data.Seeds;
using CoreApp.Application.Services;

namespace CoreApp.Api.Controllers.V1;

/// <summary>
/// Controller para gestão de clientes brasileiros multi-tenant
/// Implementa operações CRUD com isolamento por tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly CoreAppDbContext _context;
    private readonly ILogger<ClientesController> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly DatabaseSeeder _seeder;

    public ClientesController(
        CoreAppDbContext context,
        ILogger<ClientesController> logger,
        ITenantContext tenantContext,
        DatabaseSeeder seeder)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _seeder = seeder ?? throw new ArgumentNullException(nameof(seeder));
    }

    /// <summary>
    /// Lista clientes do tenant atual com paginação
    /// </summary>
    /// <param name="pagina">Número da página (padrão: 1)</param>
    /// <param name="tamanhoPagina">Tamanho da página (padrão: 20)</param>
    /// <param name="termo">Termo de busca opcional</param>
    /// <param name="apenasAtivos">Filtrar apenas ativos (padrão: true)</param>
    /// <param name="ordenarPor">Campo de ordenação (padrão: DataCadastro)</param>
    /// <param name="direcaoOrdenacao">Direção da ordenação (ASC/DESC, padrão: DESC)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de clientes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ClienteDto>>> ListarClientes(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string? termo = null,
        [FromQuery] bool apenasAtivos = true,
        [FromQuery] string ordenarPor = "DataCadastro",
        [FromQuery] string direcaoOrdenacao = "DESC",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Para desenvolvimento, usar tenant padrão se não configurado
            var tenantId = "padaria-demo"; // _tenantContext.GetCurrentTenantId() ?? "padaria-demo";

            var query = _context.Clientes
                .Where(c => c.TenantId == tenantId);

            // Filtrar apenas ativos se solicitado
            if (apenasAtivos)
            {
                query = query.Where(c => c.Ativo && !c.Excluido);
            }

            // Aplicar busca por termo se fornecido
            if (!string.IsNullOrWhiteSpace(termo))
            {
                query = query.Where(c => 
                    c.Nome.Contains(termo) ||
                    (c.Email != null && c.Email.Contains(termo)) ||
                    (c.CPF != null && c.CPF.Contains(termo)) ||
                    (c.Telefone != null && c.Telefone.Contains(termo)));
            }

            // Aplicar ordenação
            query = ordenarPor.ToLower() switch
            {
                "nome" => direcaoOrdenacao.ToUpper() == "ASC" 
                    ? query.OrderBy(c => c.Nome) 
                    : query.OrderByDescending(c => c.Nome),
                "email" => direcaoOrdenacao.ToUpper() == "ASC" 
                    ? query.OrderBy(c => c.Email) 
                    : query.OrderByDescending(c => c.Email),
                "datacadastro" => direcaoOrdenacao.ToUpper() == "ASC" 
                    ? query.OrderBy(c => c.DataCadastro) 
                    : query.OrderByDescending(c => c.DataCadastro),
                _ => direcaoOrdenacao.ToUpper() == "ASC" 
                    ? query.OrderBy(c => c.DataCadastro) 
                    : query.OrderByDescending(c => c.DataCadastro)
            };

            // Contar total de registros
            var totalItems = await query.CountAsync(cancellationToken);

            // Aplicar paginação
            var items = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(c => new ClienteDto
                {
                    Id = c.Id.ToString(),
                    Nome = c.Nome,
                    CPF = c.CPF ?? "",
                    Email = c.Email ?? "",
                    Telefone = c.Telefone ?? "",
                    DataCadastro = c.DataCadastro,
                    Ativo = c.Ativo,
                    UltimaMovimentacao = c.UltimaMovimentacao
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ClienteDto>
            {
                Items = items,
                PageNumber = pagina,
                PageSize = tamanhoPagina,
                Total = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / tamanhoPagina)
            };

            _logger.LogInformation("Listagem de clientes: Página {Pagina}, Total {Total}", pagina, totalItems);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar clientes");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um cliente específico por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Cliente encontrado</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterCliente(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var clienteId))
        {
            return BadRequest(new { message = "ID inválido" });
        }

        var tenantId = "padaria-demo"; // _tenantContext.GetCurrentTenantId() ?? "padaria-demo";

        var cliente = await _context.Clientes
            .Where(c => c.Id == clienteId && c.TenantId == tenantId)
            .Select(c => new ClienteDto
            {
                Id = c.Id.ToString(),
                Nome = c.Nome,
                CPF = c.CPF ?? "",
                Email = c.Email ?? "",
                Telefone = c.Telefone ?? "",
                DataCadastro = c.DataCadastro,
                Ativo = c.Ativo,
                UltimaMovimentacao = c.UltimaMovimentacao
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cliente == null)
        {
            return NotFound(new { message = "Cliente não encontrado" });
        }

        return Ok(cliente);
    }

    /// <summary>
    /// Executa o seeding do banco com dados brasileiros usando Bogus (apenas desenvolvimento)
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status da operação de seeding</returns>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExecutarSeed(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🌱 Executando seeding com dados brasileiros via Bogus");
            await _seeder.SeedAsync();
            
            return Ok(new { message = "✅ Seeding executado com sucesso! 1000 clientes brasileiros criados." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao executar seeding");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Erro interno do servidor durante o seeding" });
        }
    }
}

/// <summary>
/// DTO para retorno de dados do cliente
/// </summary>
public class ClienteDto
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public string CPF { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
    public DateTime UltimaMovimentacao { get; set; }
}