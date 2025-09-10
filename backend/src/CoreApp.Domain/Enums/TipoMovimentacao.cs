/// <summary>
/// Enum que define os tipos de movimentação de estoque disponíveis no sistema CoreApp
/// Suporta operações comerciais brasileiras conforme regulamentações
/// </summary>
namespace CoreApp.Domain.Enums;

/// <summary>
/// Tipos de movimentação de estoque para comércios brasileiros
/// </summary>
public enum TipoMovimentacao
{
    /// <summary>
    /// Entrada de produtos no estoque (compras, devoluções de clientes)
    /// </summary>
    ENTRADA = 1,

    /// <summary>
    /// Saída de produtos do estoque (vendas, devoluções para fornecedores)
    /// </summary>
    SAIDA = 2,

    /// <summary>
    /// Ajuste de estoque (inventário, correções)
    /// </summary>
    AJUSTE = 3,

    /// <summary>
    /// Perda de produtos (quebra, furto, deterioração)
    /// </summary>
    PERDA = 4,

    /// <summary>
    /// Produtos vencidos removidos do estoque
    /// </summary>
    VENCIMENTO = 5,

    /// <summary>
    /// Transferência entre filiais ou locais
    /// </summary>
    TRANSFERENCIA = 6
}