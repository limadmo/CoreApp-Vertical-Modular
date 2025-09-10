using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Domain.Interfaces.UnitOfWork;
using CoreApp.Domain.Interfaces.Repositories;
using CoreApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CoreApp.Application.Services;

/// <summary>
/// Serviço de aplicação para gestão de vendas multi-tenant
/// Implementa regras de negócio comerciais brasileiras
/// </summary>
public interface IVendaService
{
    /// <summary>
    /// Lista vendas do tenant atual com paginação
    /// </summary>
    Task<PagedResult<VendaDto>> ListarVendasAsync(PageRequest pageRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma venda por ID
    /// </summary>
    Task<VendaDto?> ObterVendaPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia uma nova venda
    /// </summary>
    Task<VendaDto> IniciarVendaAsync(IniciarVendaRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona item a uma venda existente
    /// </summary>
    Task<VendaDto> AdicionarItemVendaAsync(Guid vendaId, AdicionarItemVendaRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove item de uma venda
    /// </summary>
    Task<VendaDto> RemoverItemVendaAsync(Guid vendaId, Guid itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finaliza uma venda processando pagamento
    /// </summary>
    Task<VendaDto> FinalizarVendaAsync(Guid vendaId, FinalizarVendaRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancela uma venda
    /// </summary>
    Task<bool> CancelarVendaAsync(Guid vendaId, string motivo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca vendas por período
    /// </summary>
    Task<IEnumerable<VendaDto>> BuscarVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}

public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IModuleValidationService _moduleValidation;
    private readonly ILogger<VendaService> _logger;

    public VendaService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IModuleValidationService moduleValidation,
        ILogger<VendaService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _moduleValidation = moduleValidation ?? throw new ArgumentNullException(nameof(moduleValidation));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista vendas do tenant atual com paginação
    /// </summary>
    public async Task<PagedResult<VendaDto>> ListarVendasAsync(PageRequest pageRequest, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        // Valida se módulo de vendas está ativo
        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogDebug("Listando vendas para tenant {TenantId} - Página: {Page}, Tamanho: {Size}", 
                tenantId, pageRequest.PageNumber, pageRequest.PageSize);

            var repository = _unitOfWork.Repository<VendaEntity>();
            
            var vendas = await repository.GetAllAsync(
                pageRequest.PageNumber, 
                pageRequest.PageSize, 
                cancellationToken);
            
            var total = await repository.CountAsync(cancellationToken);

            var vendaDtos = vendas.Select(v => new VendaDto
            {
                Id = v.Id,
                NumeroVenda = v.NumeroVenda,
                ClienteId = v.ClienteId,
                UsuarioVendaId = v.UsuarioVendaId,
                DataVenda = v.DataVenda,
                ValorProdutos = v.ValorProdutos,
                ValorDesconto = v.ValorDesconto,
                ValorTotal = v.ValorTotal,
                ValorPago = v.ValorPago,
                ValorTroco = v.ValorTroco,
                Status = v.Status,
                TipoVenda = v.TipoVenda,
                Observacoes = v.Observacoes,
                NumeroNFCe = v.NumeroNFCe,
                ChaveNFCe = v.ChaveNFCe,
                CriadoEm = v.DataCriacao,
                AtualizadoEm = v.DataAtualizacao
            });

            var resultado = new PagedResult<VendaDto>
            {
                Items = vendaDtos,
                Total = total,
                PageNumber = pageRequest.PageNumber,
                PageSize = pageRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageRequest.PageSize)
            };
            
            _logger.LogInformation("Vendas listadas para tenant {TenantId} - Total: {Total}", 
                tenantId, total);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar vendas para tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Obtém uma venda por ID
    /// </summary>
    public async Task<VendaDto?> ObterVendaPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        
        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogDebug("Obtendo venda {VendaId} para tenant {TenantId}", id, tenantId);

            var repository = _unitOfWork.Repository<VendaEntity>();
            var vendaEntity = await repository.GetByIdAsync(id, cancellationToken);

            if (vendaEntity == null)
            {
                _logger.LogWarning("Venda {VendaId} não encontrada para tenant {TenantId}", id, tenantId);
                return null;
            }

            var venda = new VendaDto
            {
                Id = vendaEntity.Id,
                NumeroVenda = vendaEntity.NumeroVenda,
                ClienteId = vendaEntity.ClienteId,
                UsuarioVendaId = vendaEntity.UsuarioVendaId,
                DataVenda = vendaEntity.DataVenda,
                ValorProdutos = vendaEntity.ValorProdutos,
                ValorDesconto = vendaEntity.ValorDesconto,
                ValorTotal = vendaEntity.ValorTotal,
                ValorPago = vendaEntity.ValorPago,
                ValorTroco = vendaEntity.ValorTroco,
                Status = vendaEntity.Status,
                TipoVenda = vendaEntity.TipoVenda,
                Observacoes = vendaEntity.Observacoes,
                CriadoEm = vendaEntity.DataCriacao,
                AtualizadoEm = vendaEntity.DataAtualizacao
            };

            _logger.LogDebug("Venda {VendaId} encontrada para tenant {TenantId}", id, tenantId);
            return venda;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter venda {VendaId} para tenant {TenantId}", id, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Inicia uma nova venda
    /// </summary>
    public async Task<VendaDto> IniciarVendaAsync(IniciarVendaRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var tenantId = _tenantContext.GetCurrentTenantId();
        
        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogInformation("Iniciando venda para tenant {TenantId}", tenantId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Gera próximo número da venda para o tenant
                var numeroVenda = await GerarProximoNumeroVendaAsync(tenantId, cancellationToken);

                var venda = new VendaEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    NumeroVenda = numeroVenda,
                    ClienteId = request.ClienteId,
                    UsuarioVendaId = request.UsuarioVendaId,
                    DataVenda = DateTime.UtcNow,
                    ValorProdutos = 0m,
                    ValorDesconto = 0m,
                    ValorTotal = 0m,
                    ValorPago = 0m,
                    ValorTroco = 0m,
                    Status = "PENDENTE",
                    TipoVenda = request.TipoVenda ?? "BALCAO",
                    Observacoes = request.Observacoes,
                    // DataCriacao será definida automaticamente pela BaseEntity
                };

                var repository = _unitOfWork.Repository<VendaEntity>();
                var vendaSalva = await repository.AddAsync(venda);
                await _unitOfWork.CommitAsync(cancellationToken);

                var vendaDto = new VendaDto
                {
                    Id = vendaSalva.Id,
                    NumeroVenda = vendaSalva.NumeroVenda,
                    ClienteId = vendaSalva.ClienteId,
                    UsuarioVendaId = vendaSalva.UsuarioVendaId,
                    DataVenda = vendaSalva.DataVenda,
                    ValorProdutos = vendaSalva.ValorProdutos,
                    ValorTotal = vendaSalva.ValorTotal,
                    Status = vendaSalva.Status,
                    TipoVenda = vendaSalva.TipoVenda,
                    Observacoes = vendaSalva.Observacoes,
                    CriadoEm = vendaSalva.DataCriacao
                };

                _logger.LogInformation("Venda {VendaId} iniciada para tenant {TenantId} - Número: {NumeroVenda}", 
                    vendaDto.Id, tenantId, numeroVenda);

                return vendaDto;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar venda para tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Adiciona item a uma venda existente
    /// </summary>
    public async Task<VendaDto> AdicionarItemVendaAsync(Guid vendaId, AdicionarItemVendaRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var tenantId = _tenantContext.GetCurrentTenantId();

        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogInformation("Adicionando item à venda {VendaId} - Produto: {ProdutoId}", 
                vendaId, request.ProdutoId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var vendaRepository = _unitOfWork.Repository<VendaEntity>();
                var produtoRepository = _unitOfWork.Repository<ProdutoEntity>();

                var venda = await vendaRepository.GetByIdAsync(vendaId, cancellationToken);
                if (venda == null)
                    throw new NotFoundException($"Venda {vendaId} não encontrada");

                if (venda.Status != "PENDENTE")
                    throw new InvalidOperationException("Apenas vendas pendentes podem ter itens adicionados");

                var produto = await produtoRepository.GetByIdAsync(request.ProdutoId, cancellationToken);
                if (produto == null)
                    throw new NotFoundException($"Produto {request.ProdutoId} não encontrado");

                var itemVenda = new ItemVendaEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    VendaId = vendaId,
                    ProdutoId = request.ProdutoId,
                    Quantidade = request.Quantidade,
                    PrecoUnitario = request.PrecoUnitario ?? produto.PrecoVenda,
                    NomeProduto = produto.Nome,
                    CodigoProduto = produto.CodigoInterno ?? produto.CodigoBarras,
                    UnidadeMedida = produto.UnidadeMedida,
                    Observacoes = request.Observacoes
                };

                // Calcula valor do item
                itemVenda.CalcularValorTotal();

                var itemRepository = _unitOfWork.Repository<ItemVendaEntity>();
                await itemRepository.AddAsync(itemVenda);

                // Atualiza totais da venda
                venda.CalcularValorTotal();
                await vendaRepository.UpdateAsync(venda);
                
                await _unitOfWork.CommitAsync(cancellationToken);

                var vendaAtualizada = await ObterVendaPorIdAsync(vendaId, cancellationToken);
                
                _logger.LogInformation("Item adicionado à venda {VendaId} - Produto: {ProdutoId}, Quantidade: {Quantidade}", 
                    vendaId, request.ProdutoId, request.Quantidade);

                return vendaAtualizada!;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar item à venda {VendaId}", vendaId);
            throw;
        }
    }

    /// <summary>
    /// Finaliza uma venda processando pagamento
    /// </summary>
    public async Task<VendaDto> FinalizarVendaAsync(Guid vendaId, FinalizarVendaRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var tenantId = _tenantContext.GetCurrentTenantId();

        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogInformation("Finalizando venda {VendaId}", vendaId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var repository = _unitOfWork.Repository<VendaEntity>();
                var venda = await repository.GetByIdAsync(vendaId, cancellationToken);

                if (venda == null)
                    throw new NotFoundException($"Venda {vendaId} não encontrada");

                if (venda.Status != "PENDENTE")
                    throw new InvalidOperationException("Apenas vendas pendentes podem ser finalizadas");

                // Adiciona formas de pagamento
                foreach (var pagamento in request.FormasPagamento)
                {
                    var formaPagamento = new FormaPagamentoVendaEntity
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        VendaId = vendaId,
                        FormaPagamentoId = pagamento.FormaPagamentoId,
                        Valor = pagamento.Valor,
                        NumeroParcelas = pagamento.NumeroParcelas,
                        Status = "PENDENTE",
                        Observacoes = pagamento.Observacoes
                    };

                    venda.AdicionarFormaPagamento(formaPagamento);
                }

                // Finaliza a venda
                venda.Finalizar();
                
                await repository.UpdateAsync(venda);
                await _unitOfWork.CommitAsync(cancellationToken);

                var vendaFinalizada = await ObterVendaPorIdAsync(vendaId, cancellationToken);

                _logger.LogInformation("Venda {VendaId} finalizada - Valor: {ValorTotal}", 
                    vendaId, venda.ValorTotal);

                return vendaFinalizada!;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar venda {VendaId}", vendaId);
            throw;
        }
    }

    /// <summary>
    /// Cancela uma venda
    /// </summary>
    public async Task<bool> CancelarVendaAsync(Guid vendaId, string motivo, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogInformation("Cancelando venda {VendaId} - Motivo: {Motivo}", vendaId, motivo);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var repository = _unitOfWork.Repository<VendaEntity>();
                var venda = await repository.GetByIdAsync(vendaId, cancellationToken);

                if (venda == null)
                    throw new NotFoundException($"Venda {vendaId} não encontrada");

                venda.Cancelar(motivo);
                await repository.UpdateAsync(venda);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Venda {VendaId} cancelada", vendaId);
                return true;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar venda {VendaId}", vendaId);
            throw;
        }
    }

    /// <summary>
    /// Remove item de uma venda
    /// </summary>
    public async Task<VendaDto> RemoverItemVendaAsync(Guid vendaId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogInformation("Removendo item {ItemId} da venda {VendaId}", itemId, vendaId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var vendaRepository = _unitOfWork.Repository<VendaEntity>();
                var itemRepository = _unitOfWork.Repository<ItemVendaEntity>();

                var venda = await vendaRepository.GetByIdAsync(vendaId, cancellationToken);
                if (venda == null)
                    throw new NotFoundException($"Venda {vendaId} não encontrada");

                if (venda.Status != "PENDENTE")
                    throw new InvalidOperationException("Apenas vendas pendentes podem ter itens removidos");

                await itemRepository.DeleteAsync(itemId, null, "Item removido da venda", cancellationToken);

                // Recalcula totais da venda
                venda.CalcularValorTotal();
                await vendaRepository.UpdateAsync(venda);
                
                await _unitOfWork.CommitAsync(cancellationToken);

                var vendaAtualizada = await ObterVendaPorIdAsync(vendaId, cancellationToken);

                _logger.LogInformation("Item {ItemId} removido da venda {VendaId}", itemId, vendaId);
                return vendaAtualizada!;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item {ItemId} da venda {VendaId}", itemId, vendaId);
            throw;
        }
    }

    /// <summary>
    /// Busca vendas por período
    /// </summary>
    public async Task<IEnumerable<VendaDto>> BuscarVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        if (!await _moduleValidation.HasModuleAccessAsync(tenantId, "VENDAS", cancellationToken))
        {
            throw new ModuleNotActiveException($"Módulo VENDAS não está ativo para o tenant {tenantId}");
        }

        try
        {
            _logger.LogDebug("Buscando vendas por período para tenant {TenantId} - {DataInicio} a {DataFim}", 
                tenantId, dataInicio, dataFim);

            var repository = _unitOfWork.Repository<VendaEntity>();
            // Como não temos FindAsync, vamos buscar todas e filtrar em memória
            // Em uma implementação real, isso deveria ser otimizado com queries específicas
            var todasVendas = await repository.GetAllAsync(1, int.MaxValue, cancellationToken);
            var vendas = todasVendas.Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim);

            var vendaDtos = vendas.Select(v => new VendaDto
            {
                Id = v.Id,
                NumeroVenda = v.NumeroVenda,
                ClienteId = v.ClienteId,
                DataVenda = v.DataVenda,
                ValorTotal = v.ValorTotal,
                Status = v.Status,
                TipoVenda = v.TipoVenda,
                CriadoEm = v.DataCriacao
            });

            _logger.LogDebug("Busca por período retornou {Count} vendas para tenant {TenantId}", 
                vendaDtos.Count(), tenantId);

            return vendaDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por período para tenant {TenantId}", tenantId);
            throw;
        }
    }

    #region Métodos Auxiliares

    /// <summary>
    /// Gera o próximo número sequencial de venda para o tenant
    /// </summary>
    private async Task<long> GerarProximoNumeroVendaAsync(string tenantId, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<VendaEntity>();
        // Como não temos FindAsync ordenado, vamos buscar todas e processar em memória
        // Em uma implementação real, isso deveria ser otimizado
        var todasVendas = await repository.GetAllAsync(1, int.MaxValue, cancellationToken);
        var ultimaVenda = todasVendas
            .Where(v => v.TenantId == tenantId)
            .OrderByDescending(v => v.NumeroVenda)
            .FirstOrDefault();

        var ultimoNumero = ultimaVenda?.NumeroVenda ?? 0;
        return ultimoNumero + 1;
    }

    #endregion
}

#region DTOs e Classes de Apoio

public class VendaDto
{
    public Guid Id { get; set; }
    public long NumeroVenda { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid UsuarioVendaId { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorProdutos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorTroco { get; set; }
    public string Status { get; set; } = "PENDENTE";
    public string TipoVenda { get; set; } = "BALCAO";
    public string? Observacoes { get; set; }
    public string? NumeroNFCe { get; set; }
    public string? ChaveNFCe { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}

public class IniciarVendaRequest
{
    public Guid? ClienteId { get; set; }
    public Guid UsuarioVendaId { get; set; }
    public string? TipoVenda { get; set; }
    public string? Observacoes { get; set; }
}

public class AdicionarItemVendaRequest
{
    public Guid ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal? PrecoUnitario { get; set; }
    public string? Observacoes { get; set; }
}

public class FinalizarVendaRequest
{
    public List<FormaPagamentoRequest> FormasPagamento { get; set; } = new();
    public string? Observacoes { get; set; }
}

public class FormaPagamentoRequest
{
    public Guid FormaPagamentoId { get; set; }
    public decimal Valor { get; set; }
    public int? NumeroParcelas { get; set; }
    public string? Observacoes { get; set; }
}

#endregion