using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de movimentações de estoque
/// Fornece operações específicas para controle de estoque multi-tenant
/// </summary>
public interface IEstoqueRepository : IBaseRepository<MovimentacaoEstoqueEntity>
{
    /// <summary>
    /// Obtém todas as movimentações de um produto específico
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de movimentações do produto</returns>
    Task<IEnumerable<MovimentacaoEstoqueEntity>> GetByProdutoIdAsync(Guid produtoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém movimentações por tipo
    /// </summary>
    /// <param name="tipoMovimentacaoId">ID do tipo de movimentação</param>
    /// <param name="dataInicio">Data de início do filtro</param>
    /// <param name="dataFim">Data de fim do filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de movimentações filtradas</returns>
    Task<IEnumerable<MovimentacaoEstoqueEntity>> GetByTipoAsync(
        Guid tipoMovimentacaoId, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula o saldo atual de um produto baseado nas movimentações
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo atual do produto</returns>
    Task<decimal> CalcularSaldoAtualAsync(Guid produtoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém as últimas movimentações do tenant
    /// </summary>
    /// <param name="limite">Número máximo de registros</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Últimas movimentações</returns>
    Task<IEnumerable<MovimentacaoEstoqueEntity>> GetUltimasMovimentacoesAsync(int limite = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    /// <param name="quantidadeMinima">Quantidade mínima para considerar estoque baixo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de produtos com estoque baixo</returns>
    Task<IEnumerable<MovimentacaoEstoqueEntity>> GetProdutosEstoqueBaixoAsync(decimal quantidadeMinima = 10, CancellationToken cancellationToken = default);
}