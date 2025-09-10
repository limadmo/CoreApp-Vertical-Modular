using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;

namespace CoreApp.Domain.Entities.Archived;

/// <summary>
/// Entidade para armazenar vendas arquivadas com mais de 7 anos
/// Tabela: vendas_log - mantém histórico para compliance fiscal brasileiro
/// </summary>
/// <remarks>
/// Vendas são arquivadas após 7 anos conforme regulamentação fiscal brasileira.
/// Mantém todos os dados originais para auditoria e compliance tributário
/// </remarks>
[Table("vendas_log")]
public class VendaArquivada : ArchivedEntity
{
    /// <summary>
    /// Data original da venda (extraída dos dados originais para facilitar consultas)
    /// </summary>
    public DateTime DataVenda { get; set; }
    
    /// <summary>
    /// Valor total da venda (extraído dos dados originais para facilitar consultas)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    
    /// <summary>
    /// CPF/CNPJ do cliente (extraído para consultas de auditoria fiscal)
    /// </summary>
    public string? CpfCnpjCliente { get; set; }
    
    /// <summary>
    /// Indica se a venda continha medicamentos controlados
    /// Importante para auditoria ANVISA
    /// </summary>
    public bool ContinhaMedicamentosControlados { get; set; }
    
    /// <summary>
    /// Número da nota fiscal da venda (para rastreamento fiscal)
    /// </summary>
    public string? NumeroNotaFiscal { get; set; }
    
    /// <summary>
    /// Série da nota fiscal (compliance fiscal)
    /// </summary>
    public string? SerieNotaFiscal { get; set; }
}