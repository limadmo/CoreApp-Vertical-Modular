using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa a associação entre usuário e role em um tenant específico
/// Permite que um usuário tenha diferentes roles em diferentes farmácias
/// </summary>
/// <remarks>
/// No modelo multi-tenant, um usuário pode ter role "Administrador" na Farmácia A
/// e role "Vendedor" na Farmácia B, proporcionando flexibilidade total
/// </remarks>
[Table("UsuarioRoles")]
public class UsuarioRoleEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da associação usuário-role
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do usuário
    /// </summary>
    [Required]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Identificador da role
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// Data de atribuição da role
    /// </summary>
    public DateTime DataAtribuicao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de expiração da role (opcional)
    /// </summary>
    public DateTime? DataExpiracao { get; set; }

    /// <summary>
    /// Usuário que atribuiu esta role
    /// </summary>
    [StringLength(100)]
    public string? AtribuidoPor { get; set; }

    /// <summary>
    /// Se a role está ativa
    /// </summary>
    public bool Ativa { get; set; } = true;

    /// <summary>
    /// Observações sobre a atribuição da role
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Navegação

    /// <summary>
    /// Usuário que possui esta role
    /// </summary>
    public virtual UsuarioEntity? Usuario { get; set; }

    /// <summary>
    /// Role atribuída ao usuário
    /// </summary>
    public virtual RoleEntity? Role { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Verifica se a role está válida (ativa e não expirada)
    /// </summary>
    /// <returns>True se role está válida</returns>
    public bool EstaValida()
    {
        return Ativa && (DataExpiracao == null || DataExpiracao > DateTime.UtcNow);
    }

    /// <summary>
    /// Inativa a role
    /// </summary>
    /// <param name="removidoPor">Usuário que removeu a role</param>
    public void Inativar(string? removidoPor = null)
    {
        Ativa = false;
        Observacoes = $"Role removida em {DateTime.UtcNow:dd/MM/yyyy HH:mm} por {removidoPor}";
    }
}

/// <summary>
/// Entidade que representa uma role (função) no sistema farmacêutico brasileiro
/// Define conjunto de permissões agrupadas por responsabilidade
/// </summary>
/// <remarks>
/// Roles são específicas por tenant, permitindo customização das funções
/// para cada farmácia conforme sua estrutura organizacional
/// </remarks>
[Table("Roles")]
public class RoleEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da role
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Nome da role (ex: Administrador, Farmacêutico, Vendedor)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Código único da role no tenant
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da role
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Nível hierárquico da role (1 = mais alta, 10 = mais baixa)
    /// </summary>
    public int NivelHierarquico { get; set; } = 5;

    /// <summary>
    /// Se a role é de sistema (não pode ser editada/excluída)
    /// </summary>
    public bool EhSistema { get; set; } = false;

    /// <summary>
    /// Se a role está ativa no tenant
    /// </summary>
    public bool Ativa { get; set; } = true;

    /// <summary>
    /// Cor da role para identificação visual
    /// </summary>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone da role para interface
    /// </summary>
    [StringLength(50)]
    public string? Icone { get; set; }

    // Timestamps

    /// <summary>
    /// Data de criação da role
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou a role
    /// </summary>
    [StringLength(100)]
    public string? CriadoPor { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Navegação

    /// <summary>
    /// Usuários que possuem esta role
    /// </summary>
    public virtual ICollection<UsuarioRoleEntity> Usuarios { get; set; } = new List<UsuarioRoleEntity>();

    /// <summary>
    /// Permissões associadas a esta role
    /// </summary>
    public virtual ICollection<RolePermissaoEntity> Permissoes { get; set; } = new List<RolePermissaoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se role pode ser modificada
    /// </summary>
    /// <returns>True se role pode ser modificada</returns>
    public bool PodeSerModificada()
    {
        return !EhSistema && Ativa;
    }

    /// <summary>
    /// Obtém todas as permissões ativas desta role
    /// </summary>
    /// <returns>Lista de códigos das permissões</returns>
    public IEnumerable<string> ObterPermissoesAtivas()
    {
        return Permissoes
            .Where(rp => rp.Ativa)
            .Select(rp => rp.Permissao?.Codigo ?? string.Empty)
            .Where(codigo => !string.IsNullOrEmpty(codigo));
    }

    /// <summary>
    /// Verifica se role tem uma permissão específica
    /// </summary>
    /// <param name="codigoPermissao">Código da permissão a verificar</param>
    /// <returns>True se role tem a permissão</returns>
    public bool TemPermissao(string codigoPermissao)
    {
        return ObterPermissoesAtivas().Contains(codigoPermissao, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Entidade que representa a associação entre role e permissão
/// </summary>
[Table("RolePermissoes")]
public class RolePermissaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da associação role-permissão
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da role
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// Identificador da permissão
    /// </summary>
    [Required]
    public Guid PermissaoId { get; set; }

    /// <summary>
    /// Se a permissão está ativa nesta role
    /// </summary>
    public bool Ativa { get; set; } = true;

    /// <summary>
    /// Data de atribuição da permissão
    /// </summary>
    public DateTime DataAtribuicao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que atribuiu a permissão
    /// </summary>
    [StringLength(100)]
    public string? AtribuidoPor { get; set; }

    // Navegação

    /// <summary>
    /// Role que possui esta permissão
    /// </summary>
    public virtual RoleEntity? Role { get; set; }

    /// <summary>
    /// Permissão atribuída à role
    /// </summary>
    public virtual PermissaoEntity? Permissao { get; set; }
}