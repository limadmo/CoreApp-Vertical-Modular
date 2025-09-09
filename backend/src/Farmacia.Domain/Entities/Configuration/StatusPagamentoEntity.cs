using System.ComponentModel.DataAnnotations;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para status de pagamento farmacêutico
/// Permite configurar diferentes estados de pagamento por farmácia
/// </summary>
/// <remarks>
/// Esta entidade substitui o enum StatusPagamento por um sistema flexível que permite:
/// - Configurações globais do sistema (padrões brasileiros)
/// - Customizações específicas por farmácia
/// - Integração com gateways de pagamento nacionais
/// - Cores e ícones customizáveis para interface
/// - Alterações sem necessidade de deploy
/// </remarks>
public class StatusPagamentoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do status
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global do sistema)
    /// </summary>
    [StringLength(100)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Código único do status (usado na API e integrações)
    /// </summary>
    /// <example>PENDENTE, PAGO, CANCELADO, PROCESSANDO, REJEITADO, PARCIAL</example>
    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo exibido na interface
    /// </summary>
    /// <example>Pagamento Pendente, Pago, Cancelado, Em Processamento</example>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do status
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Categoria do status para agrupamento
    /// </summary>
    /// <example>PENDENTE, FINALIZADO, ERRO</example>
    [StringLength(30)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    /// <example>#ffc107 (amarelo pendente), #28a745 (verde pago), #dc3545 (vermelho cancelado)</example>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-clock, fa-check-circle, fa-times-circle, fa-spinner</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Se é um status final (não pode mudar para outro status)
    /// </summary>
    public bool IsStatusFinal { get; set; } = false;

    /// <summary>
    /// Se permite cancelamento a partir deste status
    /// </summary>
    public bool PermiteCancelamento { get; set; } = true;

    /// <summary>
    /// Se permite estorno a partir deste status
    /// </summary>
    public bool PermiteEstorno { get; set; } = false;

    /// <summary>
    /// Se deve gerar comprovante automático
    /// </summary>
    public bool GerarComprovante { get; set; } = false;

    /// <summary>
    /// Se deve enviar notificação automática
    /// </summary>
    public bool EnviarNotificacao { get; set; } = false;

    /// <summary>
    /// Se deve liberar produtos/serviços
    /// </summary>
    public bool LiberarProdutos { get; set; } = false;

    /// <summary>
    /// Se deve atualizar estoque automaticamente
    /// </summary>
    public bool AtualizarEstoque { get; set; } = false;

    /// <summary>
    /// Tempo limite em minutos para ficar neste status
    /// </summary>
    /// <remarks>
    /// Usado para pagamentos que têm tempo limite (PIX, boleto, etc.)
    /// </remarks>
    [Range(1, 43200)] // Máximo 30 dias
    public int? TempoLimiteMinutos { get; set; }

    /// <summary>
    /// Status para o qual deve transicionar automaticamente após timeout
    /// </summary>
    [StringLength(30)]
    public string? StatusAposTimeout { get; set; }

    /// <summary>
    /// Mensagem padrão para este status
    /// </summary>
    [StringLength(200)]
    public string? MensagemPadrao { get; set; }

    /// <summary>
    /// Próximos status válidos (transições permitidas)
    /// </summary>
    [StringLength(500)]
    public string? ProximosStatusValidos { get; set; }

    /// <summary>
    /// Se deve aparecer nos relatórios financeiros
    /// </summary>
    public bool AparecerRelatorios { get; set; } = true;

    /// <summary>
    /// Status ativo/inativo (permite desativar sem perder histórico)
    /// </summary>
    public bool Ativo { get; set; } = true;

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
    public virtual StatusPagamentoEntity? ConfiguracaoGlobal { get; set; }

    /// <summary>
    /// Configurações filhas que herdam desta
    /// </summary>
    public virtual ICollection<StatusPagamentoEntity> ConfiguracoesFilhas { get; set; } = new List<StatusPagamentoEntity>();

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
    /// Verifica se pode transicionar para outro status
    /// </summary>
    /// <param name="novoStatus">Código do novo status</param>
    /// <returns>True se transição é permitida</returns>
    public bool PodeTransicionarPara(string novoStatus)
    {
        if (IsStatusFinal)
            return false;

        if (string.IsNullOrEmpty(ProximosStatusValidos))
            return true;

        var statusValidos = ProximosStatusValidos.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return statusValidos.Contains(novoStatus.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica se o status expirou baseado no tempo limite
    /// </summary>
    /// <param name="dataInicio">Data de início do status</param>
    /// <returns>True se status expirou</returns>
    public bool StatusExpirou(DateTime dataInicio)
    {
        if (!TempoLimiteMinutos.HasValue)
            return false;

        var tempoLimite = dataInicio.AddMinutes(TempoLimiteMinutos.Value);
        return DateTime.Now > tempoLimite;
    }

    /// <summary>
    /// Obtém a cor padrão baseada no código do status
    /// </summary>
    /// <returns>Código da cor hexadecimal</returns>
    public string ObterCorPadrao()
    {
        if (!string.IsNullOrEmpty(Cor))
            return Cor;

        return Codigo switch
        {
            "PENDENTE" => "#FFC107",       // Amarelo
            "PAGO" => "#28A745",           // Verde
            "CANCELADO" => "#DC3545",      // Vermelho
            "PROCESSANDO" => "#007BFF",    // Azul
            "REJEITADO" => "#6C757D",      // Cinza
            "PARCIAL" => "#FD7E14",        // Laranja
            "ESTORNADO" => "#E83E8C",      // Rosa
            _ => "#6C757D"                 // Cinza padrão
        };
    }

    /// <summary>
    /// Obtém o ícone padrão baseado no código do status
    /// </summary>
    /// <returns>Classe do ícone</returns>
    public string ObterIconePadrao()
    {
        if (!string.IsNullOrEmpty(Icone))
            return Icone;

        return Codigo switch
        {
            "PENDENTE" => "fa-clock",
            "PAGO" => "fa-check-circle",
            "CANCELADO" => "fa-times-circle",
            "PROCESSANDO" => "fa-spinner",
            "REJEITADO" => "fa-ban",
            "PARCIAL" => "fa-adjust",
            "ESTORNADO" => "fa-undo",
            _ => "fa-question-circle"
        };
    }

    /// <summary>
    /// Obtém mensagem padrão baseada no código do status
    /// </summary>
    /// <returns>Mensagem padrão</returns>
    public string ObterMensagemPadrao()
    {
        if (!string.IsNullOrEmpty(MensagemPadrao))
            return MensagemPadrao;

        return Codigo switch
        {
            "PENDENTE" => "Aguardando confirmação do pagamento",
            "PAGO" => "Pagamento confirmado com sucesso",
            "CANCELADO" => "Pagamento cancelado",
            "PROCESSANDO" => "Pagamento sendo processado",
            "REJEITADO" => "Pagamento rejeitado - verifique os dados",
            "PARCIAL" => "Pagamento parcial realizado",
            "ESTORNADO" => "Pagamento estornado",
            _ => "Status do pagamento atualizado"
        };
    }

    /// <summary>
    /// Obtém próximos status válidos como lista
    /// </summary>
    /// <returns>Lista de códigos de status válidos</returns>
    public List<string> ObterProximosStatusValidos()
    {
        if (string.IsNullOrEmpty(ProximosStatusValidos))
            return new List<string>();

        return ProximosStatusValidos
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
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
    public StatusPagamentoEntity CriarPersonalizacao(string tenantId)
    {
        return new StatusPagamentoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            Categoria = Categoria,
            Cor = Cor,
            Icone = Icone,
            Ordem = Ordem,
            IsStatusFinal = IsStatusFinal,
            PermiteCancelamento = PermiteCancelamento,
            PermiteEstorno = PermiteEstorno,
            GerarComprovante = GerarComprovante,
            EnviarNotificacao = EnviarNotificacao,
            LiberarProdutos = LiberarProdutos,
            AtualizarEstoque = AtualizarEstoque,
            TempoLimiteMinutos = TempoLimiteMinutos,
            StatusAposTimeout = StatusAposTimeout,
            MensagemPadrao = MensagemPadrao,
            ProximosStatusValidos = ProximosStatusValidos,
            AparecerRelatorios = AparecerRelatorios,
            Ativo = Ativo,
            IsSistema = false, // Personalizações nunca são do sistema
            ConfiguracaoGlobalId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_HERANCA"
        };
    }

    /// <summary>
    /// Cria novo status de pagamento padrão
    /// </summary>
    /// <param name="codigo">Código do status</param>
    /// <param name="nome">Nome descritivo</param>
    /// <param name="categoria">Categoria</param>
    /// <param name="isStatusFinal">Se é status final</param>
    /// <returns>Nova instância de status</returns>
    public static StatusPagamentoEntity CriarStatusPadrao(
        string codigo,
        string nome,
        string categoria = "GERAL",
        bool isStatusFinal = false)
    {
        return new StatusPagamentoEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            Categoria = categoria,
            IsStatusFinal = isStatusFinal,
            IsSistema = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true,
            CriadoPor = "SISTEMA_PADRAO"
        };
    }
}