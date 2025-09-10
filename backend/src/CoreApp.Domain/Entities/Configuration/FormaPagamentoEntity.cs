using System.ComponentModel.DataAnnotations;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para formas de pagamento farmacêuticas brasileiras
/// Permite configurar métodos de pagamento específicos por farmácia
/// </summary>
/// <remarks>
/// Esta entidade substitui o enum FormaPagamento por um sistema flexível que permite:
/// - Configurações globais do sistema (PIX, cartão, dinheiro, etc.)
/// - Customizações específicas por farmácia (convênios locais, crediário próprio)
/// - Integração com gateways de pagamento brasileiros
/// - Taxas e parcelamentos customizáveis
/// - Alterações sem necessidade de deploy
/// </remarks>
public class FormaPagamentoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da forma de pagamento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global do sistema)
    /// </summary>
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único da forma de pagamento (usado na API e integrações)
    /// </summary>
    /// <example>PIX, DINHEIRO, DEBITO, CREDITO_VISTA, CREDITO_PARCELADO</example>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo exibido na interface
    /// </summary>
    /// <example>PIX, Dinheiro, Cartão de Débito, Cartão de Crédito à Vista</example>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da forma de pagamento
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Categoria da forma de pagamento para agrupamento
    /// </summary>
    /// <example>DIGITAL, FISICO, CARTAO, CONVENIO</example>
    [StringLength(30)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Tipo de pagamento para processamento
    /// </summary>
    /// <example>INSTANTANEO, PARCELADO, A_PRAZO</example>
    [StringLength(30)]
    public string? TipoPagamento { get; set; }

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    /// <example>#00BC9A (verde PIX), #FFD700 (amarelo cartão), #28A745 (verde dinheiro)</example>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-pix, fa-money-bill, fa-credit-card</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Se permite parcelamento
    /// </summary>
    public bool PermiteParcelamento { get; set; } = false;

    /// <summary>
    /// Número máximo de parcelas permitidas
    /// </summary>
    [Range(1, 24)]
    public int? MaximoParcelas { get; set; }

    /// <summary>
    /// Valor mínimo para parcelamento
    /// </summary>
    [Range(0, 999999)]
    public decimal? ValorMinimoParcelamento { get; set; }

    /// <summary>
    /// Taxa de juros mensal para parcelamento (%)
    /// </summary>
    [Range(0, 50)]
    public decimal? TaxaJurosMensal { get; set; }

    /// <summary>
    /// Taxa fixa cobrada por transação
    /// </summary>
    [Range(0, 999)]
    public decimal? TaxaFixa { get; set; }

    /// <summary>
    /// Taxa percentual cobrada por transação (%)
    /// </summary>
    [Range(0, 20)]
    public decimal? TaxaPercentual { get; set; }

    /// <summary>
    /// Se permite desconto para esta forma de pagamento
    /// </summary>
    public bool PermiteDesconto { get; set; } = false;

    /// <summary>
    /// Percentual de desconto padrão (%)
    /// </summary>
    [Range(0, 50)]
    public decimal? PercentualDesconto { get; set; }

    /// <summary>
    /// Se permite troco (para pagamentos em dinheiro)
    /// </summary>
    public bool PermiteTroco { get; set; } = false;

    /// <summary>
    /// Se requer comprovante obrigatório
    /// </summary>
    public bool RequerComprovante { get; set; } = false;

    /// <summary>
    /// Se requer autenticação adicional
    /// </summary>
    public bool RequerAutenticacao { get; set; } = false;

    /// <summary>
    /// Gateway de pagamento utilizado (quando aplicável)
    /// </summary>
    /// <example>MERCADOPAGO, PAGSEGURO, STONE, CIELO</example>
    [StringLength(50)]
    public string? GatewayPagamento { get; set; }

    /// <summary>
    /// Configurações específicas do gateway em formato JSON
    /// </summary>
    [StringLength(1000)]
    public string? ConfiguracaoGateway { get; set; }

    /// <summary>
    /// Prazo médio para compensação em dias
    /// </summary>
    [Range(0, 365)]
    public int? PrazoCompensacao { get; set; }

    /// <summary>
    /// Se está ativo para uso
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Se deve aparecer no PDV/caixa
    /// </summary>
    public bool DisponivelPDV { get; set; } = true;

    /// <summary>
    /// Se está disponível para vendas online
    /// </summary>
    public bool DisponivelOnline { get; set; } = false;

    /// <summary>
    /// Horário de início para disponibilidade (formato HH:mm)
    /// </summary>
    [StringLength(5)]
    public string? HorarioInicio { get; set; }

    /// <summary>
    /// Horário de fim para disponibilidade (formato HH:mm)
    /// </summary>
    [StringLength(5)]
    public string? HorarioFim { get; set; }

    /// <summary>
    /// Valor mínimo para aceitar esta forma de pagamento
    /// </summary>
    [Range(0, 999999)]
    public decimal? ValorMinimo { get; set; }

    /// <summary>
    /// Valor máximo para aceitar esta forma de pagamento
    /// </summary>
    [Range(0, 999999)]
    public decimal? ValorMaximo { get; set; }

    /// <summary>
    /// Observações específicas para a forma de pagamento
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Indica se é uma configuração padrão do sistema (não pode ser removida)
    /// </summary>
    public bool IsSistema { get; set; } = false;

    /// <summary>
    /// ID da configuração global pai (para herança)
    /// </summary>
    public Guid? ConfiguracaoGlobalId { get; set; }

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário responsável pela criação
    /// </summary>
    [StringLength(100)]
    public string? CriadoPor { get; set; }

    /// <summary>
    /// Usuário responsável pela última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Relacionamentos de navegação

    /// <summary>
    /// Configuração global pai (para herança de configurações)
    /// </summary>
    public virtual FormaPagamentoEntity? ConfiguracaoGlobal { get; set; }

    /// <summary>
    /// Configurações filhas que herdam desta
    /// </summary>
    public virtual ICollection<FormaPagamentoEntity> ConfiguracoesFilhas { get; set; } = new List<FormaPagamentoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se é uma configuração global (sistema)
    /// </summary>
    /// <returns>True se for global</returns>
    public bool IsGlobal()
    {
        return string.IsNullOrEmpty(TenantId);
    }

    /// <summary>
    /// Verifica se pode ser removido (não é sistema)
    /// </summary>
    /// <returns>True se pode ser removido</returns>
    public bool PodeSerRemovido()
    {
        return !IsSistema;
    }

    /// <summary>
    /// Verifica se está disponível no horário atual
    /// </summary>
    /// <returns>True se está disponível</returns>
    public bool EstaDisponivelAgora()
    {
        if (!Ativo)
            return false;

        if (string.IsNullOrEmpty(HorarioInicio) || string.IsNullOrEmpty(HorarioFim))
            return true;

        var agora = DateTime.Now.TimeOfDay;
        
        if (TimeSpan.TryParse(HorarioInicio, out var inicio) && 
            TimeSpan.TryParse(HorarioFim, out var fim))
        {
            return agora >= inicio && agora <= fim;
        }

        return true;
    }

    /// <summary>
    /// Verifica se o valor está dentro dos limites permitidos
    /// </summary>
    /// <param name="valor">Valor a ser verificado</param>
    /// <returns>True se valor é válido</returns>
    public bool ValidarValor(decimal valor)
    {
        if (ValorMinimo.HasValue && valor < ValorMinimo.Value)
            return false;

        if (ValorMaximo.HasValue && valor > ValorMaximo.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Calcula o valor das taxas para uma transação
    /// </summary>
    /// <param name="valor">Valor da transação</param>
    /// <returns>Valor total das taxas</returns>
    public decimal CalcularTaxas(decimal valor)
    {
        var taxa = 0m;

        if (TaxaFixa.HasValue)
            taxa += TaxaFixa.Value;

        if (TaxaPercentual.HasValue)
            taxa += valor * (TaxaPercentual.Value / 100);

        return taxa;
    }

    /// <summary>
    /// Calcula o desconto aplicável
    /// </summary>
    /// <param name="valor">Valor da transação</param>
    /// <returns>Valor do desconto</returns>
    public decimal CalcularDesconto(decimal valor)
    {
        if (!PermiteDesconto || !PercentualDesconto.HasValue)
            return 0;

        return valor * (PercentualDesconto.Value / 100);
    }

    /// <summary>
    /// Calcula o valor líquido após taxas e descontos
    /// </summary>
    /// <param name="valorBruto">Valor bruto da transação</param>
    /// <returns>Valor líquido</returns>
    public decimal CalcularValorLiquido(decimal valorBruto)
    {
        var desconto = CalcularDesconto(valorBruto);
        var valorComDesconto = valorBruto - desconto;
        var taxas = CalcularTaxas(valorComDesconto);
        
        return valorComDesconto - taxas;
    }

    /// <summary>
    /// Obtém a cor padrão baseada no código da forma de pagamento
    /// </summary>
    /// <returns>Código da cor hexadecimal</returns>
    public string ObterCorPadrao()
    {
        if (!string.IsNullOrEmpty(Cor))
            return Cor;

        return Codigo switch
        {
            "PIX" => "#00BC9A",         // Verde PIX
            "DINHEIRO" => "#28A745",    // Verde dinheiro
            "DEBITO" => "#007BFF",      // Azul débito
            "CREDITO_VISTA" => "#FFC107",  // Amarelo crédito
            "CREDITO_PARCELADO" => "#FD7E14", // Laranja parcelado
            "BOLETO" => "#6F42C1",      // Roxo boleto
            "CONVENIO" => "#20C997",    // Verde água convênio
            _ => "#6C757D"              // Cinza padrão
        };
    }

    /// <summary>
    /// Obtém o ícone padrão baseado no código da forma de pagamento
    /// </summary>
    /// <returns>Classe do ícone</returns>
    public string ObterIconePadrao()
    {
        if (!string.IsNullOrEmpty(Icone))
            return Icone;

        return Codigo switch
        {
            "PIX" => "fa-qrcode",
            "DINHEIRO" => "fa-money-bill",
            "DEBITO" => "fa-credit-card",
            "CREDITO_VISTA" or "CREDITO_PARCELADO" => "fa-credit-card",
            "BOLETO" => "fa-barcode",
            "TRANSFERENCIA" => "fa-exchange-alt",
            "CHEQUE" => "fa-check-square",
            "CONVENIO" => "fa-handshake",
            _ => "fa-dollar-sign"
        };
    }

    /// <summary>
    /// Verifica se é pagamento digital (não físico)
    /// </summary>
    /// <returns>True se é digital</returns>
    public bool IsPagamentoDigital()
    {
        return Codigo switch
        {
            "PIX" or "TRANSFERENCIA" or "DEBITO" or "CREDITO_VISTA" or "CREDITO_PARCELADO" => true,
            _ => false
        };
    }

    /// <summary>
    /// Atualiza o timestamp de modificação
    /// </summary>
    public void AtualizarTimestamp()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma cópia desta configuração para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant de destino</param>
    /// <returns>Nova configuração personalizada</returns>
    public FormaPagamentoEntity CriarPersonalizacao(string tenantId)
    {
        return new FormaPagamentoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            Categoria = Categoria,
            TipoPagamento = TipoPagamento,
            Cor = Cor,
            Icone = Icone,
            Ordem = Ordem,
            PermiteParcelamento = PermiteParcelamento,
            MaximoParcelas = MaximoParcelas,
            ValorMinimoParcelamento = ValorMinimoParcelamento,
            TaxaJurosMensal = TaxaJurosMensal,
            TaxaFixa = TaxaFixa,
            TaxaPercentual = TaxaPercentual,
            PermiteDesconto = PermiteDesconto,
            PercentualDesconto = PercentualDesconto,
            PermiteTroco = PermiteTroco,
            RequerComprovante = RequerComprovante,
            RequerAutenticacao = RequerAutenticacao,
            GatewayPagamento = GatewayPagamento,
            ConfiguracaoGateway = ConfiguracaoGateway,
            PrazoCompensacao = PrazoCompensacao,
            Ativo = Ativo,
            DisponivelPDV = DisponivelPDV,
            DisponivelOnline = DisponivelOnline,
            HorarioInicio = HorarioInicio,
            HorarioFim = HorarioFim,
            ValorMinimo = ValorMinimo,
            ValorMaximo = ValorMaximo,
            Observacoes = Observacoes,
            IsSistema = false, // Personalizações nunca são do sistema
            ConfiguracaoGlobalId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_HERANCA"
        };
    }

    /// <summary>
    /// Cria nova forma de pagamento padrão brasileira
    /// </summary>
    /// <param name="codigo">Código da forma de pagamento</param>
    /// <param name="nome">Nome descritivo</param>
    /// <param name="categoria">Categoria</param>
    /// <returns>Nova instância de forma de pagamento</returns>
    public static FormaPagamentoEntity CriarFormaPagamentoPadrao(
        string codigo,
        string nome,
        string categoria = "GERAL")
    {
        return new FormaPagamentoEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            Categoria = categoria,
            IsSistema = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true,
            DisponivelPDV = true,
            CriadoPor = "SISTEMA_PADRAO"
        };
    }
}