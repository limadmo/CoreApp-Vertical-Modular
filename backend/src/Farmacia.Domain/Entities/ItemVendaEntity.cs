using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um item de venda no sistema farmacêutico brasileiro
/// Controla produtos vendidos com rastreabilidade de lotes e compliance ANVISA
/// </summary>
/// <remarks>
/// Esta entidade armazena detalhes de cada produto vendido em uma venda,
/// incluindo preços, descontos, lotes e informações específicas para medicamentos
/// controlados conforme regulamentações brasileiras.
/// </remarks>
public class ItemVendaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity
{
    /// <summary>
    /// Identificador único do item de venda
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
    /// Identificador da venda à qual o item pertence
    /// </summary>
    [Required]
    public Guid VendaId { get; set; }

    /// <summary>
    /// Venda relacionada ao item
    /// </summary>
    public VendaEntity Venda { get; set; } = null!;

    /// <summary>
    /// Identificador do produto vendido
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Produto vendido
    /// </summary>
    public ProdutoEntity Produto { get; set; } = null!;

    /// <summary>
    /// Identificador do lote específico vendido (obrigatório para medicamentos controlados)
    /// </summary>
    public Guid? LoteId { get; set; }

    /// <summary>
    /// Lote específico do produto vendido
    /// </summary>
    public LoteEntity? Lote { get; set; }

    /// <summary>
    /// Sequência do item na venda (1, 2, 3...)
    /// </summary>
    [Range(1, 9999, ErrorMessage = "Sequência deve ser entre 1 e 9999")]
    public int Sequencia { get; set; } = 1;

    /// <summary>
    /// Quantidade vendida
    /// </summary>
    [Required]
    [Range(0.001, 99999.99, ErrorMessage = "Quantidade deve ser positiva")]
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Unidade de medida do produto vendido
    /// </summary>
    [StringLength(20)]
    public string UnidadeMedida { get; set; } = "unidade";

    /// <summary>
    /// Preço unitário no momento da venda (pode ser diferente do preço atual)
    /// </summary>
    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Preço unitário deve ser positivo")]
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Valor total do item sem desconto (quantidade × preço unitário)
    /// </summary>
    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Valor bruto deve ser positivo")]
    public decimal ValorBruto { get; set; }

    /// <summary>
    /// Percentual de desconto aplicado ao item
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; } = 0;

    /// <summary>
    /// Valor de desconto aplicado ao item
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "Valor de desconto deve ser positivo")]
    public decimal ValorDesconto { get; set; } = 0;

    /// <summary>
    /// Valor total do item com desconto aplicado
    /// </summary>
    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Valor total deve ser positivo")]
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Custo unitário de aquisição do produto (para cálculo da margem)
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "Custo unitário deve ser positivo")]
    public decimal? CustoUnitario { get; set; }

    /// <summary>
    /// Valor total de custo do item
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "Custo total deve ser positivo")]
    public decimal? CustoTotal { get; set; }

    /// <summary>
    /// Margem de lucro do item em percentual
    /// </summary>
    [Range(-999, 999, ErrorMessage = "Margem deve estar entre -999% e 999%")]
    public decimal? MargemLucro { get; set; }

    /// <summary>
    /// Indica se o item é um medicamento controlado
    /// </summary>
    public bool IsMedicamentoControlado { get; set; } = false;

    /// <summary>
    /// Classificação ANVISA do medicamento no momento da venda
    /// </summary>
    [StringLength(50)]
    public string? ClassificacaoAnvisa { get; set; }

    /// <summary>
    /// Número da receita médica específica para este item (se controlado)
    /// </summary>
    [StringLength(100)]
    public string? NumeroReceitaItem { get; set; }

    /// <summary>
    /// Quantidade prescrita na receita para este medicamento
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "Quantidade prescrita deve ser positiva")]
    public decimal? QuantidadePrescrita { get; set; }

    /// <summary>
    /// Dosagem específica do medicamento vendido
    /// </summary>
    [StringLength(100)]
    public string? Dosagem { get; set; }

    /// <summary>
    /// Instruções de uso do medicamento
    /// </summary>
    [StringLength(500)]
    public string? InstrucoesUso { get; set; }

    /// <summary>
    /// Observações específicas do item
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Data de validade do lote no momento da venda
    /// </summary>
    public DateTime? DataValidadeLote { get; set; }

    /// <summary>
    /// Número do lote no momento da venda (para histórico)
    /// </summary>
    [StringLength(50)]
    public string? NumeroLoteVenda { get; set; }

    /// <summary>
    /// Código de barras do produto no momento da venda
    /// </summary>
    [StringLength(50)]
    public string? CodigoBarrasVenda { get; set; }

    /// <summary>
    /// Código NCM (Nomenclatura Comum do Mercosul) para impostos
    /// </summary>
    [StringLength(20)]
    public string? CodigoNCM { get; set; }

    /// <summary>
    /// Alíquota de ICMS aplicada ao item
    /// </summary>
    [Range(0, 30, ErrorMessage = "Alíquota ICMS deve estar entre 0 e 30%")]
    public decimal AliquotaICMS { get; set; } = 0;

    /// <summary>
    /// Valor de ICMS do item
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "ICMS deve ser positivo")]
    public decimal ValorICMS { get; set; } = 0;

    /// <summary>
    /// Alíquota de PIS/COFINS aplicada ao item
    /// </summary>
    [Range(0, 15, ErrorMessage = "Alíquota PIS/COFINS deve estar entre 0 e 15%")]
    public decimal AliquotaPISCOFINS { get; set; } = 0;

    /// <summary>
    /// Valor de PIS/COFINS do item
    /// </summary>
    [Range(0, 99999.99, ErrorMessage = "PIS/COFINS deve ser positivo")]
    public decimal ValorPISCOFINS { get; set; } = 0;

    /// <summary>
    /// CST (Código de Situação Tributária) do item
    /// </summary>
    [StringLength(10)]
    public string? CST { get; set; }

    /// <summary>
    /// CSOSN (Código de Situação da Operação no Simples Nacional)
    /// </summary>
    [StringLength(10)]
    public string? CSOSN { get; set; }

    /// <summary>
    /// Identificador da promoção aplicada ao item (se houver)
    /// </summary>
    public Guid? PromocaoItemId { get; set; }

    /// <summary>
    /// Promoção específica aplicada ao item
    /// </summary>
    public PromocaoEntity? PromocaoItem { get; set; }

    /// <summary>
    /// Indica se o item foi substituído por genérico/similar
    /// </summary>
    public bool FoiSubstituido { get; set; } = false;

    /// <summary>
    /// ID do produto original (se foi feita substituição)
    /// </summary>
    public Guid? ProdutoOriginalId { get; set; }

    /// <summary>
    /// Produto original antes da substituição
    /// </summary>
    public ProdutoEntity? ProdutoOriginal { get; set; }

    /// <summary>
    /// Motivo da substituição do produto
    /// </summary>
    [StringLength(500)]
    public string? MotivoSubstituicao { get; set; }

    /// <summary>
    /// Indica se o cliente foi orientado sobre o uso do medicamento
    /// </summary>
    public bool ClienteOrientado { get; set; } = false;

    /// <summary>
    /// Usuário farmacêutico que orientou o cliente
    /// </summary>
    public Guid? FarmaceuticoOrientadorId { get; set; }

    /// <summary>
    /// Farmacêutico responsável pela orientação
    /// </summary>
    public UsuarioEntity? FarmaceuticoOrientador { get; set; }

    /// <summary>
    /// Data e hora da orientação farmacêutica
    /// </summary>
    public DateTime? DataOrientacao { get; set; }

    /// <summary>
    /// Detalhes da orientação farmacêutica
    /// </summary>
    [StringLength(1000)]
    public string? DetalhesOrientacao { get; set; }

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
    /// Calcula o valor bruto do item (quantidade × preço unitário)
    /// </summary>
    public void CalcularValorBruto()
    {
        ValorBruto = Quantidade * PrecoUnitario;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica desconto percentual ao item
    /// </summary>
    /// <param name="percentual">Percentual de desconto</param>
    /// <param name="usuarioId">Usuário que aplicou o desconto</param>
    public void AplicarDesconto(decimal percentual, Guid usuarioId)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100");

        PercentualDesconto = percentual;
        ValorDesconto = ValorBruto * (percentual / 100);
        CalcularValorTotal();
        
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica desconto em valor fixo ao item
    /// </summary>
    /// <param name="valorDesconto">Valor de desconto</param>
    /// <param name="usuarioId">Usuário que aplicou o desconto</param>
    public void AplicarDescontoValor(decimal valorDesconto, Guid usuarioId)
    {
        if (valorDesconto < 0 || valorDesconto > ValorBruto)
            throw new ArgumentException("Valor de desconto inválido");

        ValorDesconto = valorDesconto;
        PercentualDesconto = ValorBruto > 0 ? (valorDesconto / ValorBruto) * 100 : 0;
        CalcularValorTotal();
        
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula o valor total do item (bruto - desconto)
    /// </summary>
    public void CalcularValorTotal()
    {
        ValorTotal = ValorBruto - ValorDesconto;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula o custo total do item baseado na quantidade
    /// </summary>
    public void CalcularCustoTotal()
    {
        if (CustoUnitario.HasValue)
        {
            CustoTotal = CustoUnitario.Value * Quantidade;
            CalcularMargemLucro();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula a margem de lucro do item
    /// </summary>
    public void CalcularMargemLucro()
    {
        if (CustoTotal.HasValue && CustoTotal > 0)
        {
            var lucroLiquido = ValorTotal - CustoTotal.Value;
            MargemLucro = (lucroLiquido / CustoTotal.Value) * 100;
        }
        else
        {
            MargemLucro = null;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define as alíquotas de impostos e calcula os valores
    /// </summary>
    /// <param name="icms">Alíquota ICMS</param>
    /// <param name="pisCofins">Alíquota PIS/COFINS</param>
    public void DefinirAliquotas(decimal icms, decimal pisCofins)
    {
        AliquotaICMS = icms;
        AliquotaPISCOFINS = pisCofins;
        
        ValorICMS = ValorTotal * (icms / 100);
        ValorPISCOFINS = ValorTotal * (pisCofins / 100);
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca o item como medicamento controlado
    /// </summary>
    /// <param name="classificacaoAnvisa">Classificação ANVISA</param>
    /// <param name="numeroReceita">Número da receita</param>
    public void MarcarComoControlado(string classificacaoAnvisa, string numeroReceita)
    {
        IsMedicamentoControlado = true;
        ClassificacaoAnvisa = classificacaoAnvisa;
        NumeroReceitaItem = numeroReceita;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra orientação farmacêutica
    /// </summary>
    /// <param name="farmaceuticoId">ID do farmacêutico</param>
    /// <param name="detalhesOrientacao">Detalhes da orientação</param>
    public void RegistrarOrientacao(Guid farmaceuticoId, string? detalhesOrientacao = null)
    {
        ClienteOrientado = true;
        FarmaceuticoOrientadorId = farmaceuticoId;
        DataOrientacao = DateTime.UtcNow;
        DetalhesOrientacao = detalhesOrientacao;
        UpdatedBy = farmaceuticoId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra substituição de produto
    /// </summary>
    /// <param name="produtoOriginalId">ID do produto original</param>
    /// <param name="motivo">Motivo da substituição</param>
    /// <param name="usuarioId">Usuário que fez a substituição</param>
    public void RegistrarSubstituicao(Guid produtoOriginalId, string motivo, Guid usuarioId)
    {
        FoiSubstituido = true;
        ProdutoOriginalId = produtoOriginalId;
        MotivoSubstituicao = motivo;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza informações do lote no momento da venda
    /// </summary>
    /// <param name="lote">Lote do produto</param>
    public void AtualizarInformacoesLote(LoteEntity lote)
    {
        LoteId = lote.Id;
        NumeroLoteVenda = lote.NumeroLote;
        DataValidadeLote = lote.DataValidade;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o item necessita de receita médica
    /// </summary>
    /// <returns>True se necessita receita</returns>
    public bool NecessitaReceita()
    {
        return IsMedicamentoControlado || 
               (Produto?.IsControlado() == true) ||
               (Produto?.ClassificacaoAnvisa != null && Produto.ClassificacaoAnvisa.ToString().Contains("SUJEITO"));
    }

    /// <summary>
    /// Verifica se o item está próximo do vencimento (menos de 30 dias)
    /// </summary>
    /// <returns>True se próximo do vencimento</returns>
    public bool IsProximoVencimento()
    {
        if (!DataValidadeLote.HasValue)
            return false;

        return (DataValidadeLote.Value - DateTime.Now).TotalDays <= 30;
    }

    /// <summary>
    /// Verifica se o produto está vencido
    /// </summary>
    /// <returns>True se vencido</returns>
    public bool IsVencido()
    {
        if (!DataValidadeLote.HasValue)
            return false;

        return DateTime.Now.Date > DataValidadeLote.Value.Date;
    }

    /// <summary>
    /// Obtém o valor unitário com desconto
    /// </summary>
    /// <returns>Valor unitário final</returns>
    public decimal ObterValorUnitarioComDesconto()
    {
        return Quantidade > 0 ? ValorTotal / Quantidade : 0;
    }

    /// <summary>
    /// Calcula o valor de economia com desconto
    /// </summary>
    /// <returns>Valor economizado</returns>
    public decimal CalcularEconomia()
    {
        return ValorDesconto;
    }

    /// <summary>
    /// Obtém a margem de lucro formatada
    /// </summary>
    /// <returns>Margem formatada em percentual</returns>
    public string? ObterMargemFormatada()
    {
        return MargemLucro?.ToString("F2", new CultureInfo("pt-BR")) + "%";
    }

    /// <summary>
    /// Verifica se o item pode ter desconto aplicado
    /// </summary>
    /// <param name="percentualMaximo">Percentual máximo permitido</param>
    /// <returns>True se pode aplicar desconto</returns>
    public bool PodeAplicarDesconto(decimal percentualMaximo = 100)
    {
        return PercentualDesconto < percentualMaximo && ValorBruto > 0;
    }

    /// <summary>
    /// Obtém informações de compliance ANVISA do item
    /// </summary>
    /// <returns>String com informações de compliance</returns>
    public string ObterInformacoesCompliance()
    {
        var info = new List<string>();

        if (IsMedicamentoControlado)
        {
            info.Add($"Controlado - {ClassificacaoAnvisa}");
        }

        if (!string.IsNullOrEmpty(NumeroReceitaItem))
        {
            info.Add($"Receita: {NumeroReceitaItem}");
        }

        if (ClienteOrientado)
        {
            info.Add("Cliente orientado");
        }

        if (FoiSubstituido)
        {
            info.Add("Produto substituído");
        }

        if (IsProximoVencimento())
        {
            info.Add("Próximo do vencimento");
        }

        return string.Join(" | ", info);
    }

    /// <summary>
    /// Recalcula todos os valores do item
    /// </summary>
    public void RecalcularValores()
    {
        CalcularValorBruto();
        CalcularValorTotal();
        CalcularCustoTotal();
        DefinirAliquotas(AliquotaICMS, AliquotaPISCOFINS);
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string do item</returns>
    public override string ToString()
    {
        var produto = Produto?.Nome ?? "Produto não carregado";
        return $"Item {Sequencia}: {produto} - Qtd: {Quantidade} - Valor: R$ {ValorTotal:F2}";
    }
}