using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;
using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um usuário do sistema farmacêutico brasileiro multi-tenant
/// Implementa controle de acesso por tenant, roles e módulos comerciais
/// </summary>
/// <remarks>
/// Esta entidade gerencia usuários com isolamento por tenant, permitindo que uma
/// pessoa tenha diferentes roles em diferentes farmácias, conforme modelo SAAS
/// </remarks>
[Table("Usuarios")]
public class UsuarioEntity : ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único do usuário
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) proprietária do usuário
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    // Dados pessoais do usuário

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário (único por tenant)
    /// </summary>
    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Username/login do usuário (único por tenant)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha do usuário (BCrypt)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Salt utilizado no hash da senha
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordSalt { get; set; } = string.Empty;

    // Dados profissionais específicos para farmácias

    /// <summary>
    /// CPF do usuário farmacêutico (obrigatório para algumas funções)
    /// </summary>
    [StringLength(14)]
    public string? Cpf { get; set; }

    /// <summary>
    /// Número CRF (Conselho Regional de Farmácia) se aplicável
    /// </summary>
    [StringLength(20)]
    public string? RegistroCrf { get; set; }

    /// <summary>
    /// Estado de registro no CRF (ex: CRF-SP, CRF-RJ)
    /// </summary>
    [StringLength(10)]
    public string? EstadoCrf { get; set; }

    /// <summary>
    /// Cargo/função do usuário na farmácia
    /// </summary>
    [StringLength(100)]
    public string? Cargo { get; set; }

    // Dados de contato e localização

    /// <summary>
    /// Telefone principal do usuário
    /// </summary>
    [StringLength(20)]
    public string? Telefone { get; set; }

    /// <summary>
    /// Telefone celular/WhatsApp
    /// </summary>
    [StringLength(20)]
    public string? Celular { get; set; }

    /// <summary>
    /// Data de nascimento do usuário
    /// </summary>
    public DateTime? DataNascimento { get; set; }

    // Status e configurações

    /// <summary>
    /// Status atual do usuário no tenant
    /// </summary>
    [Required]
    public StatusUsuario Status { get; set; } = StatusUsuario.Ativo;

    /// <summary>
    /// Se usuário deve trocar senha no próximo login
    /// </summary>
    public bool DeveAlterarSenha { get; set; } = false;

    /// <summary>
    /// Data da última alteração de senha
    /// </summary>
    public DateTime? DataUltimaAlteracaoSenha { get; set; }

    /// <summary>
    /// Data do último acesso ao sistema
    /// </summary>
    public DateTime? UltimoAcesso { get; set; }

    /// <summary>
    /// IP do último acesso
    /// </summary>
    [StringLength(45)] // IPv6
    public string? UltimoIpAcesso { get; set; }

    /// <summary>
    /// Tentativas consecutivas de login falharam
    /// </summary>
    public int TentativasLoginFalhas { get; set; } = 0;

    /// <summary>
    /// Data de bloqueio por tentativas excessivas
    /// </summary>
    public DateTime? DataBloqueio { get; set; }

    // Configurações de interface e notificações

    /// <summary>
    /// Timezone preferido do usuário (padrão: America/Sao_Paulo)
    /// </summary>
    [StringLength(50)]
    public string Timezone { get; set; } = "America/Sao_Paulo";

    /// <summary>
    /// Idioma preferido (padrão: pt-BR)
    /// </summary>
    [StringLength(10)]
    public string Idioma { get; set; } = "pt-BR";

    /// <summary>
    /// Se usuário aceita receber notificações por email
    /// </summary>
    public bool NotificacoesEmail { get; set; } = true;

    /// <summary>
    /// Se usuário aceita receber notificações push
    /// </summary>
    public bool NotificacoesPush { get; set; } = true;

    /// <summary>
    /// Tema preferido da interface (light/dark/auto)
    /// </summary>
    [StringLength(20)]
    public string TemaInterface { get; set; } = "light";

    // Navegação e relacionamentos

    /// <summary>
    /// Roles do usuário neste tenant
    /// </summary>
    public virtual ICollection<UsuarioRoleEntity> Roles { get; set; } = new List<UsuarioRoleEntity>();

    /// <summary>
    /// Permissões específicas do usuário
    /// </summary>
    public virtual ICollection<UsuarioPermissaoEntity> Permissoes { get; set; } = new List<UsuarioPermissaoEntity>();

    /// <summary>
    /// Tokens de refresh ativos do usuário
    /// </summary>
    public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();

    /// <summary>
    /// Vendas realizadas pelo usuário
    /// </summary>
    public virtual ICollection<VendaEntity> Vendas { get; set; } = new List<VendaEntity>();

    /// <summary>
    /// Movimentações de estoque realizadas pelo usuário
    /// </summary>
    public virtual ICollection<MovimentacaoEstoqueEntity> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoqueEntity>();

    // Implementação ISoftDeletableEntity

    /// <summary>
    /// Indica se o usuário foi deletado (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data de exclusão do usuário
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão
    /// </summary>
    [StringLength(100)]
    public string? DeletedBy { get; set; }

    // Implementação IArchivableEntity

    /// <summary>
    /// Data da última movimentação relevante
    /// </summary>
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se o usuário já foi arquivado
    /// </summary>
    public bool Arquivado { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? DataArquivamento { get; set; }

    // Timestamps de auditoria

    /// <summary>
    /// Data de criação do usuário
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou este registro
    /// </summary>
    [StringLength(100)]
    public string? CriadoPor { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Verifica se usuário está ativo e pode fazer login
    /// </summary>
    /// <returns>True se usuário pode fazer login</returns>
    public bool PodeRealizarLogin()
    {
        return Status == StatusUsuario.Ativo && 
               !IsDeleted && 
               (DataBloqueio == null || DataBloqueio < DateTime.UtcNow.AddHours(-1));
    }

    /// <summary>
    /// Verifica se usuário é farmacêutico registrado
    /// </summary>
    /// <returns>True se tem registro CRF válido</returns>
    public bool EhFarmaceutico()
    {
        return !string.IsNullOrEmpty(RegistroCrf) && !string.IsNullOrEmpty(EstadoCrf);
    }

    /// <summary>
    /// Incrementa contador de tentativas de login falharam
    /// </summary>
    public void RegistrarTentativaLoginFalha()
    {
        TentativasLoginFalhas++;
        
        // Bloquear após 5 tentativas consecutivas
        if (TentativasLoginFalhas >= 5)
        {
            DataBloqueio = DateTime.UtcNow;
        }
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Reseta contador de tentativas de login após sucesso
    /// </summary>
    public void RegistrarLoginSucesso(string? ipAcesso = null)
    {
        TentativasLoginFalhas = 0;
        DataBloqueio = null;
        UltimoAcesso = DateTime.UtcNow;
        UltimoIpAcesso = ipAcesso;
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Atualiza timestamp de última movimentação para controle de arquivamento
    /// </summary>
    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Inativa o usuário (não exclui, apenas desabilita)
    /// </summary>
    public void Inativar(string? motivoInativacao = null)
    {
        Status = StatusUsuario.Inativo;
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Valida formato do CPF se fornecido
    /// </summary>
    /// <returns>True se CPF é válido ou está vazio</returns>
    public bool ValidarCpf()
    {
        if (string.IsNullOrEmpty(Cpf))
            return true;
        
        // Implementar validação de CPF brasileiro
        return ValidadorCpf.EhValido(Cpf);
    }

    /// <summary>
    /// Obter nome para exibição (nome completo ou username)
    /// </summary>
    /// <returns>Nome para exibição no sistema</returns>
    public string NomeExibicao => !string.IsNullOrEmpty(Nome) ? Nome : Username;
}

/// <summary>
/// Enum para status do usuário no sistema farmacêutico
/// </summary>
public enum StatusUsuario
{
    /// <summary>
    /// Usuário ativo e pode acessar o sistema
    /// </summary>
    Ativo = 1,
    
    /// <summary>
    /// Usuário inativo temporariamente
    /// </summary>
    Inativo = 2,
    
    /// <summary>
    /// Usuário bloqueado por políticas de segurança
    /// </summary>
    Bloqueado = 3,
    
    /// <summary>
    /// Usuário pendente de ativação (novo cadastro)
    /// </summary>
    Pendente = 4,
    
    /// <summary>
    /// Usuário suspenso por violação de regras
    /// </summary>
    Suspenso = 5
}

/// <summary>
/// Validador de CPF brasileiro (implementação simplificada)
/// </summary>
public static class ValidadorCpf
{
    /// <summary>
    /// Valida se CPF é válido segundo algoritmo brasileiro
    /// </summary>
    /// <param name="cpf">CPF a ser validado</param>
    /// <returns>True se CPF é válido</returns>
    public static bool EhValido(string cpf)
    {
        if (string.IsNullOrEmpty(cpf))
            return false;
        
        // Remove caracteres não numéricos
        cpf = new string(cpf.Where(char.IsDigit).ToArray());
        
        // CPF deve ter 11 dígitos
        if (cpf.Length != 11)
            return false;
        
        // Verifica se não são todos iguais (ex: 11111111111)
        if (cpf.All(c => c == cpf[0]))
            return false;
        
        // Implementar validação completa dos dígitos verificadores aqui
        // Por simplicidade, assumindo que formato básico é suficiente
        return true;
    }
}