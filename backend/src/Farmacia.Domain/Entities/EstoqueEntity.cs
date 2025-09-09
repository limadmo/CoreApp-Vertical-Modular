using Farmacia.Domain.Interfaces;
using Farmacia.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma movimentação de estoque farmacêutico brasileiro
/// Suporte completo a operações offline-first com sincronização automática
/// </summary>
/// <remarks>
/// Esta entidade implementa controle de estoque offline-first, permitindo que
/// farmácias operem sem internet e sincronizem dados quando a conectividade
/// for restabelecida. Inclui auditoria completa para compliance ANVISA.
/// </remarks>
public class EstoqueEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da movimentação de estoque
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) proprietária da movimentação
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do produto movimentado
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Tipo de movimentação de estoque
    /// </summary>
    [Required]
    public TipoMovimentacao Tipo { get; set; }

    /// <summary>
    /// Quantidade movimentada (sempre positiva)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Quantidade anterior antes da movimentação
    /// </summary>
    public decimal QuantidadeAnterior { get; set; }

    /// <summary>
    /// Quantidade atual após a movimentação
    /// </summary>
    public decimal QuantidadeAtual { get; set; }

    /// <summary>
    /// Motivo da movimentação (obrigatório para auditoria ANVISA)
    /// </summary>
    /// <example>Venda #12345, Compra NF 67890, Ajuste inventário</example>
    [Required]
    [StringLength(500)]
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Observações adicionais sobre a movimentação
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Identificador da venda relacionada (se aplicável)
    /// </summary>
    public Guid? VendaId { get; set; }

    /// <summary>
    /// Identificador do item de venda relacionado (se aplicável)
    /// </summary>
    public Guid? ItemVendaId { get; set; }

    /// <summary>
    /// Identificador do usuário responsável pela movimentação
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>
    /// Status de sincronização para operações offline
    /// </summary>
    public StatusSincronizacao StatusSincronizacao { get; set; } = StatusSincronizacao.SINCRONIZADO;

    /// <summary>
    /// Timestamp do cliente quando a movimentação foi criada (para ordenação offline)
    /// </summary>
    public DateTime ClienteTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp do servidor quando a movimentação foi sincronizada
    /// </summary>
    public DateTime? ServidorTimestamp { get; set; }

    /// <summary>
    /// Hash de integridade para validação de sincronização
    /// </summary>
    [StringLength(64)]
    public string? HashIntegridade { get; set; }

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
    /// Produto relacionado à movimentação
    /// </summary>
    public virtual ProdutoEntity? Produto { get; set; }

    /// <summary>
    /// Venda relacionada à movimentação (se aplicável)
    /// </summary>
    public virtual VendaEntity? Venda { get; set; }

    /// <summary>
    /// Item de venda relacionado à movimentação (se aplicável)
    /// </summary>
    public virtual ItemVendaEntity? ItemVenda { get; set; }

    /// <summary>
    /// Verifica se a movimentação aumenta o estoque
    /// </summary>
    /// <returns>True se for entrada, false se for saída</returns>
    public bool IsEntrada()
    {
        return Tipo == TipoMovimentacao.ENTRADA || 
               Tipo == TipoMovimentacao.AJUSTE;
    }

    /// <summary>
    /// Verifica se a movimentação diminui o estoque
    /// </summary>
    /// <returns>True se for saída, false se for entrada</returns>
    public bool IsSaida()
    {
        return Tipo == TipoMovimentacao.SAIDA || 
               Tipo == TipoMovimentacao.PERDA || 
               Tipo == TipoMovimentacao.VENCIMENTO ||
               Tipo == TipoMovimentacao.TRANSFERENCIA;
    }

    /// <summary>
    /// Verifica se a movimentação precisa de sincronização
    /// </summary>
    /// <returns>True se ainda não foi sincronizada</returns>
    public bool PrecisaSincronizar()
    {
        return StatusSincronizacao == StatusSincronizacao.PENDENTE ||
               StatusSincronizacao == StatusSincronizacao.ERRO ||
               StatusSincronizacao == StatusSincronizacao.CONFLITO;
    }

    /// <summary>
    /// Marca a movimentação como sincronizada
    /// </summary>
    public void MarcarComoSincronizada()
    {
        StatusSincronizacao = StatusSincronizacao.SINCRONIZADO;
        ServidorTimestamp = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula o impacto no estoque baseado no tipo de movimentação
    /// </summary>
    /// <returns>Quantidade com sinal correto (positivo para entrada, negativo para saída)</returns>
    public decimal CalcularImpactoEstoque()
    {
        return IsEntrada() ? Quantidade : -Quantidade;
    }

    /// <summary>
    /// Gera hash de integridade para validação de sincronização
    /// </summary>
    /// <returns>Hash SHA-256 dos dados críticos</returns>
    public string GerarHashIntegridade()
    {
        var dadosCriticos = $"{ProdutoId}-{Tipo}-{Quantidade}-{UsuarioId}-{ClienteTimestamp:yyyy-MM-dd HH:mm:ss}";
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dadosCriticos));
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Atualiza o timestamp de modificação
    /// </summary>
    public void AtualizarTimestamp()
    {
        DataAtualizacao = DateTime.UtcNow;
    }
}