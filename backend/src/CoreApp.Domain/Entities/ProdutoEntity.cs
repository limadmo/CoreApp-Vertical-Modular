using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreApp.Domain.Entities.Base;
using CoreApp.Domain.Entities.Common;
using CoreApp.Domain.Interfaces.Common;
using Newtonsoft.Json;

namespace CoreApp.Domain.Entities;

/// <summary>
/// Entidade Produto para sistema CoreApp multi-tenant
/// Implementa padrões SOLID e compliance brasileiro
/// </summary>
public class ProdutoEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity, IArchivableEntity, IVerticalEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    [Key]
    public new Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do tenant (loja/comércio) para isolamento de dados
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Nome/descrição principal do produto
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada opcional
    /// </summary>
    [StringLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Código de barras EAN/UPC para produtos brasileiros
    /// </summary>
    [StringLength(20)]
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// Código interno/SKU da loja
    /// </summary>
    [StringLength(50)]
    public string? CodigoInterno { get; set; }

    /// <summary>
    /// Preço de venda atual do produto
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoVenda { get; set; }

    /// <summary>
    /// Preço de custo do produto
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoCusto { get; set; }

    /// <summary>
    /// Margem de lucro em percentual
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal MargemLucro { get; set; }

    /// <summary>
    /// Unidade de medida (UN, KG, LT, etc.)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string UnidadeMedida { get; set; } = "UN";

    /// <summary>
    /// Estoque atual do produto
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal EstoqueAtual { get; set; }

    /// <summary>
    /// Estoque mínimo para alertas
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal EstoqueMinimo { get; set; }

    /// <summary>
    /// Categoria do produto (referência)
    /// </summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>
    /// Navegação para categoria
    /// </summary>
    public CategoriaEntity? Categoria { get; set; }

    /// <summary>
    /// NCM (Nomenclatura Comum do Mercosul) para produtos brasileiros
    /// </summary>
    [StringLength(20)]
    public string? NCM { get; set; }

    /// <summary>
    /// CEST (Código Especificador da Substituição Tributária)
    /// </summary>
    [StringLength(20)]
    public string? CEST { get; set; }

    /// <summary>
    /// Origem do produto (0-Nacional, 1-Estrangeira importação direta, etc.)
    /// </summary>
    public int? Origem { get; set; }

    /// <summary>
    /// CST (Código de Situação Tributária) para ICMS
    /// </summary>
    [StringLength(5)]
    public string? CST { get; set; }

    /// <summary>
    /// Alíquota de ICMS em percentual
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? AliquotaICMS { get; set; }

    /// <summary>
    /// Indica se o registro está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;

    // Implementação IVerticalEntity
    /// <summary>
    /// Tipo do vertical específico (PADARIA, FARMACIA, SUPERMERCADO, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string VerticalType { get; set; } = "GENERICO";

    /// <summary>
    /// Propriedades específicas do vertical em JSON
    /// </summary>
    public string? VerticalProperties { get; set; }

    /// <summary>
    /// Configurações específicas do vertical
    /// </summary>
    public string? VerticalConfiguration { get; set; }

    /// <summary>
    /// Versão do schema das propriedades verticais
    /// </summary>
    [Required]
    [StringLength(10)]
    public string VerticalSchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Indica se a entidade está ativa para o vertical específico
    /// </summary>
    public bool VerticalActive { get; set; } = true;

    // Implementação de ISoftDeletableEntity
    /// <summary>
    /// Indica se o registro foi excluído logicamente
    /// </summary>
    public bool Excluido { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DataExclusao { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão
    /// </summary>
    [StringLength(100)]
    public string? UsuarioExclusao { get; set; }

    /// <summary>
    /// Motivo da exclusão
    /// </summary>
    [StringLength(500)]
    public string? MotivoExclusao { get; set; }

    /// <summary>
    /// Marca o registro como excluído logicamente
    /// </summary>
    /// <param name="usuarioId">ID do usuário que está excluindo</param>
    /// <param name="motivo">Motivo da exclusão</param>
    public void MarkAsDeleted(string? usuarioId = null, string? motivo = null)
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        UsuarioExclusao = usuarioId;
        MotivoExclusao = motivo;
    }

    /// <summary>
    /// Restaura um registro excluído logicamente
    /// </summary>
    public void Restore()
    {
        Excluido = false;
        DataExclusao = null;
        UsuarioExclusao = null;
        MotivoExclusao = null;
    }

    // Implementação de IArchivableEntity
    /// <summary>
    /// Indica se o registro foi arquivado
    /// </summary>
    public bool Arquivado { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? DataArquivamento { get; set; }

    /// <summary>
    /// Data da última movimentação/alteração
    /// </summary>
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Atualiza a data da última movimentação
    /// </summary>
    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
    }

    // Implementação métodos IVerticalEntity
    /// <summary>
    /// Obtém uma propriedade específica do vertical de forma tipada
    /// </summary>
    /// <typeparam name="T">Tipo da propriedade</typeparam>
    /// <param name="propertyName">Nome da propriedade</param>
    /// <returns>Valor da propriedade ou default(T)</returns>
    public T? GetVerticalProperty<T>(string propertyName)
    {
        if (string.IsNullOrEmpty(VerticalProperties))
            return default;

        try
        {
            var properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(VerticalProperties);
            if (properties != null && properties.ContainsKey(propertyName))
            {
                var value = properties[propertyName];
                
                // Se o valor já é do tipo correto, retorna diretamente
                if (value is T directValue)
                    return directValue;
                    
                // Tenta conversão por casting para tipos básicos
                if (typeof(T) == typeof(bool) && value is bool boolValue)
                    return (T)(object)boolValue;
                if (typeof(T) == typeof(bool?) && value is bool boolNullableValue)
                    return (T)(object)boolNullableValue;
                if (typeof(T) == typeof(string) && value != null)
                    return (T)(object)value.ToString()!;
                if (typeof(T) == typeof(int) && value != null && int.TryParse(value.ToString(), out int intValue))
                    return (T)(object)intValue;
                if (typeof(T) == typeof(decimal) && value != null && decimal.TryParse(value.ToString(), out decimal decimalValue))
                    return (T)(object)decimalValue;
                
                // Para outros tipos, tenta deserializar do JSON da propriedade
                var jsonValue = JsonConvert.SerializeObject(value);
                return JsonConvert.DeserializeObject<T>(jsonValue);
            }
        }
        catch
        {
            // Log erro se necessário, mas retorna default para não quebrar aplicação
        }

        return default;
    }

    /// <summary>
    /// Define uma propriedade específica do vertical
    /// </summary>
    /// <typeparam name="T">Tipo da propriedade</typeparam>
    /// <param name="propertyName">Nome da propriedade</param>
    /// <param name="value">Valor da propriedade</param>
    public void SetVerticalProperty<T>(string propertyName, T value)
    {
        Dictionary<string, object> properties;

        if (string.IsNullOrEmpty(VerticalProperties))
        {
            properties = new Dictionary<string, object>();
        }
        else
        {
            try
            {
                properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(VerticalProperties) 
                           ?? new Dictionary<string, object>();
            }
            catch
            {
                properties = new Dictionary<string, object>();
            }
        }

        properties[propertyName] = value ?? (object)string.Empty;
        VerticalProperties = JsonConvert.SerializeObject(properties);
    }

    /// <summary>
    /// Verifica se uma configuração específica está habilitada
    /// </summary>
    /// <param name="configName">Nome da configuração</param>
    /// <returns>True se a configuração está habilitada</returns>
    public bool IsVerticalConfigEnabled(string configName)
    {
        if (string.IsNullOrEmpty(VerticalConfiguration))
            return false;

        try
        {
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(VerticalConfiguration);
            if (config != null && config.ContainsKey(configName))
            {
                var value = config[configName];
                
                // Se o valor já é um boolean, retorna diretamente
                if (value is bool boolValue)
                    return boolValue;
                
                // Tenta conversão para string e depois parse
                if (bool.TryParse(value?.ToString(), out bool result))
                {
                    return result;
                }
            }
        }
        catch
        {
            // Log erro se necessário
        }

        return false;
    }

    /// <summary>
    /// Valida se as propriedades verticais estão corretas para o tipo de negócio
    /// </summary>
    /// <returns>True se as propriedades são válidas</returns>
    public bool ValidateVerticalProperties()
    {
        if (!VerticalActive)
            return true; // Se vertical inativo, não precisa validar

        switch (VerticalType.ToUpper())
        {
            case "PADARIA":
                return ValidatePadariaProperties();
            case "FARMACIA":
                return ValidateFarmaciaProperties();
            case "SUPERMERCADO":
                return ValidateSupermercadoProperties();
            case "GENERICO":
            default:
                return true; // Genérico sempre válido
        }
    }

    /// <summary>
    /// Valida propriedades específicas para padaria
    /// </summary>
    private bool ValidatePadariaProperties()
    {
        // Validações específicas da padaria
        // Exemplo: verificar se produtos com glúten têm essa informação
        var temGluten = GetVerticalProperty<bool?>("temGluten");
        var validadeControlada = GetVerticalProperty<bool?>("validadeControlada");

        // Se é produto de padaria, deve informar sobre glúten
        if (temGluten == null && VerticalActive)
            return false;

        return true;
    }

    /// <summary>
    /// Valida propriedades específicas para farmácia
    /// </summary>
    private bool ValidateFarmaciaProperties()
    {
        // Validações ANVISA
        var requerReceita = GetVerticalProperty<bool?>("requerReceita");
        var medicamentoControlado = GetVerticalProperty<bool?>("medicamentoControlado");

        // Se requer receita, deve ter controles específicos
        if (requerReceita == true && medicamentoControlado == null)
            return false;

        return true;
    }

    /// <summary>
    /// Valida propriedades específicas para supermercado
    /// </summary>
    private bool ValidateSupermercadoProperties()
    {
        // Validações para supermercado
        var perecivel = GetVerticalProperty<bool?>("perecivel");
        var categoria = GetVerticalProperty<string>("categoriaEspecifica");

        // Validações básicas para supermercado
        return true;
    }
}
