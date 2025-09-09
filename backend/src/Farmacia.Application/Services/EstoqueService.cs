using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.MultiTenant;
using Microsoft.Extensions.Logging;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço de estoque farmacêutico brasileiro com suporte offline-first
/// Implementa business rules baseadas no sistema TypeScript existente
/// </summary>
/// <remarks>
/// Este serviço implementa controle de estoque offline-first, permitindo que
/// farmácias operem sem internet e sincronizem dados automaticamente.
/// Inclui validações ANVISA e business rules farmacêuticas brasileiras.
/// </remarks>
public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ITenantService _tenantService;
    private readonly ILogger<EstoqueService> _logger;

    public EstoqueService(
        IEstoqueRepository estoqueRepository,
        ITenantService tenantService,
        ILogger<EstoqueService> logger)
    {
        _estoqueRepository = estoqueRepository;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Registra uma nova movimentação de estoque
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Movimentação registrada</returns>
    public async Task<EstoqueEntity> RegistrarMovimentacaoAsync(RegistrarMovimentacaoRequest request, CancellationToken cancellationToken = default)
    {
        // Validar módulo comercial
        ValidarModuloEstoque();

        // Validações de negócio
        ValidarMovimentacao(request);

        var tenantId = _tenantService.GetCurrentTenantId();
        var usuarioId = _tenantService.GetCurrentUserId();

        // Verificar estoque atual se for saída
        if (request.Tipo == TipoMovimentacao.SAIDA || 
            request.Tipo == TipoMovimentacao.PERDA || 
            request.Tipo == TipoMovimentacao.VENCIMENTO)
        {
            var estoqueAtual = await _estoqueRepository.CalcularEstoqueAtualAsync(request.ProdutoId, tenantId, cancellationToken);
            
            if (estoqueAtual < request.Quantidade)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente. Disponível: {estoqueAtual}, Solicitado: {request.Quantidade}");
            }
        }

        // Calcular quantidades
        var quantidadeAnterior = await _estoqueRepository.CalcularEstoqueAtualAsync(request.ProdutoId, tenantId, cancellationToken);
        var impacto = CalcularImpactoEstoque(request.Tipo, request.Quantidade);
        var quantidadeAtual = quantidadeAnterior + impacto;

        // Criar movimentação
        var movimentacao = new EstoqueEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProdutoId = request.ProdutoId,
            Tipo = request.Tipo,
            Quantidade = request.Quantidade,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadeAtual = quantidadeAtual,
            Motivo = request.Motivo,
            Observacoes = request.Observacoes,
            VendaId = request.VendaId,
            ItemVendaId = request.ItemVendaId,
            UsuarioId = usuarioId,
            ClienteTimestamp = request.ClienteTimestamp ?? DateTime.UtcNow,
            CriadoPor = usuarioId,
            AtualizadoPor = usuarioId
        };

        // Gerar hash de integridade
        movimentacao.HashIntegridade = movimentacao.GerarHashIntegridade();

        // Salvar no repositório
        var resultado = await _estoqueRepository.AddAsync(movimentacao, cancellationToken);

        // Log para auditoria farmacêutica
        _logger.LogInformation(
            "Movimentação de estoque registrada: {TipoMovimentacao} {Quantidade} - Produto: {ProdutoId} - Tenant: {TenantId}",
            request.Tipo, request.Quantidade, request.ProdutoId, tenantId);

        return resultado;
    }

    /// <summary>
    /// Lista movimentações com filtros e paginação
    /// </summary>
    /// <param name="request">Filtros de busca</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de movimentações</returns>
    public async Task<PagedResult<EstoqueEntity>> ListarMovimentacoesAsync(ListarMovimentacoesRequest request, CancellationToken cancellationToken = default)
    {
        ValidarModuloEstoque();

        var tenantId = _tenantService.GetCurrentTenantId();

        var (items, total) = await _estoqueRepository.GetPagedAsync(
            tenantId,
            request.ProdutoId,
            request.Tipo,
            request.UsuarioId,
            request.DataInicio,
            request.DataFim,
            request.Page,
            request.Size,
            cancellationToken);

        return new PagedResult<EstoqueEntity>
        {
            Items = items.ToList(),
            Page = request.Page,
            Size = request.Size,
            Total = total,
            Pages = (int)Math.Ceiling(total / (double)request.Size),
            HasNext = request.Page * request.Size < total,
            HasPrevious = request.Page > 1
        };
    }

    /// <summary>
    /// Obtém resumo do estoque
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de resumos por produto</returns>
    public async Task<IEnumerable<EstoqueResumoDto>> ObterResumoEstoqueAsync(CancellationToken cancellationToken = default)
    {
        ValidarModuloEstoque();

        var tenantId = _tenantService.GetCurrentTenantId();
        
        var resumos = await _estoqueRepository.GetEstoqueResumoAsync(tenantId, null, cancellationToken);

        return resumos.Select(r => new EstoqueResumoDto
        {
            ProdutoId = r.ProdutoId,
            QuantidadeAtual = r.QuantidadeAtual,
            UltimaMovimentacao = r.UltimaMovimentacao,
            Status = DeterminarStatusEstoque(r.QuantidadeAtual, 10, 100) // TODO: Buscar limites reais do produto
        });
    }

    /// <summary>
    /// Lista produtos com estoque baixo
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de produtos com estoque baixo</returns>
    public async Task<IEnumerable<ProdutoEstoqueBaixoDto>> ListarProdutosEstoqueBaixoAsync(CancellationToken cancellationToken = default)
    {
        ValidarModuloEstoque();

        var tenantId = _tenantService.GetCurrentTenantId();
        
        var produtos = await _estoqueRepository.GetProdutosEstoqueBaixoAsync(tenantId, cancellationToken);

        return produtos.Select(p => new ProdutoEstoqueBaixoDto
        {
            ProdutoId = p.ProdutoId,
            NomeProduto = p.NomeProduto,
            EstoqueAtual = p.EstoqueAtual,
            EstoqueMinimo = p.EstoqueMinimo,
            Status = DeterminarStatusEstoque(p.EstoqueAtual, p.EstoqueMinimo, null)
        });
    }

    /// <summary>
    /// Sincroniza movimentações offline
    /// </summary>
    /// <param name="movimentacoes">Lista de movimentações offline</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da sincronização</returns>
    public async Task<SincronizacaoResult> SincronizarMovimentacoesOfflineAsync(
        IEnumerable<MovimentacaoOfflineDto> movimentacoes, 
        CancellationToken cancellationToken = default)
    {
        ValidarModuloEstoque();

        var resultado = new SincronizacaoResult
        {
            Processadas = 0,
            Sucessos = 0,
            Erros = 0,
            Conflitos = 0,
            Detalhes = new List<SincronizacaoDetalhe>()
        };

        foreach (var movOffline in movimentacoes)
        {
            resultado.Processadas++;

            try
            {
                // Validar integridade
                if (!ValidarIntegridadeMovimentacao(movOffline))
                {
                    resultado.Erros++;
                    resultado.Detalhes.Add(new SincronizacaoDetalhe
                    {
                        Id = movOffline.Id,
                        Tipo = "movimentacao",
                        Status = StatusSincronizacao.ERRO,
                        Erro = "Falha na validação de integridade"
                    });
                    continue;
                }

                // Processar movimentação
                var request = new RegistrarMovimentacaoRequest
                {
                    ProdutoId = movOffline.ProdutoId,
                    Tipo = movOffline.Tipo,
                    Quantidade = movOffline.Quantidade,
                    Motivo = movOffline.Motivo,
                    Observacoes = movOffline.Observacoes,
                    ClienteTimestamp = movOffline.ClienteTimestamp
                };

                await RegistrarMovimentacaoAsync(request, cancellationToken);

                resultado.Sucessos++;
                resultado.Detalhes.Add(new SincronizacaoDetalhe
                {
                    Id = movOffline.Id,
                    Tipo = "movimentacao",
                    Status = StatusSincronizacao.SINCRONIZADO
                });

            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Estoque insuficiente"))
            {
                resultado.Conflitos++;
                resultado.Detalhes.Add(new SincronizacaoDetalhe
                {
                    Id = movOffline.Id,
                    Tipo = "movimentacao",
                    Status = StatusSincronizacao.CONFLITO,
                    Erro = ex.Message
                });
            }
            catch (Exception ex)
            {
                resultado.Erros++;
                resultado.Detalhes.Add(new SincronizacaoDetalhe
                {
                    Id = movOffline.Id,
                    Tipo = "movimentacao",
                    Status = StatusSincronizacao.ERRO,
                    Erro = ex.Message
                });

                _logger.LogError(ex, "Erro ao sincronizar movimentação {MovimentacaoId}", movOffline.Id);
            }
        }

        _logger.LogInformation(
            "Sincronização concluída: {Sucessos} sucessos, {Erros} erros, {Conflitos} conflitos",
            resultado.Sucessos, resultado.Erros, resultado.Conflitos);

        return resultado;
    }

    /// <summary>
    /// Obtém estatísticas do dashboard de estoque
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas do dashboard</returns>
    public async Task<DashboardEstoqueDto> ObterDashboardEstoqueAsync(CancellationToken cancellationToken = default)
    {
        ValidarModuloEstoque();

        var tenantId = _tenantService.GetCurrentTenantId();
        var hoje = DateTime.Today;
        var inicioSemana = hoje.AddDays(-7);

        var tasks = new[]
        {
            _estoqueRepository.GetEstoqueResumoAsync(tenantId, null, cancellationToken),
            _estoqueRepository.GetProdutosEstoqueBaixoAsync(tenantId, cancellationToken),
            _estoqueRepository.GetEstatisticasMovimentacaoAsync(tenantId, hoje, hoje.AddDays(1), cancellationToken),
            _estoqueRepository.GetEstatisticasMovimentacaoAsync(tenantId, inicioSemana, hoje, cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        
        var resumoEstoque = results[0] as IEnumerable<(Guid ProdutoId, decimal QuantidadeAtual, DateTime? UltimaMovimentacao)>;
        var produtosEstoqueBaixo = results[1] as IEnumerable<(Guid ProdutoId, string NomeProduto, decimal EstoqueAtual, decimal EstoqueMinimo)>;
        var movimentacoesHoje = results[2] as Dictionary<TipoMovimentacao, int>;
        var movimentacoesSemana = results[3] as Dictionary<TipoMovimentacao, int>;

        return new DashboardEstoqueDto
        {
            TotalProdutos = resumoEstoque?.Count() ?? 0,
            ProdutosEstoqueBaixo = produtosEstoqueBaixo?.Count() ?? 0,
            MovimentacoesHoje = movimentacoesHoje?.Values.Sum() ?? 0,
            MovimentacoesSemana = movimentacoesSemana?.Values.Sum() ?? 0,
            ProdutosZerados = resumoEstoque?.Count(r => r.QuantidadeAtual == 0) ?? 0
        };
    }

    // Métodos auxiliares privados

    private void ValidarModuloEstoque()
    {
        if (!_tenantService.HasModuleAccess("STOCK"))
        {
            throw new UnauthorizedAccessException("Módulo STOCK não está ativo para esta farmácia");
        }
    }

    private static void ValidarMovimentacao(RegistrarMovimentacaoRequest request)
    {
        if (request.ProdutoId == Guid.Empty)
            throw new ArgumentException("Produto é obrigatório");

        if (request.Quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new ArgumentException("Motivo é obrigatório para auditoria ANVISA");

        if (request.Motivo.Length > 500)
            throw new ArgumentException("Motivo não pode exceder 500 caracteres");
    }

    private static decimal CalcularImpactoEstoque(TipoMovimentacao tipo, decimal quantidade)
    {
        return tipo switch
        {
            TipoMovimentacao.ENTRADA or TipoMovimentacao.AJUSTE => quantidade,
            TipoMovimentacao.SAIDA or TipoMovimentacao.PERDA or 
            TipoMovimentacao.VENCIMENTO or TipoMovimentacao.TRANSFERENCIA => -quantidade,
            TipoMovimentacao.INVENTARIO => 0, // Inventário não altera estoque diretamente
            _ => throw new ArgumentException($"Tipo de movimentação não suportado: {tipo}")
        };
    }

    private static StatusEstoque DeterminarStatusEstoque(decimal quantidade, decimal minimo, decimal? maximo)
    {
        if (quantidade == 0)
            return StatusEstoque.ZERADO;

        if (quantidade <= minimo)
            return StatusEstoque.CRITICO;

        if (quantidade <= minimo * 1.2m) // 20% acima do mínimo
            return StatusEstoque.BAIXO;

        if (maximo.HasValue && quantidade > maximo.Value)
            return StatusEstoque.EXCESSIVO;

        return StatusEstoque.NORMAL;
    }

    private static bool ValidarIntegridadeMovimentacao(MovimentacaoOfflineDto movimentacao)
    {
        // Validação básica de dados
        if (movimentacao.ProdutoId == Guid.Empty ||
            movimentacao.Quantidade <= 0 ||
            string.IsNullOrWhiteSpace(movimentacao.Motivo))
        {
            return false;
        }

        // TODO: Implementar validação de hash mais robusta
        return true;
    }
}

// DTOs para o serviço

/// <summary>
/// Request para registrar movimentação de estoque
/// </summary>
public class RegistrarMovimentacaoRequest
{
    public Guid ProdutoId { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public decimal Quantidade { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public Guid? VendaId { get; set; }
    public Guid? ItemVendaId { get; set; }
    public DateTime? ClienteTimestamp { get; set; }
}

/// <summary>
/// Request para listar movimentações
/// </summary>
public class ListarMovimentacoesRequest
{
    public Guid? ProdutoId { get; set; }
    public TipoMovimentacao? Tipo { get; set; }
    public string? UsuarioId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}

/// <summary>
/// DTO para movimentação offline
/// </summary>
public class MovimentacaoOfflineDto
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
/// DTO para resumo de estoque
/// </summary>
public class EstoqueResumoDto
{
    public Guid ProdutoId { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public DateTime? UltimaMovimentacao { get; set; }
    public StatusEstoque Status { get; set; }
}

/// <summary>
/// DTO para produto com estoque baixo
/// </summary>
public class ProdutoEstoqueBaixoDto
{
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public decimal EstoqueAtual { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public StatusEstoque Status { get; set; }
}

/// <summary>
/// DTO para dashboard de estoque
/// </summary>
public class DashboardEstoqueDto
{
    public int TotalProdutos { get; set; }
    public int ProdutosEstoqueBaixo { get; set; }
    public int ProdutosZerados { get; set; }
    public int MovimentacoesHoje { get; set; }
    public int MovimentacoesSemana { get; set; }
}

/// <summary>
/// Resultado de sincronização
/// </summary>
public class SincronizacaoResult
{
    public int Processadas { get; set; }
    public int Sucessos { get; set; }
    public int Erros { get; set; }
    public int Conflitos { get; set; }
    public List<SincronizacaoDetalhe> Detalhes { get; set; } = new();
}

/// <summary>
/// Detalhe de sincronização
/// </summary>
public class SincronizacaoDetalhe
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public StatusSincronizacao Status { get; set; }
    public string? Erro { get; set; }
}

/// <summary>
/// Resultado paginado genérico
/// </summary>
public class PagedResult<T>
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
/// Interface do serviço de estoque
/// </summary>
public interface IEstoqueService
{
    Task<EstoqueEntity> RegistrarMovimentacaoAsync(RegistrarMovimentacaoRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<EstoqueEntity>> ListarMovimentacoesAsync(ListarMovimentacoesRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<EstoqueResumoDto>> ObterResumoEstoqueAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProdutoEstoqueBaixoDto>> ListarProdutosEstoqueBaixoAsync(CancellationToken cancellationToken = default);
    Task<SincronizacaoResult> SincronizarMovimentacoesOfflineAsync(IEnumerable<MovimentacaoOfflineDto> movimentacoes, CancellationToken cancellationToken = default);
    Task<DashboardEstoqueDto> ObterDashboardEstoqueAsync(CancellationToken cancellationToken = default);
}