using CoreApp.Domain.Entities;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de vendas
/// Fornece operações específicas para gestão de vendas multi-tenant
/// </summary>
public interface IVendaRepository : IBaseRepository<VendaEntity>
{
    /// <summary>
    /// Obtém vendas por período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas no período</returns>
    Task<IEnumerable<VendaEntity>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém vendas por cliente
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas do cliente</returns>
    Task<IEnumerable<VendaEntity>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém vendas por vendedor
    /// </summary>
    /// <param name="vendedorId">ID do vendedor</param>
    /// <param name="dataInicio">Data de início (opcional)</param>
    /// <param name="dataFim">Data de fim (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vendas do vendedor</returns>
    Task<IEnumerable<VendaEntity>> GetByVendedorIdAsync(Guid vendedorId, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula o total de vendas em um período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Valor total das vendas</returns>
    Task<decimal> CalcularTotalVendasPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém as vendas mais recentes
    /// </summary>
    /// <param name="limite">Número máximo de registros</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Vendas mais recentes</returns>
    Task<IEnumerable<VendaEntity>> GetVendasRecentesAsync(int limite = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém itens de venda por venda
    /// </summary>
    /// <param name="vendaId">ID da venda</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de itens da venda</returns>
    Task<IEnumerable<ItemVendaEntity>> GetItensVendaAsync(Guid vendaId, CancellationToken cancellationToken = default);
}