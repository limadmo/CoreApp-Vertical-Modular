using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa uma permissão no sistema farmacêutico brasileiro
/// Define ações específicas que podem ser realizadas no sistema
/// </summary>
/// <remarks>
/// Permissões são granulares e específicas, permitindo controle fino sobre
/// funcionalidades do sistema farmacêutico conforme compliance brasileiro
/// </remarks>
[Table("Permissoes")]
public class PermissaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da permissão
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) - pode ser global se aplicável a todos
    /// </summary>
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único da permissão (ex: PRODUTOS_CRIAR, VENDAS_VISUALIZAR)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome amigável da permissão
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da permissão
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Categoria/módulo da permissão (PRODUTOS, VENDAS, ESTOQUE, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de operação (CREATE, READ, UPDATE, DELETE, EXECUTE)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoOperacao { get; set; } = string.Empty;

    /// <summary>
    /// Módulo comercial necessário para esta permissão (STARTER, PROFESSIONAL, ENTERPRISE)
    /// </summary>
    [StringLength(50)]
    public string? ModuloRequerido { get; set; }

    /// <summary>
    /// Nível de sensibilidade da permissão (1-5, sendo 5 o mais crítico)
    /// </summary>
    public int NivelSensibilidade { get; set; } = 1;

    /// <summary>
    /// Se a permissão é de sistema (não pode ser editada/excluída)
    /// </summary>
    public bool EhSistema { get; set; } = true;

    /// <summary>
    /// Se a permissão está ativa
    /// </summary>
    public bool Ativa { get; set; } = true;

    /// <summary>
    /// Se requer autenticação específica (ex: farmacêutico responsável)
    /// </summary>
    public bool RequerAutenticacaoEspecial { get; set; } = false;

    /// <summary>
    /// Se requer log de auditoria obrigatório
    /// </summary>
    public bool RequerAuditoria { get; set; } = false;

    /// <summary>
    /// Permissões que são pré-requisitos para esta
    /// </summary>
    [StringLength(500)]
    public string? PermissoesDependentes { get; set; }

    // Compliance farmacêutico brasileiro

    /// <summary>
    /// Se permissão está relacionada a medicamentos controlados (requer CRF)
    /// </summary>
    public bool RelacionadaMedicamentosControlados { get; set; } = false;

    /// <summary>
    /// Se permissão requer registro CRF válido
    /// </summary>
    public bool RequerCrf { get; set; } = false;

    /// <summary>
    /// Se permissão está relacionada a receituário médico
    /// </summary>
    public bool RelacionadaReceituario { get; set; } = false;

    /// <summary>
    /// Se permissão requer compliance ANVISA
    /// </summary>
    public bool RequerComplianceAnvisa { get; set; } = false;

    // Timestamps

    /// <summary>
    /// Data de criação da permissão
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou a permissão
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
    /// Roles que possuem esta permissão
    /// </summary>
    public virtual ICollection<RolePermissaoEntity> Roles { get; set; } = new List<RolePermissaoEntity>();

    /// <summary>
    /// Usuários que possuem esta permissão diretamente
    /// </summary>
    public virtual ICollection<UsuarioPermissaoEntity> Usuarios { get; set; } = new List<UsuarioPermissaoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se permissão pode ser concedida a um usuário específico
    /// </summary>
    /// <param name="usuario">Usuário a verificar</param>
    /// <returns>True se permissão pode ser concedida</returns>
    public bool PodeSerConcedida(UsuarioEntity usuario)
    {
        // Verificar se requer CRF e usuário é farmacêutico
        if (RequerCrf && !usuario.EhFarmaceutico())
            return false;

        // Verificar outras validações específicas
        if (RelacionadaMedicamentosControlados && !usuario.EhFarmaceutico())
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se permissão está disponível para o módulo comercial
    /// </summary>
    /// <param name="moduloAtivo">Módulo comercial ativo do tenant</param>
    /// <returns>True se permissão está disponível</returns>
    public bool EstaDisponivelParaModulo(string moduloAtivo)
    {
        if (string.IsNullOrEmpty(ModuloRequerido))
            return true;

        // Lógica de hierarquia: ENTERPRISE inclui PROFESSIONAL que inclui STARTER
        return ModuloRequerido switch
        {
            "STARTER" => new[] { "STARTER", "PROFESSIONAL", "ENTERPRISE" }.Contains(moduloAtivo),
            "PROFESSIONAL" => new[] { "PROFESSIONAL", "ENTERPRISE" }.Contains(moduloAtivo),
            "ENTERPRISE" => moduloAtivo == "ENTERPRISE",
            _ => true
        };
    }

    /// <summary>
    /// Obtém descrição completa da permissão incluindo restrições
    /// </summary>
    /// <returns>Descrição detalhada</returns>
    public string DescricaoCompleta()
    {
        var descricao = Descricao ?? Nome;
        
        var restricoes = new List<string>();
        
        if (RequerCrf)
            restricoes.Add("Requer CRF válido");
            
        if (RelacionadaMedicamentosControlados)
            restricoes.Add("Medicamentos controlados");
            
        if (!string.IsNullOrEmpty(ModuloRequerido))
            restricoes.Add($"Módulo {ModuloRequerido}");
            
        if (restricoes.Any())
            descricao += $" ({string.Join(", ", restricoes)})";
            
        return descricao;
    }
}

/// <summary>
/// Entidade que representa permissões específicas atribuídas diretamente a um usuário
/// Complementa as permissões obtidas via roles
/// </summary>
/// <remarks>
/// Permite concessão de permissões específicas que não estão em nenhuma role,
/// útil para casos especiais ou temporários
/// </remarks>
[Table("UsuarioPermissoes")]
public class UsuarioPermissaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da associação usuário-permissão
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
    /// Identificador da permissão
    /// </summary>
    [Required]
    public Guid PermissaoId { get; set; }

    /// <summary>
    /// Tipo de concessão (CONCEDIDA ou NEGADA - permite negar permissões de roles)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoConcessao { get; set; } = "CONCEDIDA";

    /// <summary>
    /// Data de concessão/negação da permissão
    /// </summary>
    public DateTime DataConcessao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de expiração da permissão (opcional)
    /// </summary>
    public DateTime? DataExpiracao { get; set; }

    /// <summary>
    /// Usuário que concedeu/negou a permissão
    /// </summary>
    [StringLength(100)]
    public string? ConcedidoPor { get; set; }

    /// <summary>
    /// Se a concessão está ativa
    /// </summary>
    public bool Ativa { get; set; } = true;

    /// <summary>
    /// Motivo da concessão/negação específica
    /// </summary>
    [StringLength(500)]
    public string? Motivo { get; set; }

    /// <summary>
    /// Observações adicionais sobre a concessão
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    // Navegação

    /// <summary>
    /// Usuário que possui esta permissão
    /// </summary>
    public virtual UsuarioEntity? Usuario { get; set; }

    /// <summary>
    /// Permissão concedida/negada ao usuário
    /// </summary>
    public virtual PermissaoEntity? Permissao { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Verifica se concessão está válida (ativa e não expirada)
    /// </summary>
    /// <returns>True se concessão está válida</returns>
    public bool EstaValida()
    {
        return Ativa && (DataExpiracao == null || DataExpiracao > DateTime.UtcNow);
    }

    /// <summary>
    /// Verifica se é uma concessão positiva (concede permissão)
    /// </summary>
    /// <returns>True se concede permissão</returns>
    public bool ConcederPermissao()
    {
        return TipoConcessao == "CONCEDIDA" && EstaValida();
    }

    /// <summary>
    /// Verifica se é uma negação (remove permissão mesmo se vem de role)
    /// </summary>
    /// <returns>True se nega permissão</returns>
    public bool NegarPermissao()
    {
        return TipoConcessao == "NEGADA" && EstaValida();
    }

    /// <summary>
    /// Revoga a concessão/negação
    /// </summary>
    /// <param name="revogadoPor">Usuário que revogou</param>
    public void Revogar(string? revogadoPor = null)
    {
        Ativa = false;
        Observacoes = $"Revogada em {DateTime.UtcNow:dd/MM/yyyy HH:mm} por {revogadoPor}. {Observacoes}";
    }
}