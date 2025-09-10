using System.ComponentModel.DataAnnotations;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para status de estoque farmacêutico
/// Permite configurar diferentes níveis de alerta de estoque por farmácia
/// </summary>
/// <remarks>
/// Esta entidade substitui o enum StatusEstoque por um sistema flexível que permite:
/// - Configurações globais do sistema (padrões brasileiros)
/// - Customizações específicas por farmácia
/// - Alertas personalizados baseados em percentuais ou quantidades
/// - Cores e ícones customizáveis para interface
/// - Alterações sem necessidade de deploy
/// </remarks>
public class StatusEstoqueEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do status
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global do sistema)
    /// </summary>
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único do status (usado na API e integrações)
    /// </summary>
    /// <example>NORMAL, BAIXO, CRITICO, ZERADO, EXCESSIVO</example>
    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo exibido na interface
    /// </summary>
    /// <example>Estoque Normal, Estoque Baixo, Estoque Crítico</example>
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
    /// <example>OPERACIONAL, ALERTA, CRITICO</example>
    [StringLength(30)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    /// <example>#28a745 (verde normal), #ffc107 (amarelo baixo), #dc3545 (vermelho crítico)</example>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-check-circle, fa-exclamation-triangle, fa-times-circle</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Percentual mínimo para este status (relacionado ao estoque mínimo)
    /// </summary>
    /// <example>100 (normal), 50 (baixo), 10 (crítico), 0 (zerado)</example>
    [Range(0, 1000)]
    public decimal? PercentualMinimo { get; set; }

    /// <summary>
    /// Percentual máximo para este status
    /// </summary>
    /// <example>null (normal), 100 (baixo), 50 (crítico)</example>
    [Range(0, 1000)]
    public decimal? PercentualMaximo { get; set; }

    /// <summary>
    /// Se deve gerar alerta automático
    /// </summary>
    public bool GerarAlerta { get; set; } = false;

    /// <summary>
    /// Prioridade do alerta (1 = mais alta, 5 = mais baixa)
    /// </summary>
    [Range(1, 5)]
    public int? PrioridadeAlerta { get; set; }

    /// <summary>
    /// Se deve enviar notificação por email
    /// </summary>
    public bool NotificarEmail { get; set; } = false;

    /// <summary>
    /// Se deve enviar notificação por WhatsApp
    /// </summary>
    public bool NotificarWhatsApp { get; set; } = false;

    /// <summary>
    /// Se deve bloquear vendas neste status
    /// </summary>
    public bool BloquearVendas { get; set; } = false;

    /// <summary>
    /// Mensagem de alerta customizada
    /// </summary>
    [StringLength(200)]
    public string? MensagemAlerta { get; set; }

    /// <summary>
    /// Ação recomendada para este status
    /// </summary>
    /// <example>Repor estoque, Fazer pedido urgente, Suspender vendas</example>
    [StringLength(200)]
    public string? AcaoRecomendada { get; set; }

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
    public virtual StatusEstoqueEntity? ConfiguracaoGlobal { get; set; }

    /// <summary>
    /// Configurações filhas que herdam desta
    /// </summary>
    public virtual ICollection<StatusEstoqueEntity> ConfiguracoesFilhas { get; set; } = new List<StatusEstoqueEntity>();

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
    /// Calcula o status baseado na quantidade atual e estoque mínimo
    /// </summary>
    /// <param name="quantidadeAtual">Quantidade atual em estoque</param>
    /// <param name="estoqueMinimo">Estoque mínimo configurado</param>
    /// <returns>True se a quantidade se enquadra neste status</returns>
    public bool AplicaStatus(decimal quantidadeAtual, decimal estoqueMinimo)
    {
        // Se estoque mínimo é zero, usar quantidade absoluta
        if (estoqueMinimo == 0)
        {
            return Codigo switch
            {
                "ZERADO" => quantidadeAtual == 0,
                "CRITICO" => quantidadeAtual > 0 && quantidadeAtual <= 5,
                "BAIXO" => quantidadeAtual > 5 && quantidadeAtual <= 20,
                "NORMAL" => quantidadeAtual > 20 && quantidadeAtual <= 100,
                "EXCESSIVO" => quantidadeAtual > 100,
                _ => false
            };
        }

        // Calcular percentual em relação ao estoque mínimo
        var percentual = estoqueMinimo > 0 ? (quantidadeAtual / estoqueMinimo) * 100 : 0;

        // Verificar se está dentro do range
        if (PercentualMinimo.HasValue && percentual < PercentualMinimo.Value)
            return false;

        if (PercentualMaximo.HasValue && percentual > PercentualMaximo.Value)
            return false;

        return true;
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
            "NORMAL" => "#28A745",      // Verde
            "BAIXO" => "#FFC107",       // Amarelo
            "CRITICO" => "#DC3545",     // Vermelho
            "ZERADO" => "#6C757D",      // Cinza
            "EXCESSIVO" => "#17A2B8",   // Azul
            _ => "#6C757D"              // Cinza padrão
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
            "NORMAL" => "fa-check-circle",
            "BAIXO" => "fa-exclamation-triangle",
            "CRITICO" => "fa-times-circle",
            "ZERADO" => "fa-ban",
            "EXCESSIVO" => "fa-arrow-up",
            _ => "fa-circle"
        };
    }

    /// <summary>
    /// Obtém mensagem de alerta padrão
    /// </summary>
    /// <returns>Mensagem de alerta</returns>
    public string ObterMensagemAlertaPadrao()
    {
        if (!string.IsNullOrEmpty(MensagemAlerta))
            return MensagemAlerta;

        return Codigo switch
        {
            "BAIXO" => "Estoque baixo - considere fazer novo pedido",
            "CRITICO" => "Estoque crítico - reposição urgente necessária",
            "ZERADO" => "Produto sem estoque - vendas indisponíveis",
            "EXCESSIVO" => "Estoque excessivo - verificar necessidade",
            _ => "Status do estoque atualizado"
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
    public StatusEstoqueEntity CriarPersonalizacao(string tenantId)
    {
        return new StatusEstoqueEntity
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
            PercentualMinimo = PercentualMinimo,
            PercentualMaximo = PercentualMaximo,
            GerarAlerta = GerarAlerta,
            PrioridadeAlerta = PrioridadeAlerta,
            NotificarEmail = NotificarEmail,
            NotificarWhatsApp = NotificarWhatsApp,
            BloquearVendas = BloquearVendas,
            MensagemAlerta = MensagemAlerta,
            AcaoRecomendada = AcaoRecomendada,
            Ativo = Ativo,
            IsSistema = false, // Personalizações nunca são do sistema
            ConfiguracaoGlobalId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_HERANCA"
        };
    }

    /// <summary>
    /// Cria novo status de estoque padrão
    /// </summary>
    /// <param name="codigo">Código do status</param>
    /// <param name="nome">Nome descritivo</param>
    /// <param name="percentualMin">Percentual mínimo</param>
    /// <param name="percentualMax">Percentual máximo</param>
    /// <returns>Nova instância de status</returns>
    public static StatusEstoqueEntity CriarStatusPadrao(
        string codigo,
        string nome,
        decimal? percentualMin = null,
        decimal? percentualMax = null)
    {
        return new StatusEstoqueEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            PercentualMinimo = percentualMin,
            PercentualMaximo = percentualMax,
            IsSistema = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true,
            CriadoPor = "SISTEMA_PADRAO"
        };
    }
}