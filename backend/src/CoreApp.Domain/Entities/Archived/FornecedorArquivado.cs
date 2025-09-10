using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;

namespace CoreApp.Domain.Entities.Archived;

/// <summary>
/// Entidade para armazenar fornecedores arquivados com mais de 5 anos
/// Tabela: fornecedores_log - mantém histórico para auditoria comercial e fiscal
/// </summary>
/// <remarks>
/// Fornecedores são arquivados após 5 anos de inatividade.
/// Preserva histórico para auditorias fiscais e análise comercial
/// </remarks>
[Table("fornecedores_log")]
public class FornecedorArquivado : ArchivedEntity
{
    /// <summary>
    /// CNPJ do fornecedor (para consultas fiscais)
    /// </summary>
    public string Cnpj { get; set; } = string.Empty;
    
    /// <summary>
    /// Razão social do fornecedor
    /// </summary>
    public string RazaoSocial { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome fantasia do fornecedor
    /// </summary>
    public string? NomeFantasia { get; set; }
    
    /// <summary>
    /// Data do cadastro original do fornecedor
    /// </summary>
    public DateTime DataCadastro { get; set; }
    
    /// <summary>
    /// Data da última compra realizada com o fornecedor
    /// </summary>
    public DateTime? DataUltimaCompra { get; set; }
    
    /// <summary>
    /// Valor total de compras históricas
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalCompras { get; set; }
    
    /// <summary>
    /// Número total de pedidos realizados
    /// </summary>
    public int TotalPedidos { get; set; }
    
    /// <summary>
    /// Indica se fornecia medicamentos controlados
    /// Importante para auditoria ANVISA
    /// </summary>
    public bool ForneciaControlados { get; set; }
    
    /// <summary>
    /// Status do fornecedor no momento do arquivamento
    /// </summary>
    public string StatusFornecedor { get; set; } = string.Empty;
    
    /// <summary>
    /// Motivo da inativação do fornecedor
    /// </summary>
    public string? MotivoInativacao { get; set; }
    
    /// <summary>
    /// Estado do fornecedor (análise regional)
    /// </summary>
    public string? Estado { get; set; }
    
    /// <summary>
    /// Cidade do fornecedor (análise regional)
    /// </summary>
    public string? Cidade { get; set; }
    
    /// <summary>
    /// Telefone principal para contato se necessário
    /// </summary>
    public string? Telefone { get; set; }
    
    /// <summary>
    /// Email principal para contato se necessário
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Avaliação média do fornecedor (1-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal? AvaliacaoMedia { get; set; }
}