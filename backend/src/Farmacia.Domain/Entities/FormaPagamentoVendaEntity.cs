using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma forma de pagamento utilizada em uma venda
/// Permite múltiplas formas de pagamento por venda com controle detalhado
/// </summary>
/// <remarks>
/// Esta entidade implementa controle completo de formas de pagamento brasileiras
/// incluindo dinheiro, cartões, PIX, boleto e parcelamentos com validações específicas
/// para o mercado farmacêutico nacional.
/// </remarks>
public class FormaPagamentoVendaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity
{
    /// <summary>
    /// Identificador único da forma de pagamento da venda
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) - isolamento automático
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da venda
    /// </summary>
    [Required]
    public Guid VendaId { get; set; }

    /// <summary>
    /// Venda relacionada
    /// </summary>
    public VendaEntity Venda { get; set; } = null!;

    /// <summary>
    /// Tipo da forma de pagamento
    /// </summary>
    [Required]
    public TipoFormaPagamento Tipo { get; set; }

    /// <summary>
    /// Sequência da forma de pagamento na venda (1, 2, 3...)
    /// </summary>
    [Range(1, 99, ErrorMessage = "Sequência deve ser entre 1 e 99")]
    public int Sequencia { get; set; } = 1;

    /// <summary>
    /// Valor pago com esta forma de pagamento
    /// </summary>
    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Valor deve ser positivo")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Número de parcelas (para cartões e crediários)
    /// </summary>
    [Range(1, 48, ErrorMessage = "Parcelas deve ser entre 1 e 48")]
    public int Parcelas { get; set; } = 1;

    /// <summary>
    /// Valor de cada parcela
    /// </summary>
    [Range(0.01, 999999.99, ErrorMessage = "Valor da parcela deve ser positivo")]
    public decimal? ValorParcela { get; set; }

    /// <summary>
    /// Taxa ou juros aplicados (percentual)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Taxa deve estar entre 0 e 100%")]
    public decimal Taxa { get; set; } = 0;

    /// <summary>
    /// Valor da taxa em reais
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor da taxa deve ser positivo")]
    public decimal ValorTaxa { get; set; } = 0;

    /// <summary>
    /// Valor líquido recebido (valor - taxa)
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor líquido deve ser positivo")]
    public decimal ValorLiquido { get; set; }

    /// <summary>
    /// Status do pagamento
    /// </summary>
    [Required]
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    /// <summary>
    /// Data de vencimento (para boletos e crediários)
    /// </summary>
    public DateTime? DataVencimento { get; set; }

    /// <summary>
    /// Data da confirmação do pagamento
    /// </summary>
    public DateTime? DataConfirmacao { get; set; }

    /// <summary>
    /// Bandeira do cartão (Visa, Mastercard, Elo, etc.)
    /// </summary>
    [StringLength(50)]
    public string? BandeiraCartao { get; set; }

    /// <summary>
    /// Últimos 4 dígitos do cartão (para segurança)
    /// </summary>
    [StringLength(4)]
    public string? UltimosDigitosCartao { get; set; }

    /// <summary>
    /// Número de autorização da transação
    /// </summary>
    [StringLength(50)]
    public string? NumeroAutorizacao { get; set; }

    /// <summary>
    /// Código de autorização da transação
    /// </summary>
    [StringLength(50)]
    public string? CodigoAutorizacao { get; set; }

    /// <summary>
    /// NSU (Número Sequencial Único) da transação
    /// </summary>
    [StringLength(50)]
    public string? NSU { get; set; }

    /// <summary>
    /// TID (Transaction ID) da transação
    /// </summary>
    [StringLength(50)]
    public string? TID { get; set; }

    /// <summary>
    /// Chave PIX (para pagamentos PIX)
    /// </summary>
    [StringLength(200)]
    public string? ChavePIX { get; set; }

    /// <summary>
    /// ID da transação PIX
    /// </summary>
    [StringLength(100)]
    public string? TransacaoPIXId { get; set; }

    /// <summary>
    /// Código de barras do boleto
    /// </summary>
    [StringLength(100)]
    public string? CodigoBarrasBoleto { get; set; }

    /// <summary>
    /// Linha digitável do boleto
    /// </summary>
    [StringLength(100)]
    public string? LinhaDigitavelBoleto { get; set; }

    /// <summary>
    /// URL do boleto para download
    /// </summary>
    [StringLength(500)]
    public string? UrlBoleto { get; set; }

    /// <summary>
    /// Adquirente da transação (Stone, Cielo, Rede, etc.)
    /// </summary>
    [StringLength(50)]
    public string? Adquirente { get; set; }

    /// <summary>
    /// Número do estabelecimento comercial
    /// </summary>
    [StringLength(50)]
    public string? EstabelecimentoComercial { get; set; }

    /// <summary>
    /// Terminal utilizado na transação
    /// </summary>
    [StringLength(50)]
    public string? Terminal { get; set; }

    /// <summary>
    /// Comprovante de pagamento (texto ou URL)
    /// </summary>
    [StringLength(2000)]
    public string? Comprovante { get; set; }

    /// <summary>
    /// Data prevista de compensação (para cartões)
    /// </summary>
    public DateTime? DataPrevistaCompensacao { get; set; }

    /// <summary>
    /// Data efetiva de compensação
    /// </summary>
    public DateTime? DataCompensacao { get; set; }

    /// <summary>
    /// Observações sobre a forma de pagamento
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Dados extras em JSON (para integrações específicas)
    /// </summary>
    [StringLength(2000)]
    public string? DadosExtras { get; set; }

    /// <summary>
    /// Motivo do cancelamento ou estorno
    /// </summary>
    [StringLength(500)]
    public string? MotivoCancelamento { get; set; }

    /// <summary>
    /// Data do cancelamento ou estorno
    /// </summary>
    public DateTime? DataCancelamento { get; set; }

    /// <summary>
    /// Valor estornado
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor estornado deve ser positivo")]
    public decimal? ValorEstornado { get; set; }

    // Propriedades de soft delete
    /// <summary>
    /// Indica se o registro está marcado como deletado
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão lógica
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Motivo da exclusão
    /// </summary>
    [StringLength(500)]
    public string? DeleteReason { get; set; }

    // Propriedades de auditoria
    /// <summary>
    /// Data de criação do registro
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou o registro
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Versão do registro para controle de concorrência
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Métodos de negócio

    /// <summary>
    /// Calcula o valor líquido (valor - taxa)
    /// </summary>
    public void CalcularValorLiquido()
    {
        ValorLiquido = Valor - ValorTaxa;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula a taxa em reais baseada no percentual
    /// </summary>
    public void CalcularTaxaReais()
    {
        if (Taxa > 0)
        {
            ValorTaxa = Valor * (Taxa / 100);
            CalcularValorLiquido();
        }
    }

    /// <summary>
    /// Calcula o valor da parcela
    /// </summary>
    public void CalcularValorParcela()
    {
        if (Parcelas > 0)
        {
            ValorParcela = Valor / Parcelas;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Confirma o pagamento
    /// </summary>
    /// <param name="usuarioId">Usuário que está confirmando</param>
    /// <param name="codigoAutorizacao">Código de autorização</param>
    public void Confirmar(Guid usuarioId, string? codigoAutorizacao = null)
    {
        Status = StatusPagamento.Confirmado;
        DataConfirmacao = DateTime.UtcNow;
        CodigoAutorizacao = codigoAutorizacao;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela o pagamento
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="usuarioId">Usuário que está cancelando</param>
    public void Cancelar(string motivo, Guid usuarioId)
    {
        Status = StatusPagamento.Cancelado;
        MotivoCancelamento = motivo;
        DataCancelamento = DateTime.UtcNow;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Estorna o pagamento (total ou parcial)
    /// </summary>
    /// <param name="valorEstorno">Valor a ser estornado</param>
    /// <param name="motivo">Motivo do estorno</param>
    /// <param name="usuarioId">Usuário que está estornando</param>
    public void Estornar(decimal valorEstorno, string motivo, Guid usuarioId)
    {
        if (valorEstorno <= 0 || valorEstorno > Valor)
            throw new ArgumentException("Valor de estorno inválido");

        ValorEstornado = (ValorEstornado ?? 0) + valorEstorno;
        MotivoCancelamento = motivo;
        DataCancelamento = DateTime.UtcNow;
        
        // Se estorno total, muda status para cancelado
        if (ValorEstornado >= Valor)
        {
            Status = StatusPagamento.Cancelado;
        }
        else
        {
            Status = StatusPagamento.EstornadoParcial;
        }

        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se é um pagamento à vista
    /// </summary>
    /// <returns>True se é à vista</returns>
    public bool IsAVista()
    {
        return Parcelas <= 1 && Tipo != TipoFormaPagamento.Crediario;
    }

    /// <summary>
    /// Verifica se é um pagamento parcelado
    /// </summary>
    /// <returns>True se é parcelado</returns>
    public bool IsParcelado()
    {
        return Parcelas > 1 || Tipo == TipoFormaPagamento.Crediario;
    }

    /// <summary>
    /// Verifica se é um pagamento digital
    /// </summary>
    /// <returns>True se é digital</returns>
    public bool IsDigital()
    {
        return Tipo == TipoFormaPagamento.PIX ||
               Tipo == TipoFormaPagamento.TransferenciaBancaria ||
               Tipo == TipoFormaPagamento.Boleto ||
               Tipo == TipoFormaPagamento.CartaoCredito ||
               Tipo == TipoFormaPagamento.CartaoDebito;
    }

    /// <summary>
    /// Verifica se requer confirmação manual
    /// </summary>
    /// <returns>True se requer confirmação</returns>
    public bool RequerConfirmacaoManual()
    {
        return Tipo == TipoFormaPagamento.Boleto ||
               Tipo == TipoFormaPagamento.TransferenciaBancaria ||
               Tipo == TipoFormaPagamento.Crediario ||
               Tipo == TipoFormaPagamento.Cheque;
    }

    /// <summary>
    /// Obtém descrição da forma de pagamento para exibição
    /// </summary>
    /// <returns>Descrição formatada</returns>
    public string ObterDescricao()
    {
        var descricao = Tipo.ToString();

        if (!string.IsNullOrEmpty(BandeiraCartao))
        {
            descricao += $" {BandeiraCartao}";
        }

        if (!string.IsNullOrEmpty(UltimosDigitosCartao))
        {
            descricao += $" ****{UltimosDigitosCartao}";
        }

        if (IsParcelado())
        {
            descricao += $" ({Parcelas}x)";
        }

        return descricao;
    }

    /// <summary>
    /// Obtém informações de compensação
    /// </summary>
    /// <returns>String com informações de compensação</returns>
    public string ObterInformacoesCompensacao()
    {
        if (DataCompensacao.HasValue)
        {
            return $"Compensado em {DataCompensacao:dd/MM/yyyy}";
        }

        if (DataPrevistaCompensacao.HasValue)
        {
            var dias = (DataPrevistaCompensacao.Value - DateTime.Now).Days;
            if (dias == 0)
                return "Compensação hoje";
            if (dias == 1)
                return "Compensação amanhã";
            if (dias > 0)
                return $"Compensação em {dias} dias";
            
            return $"Vencida há {Math.Abs(dias)} dias";
        }

        return IsAVista() && IsDigital() ? "Compensação imediata" : "Sem previsão";
    }

    /// <summary>
    /// Verifica se o pagamento está em atraso
    /// </summary>
    /// <returns>True se em atraso</returns>
    public bool IsEmAtraso()
    {
        return DataVencimento.HasValue && 
               DateTime.Now.Date > DataVencimento.Value.Date && 
               Status == StatusPagamento.Pendente;
    }

    /// <summary>
    /// Calcula dias de atraso
    /// </summary>
    /// <returns>Número de dias em atraso</returns>
    public int DiasAtraso()
    {
        if (!IsEmAtraso())
            return 0;

        return (DateTime.Now.Date - DataVencimento!.Value.Date).Days;
    }

    /// <summary>
    /// Obtém código de identificação único da transação
    /// </summary>
    /// <returns>Código único</returns>
    public string? ObterCodigoTransacao()
    {
        return CodigoAutorizacao ?? TransacaoPIXId ?? NSU ?? TID;
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da forma de pagamento</returns>
    public override string ToString()
    {
        return $"{ObterDescricao()} - R$ {Valor:F2} ({Status})";
    }
}

/// <summary>
/// Tipos de forma de pagamento suportados
/// </summary>
public enum TipoFormaPagamento
{
    /// <summary>
    /// Pagamento em dinheiro
    /// </summary>
    Dinheiro = 0,

    /// <summary>
    /// Cartão de débito
    /// </summary>
    CartaoDebito = 1,

    /// <summary>
    /// Cartão de crédito à vista
    /// </summary>
    CartaoCredito = 2,

    /// <summary>
    /// Cartão de crédito parcelado
    /// </summary>
    CartaoCreditoParcelado = 3,

    /// <summary>
    /// PIX
    /// </summary>
    PIX = 4,

    /// <summary>
    /// Transferência bancária
    /// </summary>
    TransferenciaBancaria = 5,

    /// <summary>
    /// Boleto bancário
    /// </summary>
    Boleto = 6,

    /// <summary>
    /// Crediário da farmácia
    /// </summary>
    Crediario = 7,

    /// <summary>
    /// Cheque
    /// </summary>
    Cheque = 8,

    /// <summary>
    /// Cartão de loja
    /// </summary>
    CartaoLoja = 9,

    /// <summary>
    /// Convênio/plano de saúde
    /// </summary>
    Convenio = 10,

    /// <summary>
    /// Vale alimentação
    /// </summary>
    ValeAlimentacao = 11,

    /// <summary>
    /// Vale refeição
    /// </summary>
    ValeRefeicao = 12,

    /// <summary>
    /// Cashback/crédito da loja
    /// </summary>
    CreditoLoja = 13
}

/// <summary>
/// Status possíveis de um pagamento
/// </summary>
public enum StatusPagamento
{
    /// <summary>
    /// Pagamento pendente
    /// </summary>
    Pendente = 0,

    /// <summary>
    /// Pagamento confirmado
    /// </summary>
    Confirmado = 1,

    /// <summary>
    /// Pagamento cancelado
    /// </summary>
    Cancelado = 2,

    /// <summary>
    /// Pagamento estornado parcialmente
    /// </summary>
    EstornadoParcial = 3,

    /// <summary>
    /// Pagamento em processamento
    /// </summary>
    Processando = 4,

    /// <summary>
    /// Pagamento rejeitado
    /// </summary>
    Rejeitado = 5,

    /// <summary>
    /// Pagamento compensado
    /// </summary>
    Compensado = 6
}