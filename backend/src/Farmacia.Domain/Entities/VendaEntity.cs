using Farmacia.Domain.Interfaces;
using Farmacia.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma venda no sistema farmacêutico brasileiro
/// Controla todas as vendas com compliance ANVISA, impostos brasileiros e receituários digitais
/// </summary>
/// <remarks>
/// Esta entidade implementa controle rigoroso de vendas farmacêuticas conforme
/// exigências da ANVISA, CFF e LGPD. Inclui validação de receitas para medicamentos
/// controlados e cálculos automáticos de impostos brasileiros (ICMS, PIS/COFINS).
/// </remarks>
public class VendaEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único da venda
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
    /// Número sequencial da venda (por tenant)
    /// Gerado automaticamente para cada farmácia
    /// </summary>
    [Required]
    public long NumeroVenda { get; set; }

    /// <summary>
    /// Código de barras da venda (se aplicável)
    /// </summary>
    [StringLength(50)]
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// Data e hora da venda
    /// </summary>
    [Required]
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador do cliente (opcional para vendas balcão)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Cliente da venda
    /// </summary>
    public ClienteEntity? Cliente { get; set; }

    /// <summary>
    /// CPF do cliente para cupom fiscal (obrigatório se informado)
    /// </summary>
    [StringLength(14)]
    public string? CpfCliente { get; set; }

    /// <summary>
    /// Nome do cliente para cupom fiscal
    /// </summary>
    [StringLength(200)]
    public string? NomeCliente { get; set; }

    /// <summary>
    /// Identificador do usuário vendedor
    /// </summary>
    [Required]
    public Guid VendedorId { get; set; }

    /// <summary>
    /// Usuário vendedor
    /// </summary>
    public UsuarioEntity Vendedor { get; set; } = null!;

    /// <summary>
    /// Status atual da venda
    /// </summary>
    [Required]
    public StatusVenda Status { get; set; } = StatusVenda.Aberta;

    /// <summary>
    /// Data da alteração do status
    /// </summary>
    public DateTime? DataAlteracaoStatus { get; set; }

    /// <summary>
    /// Motivo da alteração de status
    /// </summary>
    [StringLength(500)]
    public string? MotivoAlteracaoStatus { get; set; }

    /// <summary>
    /// Subtotal dos produtos (sem descontos e impostos)
    /// </summary>
    [Required]
    [Range(0, 999999.99, ErrorMessage = "Subtotal deve ser positivo")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Valor total de descontos aplicados
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Desconto deve ser positivo")]
    public decimal ValorDesconto { get; set; } = 0;

    /// <summary>
    /// Percentual de desconto geral aplicado
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; } = 0;

    /// <summary>
    /// Valor do ICMS calculado automaticamente
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "ICMS deve ser positivo")]
    public decimal ValorICMS { get; set; } = 0;

    /// <summary>
    /// Alíquota do ICMS aplicada (%)
    /// </summary>
    [Range(0, 30, ErrorMessage = "Alíquota ICMS deve estar entre 0 e 30%")]
    public decimal AliquotaICMS { get; set; } = 0;

    /// <summary>
    /// Valor do PIS/COFINS calculado automaticamente
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "PIS/COFINS deve ser positivo")]
    public decimal ValorPISCOFINS { get; set; } = 0;

    /// <summary>
    /// Alíquota do PIS/COFINS aplicada (%)
    /// </summary>
    [Range(0, 15, ErrorMessage = "Alíquota PIS/COFINS deve estar entre 0 e 15%")]
    public decimal AliquotaPISCOFINS { get; set; } = 0;

    /// <summary>
    /// Outros impostos e taxas
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Outros impostos devem ser positivos")]
    public decimal OutrosImpostos { get; set; } = 0;

    /// <summary>
    /// Valor total da venda (subtotal - desconto + impostos)
    /// </summary>
    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Total deve ser positivo")]
    public decimal Total { get; set; }

    /// <summary>
    /// Valor pago pelo cliente
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor pago deve ser positivo")]
    public decimal ValorPago { get; set; } = 0;

    /// <summary>
    /// Valor do troco
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Troco deve ser positivo")]
    public decimal ValorTroco { get; set; } = 0;

    /// <summary>
    /// Forma de pagamento principal
    /// </summary>
    [StringLength(50)]
    public string? FormaPagamento { get; set; }

    /// <summary>
    /// Detalhes do pagamento (JSON com múltiplas formas)
    /// </summary>
    [StringLength(2000)]
    public string? DetalhesFormasPagamento { get; set; }

    /// <summary>
    /// Observações da venda
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Número da receita médica (se aplicável)
    /// Obrigatório para medicamentos controlados
    /// </summary>
    [StringLength(100)]
    public string? NumeroReceita { get; set; }

    /// <summary>
    /// CRM do médico prescritor
    /// </summary>
    [StringLength(20)]
    public string? CrmMedico { get; set; }

    /// <summary>
    /// Nome do médico prescritor
    /// </summary>
    [StringLength(200)]
    public string? NomeMedico { get; set; }

    /// <summary>
    /// Data de emissão da receita
    /// </summary>
    public DateTime? DataReceita { get; set; }

    /// <summary>
    /// Tipo de receita (branca, azul, amarela)
    /// </summary>
    [StringLength(20)]
    public string? TipoReceita { get; set; }

    /// <summary>
    /// Indica se a venda contém medicamentos controlados
    /// </summary>
    public bool TemMedicamentosControlados { get; set; } = false;

    /// <summary>
    /// Indica se a venda foi aprovada para medicamentos controlados
    /// </summary>
    public bool AprovadaMedicamentosControlados { get; set; } = false;

    /// <summary>
    /// Data da aprovação para medicamentos controlados
    /// </summary>
    public DateTime? DataAprovacaoControlados { get; set; }

    /// <summary>
    /// Usuário que aprovou medicamentos controlados
    /// </summary>
    public Guid? UsuarioAprovacaoControladosId { get; set; }

    /// <summary>
    /// Usuário responsável pela aprovação de controlados
    /// </summary>
    public UsuarioEntity? UsuarioAprovacaoControlados { get; set; }

    /// <summary>
    /// Número do cupom fiscal (se emitido)
    /// </summary>
    [StringLength(50)]
    public string? NumeroCupomFiscal { get; set; }

    /// <summary>
    /// Chave de acesso da NFCe (se emitida)
    /// </summary>
    [StringLength(100)]
    public string? ChaveNFCe { get; set; }

    /// <summary>
    /// Protocolo de autorização da NFCe
    /// </summary>
    [StringLength(50)]
    public string? ProtocoloNFCe { get; set; }

    /// <summary>
    /// Data de emissão do cupom fiscal
    /// </summary>
    public DateTime? DataEmissaoCupom { get; set; }

    /// <summary>
    /// Identificador da promoção aplicada (se houver)
    /// </summary>
    public Guid? PromocaoId { get; set; }

    /// <summary>
    /// Promoção aplicada na venda
    /// </summary>
    public PromocaoEntity? Promocao { get; set; }

    /// <summary>
    /// Código do cupom de desconto usado
    /// </summary>
    [StringLength(50)]
    public string? CupomDesconto { get; set; }

    /// <summary>
    /// Canal de venda (balcão, delivery, online, etc.)
    /// </summary>
    [StringLength(50)]
    public string CanalVenda { get; set; } = "BALCAO";

    /// <summary>
    /// Endereço de entrega (se delivery)
    /// </summary>
    [StringLength(1000)]
    public string? EnderecoEntrega { get; set; }

    /// <summary>
    /// Valor da taxa de entrega
    /// </summary>
    [Range(0, 999.99, ErrorMessage = "Taxa de entrega deve ser positiva")]
    public decimal TaxaEntrega { get; set; } = 0;

    /// <summary>
    /// Data prevista de entrega
    /// </summary>
    public DateTime? DataPrevistaEntrega { get; set; }

    /// <summary>
    /// Data efetiva da entrega
    /// </summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>
    /// Hash MD5 dos dados da venda para integridade
    /// </summary>
    [StringLength(32)]
    public string? HashIntegridade { get; set; }

    // Coleções de navegação

    /// <summary>
    /// Itens da venda
    /// </summary>
    public ICollection<ItemVendaEntity> Itens { get; set; } = new List<ItemVendaEntity>();

    /// <summary>
    /// Formas de pagamento da venda
    /// </summary>
    public ICollection<FormaPagamentoVendaEntity> FormasPagamento { get; set; } = new List<FormaPagamentoVendaEntity>();

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

    // Propriedades de arquivamento
    /// <summary>
    /// Indica se o registro está arquivado
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>
    /// Usuário que executou o arquivamento
    /// </summary>
    public Guid? ArchivedBy { get; set; }

    /// <summary>
    /// Motivo do arquivamento
    /// </summary>
    [StringLength(500)]
    public string? ArchiveReason { get; set; }

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
    /// Adiciona um item à venda
    /// </summary>
    /// <param name="item">Item a ser adicionado</param>
    public void AdicionarItem(ItemVendaEntity item)
    {
        if (item.VendaId != Id)
            item.VendaId = Id;

        item.TenantId = TenantId;
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = CreatedBy;

        Itens.Add(item);
        RecalcularTotais();
    }

    /// <summary>
    /// Remove um item da venda
    /// </summary>
    /// <param name="itemId">ID do item a ser removido</param>
    /// <returns>True se removeu, false se não encontrou</returns>
    public bool RemoverItem(Guid itemId)
    {
        var item = Itens.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return false;

        Itens.Remove(item);
        RecalcularTotais();
        return true;
    }

    /// <summary>
    /// Recalcula todos os totais da venda
    /// </summary>
    public void RecalcularTotais()
    {
        Subtotal = Itens.Sum(i => i.ValorTotal);
        
        // Aplica desconto percentual se definido
        if (PercentualDesconto > 0)
        {
            ValorDesconto = Subtotal * (PercentualDesconto / 100);
        }

        var subtotalComDesconto = Subtotal - ValorDesconto;

        // Calcula impostos brasileiros
        ValorICMS = subtotalComDesconto * (AliquotaICMS / 100);
        ValorPISCOFINS = subtotalComDesconto * (AliquotaPISCOFINS / 100);

        Total = subtotalComDesconto + ValorICMS + ValorPISCOFINS + OutrosImpostos + TaxaEntrega;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica desconto percentual à venda
    /// </summary>
    /// <param name="percentual">Percentual de desconto</param>
    /// <param name="usuarioId">Usuário que aplicou o desconto</param>
    public void AplicarDesconto(decimal percentual, Guid usuarioId)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100");

        PercentualDesconto = percentual;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
        
        RecalcularTotais();
    }

    /// <summary>
    /// Aplica desconto em valor fixo
    /// </summary>
    /// <param name="valorDesconto">Valor fixo de desconto</param>
    /// <param name="usuarioId">Usuário que aplicou o desconto</param>
    public void AplicarDescontoValor(decimal valorDesconto, Guid usuarioId)
    {
        if (valorDesconto < 0 || valorDesconto > Subtotal)
            throw new ArgumentException("Valor de desconto inválido");

        ValorDesconto = valorDesconto;
        PercentualDesconto = Subtotal > 0 ? (valorDesconto / Subtotal) * 100 : 0;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
        
        RecalcularTotais();
    }

    /// <summary>
    /// Define as alíquotas de impostos brasileiros
    /// </summary>
    /// <param name="icms">Alíquota ICMS</param>
    /// <param name="pisCofins">Alíquota PIS/COFINS</param>
    public void DefinirAliquotas(decimal icms, decimal pisCofins)
    {
        AliquotaICMS = icms;
        AliquotaPISCOFINS = pisCofins;
        RecalcularTotais();
    }

    /// <summary>
    /// Finaliza a venda
    /// </summary>
    /// <param name="usuarioId">Usuário que está finalizando</param>
    public void Finalizar(Guid usuarioId)
    {
        if (Status != StatusVenda.Aberta)
            throw new InvalidOperationException("Apenas vendas abertas podem ser finalizadas");

        if (!Itens.Any())
            throw new InvalidOperationException("Venda deve ter pelo menos um item");

        if (TemMedicamentosControlados && !AprovadaMedicamentosControlados)
            throw new InvalidOperationException("Venda com medicamentos controlados deve ser aprovada");

        AlterarStatus(StatusVenda.Finalizada, "Venda finalizada", usuarioId);
        DataVenda = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela a venda
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="usuarioId">Usuário que está cancelando</param>
    public void Cancelar(string motivo, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo do cancelamento é obrigatório");

        if (Status == StatusVenda.Cancelada)
            throw new InvalidOperationException("Venda já está cancelada");

        AlterarStatus(StatusVenda.Cancelada, motivo, usuarioId);
    }

    /// <summary>
    /// Altera o status da venda
    /// </summary>
    /// <param name="novoStatus">Novo status</param>
    /// <param name="motivo">Motivo da alteração</param>
    /// <param name="usuarioId">Usuário responsável</param>
    private void AlterarStatus(StatusVenda novoStatus, string motivo, Guid usuarioId)
    {
        Status = novoStatus;
        DataAlteracaoStatus = DateTime.UtcNow;
        MotivoAlteracaoStatus = motivo;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aprova venda com medicamentos controlados
    /// </summary>
    /// <param name="usuarioAprovadorId">Usuário aprovador</param>
    /// <param name="numeroReceita">Número da receita</param>
    /// <param name="crmMedico">CRM do médico</param>
    /// <param name="tipoReceita">Tipo da receita</param>
    public void AprovarMedicamentosControlados(Guid usuarioAprovadorId, string numeroReceita, 
        string crmMedico, string tipoReceita)
    {
        if (!TemMedicamentosControlados)
            throw new InvalidOperationException("Venda não contém medicamentos controlados");

        AprovadaMedicamentosControlados = true;
        DataAprovacaoControlados = DateTime.UtcNow;
        UsuarioAprovacaoControladosId = usuarioAprovadorId;
        NumeroReceita = numeroReceita;
        CrmMedico = crmMedico;
        TipoReceita = tipoReceita;
        DataReceita = DateTime.UtcNow;
        UpdatedBy = usuarioAprovadorId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra pagamento da venda
    /// </summary>
    /// <param name="valorPago">Valor pago</param>
    /// <param name="formaPagamento">Forma de pagamento</param>
    public void RegistrarPagamento(decimal valorPago, string formaPagamento)
    {
        ValorPago += valorPago;
        FormaPagamento = formaPagamento;

        // Calcula troco se pagamento em dinheiro
        if (formaPagamento.ToUpper().Contains("DINHEIRO") && ValorPago > Total)
        {
            ValorTroco = ValorPago - Total;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a venda pode ser cancelada
    /// </summary>
    /// <returns>True se pode cancelar</returns>
    public bool PodeCancelar()
    {
        return Status != StatusVenda.Cancelada && 
               !string.IsNullOrEmpty(NumeroCupomFiscal) == false;
    }

    /// <summary>
    /// Verifica se a venda está quitada
    /// </summary>
    /// <returns>True se está quitada</returns>
    public bool IsQuitada()
    {
        return ValorPago >= Total;
    }

    /// <summary>
    /// Obtém o saldo pendente de pagamento
    /// </summary>
    /// <returns>Valor pendente</returns>
    public decimal ObterSaldoPendente()
    {
        return Math.Max(0, Total - ValorPago);
    }

    /// <summary>
    /// Calcula o desconto total aplicado
    /// </summary>
    /// <returns>Valor total de desconto</returns>
    public decimal CalcularDescontoTotal()
    {
        var descontoItens = Itens.Sum(i => i.ValorDesconto);
        return ValorDesconto + descontoItens;
    }

    /// <summary>
    /// Verifica se a venda necessita de receita médica
    /// </summary>
    /// <returns>True se necessita receita</returns>
    public bool NecessitaReceita()
    {
        return TemMedicamentosControlados || 
               Itens.Any(i => i.Produto?.ClassificacaoAnvisa != null && 
                              i.Produto.IsControlado());
    }

    /// <summary>
    /// Atualiza flag de medicamentos controlados baseado nos itens
    /// </summary>
    public void AtualizarFlagMedicamentosControlados()
    {
        TemMedicamentosControlados = Itens.Any(i => i.Produto?.IsControlado() == true);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gera hash de integridade para os dados da venda
    /// </summary>
    public void GerarHashIntegridade()
    {
        var dados = $"{TenantId}|{NumeroVenda}|{DataVenda:yyyyMMddHHmmss}|{Total:F2}|{Itens.Count}";
        
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dados));
        HashIntegridade = Convert.ToHexString(hash);
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Valida se o hash de integridade está correto
    /// </summary>
    /// <returns>True se válido</returns>
    public bool ValidarIntegridade()
    {
        if (string.IsNullOrEmpty(HashIntegridade))
            return false;

        var hashCalculado = HashIntegridade;
        GerarHashIntegridade();
        var hashAtual = HashIntegridade;
        HashIntegridade = hashCalculado;

        return hashCalculado == hashAtual;
    }

    /// <summary>
    /// Formata o número da venda para exibição
    /// </summary>
    /// <returns>Número formatado</returns>
    public string FormatarNumeroVenda()
    {
        return NumeroVenda.ToString("D6", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Obtém resumo da venda para relatórios
    /// </summary>
    /// <returns>String com resumo</returns>
    public string ObterResumo()
    {
        var cliente = !string.IsNullOrEmpty(NomeCliente) ? $" - {NomeCliente}" : "";
        return $"Venda {FormatarNumeroVenda()}{cliente} - {DataVenda:dd/MM/yyyy HH:mm} - R$ {Total:F2} ({Status})";
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da venda</returns>
    public override string ToString()
    {
        return ObterResumo();
    }
}

/// <summary>
/// Status possíveis para uma venda
/// </summary>
public enum StatusVenda
{
    /// <summary>
    /// Venda em aberto (sendo criada)
    /// </summary>
    Aberta = 0,

    /// <summary>
    /// Venda finalizada
    /// </summary>
    Finalizada = 1,

    /// <summary>
    /// Venda cancelada
    /// </summary>
    Cancelada = 2,

    /// <summary>
    /// Venda em orçamento
    /// </summary>
    Orcamento = 3,

    /// <summary>
    /// Venda pendente de aprovação
    /// </summary>
    PendenteAprovacao = 4,

    /// <summary>
    /// Venda aprovada mas não finalizada
    /// </summary>
    Aprovada = 5,

    /// <summary>
    /// Venda em entrega
    /// </summary>
    EmEntrega = 6,

    /// <summary>
    /// Venda entregue
    /// </summary>
    Entregue = 7,

    /// <summary>
    /// Venda devolvida
    /// </summary>
    Devolvida = 8
}