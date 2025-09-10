using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;

namespace CoreApp.Domain.Entities.Archived;

/// <summary>
/// Entidade para armazenar clientes arquivados com mais de 10 anos
/// Tabela: clientes_log - mantém histórico para compliance LGPD e comercial
/// </summary>
/// <remarks>
/// Clientes são arquivados após 10 anos considerando LGPD e relacionamento comercial.
/// Preserva dados para auditoria enquanto respeita direitos de privacidade
/// </remarks>
[Table("clientes_log")]
public class ClienteArquivado : ArchivedEntity
{
    /// <summary>
    /// CPF/CNPJ do cliente (para consultas de auditoria)
    /// </summary>
    public string CpfCnpj { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do cliente no momento do arquivamento
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Email principal do cliente (para contato se necessário)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Data do cadastro original do cliente
    /// </summary>
    public DateTime DataCadastro { get; set; }
    
    /// <summary>
    /// Data da última compra realizada
    /// </summary>
    public DateTime? DataUltimaCompra { get; set; }
    
    /// <summary>
    /// Valor total de compras históricas (para análise comercial)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalCompras { get; set; }
    
    /// <summary>
    /// Número total de compras realizadas
    /// </summary>
    public int TotalCompras { get; set; }
    
    /// <summary>
    /// Indica se cliente utilizava medicamentos controlados
    /// Importante para histórico farmacêutico
    /// </summary>
    public bool UtilizavaMedicamentosControlados { get; set; }
    
    /// <summary>
    /// Status do cliente no momento do arquivamento
    /// </summary>
    public string StatusCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Motivo da inativação/exclusão do cliente
    /// </summary>
    public string? MotivoInativacao { get; set; }
    
    /// <summary>
    /// Data de nascimento (para análise demográfica anonimizada)
    /// </summary>
    public DateTime? DataNascimento { get; set; }
    
    /// <summary>
    /// Estado onde residia (análise regional)
    /// </summary>
    public string? Estado { get; set; }
    
    /// <summary>
    /// Cidade onde residia (análise regional)
    /// </summary>
    public string? Cidade { get; set; }
}