using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade Venda do sistema CoreApp multi-tenant
/// Implementa compliance fiscal brasileiro e controles comerciais
/// </summary>
public class VendaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Número sequencial da venda no tenant
    /// </summary>
    public long NumeroVenda { get; set; }

    /// <summary>
    /// Cliente da venda (pode ser opcional para vendas balcão)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Navegação para cliente
    /// </summary>
    public ClienteEntity? Cliente { get; set; }

    /// <summary>
    /// Usuário que realizou a venda
    /// </summary>
    [Required]
    public Guid UsuarioVendaId { get; set; }

    /// <summary>
    /// Navegação para usuário vendedor
    /// </summary>
    public UsuarioEntity? UsuarioVenda { get; set; }

    /// <summary>
    /// Data e hora da venda
    /// </summary>
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Valor total dos produtos (sem desconto)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorProdutos { get; set; }

    /// <summary>
    /// Valor total de desconto aplicado
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Valor dos impostos (se aplicável)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorImpostos { get; set; }

    /// <summary>
    /// Valor total da venda
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Valor efetivamente pago
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorPago { get; set; }

    /// <summary>
    /// Valor do troco dado
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTroco { get; set; }

    /// <summary>
    /// Status da venda (PENDENTE, FINALIZADA, CANCELADA, DEVOLVIDA)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDENTE";

    /// <summary>
    /// Tipo da venda (BALCAO, DELIVERY, ONLINE, etc.)
    /// </summary>
    [StringLength(20)]
    public string TipoVenda { get; set; } = "BALCAO";

    /// <summary>
    /// Observações sobre a venda
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Número da NFCe se emitida
    /// </summary>
    [StringLength(50)]
    public string? NumeroNFCe { get; set; }

    /// <summary>
    /// Chave da NFCe se emitida
    /// </summary>
    [StringLength(100)]
    public string? ChaveNFCe { get; set; }

    /// <summary>
    /// Data de cancelamento se aplicável
    /// </summary>
    public DateTime? DataCancelamento { get; set; }

    /// <summary>
    /// Motivo do cancelamento se aplicável
    /// </summary>
    [StringLength(500)]
    public string? MotivoCancelamento { get; set; }

    /// <summary>
    /// Lista de itens da venda
    /// </summary>
    public virtual ICollection<ItemVendaEntity> Itens { get; set; } = new List<ItemVendaEntity>();

    /// <summary>
    /// Lista de formas de pagamento utilizadas
    /// </summary>
    public virtual ICollection<FormaPagamentoVendaEntity> FormasPagamento { get; set; } = new List<FormaPagamentoVendaEntity>();

    // Implementação de ISoftDeletableEntity
    public bool Excluido { get; set; } = false;
    public DateTime? DataExclusao { get; set; }
    [StringLength(100)]
    public string? UsuarioExclusao { get; set; }
    [StringLength(500)]
    public string? MotivoExclusao { get; set; }

    public void MarkAsDeleted(string? usuarioId = null, string? motivo = null)
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        UsuarioExclusao = usuarioId;
        MotivoExclusao = motivo;
    }

    public void Restore()
    {
        Excluido = false;
        DataExclusao = null;
        UsuarioExclusao = null;
        MotivoExclusao = null;
    }

    // Implementação de IArchivableEntity
    public bool Arquivado { get; set; } = false;
    public DateTime? DataArquivamento { get; set; }
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a venda foi finalizada
    /// </summary>
    public bool IsRealizada()
    {
        return Status == "FINALIZADA";
    }

    /// <summary>
    /// Verifica se a venda foi cancelada
    /// </summary>
    public bool IsCancelada()
    {
        return Status == "CANCELADA";
    }

    /// <summary>
    /// Verifica se a venda está pendente
    /// </summary>
    public bool IsPendente()
    {
        return Status == "PENDENTE";
    }

    /// <summary>
    /// Calcula o valor total da venda baseado nos itens
    /// </summary>
    public void CalcularValorTotal()
    {
        ValorProdutos = Itens.Sum(i => i.ValorTotal);
        ValorTotal = ValorProdutos - ValorDesconto + ValorImpostos;
    }

    /// <summary>
    /// Finaliza a venda
    /// </summary>
    public void Finalizar()
    {
        if (Status != "PENDENTE")
            throw new InvalidOperationException("Apenas vendas pendentes podem ser finalizadas");

        CalcularValorTotal();
        Status = "FINALIZADA";
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Cancela a venda
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="usuarioId">Usuário que está cancelando</param>
    public void Cancelar(string motivo, string? usuarioId = null)
    {
        if (Status == "CANCELADA")
            throw new InvalidOperationException("Venda já está cancelada");

        Status = "CANCELADA";
        DataCancelamento = DateTime.UtcNow;
        MotivoCancelamento = motivo;
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Adiciona uma forma de pagamento à venda
    /// </summary>
    public void AdicionarFormaPagamento(FormaPagamentoVendaEntity formaPagamento)
    {
        FormasPagamento.Add(formaPagamento);
        ValorPago = FormasPagamento.Sum(fp => fp.Valor);
        ValorTroco = ValorPago > ValorTotal ? ValorPago - ValorTotal : 0;
    }

    /// <summary>
    /// Verifica se o pagamento está completo
    /// </summary>
    public bool IsPagamentoCompleto()
    {
        return ValorPago >= ValorTotal;
    }
}