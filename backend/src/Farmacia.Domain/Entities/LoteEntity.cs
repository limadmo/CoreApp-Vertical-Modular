using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um lote de produto no sistema farmacêutico brasileiro
/// Controla rastreabilidade, validades e compliance ANVISA para medicamentos
/// </summary>
/// <remarks>
/// Esta entidade implementa controle rigoroso de lotes farmacêuticos conforme
/// exigências da ANVISA para rastreabilidade completa de medicamentos.
/// Essencial para medicamentos controlados e compliance regulatório.
/// </remarks>
public class LoteEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único do lote
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
    /// Identificador do produto ao qual o lote pertence
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Produto relacionado ao lote
    /// </summary>
    public ProdutoEntity Produto { get; set; } = null!;

    /// <summary>
    /// Número do lote fornecido pelo fabricante
    /// Obrigatório para rastreabilidade ANVISA
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NumeroLote { get; set; } = string.Empty;

    /// <summary>
    /// Data de fabricação do lote
    /// </summary>
    [Required]
    public DateTime DataFabricacao { get; set; }

    /// <summary>
    /// Data de validade do lote
    /// Crítica para controle farmacêutico
    /// </summary>
    [Required]
    public DateTime DataValidade { get; set; }

    /// <summary>
    /// Quantidade inicial do lote quando foi recebido
    /// </summary>
    [Required]
    [Range(0.001, 999999.99, ErrorMessage = "Quantidade inicial deve ser positiva")]
    public decimal QuantidadeInicial { get; set; }

    /// <summary>
    /// Quantidade atual disponível do lote
    /// </summary>
    [Required]
    [Range(0, 999999.99, ErrorMessage = "Quantidade atual deve ser zero ou positiva")]
    public decimal QuantidadeAtual { get; set; }

    /// <summary>
    /// Quantidade reservada para vendas pendentes
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Quantidade reservada deve ser zero ou positiva")]
    public decimal QuantidadeReservada { get; set; } = 0;

    /// <summary>
    /// Unidade de medida do lote
    /// </summary>
    [Required]
    [StringLength(20)]
    public string UnidadeMedida { get; set; } = "unidade";

    /// <summary>
    /// Custo unitário de aquisição do lote
    /// Usado para cálculo do PEPS/CMV
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Custo unitário deve ser positivo")]
    public decimal? CustoUnitario { get; set; }

    /// <summary>
    /// Preço de venda sugerido do lote
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Preço de venda deve ser positivo")]
    public decimal? PrecoVenda { get; set; }

    /// <summary>
    /// Identificador do fornecedor do lote
    /// </summary>
    public Guid? FornecedorId { get; set; }

    /// <summary>
    /// Fornecedor do lote
    /// </summary>
    public FornecedorEntity? Fornecedor { get; set; }

    /// <summary>
    /// Número da nota fiscal de entrada do lote
    /// </summary>
    [StringLength(100)]
    public string? NumeroNotaFiscal { get; set; }

    /// <summary>
    /// Data de entrada do lote no estoque
    /// </summary>
    [Required]
    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Localização física do lote no estoque
    /// </summary>
    [StringLength(100)]
    public string? Localizacao { get; set; }

    /// <summary>
    /// Temperatura ideal de armazenamento (°C)
    /// Importante para medicamentos termolábeis
    /// </summary>
    public decimal? TemperaturaArmazenamento { get; set; }

    /// <summary>
    /// Umidade relativa ideal de armazenamento (%)
    /// </summary>
    public decimal? UmidadeArmazenamento { get; set; }

    /// <summary>
    /// Observações específicas do lote
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Status do lote
    /// </summary>
    [Required]
    public StatusLote Status { get; set; } = StatusLote.Disponivel;

    /// <summary>
    /// Data da alteração do status
    /// </summary>
    public DateTime? DataAlteracaoStatus { get; set; }

    /// <summary>
    /// Motivo da alteração do status
    /// </summary>
    [StringLength(500)]
    public string? MotivoAlteracaoStatus { get; set; }

    /// <summary>
    /// Usuário que alterou o status
    /// </summary>
    public Guid? UsuarioAlteracaoStatusId { get; set; }

    /// <summary>
    /// Indica se o lote está bloqueado para vendas
    /// </summary>
    public bool Bloqueado { get; set; } = false;

    /// <summary>
    /// Motivo do bloqueio do lote
    /// </summary>
    [StringLength(500)]
    public string? MotivoBloqueio { get; set; }

    /// <summary>
    /// Data do bloqueio
    /// </summary>
    public DateTime? DataBloqueio { get; set; }

    /// <summary>
    /// Usuário que bloqueou o lote
    /// </summary>
    public Guid? UsuarioBloqueioId { get; set; }

    /// <summary>
    /// Código de barras específico do lote (se diferente do produto)
    /// </summary>
    [StringLength(50)]
    public string? CodigoBarrasLote { get; set; }

    /// <summary>
    /// Registro ANVISA específico do lote
    /// </summary>
    [StringLength(50)]
    public string? RegistroAnvisa { get; set; }

    /// <summary>
    /// Informações do fabricante específicas do lote
    /// </summary>
    [StringLength(200)]
    public string? InformacoesFabricante { get; set; }

    /// <summary>
    /// Condições especiais de armazenamento
    /// </summary>
    [StringLength(500)]
    public string? CondicoesEspeciais { get; set; }

    // Coleções de navegação

    /// <summary>
    /// Movimentações relacionadas a este lote
    /// </summary>
    public ICollection<MovimentacaoEstoqueEntity> Movimentacoes { get; set; } = new List<MovimentacaoEstoqueEntity>();

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
    /// Verifica se o lote está vencido
    /// </summary>
    /// <returns>True se vencido, false caso contrário</returns>
    public bool IsVencido()
    {
        return DateTime.Now.Date > DataValidade.Date;
    }

    /// <summary>
    /// Verifica se o lote está próximo do vencimento
    /// </summary>
    /// <param name="diasAntes">Número de dias antes do vencimento para considerar próximo</param>
    /// <returns>True se próximo do vencimento</returns>
    public bool IsProximoVencimento(int diasAntes = 30)
    {
        var dataLimite = DateTime.Now.AddDays(diasAntes).Date;
        return DataValidade.Date <= dataLimite && !IsVencido();
    }

    /// <summary>
    /// Calcula os dias restantes até o vencimento
    /// </summary>
    /// <returns>Número de dias até vencer (negativo se já vencido)</returns>
    public int DiasParaVencimento()
    {
        return (DataValidade.Date - DateTime.Now.Date).Days;
    }

    /// <summary>
    /// Verifica se o lote tem estoque disponível
    /// </summary>
    /// <returns>True se tem estoque disponível</returns>
    public bool TemEstoqueDisponivel()
    {
        return QuantidadeAtual > 0 && !Bloqueado && Status == StatusLote.Disponivel;
    }

    /// <summary>
    /// Calcula a quantidade disponível para venda (atual - reservada)
    /// </summary>
    /// <returns>Quantidade disponível</returns>
    public decimal QuantidadeDisponivelVenda()
    {
        return Math.Max(0, QuantidadeAtual - QuantidadeReservada);
    }

    /// <summary>
    /// Reserva uma quantidade do lote
    /// </summary>
    /// <param name="quantidade">Quantidade a reservar</param>
    /// <returns>True se conseguiu reservar, false caso contrário</returns>
    public bool ReservarQuantidade(decimal quantidade)
    {
        if (quantidade <= 0)
            return false;

        var disponivelParaReserva = QuantidadeDisponivelVenda();
        
        if (quantidade > disponivelParaReserva)
            return false;

        QuantidadeReservada += quantidade;
        UpdatedAt = DateTime.UtcNow;
        
        return true;
    }

    /// <summary>
    /// Libera uma quantidade reservada
    /// </summary>
    /// <param name="quantidade">Quantidade a liberar</param>
    public void LiberarQuantidadeReservada(decimal quantidade)
    {
        if (quantidade <= 0)
            return;

        QuantidadeReservada = Math.Max(0, QuantidadeReservada - quantidade);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Baixa uma quantidade do estoque atual
    /// </summary>
    /// <param name="quantidade">Quantidade a baixar</param>
    /// <returns>True se conseguiu baixar, false caso contrário</returns>
    public bool BaixarEstoque(decimal quantidade)
    {
        if (quantidade <= 0)
            return false;

        if (quantidade > QuantidadeAtual)
            return false;

        QuantidadeAtual -= quantidade;
        UpdatedAt = DateTime.UtcNow;

        // Atualiza status se zerou o estoque
        if (QuantidadeAtual == 0)
        {
            Status = StatusLote.Esgotado;
            DataAlteracaoStatus = DateTime.UtcNow;
        }

        return true;
    }

    /// <summary>
    /// Adiciona quantidade ao estoque atual
    /// </summary>
    /// <param name="quantidade">Quantidade a adicionar</param>
    public void AdicionarEstoque(decimal quantidade)
    {
        if (quantidade <= 0)
            return;

        QuantidadeAtual += quantidade;
        
        // Atualiza status se estava esgotado
        if (Status == StatusLote.Esgotado)
        {
            Status = StatusLote.Disponivel;
            DataAlteracaoStatus = DateTime.UtcNow;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bloqueia o lote para vendas
    /// </summary>
    /// <param name="motivo">Motivo do bloqueio</param>
    /// <param name="usuarioId">Usuário que está bloqueando</param>
    public void Bloquear(string motivo, Guid usuarioId)
    {
        Bloqueado = true;
        MotivoBloqueio = motivo;
        DataBloqueio = DateTime.UtcNow;
        UsuarioBloqueioId = usuarioId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioId;
    }

    /// <summary>
    /// Desbloqueia o lote para vendas
    /// </summary>
    /// <param name="usuarioId">Usuário que está desbloqueando</param>
    public void Desbloquear(Guid usuarioId)
    {
        Bloqueado = false;
        MotivoBloqueio = null;
        DataBloqueio = null;
        UsuarioBloqueioId = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioId;
    }

    /// <summary>
    /// Altera o status do lote
    /// </summary>
    /// <param name="novoStatus">Novo status</param>
    /// <param name="motivo">Motivo da alteração</param>
    /// <param name="usuarioId">Usuário responsável pela alteração</param>
    public void AlterarStatus(StatusLote novoStatus, string motivo, Guid usuarioId)
    {
        Status = novoStatus;
        MotivoAlteracaoStatus = motivo;
        DataAlteracaoStatus = DateTime.UtcNow;
        UsuarioAlteracaoStatusId = usuarioId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioId;
    }

    /// <summary>
    /// Calcula a margem de lucro do lote baseado no custo e preço de venda
    /// </summary>
    /// <returns>Percentual de margem de lucro</returns>
    public decimal? CalcularMargemLucro()
    {
        if (!CustoUnitario.HasValue || !PrecoVenda.HasValue || CustoUnitario == 0)
            return null;

        return ((PrecoVenda.Value - CustoUnitario.Value) / CustoUnitario.Value) * 100;
    }

    /// <summary>
    /// Valida se o lote está em condições adequadas para venda
    /// </summary>
    /// <returns>True se pode ser vendido, false caso contrário</returns>
    public bool PodeSerVendido()
    {
        return !IsVencido() &&
               !Bloqueado &&
               TemEstoqueDisponivel() &&
               Status == StatusLote.Disponivel;
    }

    /// <summary>
    /// Obtém a classificação de prioridade do lote baseada na validade
    /// </summary>
    /// <returns>Prioridade de venda do lote</returns>
    public PrioridadeLote ObterPrioridade()
    {
        if (IsVencido())
            return PrioridadeLote.Vencido;

        var diasParaVencer = DiasParaVencimento();

        if (diasParaVencer <= 30)
            return PrioridadeLote.VencimentoUrgente;
        
        if (diasParaVencer <= 90)
            return PrioridadeLote.VencimentoProximo;

        if (diasParaVencer <= 180)
            return PrioridadeLote.VencimentoMedio;

        return PrioridadeLote.VencimentoLongo;
    }

    /// <summary>
    /// Formata a data de validade para exibição brasileira
    /// </summary>
    /// <returns>Data formatada</returns>
    public string FormatarDataValidade()
    {
        return DataValidade.ToString("dd/MM/yyyy", new CultureInfo("pt-BR"));
    }

    /// <summary>
    /// Formata a data de fabricação para exibição brasileira
    /// </summary>
    /// <returns>Data formatada</returns>
    public string FormatarDataFabricacao()
    {
        return DataFabricacao.ToString("dd/MM/yyyy", new CultureInfo("pt-BR"));
    }

    /// <summary>
    /// Obtém informações resumidas do lote para relatórios
    /// </summary>
    /// <returns>Resumo do lote</returns>
    public string ObterResumo()
    {
        var resumo = $"Lote: {NumeroLote}";
        
        if (DataValidade.Date < DateTime.Now.Date.AddDays(90))
        {
            resumo += $" (Vence: {FormatarDataValidade()})";
        }

        if (QuantidadeAtual > 0)
        {
            resumo += $" - Qtd: {QuantidadeAtual} {UnidadeMedida}";
        }

        if (Bloqueado)
        {
            resumo += " - BLOQUEADO";
        }

        return resumo;
    }

    /// <summary>
    /// Verifica se o lote precisa de condições especiais de armazenamento
    /// </summary>
    /// <returns>True se precisa de condições especiais</returns>
    public bool PrecisaCondicoesEspeciais()
    {
        return TemperaturaArmazenamento.HasValue || 
               UmidadeArmazenamento.HasValue || 
               !string.IsNullOrEmpty(CondicoesEspeciais);
    }

    /// <summary>
    /// Obtém as condições ideais de armazenamento formatadas
    /// </summary>
    /// <returns>String com condições de armazenamento</returns>
    public string? ObterCondicoesArmazenamento()
    {
        var condicoes = new List<string>();

        if (TemperaturaArmazenamento.HasValue)
        {
            condicoes.Add($"Temp: {TemperaturaArmazenamento}°C");
        }

        if (UmidadeArmazenamento.HasValue)
        {
            condicoes.Add($"Umidade: {UmidadeArmazenamento}%");
        }

        if (!string.IsNullOrEmpty(CondicoesEspeciais))
        {
            condicoes.Add(CondicoesEspeciais);
        }

        return condicoes.Any() ? string.Join(" | ", condicoes) : null;
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string do lote</returns>
    public override string ToString()
    {
        return $"Lote {NumeroLote}: {Produto?.Nome} - Validade: {FormatarDataValidade()} - Qtd: {QuantidadeAtual}";
    }
}

/// <summary>
/// Status possíveis para um lote
/// </summary>
public enum StatusLote
{
    /// <summary>
    /// Lote disponível para venda
    /// </summary>
    Disponivel = 0,

    /// <summary>
    /// Lote com estoque esgotado
    /// </summary>
    Esgotado = 1,

    /// <summary>
    /// Lote bloqueado por problemas de qualidade
    /// </summary>
    BloqueadoQualidade = 2,

    /// <summary>
    /// Lote vencido
    /// </summary>
    Vencido = 3,

    /// <summary>
    /// Lote em quarentena
    /// </summary>
    Quarentena = 4,

    /// <summary>
    /// Lote devolvido
    /// </summary>
    Devolvido = 5,

    /// <summary>
    /// Lote danificado
    /// </summary>
    Danificado = 6
}

/// <summary>
/// Prioridade de venda baseada na proximidade do vencimento
/// </summary>
public enum PrioridadeLote
{
    /// <summary>
    /// Lote já vencido
    /// </summary>
    Vencido = 0,

    /// <summary>
    /// Vencimento urgente (até 30 dias)
    /// </summary>
    VencimentoUrgente = 1,

    /// <summary>
    /// Vencimento próximo (31-90 dias)
    /// </summary>
    VencimentoProximo = 2,

    /// <summary>
    /// Vencimento médio (91-180 dias)
    /// </summary>
    VencimentoMedio = 3,

    /// <summary>
    /// Vencimento longo (mais de 180 dias)
    /// </summary>
    VencimentoLongo = 4
}