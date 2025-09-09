using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Interfaces;

/// <summary>
/// Interface de repositório para operações de vendas farmacêuticas brasileiras
/// Inclui compliance ANVISA e operações offline-first
/// </summary>
/// <remarks>
/// Este repositório implementa padrões para controle de vendas com suporte
/// a medicamentos controlados, operações offline e compliance farmacêutico
/// </remarks>
public interface IVendaRepository
{
    // Operações CRUD básicas

    /// <summary>
    /// Adiciona uma nova venda
    /// </summary>
    /// <param name="venda">Venda a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda criada</returns>
    Task<VendaEntity> AddAsync(VendaEntity venda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma venda existente
    /// </summary>
    /// <param name="venda">Venda a ser atualizada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda atualizada</returns>
    Task<VendaEntity> UpdateAsync(VendaEntity venda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca venda por ID e tenant (com itens)
    /// </summary>
    /// <param name="id">ID da venda</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="incluirItens">Se deve incluir os itens da venda</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Venda encontrada ou null</returns>
    Task<VendaEntity?> GetByIdAsync(Guid id, string tenantId, bool incluirItens = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista vendas com filtros e paginação
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="clienteId">ID do cliente (opcional)</param>
    /// <param name="usuarioId">ID do usuário (opcional)</param>
    /// <param name="formaPagamento">Forma de pagamento (opcional)</param>
    /// <param name="statusPagamento">Status do pagamento (opcional)</param>
    /// <param name="temMedicamentoControlado">Filtro por medicamento controlado (opcional)</param>
    /// <param name="dataInicio">Data início (opcional)</param>
    /// <param name="dataFim">Data fim (opcional)</param>
    /// <param name="page">Página (1-based)</param>
    /// <param name="size">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de vendas</returns>
    Task<(IEnumerable<VendaEntity> Items, int Total)> GetPagedAsync(
        string tenantId,
        Guid? clienteId = null,
        string? usuarioId = null,
        FormaPagamento? formaPagamento = null,
        StatusPagamento? statusPagamento = null,
        bool? temMedicamentoControlado = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int page = 1,
        int size = 20,
        CancellationToken cancellationToken = default);

    // Operações de sincronização offline

    /// <summary>
    /// Busca vendas pendentes de sincronização
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas pendentes</returns>
    Task<IEnumerable<VendaEntity>> GetPendingSyncAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca vendas como sincronizadas
    /// </summary>
    /// <param name="ids">IDs das vendas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task de conclusão</returns>
    Task MarkAsSyncedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    // Operações específicas de vendas farmacêuticas

    /// <summary>
    /// Busca vendas por número de receita
    /// </summary>
    /// <param name="numeroReceita">Número da receita</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas com a receita</returns>
    Task<IEnumerable<VendaEntity>> GetByReceitaAsync(string numeroReceita, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca vendas com medicamentos controlados
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="dataInicio">Data início</param>
    /// <param name="dataFim">Data fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas com controlados</returns>
    Task<IEnumerable<VendaEntity>> GetVendasControladosAsync(
        string tenantId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca vendas que precisam arquivar receita
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas pendentes de arquivo</returns>
    Task<IEnumerable<VendaEntity>> GetPendenteArquivoReceitaAsync(string tenantId, CancellationToken cancellationToken = default);

    // Operações de relatório e dashboard

    /// <summary>
    /// Calcula vendas por período
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas de vendas do período</returns>
    Task<(decimal ValorTotal, int QuantidadeVendas, decimal TicketMedio)> GetVendasPeriodoAsync(
        string tenantId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos mais vendidos
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="limit">Limite de resultados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de produtos mais vendidos</returns>
    Task<IEnumerable<(Guid ProdutoId, string NomeProduto, decimal QuantidadeVendida, decimal ValorTotal)>> GetProdutosMaisVendidosAsync(
        string tenantId,
        DateTime dataInicio,
        DateTime dataFim,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas por forma de pagamento
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas por forma de pagamento</returns>
    Task<Dictionary<FormaPagamento, (int Quantidade, decimal Valor)>> GetEstatisticasFormaPagamentoAsync(
        string tenantId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    // Validações específicas

    /// <summary>
    /// Verifica se receita já foi utilizada
    /// </summary>
    /// <param name="numeroReceita">Número da receita</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="excludeVendaId">ID da venda a ser excluída da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se receita já foi utilizada</returns>
    Task<bool> ReceitaJaUtilizadaAsync(string numeroReceita, string tenantId, Guid? excludeVendaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca última venda do cliente para validações de receita
    /// </summary>
    /// <param name="clienteDocumento">Documento do cliente</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Última venda do cliente ou null</returns>
    Task<VendaEntity?> GetUltimaVendaClienteAsync(string clienteDocumento, string tenantId, CancellationToken cancellationToken = default);
}