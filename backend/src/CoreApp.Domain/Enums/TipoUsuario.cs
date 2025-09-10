/// <summary>
/// Enum que define os tipos de usuário no sistema CoreApp
/// Controla permissões e funcionalidades disponíveis por perfil
/// </summary>
namespace CoreApp.Domain.Enums;

/// <summary>
/// Tipos de usuário para sistema comercial brasileiro
/// </summary>
public enum TipoUsuario
{
    /// <summary>
    /// Administrador do sistema com acesso total
    /// </summary>
    ADMINISTRADOR = 1,

    /// <summary>
    /// Proprietário da loja/comércio
    /// </summary>
    PROPRIETARIO = 2,

    /// <summary>
    /// Gerente com permissões de gestão
    /// </summary>
    GERENTE = 3,

    /// <summary>
    /// Vendedor com acesso ao módulo de vendas
    /// </summary>
    VENDEDOR = 4,

    /// <summary>
    /// Operador de caixa
    /// </summary>
    CAIXA = 5,

    /// <summary>
    /// Operador de estoque
    /// </summary>
    ESTOQUISTA = 6,

    /// <summary>
    /// Usuário com acesso apenas a relatórios
    /// </summary>
    VISUALIZADOR = 7
}