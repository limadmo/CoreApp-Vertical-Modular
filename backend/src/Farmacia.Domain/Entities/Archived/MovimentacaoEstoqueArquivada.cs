using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Entities.Base;

namespace Farmacia.Domain.Entities.Archived;

/// <summary>
/// Entidade para armazenar movimentações de estoque arquivadas com mais de 5 anos
/// Tabela: estoque_movimentacoes_log - mantém histórico para auditoria farmacêutica
/// </summary>
/// <remarks>
/// Movimentações são arquivadas após 5 anos conforme padrão farmacêutico brasileiro.
/// Preserva histórico completo para auditorias ANVISA e controle de lotes
/// </remarks>
[Table("estoque_movimentacoes_log")]
public class MovimentacaoEstoqueArquivada : ArchivedEntity
{
    /// <summary>
    /// Data da movimentação (extraída dos dados originais para facilitar consultas)
    /// </summary>
    public DateTime DataMovimentacao { get; set; }
    
    /// <summary>
    /// ID do produto movimentado (para rastreamento histórico)
    /// </summary>
    public Guid ProdutoId { get; set; }
    
    /// <summary>
    /// Nome do produto no momento da movimentação (preserva nome histórico)
    /// </summary>
    public string NomeProduto { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo da movimentação (ENTRADA/SAIDA/TRANSFERENCIA/AJUSTE)
    /// </summary>
    public string TipoMovimentacao { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantidade movimentada
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }
    
    /// <summary>
    /// Saldo anterior à movimentação
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal SaldoAnterior { get; set; }
    
    /// <summary>
    /// Saldo após a movimentação
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal SaldoAtual { get; set; }
    
    /// <summary>
    /// Lote do produto (importante para rastreabilidade farmacêutica)
    /// </summary>
    public string? Lote { get; set; }
    
    /// <summary>
    /// Data de validade do lote (auditoria ANVISA)
    /// </summary>
    public DateTime? DataValidade { get; set; }
    
    /// <summary>
    /// Indica se era medicamento controlado no momento da movimentação
    /// </summary>
    public bool EraControlado { get; set; }
    
    /// <summary>
    /// Classificação ANVISA no momento da movimentação
    /// </summary>
    public string? ClassificacaoAnvisa { get; set; }
    
    /// <summary>
    /// ID do usuário que executou a movimentação
    /// </summary>
    public Guid UsuarioMovimentacaoId { get; set; }
    
    /// <summary>
    /// Nome do usuário no momento da movimentação
    /// </summary>
    public string NomeUsuario { get; set; } = string.Empty;
}