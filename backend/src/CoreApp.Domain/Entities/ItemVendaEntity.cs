using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade ItemVenda para sistema CoreApp multi-tenant
/// Representa um item individual dentro de uma venda
/// </summary>
public class ItemVendaEntity : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do tenant (loja/comércio) para isolamento de dados
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// ID da venda a que pertence este item
    /// </summary>
    [Required]
    public Guid VendaId { get; set; }

    /// <summary>
    /// Navegação para a venda
    /// </summary>
    public VendaEntity? Venda { get; set; }

    /// <summary>
    /// ID do produto vendido
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Navegação para o produto
    /// </summary>
    public ProdutoEntity? Produto { get; set; }

    /// <summary>
    /// Quantidade vendida
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Preço unitário no momento da venda
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Valor total do item (Quantidade * PrecoUnitario - Desconto)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Valor de desconto aplicado no item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Percentual de desconto aplicado
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal PercentualDesconto { get; set; }

    /// <summary>
    /// Nome do produto no momento da venda (para histórico)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string NomeProduto { get; set; } = string.Empty;

    /// <summary>
    /// Código do produto no momento da venda
    /// </summary>
    [StringLength(50)]
    public string? CodigoProduto { get; set; }

    /// <summary>
    /// Unidade de medida do produto
    /// </summary>
    [Required]
    [StringLength(10)]
    public string UnidadeMedida { get; set; } = "UN";

    /// <summary>
    /// Observações específicas do item
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Calcula o valor total do item baseado na quantidade e preço unitário
    /// </summary>
    public void CalcularValorTotal()
    {
        var valorBruto = Quantidade * PrecoUnitario;
        ValorTotal = valorBruto - ValorDesconto;
    }

    /// <summary>
    /// Aplica desconto por percentual
    /// </summary>
    /// <param name="percentual">Percentual de desconto (0-100)</param>
    public void AplicarDescontoPercentual(decimal percentual)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual deve estar entre 0 e 100");

        PercentualDesconto = percentual;
        var valorBruto = Quantidade * PrecoUnitario;
        ValorDesconto = valorBruto * (percentual / 100);
        CalcularValorTotal();
    }

    /// <summary>
    /// Aplica desconto por valor fixo
    /// </summary>
    /// <param name="valor">Valor do desconto</param>
    public void AplicarDescontoValor(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor do desconto não pode ser negativo");

        var valorBruto = Quantidade * PrecoUnitario;
        if (valor > valorBruto)
            throw new ArgumentException("Desconto não pode ser maior que o valor bruto");

        ValorDesconto = valor;
        PercentualDesconto = valorBruto > 0 ? (valor / valorBruto) * 100 : 0;
        CalcularValorTotal();
    }

    /// <summary>
    /// Atualiza a quantidade e recalcula o valor
    /// </summary>
    /// <param name="novaQuantidade">Nova quantidade</param>
    public void AtualizarQuantidade(decimal novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        Quantidade = novaQuantidade;
        CalcularValorTotal();
    }
}
