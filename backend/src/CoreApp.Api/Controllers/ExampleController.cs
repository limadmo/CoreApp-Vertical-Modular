using CoreApp.Verticals.Common;
using CoreApp.Verticals.Padaria.Models;
using CoreApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoreApp.Api.Controllers;

/// <summary>
/// Controller de exemplo demonstrando o uso do sistema de verticais
/// Mostra como integrar propriedades verticais de forma transparente
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExampleController : ControllerBase
{
    private readonly IVerticalManager _verticalManager;
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(
        IVerticalManager verticalManager,
        ILogger<ExampleController> logger)
    {
        _verticalManager = verticalManager;
        _logger = logger;
    }

    /// <summary>
    /// Exemplo de criação de produto com propriedades verticais da Padaria
    /// </summary>
    /// <param name="request">Dados do produto incluindo propriedades da vertical Padaria</param>
    [HttpPost("produtos")]
    public async Task<ActionResult<ProductCreateResponse>> CriarProdutoComVertical(
        [FromBody] ProductCreateRequest request)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            
            // Verifica se a vertical Padaria está ativa
            var activeVerticals = await _verticalManager.GetActiveVerticalsAsync(tenantId);
            var padariaAtiva = activeVerticals.Any(v => v.VerticalName == "Padaria");

            // Se a vertical Padaria estiver ativa e houver propriedades específicas
            if (padariaAtiva && request.PadariaProperties != null)
            {
                // Valida as propriedades específicas da padaria
                var validationResult = await _verticalManager.ValidateVerticalPropertiesAsync(
                    tenantId, 
                    "Padaria", 
                    "produto", 
                    ConvertToPropertyDictionary(request.PadariaProperties));

                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Propriedades da vertical Padaria inválidas",
                        details = validationResult.Errors
                    });
                }

                // Log das propriedades validadas
                _logger.LogInformation(
                    "Produto com propriedades da Padaria validado para tenant {TenantId}: {ProdutoNome}",
                    tenantId, request.Nome);
            }

            // Simula criação do produto (aqui integraria com o serviço real)
            var produtoId = Guid.NewGuid();
            
            var response = new ProductCreateResponse
            {
                Id = produtoId,
                Nome = request.Nome,
                Categoria = request.Categoria,
                PrecoVenda = request.PrecoVenda,
                VerticaisAtivas = activeVerticals.Select(v => v.VerticalName).ToList(),
                Message = "Produto criado com sucesso"
            };

            if (padariaAtiva && request.PadariaProperties != null)
            {
                response.Message += $" (com propriedades da Padaria: {request.PadariaProperties.Tipo})";
            }

            return CreatedAtAction(
                nameof(ObterProduto), 
                new { id = produtoId }, 
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto com verticais");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exemplo de obtenção de produto por ID
    /// </summary>
    /// <param name="id">ID do produto</param>
    [HttpGet("produtos/{id}")]
    public async Task<ActionResult<ProductResponse>> ObterProduto(Guid id)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            
            // Simula busca do produto (aqui integraria com o serviço real)
            var produto = new ProductResponse
            {
                Id = id,
                Nome = "Pão Frances",
                Categoria = "Pães Salgados",
                PrecoVenda = 0.50m,
                // Simula propriedades da padaria se a vertical estiver ativa
                PadariaProperties = await GetPadariaPropertiesIfActive(tenantId)
            };

            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {ProdutoId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exemplo de ativação de vertical para demonstração
    /// </summary>
    /// <param name="verticalName">Nome da vertical a ativar</param>
    [HttpPost("ativar-vertical/{verticalName}")]
    public async Task<ActionResult<VerticalActivationResult>> AtivarVertical(string verticalName)
    {
        try
        {
            var tenantId = HttpContext.GetTenantId();
            
            // Configurações específicas para a vertical Padaria
            var configuration = new Dictionary<string, object>();
            
            if (verticalName.Equals("Padaria", StringComparison.OrdinalIgnoreCase))
            {
                configuration.Add("ValidadePadraoHoras", 12);
                configuration.Add("HorarioAbertura", "06:30");
                configuration.Add("HorarioFechamento", "18:00");
                configuration.Add("DescontoFidelidadePadrao", 10.0);
            }

            var result = await _verticalManager.ActivateVerticalAsync(tenantId, verticalName, configuration);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar vertical {VerticalName}", verticalName);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    #region Métodos Privados

    private Dictionary<string, object> ConvertToPropertyDictionary(PadariaProdutoProperties properties)
    {
        return new Dictionary<string, object>
        {
            { nameof(properties.Tipo), properties.Tipo.ToString() },
            { nameof(properties.ValidadeHoras), properties.ValidadeHoras },
            { nameof(properties.TemperaturaIdeal), properties.TemperaturaIdeal },
            { nameof(properties.AssadoNoDia), properties.AssadoNoDia },
            { nameof(properties.PesoMedioGramas), properties.PesoMedioGramas },
            { nameof(properties.RendimentoFornada), properties.RendimentoFornada },
            { nameof(properties.PodeCongelar), properties.PodeCongelar },
            { nameof(properties.IngredientesPrincipais), properties.IngredientesPrincipais },
            { nameof(properties.Alergenicos), properties.Alergenicos.Select(a => a.ToString()).ToList() }
        };
    }

    private async Task<PadariaProdutoProperties?> GetPadariaPropertiesIfActive(string tenantId)
    {
        var activeVerticals = await _verticalManager.GetActiveVerticalsAsync(tenantId);
        var padariaAtiva = activeVerticals.Any(v => v.VerticalName == "Padaria");

        if (padariaAtiva)
        {
            // Simula propriedades da padaria para o produto de exemplo
            return new PadariaProdutoProperties
            {
                Tipo = TipoProdutoPadaria.PaoSalgado,
                ValidadeHoras = 12,
                TemperaturaIdeal = 25,
                AssadoNoDia = true,
                PesoMedioGramas = 50,
                RendimentoFornada = 100,
                IngredientesPrincipais = new List<string> { "Farinha", "Água", "Sal", "Fermento" },
                Alergenicos = new List<TipoAlergenico> { TipoAlergenico.Gluten }
            };
        }

        return null;
    }

    #endregion
}

#region DTOs

public class ProductCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public PadariaProdutoProperties? PadariaProperties { get; set; }
}

public class ProductCreateResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public List<string> VerticaisAtivas { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public PadariaProdutoProperties? PadariaProperties { get; set; }
}

#endregion