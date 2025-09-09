using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Interfaces;

/// <summary>
/// Interface de repositório para operações de estoque farmacêutico brasileiro
/// Inclui operações offline-first e sincronização automática
/// </summary>
/// <remarks>
/// Este repositório implementa padrões para controle de estoque com suporte
/// a operações offline, sincronização e compliance farmacêutico ANVISA
/// </remarks>
public interface IEstoqueRepository
{
    // Operações CRUD básicas

    /// <summary>
    /// Adiciona uma nova movimentação de estoque
    /// </summary>
    /// <param name="movimentacao">Movimentação a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Movimentação criada</returns>
    Task<EstoqueEntity> AddAsync(EstoqueEntity movimentacao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca movimentação por ID e tenant
    /// </summary>
    /// <param name="id">ID da movimentação</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Movimentação encontrada ou null</returns>
    Task<EstoqueEntity?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista movimentações com filtros e paginação
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="produtoId">ID do produto (opcional)</param>
    /// <param name="tipo">Tipo de movimentação (opcional)</param>
    /// <param name="usuarioId">ID do usuário (opcional)</param>
    /// <param name="dataInicio">Data início (opcional)</param>
    /// <param name="dataFim">Data fim (opcional)</param>
    /// <param name="page">Página (1-based)</param>
    /// <param name="size">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de movimentações</returns>
    Task<(IEnumerable<EstoqueEntity> Items, int Total)> GetPagedAsync(
        string tenantId,
        Guid? produtoId = null,
        TipoMovimentacao? tipo = null,
        string? usuarioId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int page = 1,
        int size = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca movimentações por produto
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de movimentações do produto</returns>
    Task<IEnumerable<EstoqueEntity>> GetByProdutoAsync(Guid produtoId, string tenantId, CancellationToken cancellationToken = default);

    // Operações de sincronização offline

    /// <summary>
    /// Busca movimentações pendentes de sincronização
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de movimentações pendentes</returns>
    Task<IEnumerable<EstoqueEntity>> GetPendingSyncAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca movimentações como sincronizadas
    /// </summary>
    /// <param name="ids">IDs das movimentações</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task de conclusão</returns>
    Task MarkAsSyncedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    // Operações de relatório e dashboard

    /// <summary>
    /// Calcula resumo de estoque por produto
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="produtoIds">IDs dos produtos (opcional - se vazio, todos os produtos)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resumo de estoque por produto</returns>
    Task<IEnumerable<(Guid ProdutoId, decimal QuantidadeAtual, DateTime? UltimaMovimentacao)>> GetEstoqueResumoAsync(
        string tenantId, 
        IEnumerable<Guid>? produtoIds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos com estoque baixo
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de produtos com estoque baixo</returns>
    Task<IEnumerable<(Guid ProdutoId, string NomeProduto, decimal EstoqueAtual, decimal EstoqueMinimo)>> GetProdutosEstoqueBaixoAsync(
        string tenantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas de movimentação por período
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas de movimentação</returns>
    Task<Dictionary<TipoMovimentacao, int>> GetEstatisticasMovimentacaoAsync(
        string tenantId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    // Validações de integridade

    /// <summary>
    /// Verifica se produto tem estoque suficiente
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="quantidade">Quantidade necessária</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se tem estoque suficiente</returns>
    Task<bool> VerificarEstoqueSuficienteAsync(Guid produtoId, decimal quantidade, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula estoque atual do produto baseado nas movimentações
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Quantidade atual em estoque</returns>
    Task<decimal> CalcularEstoqueAtualAsync(Guid produtoId, string tenantId, CancellationToken cancellationToken = default);
}