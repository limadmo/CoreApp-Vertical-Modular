using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma promoção no sistema farmacêutico brasileiro
/// Controla campanhas promocionais com regras flexíveis e compliance comercial
/// </summary>
/// <remarks>
/// Esta entidade implementa sistema completo de promoções farmacêuticas brasileiras,
/// incluindo descontos progressivos, combos, cashback e validações específicas
/// para medicamentos controlados conforme regulamentações do setor.
/// </remarks>
public class PromocaoEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único da promoção
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
    /// Nome da promoção
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da promoção
    /// </summary>
    [StringLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Código único da promoção para cupons e referência
    /// </summary>
    [StringLength(50)]
    public string? Codigo { get; set; }

    /// <summary>
    /// Tipo de promoção
    /// </summary>
    [Required]
    public TipoPromocao Tipo { get; set; }

    /// <summary>
    /// Status atual da promoção
    /// </summary>
    [Required]
    public StatusPromocao Status { get; set; } = StatusPromocao.Inativa;

    /// <summary>
    /// Data de início da promoção
    /// </summary>
    [Required]
    public DateTime DataInicio { get; set; }

    /// <summary>
    /// Data de fim da promoção
    /// </summary>
    [Required]
    public DateTime DataFim { get; set; }

    /// <summary>
    /// Valor do desconto (em reais para tipo fixo, percentual para tipo percentual)
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor de desconto deve ser positivo")]
    public decimal ValorDesconto { get; set; } = 0;

    /// <summary>
    /// Percentual de desconto (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal PercentualDesconto { get; set; } = 0;

    /// <summary>
    /// Valor mínimo para aplicar a promoção
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor mínimo deve ser positivo")]
    public decimal ValorMinimo { get; set; } = 0;

    /// <summary>
    /// Valor máximo de desconto (para promoções percentuais)
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor máximo deve ser positivo")]
    public decimal? ValorMaximoDesconto { get; set; }

    /// <summary>
    /// Quantidade mínima de itens para aplicar a promoção
    /// </summary>
    [Range(1, 9999, ErrorMessage = "Quantidade mínima deve ser positiva")]
    public int QuantidadeMinima { get; set; } = 1;

    /// <summary>
    /// Limite máximo de uso da promoção (null = ilimitado)
    /// </summary>
    [Range(1, 999999, ErrorMessage = "Limite de uso deve ser positivo")]
    public int? LimiteUso { get; set; }

    /// <summary>
    /// Contador de quantas vezes a promoção foi usada
    /// </summary>
    public int ContadorUso { get; set; } = 0;

    /// <summary>
    /// Limite por cliente (quantas vezes o mesmo cliente pode usar)
    /// </summary>
    [Range(1, 999, ErrorMessage = "Limite por cliente deve ser positivo")]
    public int? LimitePorCliente { get; set; }

    /// <summary>
    /// Indica se aplica apenas no primeiro produto (para promoções "leve mais por menos")
    /// </summary>
    public bool AplicaApenasPrimeiro { get; set; } = false;

    /// <summary>
    /// Indica se é aplicável a medicamentos controlados
    /// </summary>
    public bool AplicavelMedicamentosControlados { get; set; } = true;

    /// <summary>
    /// Indica se é combinável com outras promoções
    /// </summary>
    public bool CombinavelComOutras { get; set; } = false;

    /// <summary>
    /// Prioridade da promoção (maior número = maior prioridade)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Prioridade deve estar entre 0 e 100")]
    public int Prioridade { get; set; } = 0;

    /// <summary>
    /// Dias da semana em que a promoção é válida (formato: 1234567 = Dom a Sab)
    /// </summary>
    [StringLength(7)]
    public string? DiasValidos { get; set; }

    /// <summary>
    /// Horário de início (formato HH:mm)
    /// </summary>
    [StringLength(5)]
    public string? HorarioInicio { get; set; }

    /// <summary>
    /// Horário de fim (formato HH:mm)
    /// </summary>
    [StringLength(5)]
    public string? HorarioFim { get; set; }

    /// <summary>
    /// Condições especiais em JSON
    /// </summary>
    [StringLength(2000)]
    public string? CondicoesEspeciais { get; set; }

    /// <summary>
    /// Configurações específicas do tipo de promoção (JSON)
    /// </summary>
    [StringLength(2000)]
    public string? ConfiguracoesTipo { get; set; }

    /// <summary>
    /// Observações internas sobre a promoção
    /// </summary>
    [StringLength(1000)]
    public string? ObservacoesInternas { get; set; }

    /// <summary>
    /// Texto para exibição ao cliente
    /// </summary>
    [StringLength(500)]
    public string? TextoCliente { get; set; }

    /// <summary>
    /// Banner/imagem da promoção (URL ou path)
    /// </summary>
    [StringLength(500)]
    public string? ImagemPromocao { get; set; }

    /// <summary>
    /// Cor tema da promoção (hexadecimal)
    /// </summary>
    [StringLength(7)]
    public string? CorTema { get; set; }

    /// <summary>
    /// Usuário que criou a promoção
    /// </summary>
    [Required]
    public Guid CriadoPorUsuarioId { get; set; }

    /// <summary>
    /// Usuário criador da promoção
    /// </summary>
    public UsuarioEntity CriadoPorUsuario { get; set; } = null!;

    /// <summary>
    /// Usuário que aprovou a promoção
    /// </summary>
    public Guid? AprovadoPorUsuarioId { get; set; }

    /// <summary>
    /// Usuário que aprovou a promoção
    /// </summary>
    public UsuarioEntity? AprovadoPorUsuario { get; set; }

    /// <summary>
    /// Data de aprovação da promoção
    /// </summary>
    public DateTime? DataAprovacao { get; set; }

    // Coleções de navegação

    /// <summary>
    /// Produtos incluídos na promoção
    /// </summary>
    public ICollection<PromocaoProdutoEntity> Produtos { get; set; } = new List<PromocaoProdutoEntity>();

    /// <summary>
    /// Categorias incluídas na promoção
    /// </summary>
    public ICollection<PromocaoCategoriaEntity> Categorias { get; set; } = new List<PromocaoCategoriaEntity>();

    /// <summary>
    /// Vendas que utilizaram esta promoção
    /// </summary>
    public ICollection<VendaEntity> Vendas { get; set; } = new List<VendaEntity>();

    /// <summary>
    /// Itens de venda que utilizaram esta promoção
    /// </summary>
    public ICollection<ItemVendaEntity> ItensVenda { get; set; } = new List<ItemVendaEntity>();

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
    /// Verifica se a promoção está ativa no momento atual
    /// </summary>
    /// <returns>True se ativa</returns>
    public bool IsAtiva()
    {
        var agora = DateTime.Now;
        return Status == StatusPromocao.Ativa &&
               agora >= DataInicio &&
               agora <= DataFim &&
               !IsDeleted &&
               !IsExpirada();
    }

    /// <summary>
    /// Verifica se a promoção está expirada
    /// </summary>
    /// <returns>True se expirada</returns>
    public bool IsExpirada()
    {
        return DateTime.Now > DataFim;
    }

    /// <summary>
    /// Verifica se ainda não iniciou
    /// </summary>
    /// <returns>True se ainda não iniciou</returns>
    public bool AindaNaoIniciou()
    {
        return DateTime.Now < DataInicio;
    }

    /// <summary>
    /// Verifica se atingiu o limite de uso
    /// </summary>
    /// <returns>True se atingiu o limite</returns>
    public bool AtingiuLimiteUso()
    {
        return LimiteUso.HasValue && ContadorUso >= LimiteUso.Value;
    }

    /// <summary>
    /// Verifica se a promoção é válida no horário atual
    /// </summary>
    /// <returns>True se válida no horário</returns>
    public bool IsValidaNoHorario()
    {
        if (string.IsNullOrEmpty(HorarioInicio) || string.IsNullOrEmpty(HorarioFim))
            return true;

        var agora = DateTime.Now.TimeOfDay;
        
        if (TimeSpan.TryParse(HorarioInicio, out var inicio) && 
            TimeSpan.TryParse(HorarioFim, out var fim))
        {
            if (inicio <= fim)
            {
                return agora >= inicio && agora <= fim;
            }
            else
            {
                // Horário que cruza a meia-noite
                return agora >= inicio || agora <= fim;
            }
        }

        return true;
    }

    /// <summary>
    /// Verifica se a promoção é válida no dia da semana atual
    /// </summary>
    /// <returns>True se válida no dia</returns>
    public bool IsValidaNoDia()
    {
        if (string.IsNullOrEmpty(DiasValidos))
            return true;

        var diaSemana = (int)DateTime.Now.DayOfWeek; // 0=Domingo, 6=Sábado
        return DiasValidos.Length > diaSemana && DiasValidos[diaSemana] == '1';
    }

    /// <summary>
    /// Verifica se pode ser aplicada considerando todas as condições
    /// </summary>
    /// <returns>True se aplicável</returns>
    public bool PodeSerAplicada()
    {
        return IsAtiva() &&
               IsValidaNoHorario() &&
               IsValidaNoDia() &&
               !AtingiuLimiteUso();
    }

    /// <summary>
    /// Calcula o desconto para um valor específico
    /// </summary>
    /// <param name="valor">Valor sobre o qual aplicar o desconto</param>
    /// <returns>Valor do desconto</returns>
    public decimal CalcularDesconto(decimal valor)
    {
        if (valor < ValorMinimo)
            return 0;

        decimal desconto = 0;

        switch (Tipo)
        {
            case TipoPromocao.DescontoFixo:
                desconto = ValorDesconto;
                break;

            case TipoPromocao.DescontoPercentual:
                desconto = valor * (PercentualDesconto / 100);
                if (ValorMaximoDesconto.HasValue && desconto > ValorMaximoDesconto.Value)
                    desconto = ValorMaximoDesconto.Value;
                break;

            case TipoPromocao.DescontoProgressivo:
                desconto = CalcularDescontoProgressivo(valor);
                break;

            default:
                break;
        }

        // Não pode ser maior que o valor original
        return Math.Min(desconto, valor);
    }

    /// <summary>
    /// Calcula desconto progressivo baseado no valor
    /// </summary>
    /// <param name="valor">Valor da compra</param>
    /// <returns>Desconto calculado</returns>
    private decimal CalcularDescontoProgressivo(decimal valor)
    {
        // Implementação básica - pode ser expandida com configurações específicas
        if (valor >= ValorMinimo * 3)
            return valor * 0.20m; // 20% para valores altos
        else if (valor >= ValorMinimo * 2)
            return valor * 0.15m; // 15% para valores médios
        else
            return valor * (PercentualDesconto / 100); // Percentual padrão
    }

    /// <summary>
    /// Incrementa o contador de uso da promoção
    /// </summary>
    /// <param name="usuarioId">Usuário que está usando a promoção</param>
    public void IncrementarUso(Guid usuarioId)
    {
        ContadorUso++;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;

        // Desativa automaticamente se atingiu o limite
        if (AtingiuLimiteUso())
        {
            Status = StatusPromocao.Esgotada;
        }
    }

    /// <summary>
    /// Aprova a promoção
    /// </summary>
    /// <param name="usuarioAprovadorId">Usuário que está aprovando</param>
    public void Aprovar(Guid usuarioAprovadorId)
    {
        AprovadoPorUsuarioId = usuarioAprovadorId;
        DataAprovacao = DateTime.UtcNow;
        Status = StatusPromocao.Ativa;
        UpdatedBy = usuarioAprovadorId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ativa a promoção
    /// </summary>
    /// <param name="usuarioId">Usuário que está ativando</param>
    public void Ativar(Guid usuarioId)
    {
        Status = StatusPromocao.Ativa;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desativa a promoção
    /// </summary>
    /// <param name="usuarioId">Usuário que está desativando</param>
    /// <param name="motivo">Motivo da desativação</param>
    public void Desativar(Guid usuarioId, string? motivo = null)
    {
        Status = StatusPromocao.Inativa;
        if (!string.IsNullOrEmpty(motivo))
        {
            ObservacoesInternas += $"\nDesativada em {DateTime.Now:dd/MM/yyyy HH:mm}: {motivo}";
        }
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Pausa a promoção temporariamente
    /// </summary>
    /// <param name="usuarioId">Usuário que está pausando</param>
    /// <param name="motivo">Motivo da pausa</param>
    public void Pausar(Guid usuarioId, string? motivo = null)
    {
        Status = StatusPromocao.Pausada;
        if (!string.IsNullOrEmpty(motivo))
        {
            ObservacoesInternas += $"\nPausada em {DateTime.Now:dd/MM/yyyy HH:mm}: {motivo}";
        }
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a promoção se aplica a um produto específico
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <returns>True se aplicável</returns>
    public bool AplicaAoProduto(Guid produtoId)
    {
        return Produtos.Any(p => p.ProdutoId == produtoId);
    }

    /// <summary>
    /// Calcula dias restantes da promoção
    /// </summary>
    /// <returns>Número de dias restantes</returns>
    public int DiasRestantes()
    {
        if (IsExpirada())
            return 0;

        return Math.Max(0, (DataFim - DateTime.Now).Days);
    }

    /// <summary>
    /// Obtém resumo da promoção para exibição
    /// </summary>
    /// <returns>String com resumo</returns>
    public string ObterResumo()
    {
        var desconto = Tipo == TipoPromocao.DescontoPercentual 
            ? $"{PercentualDesconto}% OFF"
            : $"R$ {ValorDesconto:F2} OFF";

        var prazo = DiasRestantes() > 0 
            ? $"- Válida por mais {DiasRestantes()} dias"
            : "- Expira hoje!";

        return $"{Nome} - {desconto} {prazo}";
    }

    /// <summary>
    /// Verifica se a promoção precisa de aprovação
    /// </summary>
    /// <returns>True se precisa aprovar</returns>
    public bool PrecisaAprovacao()
    {
        // Promoções com desconto alto ou sem limite precisam de aprovação
        return PercentualDesconto > 30 || 
               ValorDesconto > 100 || 
               !LimiteUso.HasValue ||
               Status == StatusPromocao.PendenteAprovacao;
    }

    /// <summary>
    /// Obtém cor tema padrão baseada no tipo
    /// </summary>
    /// <returns>Cor hexadecimal</returns>
    public string ObterCorTema()
    {
        if (!string.IsNullOrEmpty(CorTema))
            return CorTema;

        return Tipo switch
        {
            TipoPromocao.DescontoFixo => "#FF5722",          // Vermelho
            TipoPromocao.DescontoPercentual => "#4CAF50",    // Verde
            TipoPromocao.DescontoProgressivo => "#9C27B0",   // Roxo
            TipoPromocao.LeveMaisPorMenos => "#2196F3",      // Azul
            TipoPromocao.Cashback => "#FF9800",              // Laranja
            TipoPromocao.FreteGratis => "#607D8B",           // Azul acinzentado
            _ => "#757575"                                    // Cinza padrão
        };
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da promoção</returns>
    public override string ToString()
    {
        return $"Promoção: {Nome} ({Status}) - {DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";
    }
}

/// <summary>
/// Tipos de promoção disponíveis
/// </summary>
public enum TipoPromocao
{
    /// <summary>
    /// Desconto fixo em reais
    /// </summary>
    DescontoFixo = 0,

    /// <summary>
    /// Desconto percentual
    /// </summary>
    DescontoPercentual = 1,

    /// <summary>
    /// Desconto progressivo por valor da compra
    /// </summary>
    DescontoProgressivo = 2,

    /// <summary>
    /// Promoção "leve mais pague menos" (ex: leve 3 pague 2)
    /// </summary>
    LeveMaisPorMenos = 3,

    /// <summary>
    /// Cashback em compras futuras
    /// </summary>
    Cashback = 4,

    /// <summary>
    /// Frete grátis
    /// </summary>
    FreteGratis = 5,

    /// <summary>
    /// Combo/kit promocional
    /// </summary>
    Combo = 6,

    /// <summary>
    /// Brinde por compra
    /// </summary>
    Brinde = 7
}

/// <summary>
/// Status possíveis de uma promoção
/// </summary>
public enum StatusPromocao
{
    /// <summary>
    /// Promoção inativa
    /// </summary>
    Inativa = 0,

    /// <summary>
    /// Promoção ativa
    /// </summary>
    Ativa = 1,

    /// <summary>
    /// Promoção pausada temporariamente
    /// </summary>
    Pausada = 2,

    /// <summary>
    /// Promoção pendente de aprovação
    /// </summary>
    PendenteAprovacao = 3,

    /// <summary>
    /// Promoção esgotada (atingiu limite de uso)
    /// </summary>
    Esgotada = 4,

    /// <summary>
    /// Promoção expirada
    /// </summary>
    Expirada = 5,

    /// <summary>
    /// Promoção rejeitada
    /// </summary>
    Rejeitada = 6
}