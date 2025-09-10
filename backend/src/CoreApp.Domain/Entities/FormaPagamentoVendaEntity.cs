using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;
using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Entities.Configuration;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade que representa as formas de pagamento utilizadas em uma venda específica
/// Tabela de associação entre Venda e FormaPagamento com valor pago
/// </summary>
public class FormaPagamentoVendaEntity : BaseEntity, ITenantEntity
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
    /// ID da venda a que pertence este pagamento
    /// </summary>
    [Required]
    public Guid VendaId { get; set; }

    /// <summary>
    /// Navegação para a venda
    /// </summary>
    public VendaEntity? Venda { get; set; }

    /// <summary>
    /// ID da forma de pagamento utilizada
    /// </summary>
    [Required]
    public Guid FormaPagamentoId { get; set; }

    /// <summary>
    /// Navegação para forma de pagamento
    /// </summary>
    public FormaPagamentoEntity? FormaPagamento { get; set; }

    /// <summary>
    /// Valor pago com esta forma de pagamento
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Número de parcelas (para cartão de crédito)
    /// </summary>
    public int? NumeroParcelas { get; set; }

    /// <summary>
    /// Dados adicionais específicos do pagamento (JSON)
    /// Exemplo: {"numeroCartao": "****1234", "nsu": "123456", "autorizacao": "ABC123"}
    /// </summary>
    public string? DadosAdicionais { get; set; }

    /// <summary>
    /// Status do pagamento (PENDENTE, APROVADO, REJEITADO, CANCELADO)
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "PENDENTE";

    /// <summary>
    /// ID da transação externa (gateway de pagamento)
    /// </summary>
    [StringLength(100)]
    public string? TransacaoExternaId { get; set; }

    /// <summary>
    /// Data e hora do processamento do pagamento
    /// </summary>
    public DateTime? DataProcessamento { get; set; }

    /// <summary>
    /// Observações sobre o pagamento
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Aprova o pagamento
    /// </summary>
    public void Aprovar()
    {
        Status = "APROVADO";
        DataProcessamento = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejeita o pagamento
    /// </summary>
    /// <param name="motivo">Motivo da rejeição</param>
    public void Rejeitar(string? motivo = null)
    {
        Status = "REJEITADO";
        DataProcessamento = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(motivo))
        {
            Observacoes = motivo;
        }
    }

    /// <summary>
    /// Cancela o pagamento
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    public void Cancelar(string? motivo = null)
    {
        Status = "CANCELADO";
        DataProcessamento = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(motivo))
        {
            Observacoes = motivo;
        }
    }

    /// <summary>
    /// Verifica se o pagamento foi aprovado
    /// </summary>
    public bool IsAprovado()
    {
        return Status == "APROVADO";
    }
}
