using Farmacia.Domain.Entities.Common;
using Farmacia.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma movimentação de estoque no sistema farmacêutico brasileiro
/// Controla entradas e saídas de medicamentos com compliance ANVISA e auditoria completa
/// </summary>
/// <remarks>
/// Esta entidade implementa controle rigoroso de movimentações farmacêuticas conforme
/// exigências da ANVISA para rastreabilidade de medicamentos controlados e não controlados.
/// Suporta isolamento multi-tenant para farmácias independentes e redes.
/// </remarks>
public class MovimentacaoEstoqueEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único da movimentação de estoque
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
    /// Identificador do produto que teve movimentação
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Produto relacionado à movimentação
    /// </summary>
    public ProdutoEntity Produto { get; set; } = null!;

    /// <summary>
    /// Identificador do lote específico do produto
    /// </summary>
    public Guid? LoteId { get; set; }

    /// <summary>
    /// Lote específico da movimentação (opcional para produtos não controlados)
    /// </summary>
    public LoteEntity? Lote { get; set; }

    /// <summary>
    /// Tipo de movimentação (entrada, saída, transferência, ajuste)
    /// Configurável por tenant sem necessidade de deploy
    /// </summary>
    [Required]
    public Guid TipoMovimentacaoId { get; set; }

    /// <summary>
    /// Configuração do tipo de movimentação
    /// </summary>
    public TipoMovimentacaoEntity TipoMovimentacao { get; set; } = null!;

    /// <summary>
    /// Quantidade movimentada (positiva para entrada, negativa para saída)
    /// </summary>
    [Required]
    [Range(-999999.99, 999999.99, ErrorMessage = "Quantidade deve estar entre -999999.99 e 999999.99")]
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Unidade de medida da quantidade (comprimidos, caixas, frascos, etc.)
    /// </summary>
    [StringLength(20)]
    public string UnidadeMedida { get; set; } = "unidade";

    /// <summary>
    /// Saldo anterior do produto antes da movimentação
    /// </summary>
    [Required]
    public decimal SaldoAnterior { get; set; }

    /// <summary>
    /// Saldo atual do produto após a movimentação
    /// </summary>
    [Required]
    public decimal SaldoAtual { get; set; }

    /// <summary>
    /// Custo unitário do produto na movimentação
    /// Importante para cálculo do CMV farmacêutico
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Custo unitário deve ser positivo")]
    public decimal? CustoUnitario { get; set; }

    /// <summary>
    /// Custo total da movimentação
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Custo total deve ser positivo")]
    public decimal? CustoTotal { get; set; }

    /// <summary>
    /// Data e hora da movimentação
    /// </summary>
    [Required]
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador do usuário responsável pela movimentação
    /// </summary>
    [Required]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Usuário que realizou a movimentação
    /// </summary>
    public UsuarioEntity Usuario { get; set; } = null!;

    /// <summary>
    /// Identificador do fornecedor (apenas para movimentações de entrada)
    /// </summary>
    public Guid? FornecedorId { get; set; }

    /// <summary>
    /// Fornecedor da mercadoria (para entradas)
    /// </summary>
    public FornecedorEntity? Fornecedor { get; set; }

    /// <summary>
    /// Número da nota fiscal de entrada/saída
    /// Obrigatório para compliance fiscal brasileiro
    /// </summary>
    [StringLength(100)]
    public string? NumeroNotaFiscal { get; set; }

    /// <summary>
    /// Série da nota fiscal
    /// </summary>
    [StringLength(10)]
    public string? SerieNotaFiscal { get; set; }

    /// <summary>
    /// Data de emissão da nota fiscal
    /// </summary>
    public DateTime? DataNotaFiscal { get; set; }

    /// <summary>
    /// Observações da movimentação
    /// Importante para auditoria e rastreabilidade ANVISA
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Motivo da movimentação (venda, compra, devolução, perda, etc.)
    /// </summary>
    [StringLength(200)]
    public string? Motivo { get; set; }

    /// <summary>
    /// Referência externa (ID da venda, compra, transferência, etc.)
    /// </summary>
    [StringLength(100)]
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// Localização física do produto no estoque
    /// </summary>
    [StringLength(100)]
    public string? Localizacao { get; set; }

    /// <summary>
    /// Temperatura de armazenamento no momento da movimentação (°C)
    /// Importante para medicamentos que exigem cadeia fria
    /// </summary>
    public decimal? TemperaturaArmazenamento { get; set; }

    /// <summary>
    /// Umidade relativa do ambiente no momento da movimentação (%)
    /// </summary>
    public decimal? UmidadeArmazenamento { get; set; }

    /// <summary>
    /// Indica se a movimentação requer aprovação
    /// Usado para movimentações de medicamentos controlados
    /// </summary>
    public bool RequerAprovacao { get; set; } = false;

    /// <summary>
    /// Status da aprovação da movimentação
    /// </summary>
    public StatusAprovacao StatusAprovacao { get; set; } = StatusAprovacao.Pendente;

    /// <summary>
    /// Data da aprovação/rejeição
    /// </summary>
    public DateTime? DataAprovacao { get; set; }

    /// <summary>
    /// Usuário que aprovou/rejeitou a movimentação
    /// </summary>
    public Guid? UsuarioAprovacaoId { get; set; }

    /// <summary>
    /// Usuário responsável pela aprovação
    /// </summary>
    public UsuarioEntity? UsuarioAprovacao { get; set; }

    /// <summary>
    /// Comentários da aprovação/rejeição
    /// </summary>
    [StringLength(500)]
    public string? ComentariosAprovacao { get; set; }

    // Propriedades de controle de soft delete
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
    /// Verifica se a movimentação é de entrada de mercadoria
    /// </summary>
    /// <returns>True se for entrada, false caso contrário</returns>
    public bool IsEntrada()
    {
        return Quantidade > 0;
    }

    /// <summary>
    /// Verifica se a movimentação é de saída de mercadoria
    /// </summary>
    /// <returns>True se for saída, false caso contrário</returns>
    public bool IsSaida()
    {
        return Quantidade < 0;
    }

    /// <summary>
    /// Calcula o valor absoluto da quantidade movimentada
    /// </summary>
    /// <returns>Quantidade absoluta</returns>
    public decimal ObterQuantidadeAbsoluta()
    {
        return Math.Abs(Quantidade);
    }

    /// <summary>
    /// Valida se a movimentação está consistente
    /// </summary>
    /// <returns>True se válida, false caso contrário</returns>
    public bool IsMovimentacaoValida()
    {
        // Não pode ter quantidade zero
        if (Quantidade == 0)
            return false;

        // Saldo atual deve ser coerente
        var saldoEsperado = SaldoAnterior + Quantidade;
        if (Math.Abs(SaldoAtual - saldoEsperado) > 0.001m)
            return false;

        // Movimentações de entrada devem ter fornecedor
        if (IsEntrada() && FornecedorId == null)
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se a movimentação envolve medicamento controlado
    /// </summary>
    /// <returns>True se for medicamento controlado</returns>
    public bool EnvolveMedicamentoControlado()
    {
        return Produto?.IsControlado() ?? false;
    }

    /// <summary>
    /// Obtém o impacto da movimentação no estoque
    /// </summary>
    /// <returns>Descrição do impacto</returns>
    public string ObterDescricaoImpacto()
    {
        if (IsEntrada())
            return $"Entrada de {ObterQuantidadeAbsoluta()} {UnidadeMedida}(s)";
        else
            return $"Saída de {ObterQuantidadeAbsoluta()} {UnidadeMedida}(s)";
    }

    /// <summary>
    /// Verifica se a movimentação pode ser cancelada
    /// </summary>
    /// <returns>True se pode ser cancelada, false caso contrário</returns>
    public bool PodeCancelar()
    {
        // Não pode cancelar se já foi aprovada e é de medicamento controlado
        if (EnvolveMedicamentoControlado() && StatusAprovacao == StatusAprovacao.Aprovada)
            return false;

        // Não pode cancelar movimentações muito antigas (mais de 30 dias)
        if (DateTime.UtcNow.Subtract(DataMovimentacao).TotalDays > 30)
            return false;

        return !IsDeleted && !IsArchived;
    }

    /// <summary>
    /// Aprova a movimentação
    /// </summary>
    /// <param name="usuarioAprovadorId">ID do usuário que está aprovando</param>
    /// <param name="comentarios">Comentários da aprovação</param>
    public void Aprovar(Guid usuarioAprovadorId, string? comentarios = null)
    {
        StatusAprovacao = StatusAprovacao.Aprovada;
        DataAprovacao = DateTime.UtcNow;
        UsuarioAprovacaoId = usuarioAprovadorId;
        ComentariosAprovacao = comentarios;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioAprovadorId;
    }

    /// <summary>
    /// Rejeita a movimentação
    /// </summary>
    /// <param name="usuarioRejeitadorId">ID do usuário que está rejeitando</param>
    /// <param name="motivo">Motivo da rejeição</param>
    public void Rejeitar(Guid usuarioRejeitadorId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da rejeição é obrigatório", nameof(motivo));

        StatusAprovacao = StatusAprovacao.Rejeitada;
        DataAprovacao = DateTime.UtcNow;
        UsuarioAprovacaoId = usuarioRejeitadorId;
        ComentariosAprovacao = motivo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioRejeitadorId;
    }

    /// <summary>
    /// Calcula o valor total da movimentação baseado no custo unitário
    /// </summary>
    /// <returns>Valor total da movimentação</returns>
    public decimal CalcularValorTotal()
    {
        if (!CustoUnitario.HasValue)
            return 0;

        return CustoUnitario.Value * ObterQuantidadeAbsoluta();
    }

    /// <summary>
    /// Atualiza o custo total automaticamente
    /// </summary>
    public void AtualizarCustoTotal()
    {
        CustoTotal = CalcularValorTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a movimentação necessita de condições especiais de armazenamento
    /// </summary>
    /// <returns>True se precisa de condições especiais</returns>
    public bool NecessitaCondicoesEspeciais()
    {
        return TemperaturaArmazenamento.HasValue || UmidadeArmazenamento.HasValue;
    }

    /// <summary>
    /// Obtém informações de compliance ANVISA para a movimentação
    /// </summary>
    /// <returns>Informações de compliance</returns>
    public string ObterInformacoesCompliance()
    {
        var info = new List<string>();

        if (EnvolveMedicamentoControlado())
        {
            info.Add($"Medicamento controlado - Lista {Produto?.ClassificacaoAnvisa}");
        }

        if (RequerAprovacao)
        {
            info.Add($"Aprovação: {StatusAprovacao}");
        }

        if (NecessitaCondicoesEspeciais())
        {
            info.Add("Condições especiais de armazenamento");
        }

        if (!string.IsNullOrEmpty(NumeroNotaFiscal))
        {
            info.Add($"NF: {NumeroNotaFiscal}");
        }

        return string.Join(" | ", info);
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da movimentação</returns>
    public override string ToString()
    {
        return $"Movimentação {Id}: {ObterDescricaoImpacto()} - {Produto?.Nome} - {DataMovimentacao:dd/MM/yyyy HH:mm}";
    }
}

/// <summary>
/// Enum para status de aprovação de movimentações
/// </summary>
public enum StatusAprovacao
{
    /// <summary>
    /// Aguardando aprovação
    /// </summary>
    Pendente = 0,

    /// <summary>
    /// Movimentação aprovada
    /// </summary>
    Aprovada = 1,

    /// <summary>
    /// Movimentação rejeitada
    /// </summary>
    Rejeitada = 2,

    /// <summary>
    /// Aprovação não necessária
    /// </summary>
    NaoNecessaria = 3
}