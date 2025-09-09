using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Farmacia.Domain.Entities.Base;

/// <summary>
/// Entidade base para tabelas de arquivo (_log) no sistema farmacêutico brasileiro
/// Contém metadados adicionais para auditoria histórica e compliance ANVISA
/// </summary>
/// <remarks>
/// Esta classe base é utilizada para todas as tabelas _log que armazenam
/// dados arquivados automaticamente após período de retenção farmacêutica
/// </remarks>
public abstract class ArchivedEntity
{
    /// <summary>
    /// Identificador único do registro arquivado
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// ID original da entidade na tabela principal
    /// Permite rastreamento e referência ao dado original
    /// </summary>
    [Required]
    public Guid OriginalId { get; set; }
    
    /// <summary>
    /// Dados completos da entidade em JSON para preservação total
    /// Garante que nenhuma informação seja perdida no arquivamento
    /// </summary>
    [Required]
    public string DadosOriginais { get; set; } = string.Empty;
    
    /// <summary>
    /// Data que o registro foi deletado (soft delete) originalmente
    /// </summary>
    [Required]
    public DateTime DataDelecao { get; set; }
    
    /// <summary>
    /// Data que o registro foi arquivado para esta tabela _log
    /// </summary>
    [Required]
    public DateTime DataArquivamento { get; set; }
    
    /// <summary>
    /// Tenant (farmácia) que possuía os dados para auditoria
    /// Mantém isolamento mesmo nos dados arquivados
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Usuário que executou o soft delete original
    /// Para auditoria e rastreabilidade farmacêutica
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UsuarioDeletou { get; set; } = string.Empty;
    
    /// <summary>
    /// Motivo do arquivamento automático para compliance
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string MotivoArquivamento { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash MD5 dos dados para verificação de integridade
    /// Detecta corrupção de dados ao longo do tempo
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string HashIntegridade { get; set; } = string.Empty;
    
    /// <summary>
    /// Versão do sistema no momento do arquivamento
    /// Para compatibilidade futura de restauração
    /// </summary>
    [MaxLength(50)]
    public string VersaoSistema { get; set; } = "1.0.0";
    
    /// <summary>
    /// Observações adicionais sobre o arquivamento
    /// </summary>
    [MaxLength(1000)]
    public string Observacoes { get; set; } = string.Empty;

    /// <summary>
    /// Define os dados originais e calcula automaticamente o hash de integridade
    /// </summary>
    /// <param name="dadosOriginais">Objeto a ser serializado</param>
    public void DefinirDadosOriginais(object dadosOriginais)
    {
        var opcoesSerialization = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        DadosOriginais = JsonSerializer.Serialize(dadosOriginais, opcoesSerialization);
        HashIntegridade = CalcularHashMD5(DadosOriginais);
    }

    /// <summary>
    /// Verifica se os dados mantiveram sua integridade
    /// Compara hash atual com hash armazenado
    /// </summary>
    /// <returns>True se dados estão íntegros</returns>
    public bool VerificarIntegridade()
    {
        if (string.IsNullOrEmpty(DadosOriginais) || string.IsNullOrEmpty(HashIntegridade))
            return false;

        var hashAtual = CalcularHashMD5(DadosOriginais);
        return hashAtual.Equals(HashIntegridade, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Desserializa os dados originais para o tipo especificado
    /// </summary>
    /// <typeparam name="T">Tipo da entidade original</typeparam>
    /// <returns>Objeto desserializado ou null se houver erro</returns>
    public T? RecuperarDadosOriginais<T>() where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(DadosOriginais))
                return null;

            return JsonSerializer.Deserialize<T>(DadosOriginais);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calcula hash MD5 para verificação de integridade dos dados
    /// </summary>
    /// <param name="dados">String dos dados a serem calculados</param>
    /// <returns>Hash MD5 em hexadecimal</returns>
    private static string CalcularHashMD5(string dados)
    {
        if (string.IsNullOrEmpty(dados))
            return string.Empty;

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(dados));
        return Convert.ToHexString(hash);
    }
}