using Farmacia.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para tipos de movimentação de estoque
/// Sistema flexível que permite customização por tenant farmacêutico
/// </summary>
/// <remarks>
/// Esta entidade substitui os enums rígidos por um sistema configurável que permite:
/// - Configurações globais do sistema (ANVISA, padrões brasileiros)
/// - Customizações específicas por farmácia
/// - Herança hierárquica: Global → Tenant → Usuario
/// - Alterações sem necessidade de deploy
/// </remarks>
public class TipoMovimentacaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do tipo de movimentação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global do sistema)
    /// </summary>
    [StringLength(100)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Código único do tipo (usado na API e integrações)
    /// </summary>
    /// <example>ENTRADA_COMPRA, SAIDA_VENDA, AJUSTE_INVENTARIO</example>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo exibido na interface
    /// </summary>
    /// <example>Entrada por Compra, Saída por Venda</example>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do tipo de movimentação
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Indica se a movimentação aumenta o estoque (true) ou diminui (false)
    /// </summary>
    [Required]
    public bool IsEntrada { get; set; }

    /// <summary>
    /// Categoria da movimentação para agrupamento
    /// </summary>
    /// <example>OPERACIONAL, AJUSTE, PERDA, TRANSFERENCIA</example>
    [StringLength(50)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    /// <example>#28a745 (verde para entradas), #dc3545 (vermelho para saídas)</example>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-arrow-up, fa-arrow-down, fa-adjust</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Indica se o motivo é obrigatório para auditoria
    /// </summary>
    public bool RequerMotivo { get; set; } = true;

    /// <summary>
    /// Indica se requer aprovação de supervisor/farmacêutico
    /// </summary>
    public bool RequerAprovacao { get; set; }

    /// <summary>
    /// Nível de permissão mínimo para usar este tipo
    /// </summary>
    /// <example>BALCONISTA, SUPERVISOR, FARMACEUTICO, GERENTE</example>
    [StringLength(30)]
    public string? NivelPermissao { get; set; }

    /// <summary>
    /// Indica se afeta o cálculo de custo médio ponderado
    /// </summary>
    public bool AfetaCustoMedio { get; set; }

    /// <summary>
    /// Indica se deve gerar alerta para gestão
    /// </summary>
    public bool GerarAlerta { get; set; }

    /// <summary>
    /// Regras de validação customizáveis em formato JSON
    /// </summary>
    /// <example>{"quantidade_maxima": 1000, "requer_lote": true}</example>
    [StringLength(1000)]
    public string? RegrasValidacao { get; set; }

    /// <summary>
    /// Status ativo/inativo (permite desativar sem perder histórico)
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se é uma configuração padrão do sistema (não pode ser removida)
    /// </summary>
    public bool IsSistema { get; set; }

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
    public virtual TipoMovimentacaoEntity? ConfiguracaoGlobal { get; set; }

    /// <summary>
    /// Configurações filhas que herdam desta
    /// </summary>
    public virtual ICollection<TipoMovimentacaoEntity> ConfiguracoesFilhas { get; set; } = new List<TipoMovimentacaoEntity>();

    /// <summary>
    /// Movimentações que usam este tipo
    /// </summary>
    public virtual ICollection<EstoqueEntity> Movimentacoes { get; set; } = new List<EstoqueEntity>();

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
    /// Verifica se pode ser removido (não é sistema e não tem movimentações)
    /// </summary>
    /// <returns>True se pode ser removido</returns>
    public bool PodeSerRemovido()
    {
        return !IsSistema && !Movimentacoes.Any();
    }

    /// <summary>
    /// Calcula o impacto no estoque baseado no tipo
    /// </summary>
    /// <param name="quantidade">Quantidade movimentada</param>
    /// <returns>Quantidade com sinal correto (+ para entrada, - para saída)</returns>
    public decimal CalcularImpactoEstoque(decimal quantidade)
    {
        return IsEntrada ? quantidade : -quantidade;
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
    public TipoMovimentacaoEntity CriarPersonalizacao(string tenantId)
    {
        return new TipoMovimentacaoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            IsEntrada = IsEntrada,
            Categoria = Categoria,
            Cor = Cor,
            Icone = Icone,
            Ordem = Ordem,
            RequerMotivo = RequerMotivo,
            RequerAprovacao = RequerAprovacao,
            NivelPermissao = NivelPermissao,
            AfetaCustoMedio = AfetaCustoMedio,
            GerarAlerta = GerarAlerta,
            RegrasValidacao = RegrasValidacao,
            Ativo = Ativo,
            IsSistema = false, // Personalizações nunca são do sistema
            ConfiguracaoGlobalId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_HERANCA"
        };
    }
}