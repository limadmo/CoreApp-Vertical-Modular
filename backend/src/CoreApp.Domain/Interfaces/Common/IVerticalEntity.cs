namespace CoreApp.Domain.Interfaces.Common;

/// <summary>
/// Interface para entidades que podem ser estendidas por verticais específicos de negócio
/// Permite composição ao invés de herança complexa conforme princípios SOLID
/// </summary>
/// <remarks>
/// Esta interface permite que entidades base sejam especializadas por diferentes verticais
/// (Padaria, Farmácia, Supermercado, etc.) sem modificar o código core.
/// Exemplo: ProdutoEntity pode ser estendido para ProdutoPadaria, ProdutoFarmacia, etc.
/// </remarks>
public interface IVerticalEntity
{
    /// <summary>
    /// Identificador do vertical de negócio (ex: "PADARIA", "FARMACIA", "SUPERMERCADO")
    /// </summary>
    string VerticalType { get; set; }

    /// <summary>
    /// Propriedades específicas do vertical armazenadas como JSON
    /// Permite extensibilidade sem alterar schema da entidade base
    /// </summary>
    /// <example>
    /// Para ProdutoPadaria: {"temGluten": true, "validadeControlada": false}
    /// Para ProdutoFarmacia: {"princípioAtivo": "Paracetamol", "dosagem": "500mg"}
    /// </example>
    string? VerticalProperties { get; set; }

    /// <summary>
    /// Configurações específicas do vertical
    /// Permite definir comportamentos customizados por tipo de negócio
    /// </summary>
    /// <example>
    /// {"validacaoIdadeMinima": 18, "requerReceita": true}
    /// </example>
    string? VerticalConfiguration { get; set; }

    /// <summary>
    /// Versão do schema das propriedades verticais
    /// Permite evolução das propriedades sem quebrar compatibilidade
    /// </summary>
    string VerticalSchemaVersion { get; set; }

    /// <summary>
    /// Indica se a entidade está ativa para o vertical específico
    /// Permite desabilitar funcionalidades por vertical sem afetar outros
    /// </summary>
    bool VerticalActive { get; set; }

    /// <summary>
    /// Obtém uma propriedade específica do vertical de forma tipada
    /// </summary>
    /// <typeparam name="T">Tipo da propriedade</typeparam>
    /// <param name="propertyName">Nome da propriedade</param>
    /// <returns>Valor da propriedade ou default(T)</returns>
    T? GetVerticalProperty<T>(string propertyName);

    /// <summary>
    /// Define uma propriedade específica do vertical
    /// </summary>
    /// <typeparam name="T">Tipo da propriedade</typeparam>
    /// <param name="propertyName">Nome da propriedade</param>
    /// <param name="value">Valor da propriedade</param>
    void SetVerticalProperty<T>(string propertyName, T value);

    /// <summary>
    /// Verifica se uma configuração específica está habilitada
    /// </summary>
    /// <param name="configName">Nome da configuração</param>
    /// <returns>True se a configuração está habilitada</returns>
    bool IsVerticalConfigEnabled(string configName);

    /// <summary>
    /// Valida se as propriedades verticais estão corretas para o tipo de negócio
    /// </summary>
    /// <returns>True se as propriedades são válidas</returns>
    bool ValidateVerticalProperties();
}