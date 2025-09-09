using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;
using System.Security.Cryptography;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um refresh token para renovação de JWT
/// Implementa segurança avançada para sistema farmacêutico multi-tenant
/// </summary>
/// <remarks>
/// Refresh tokens permitem renovação segura de access tokens sem requerer
/// nova autenticação, essencial para experiência de usuário em farmácias
/// </remarks>
[Table("RefreshTokens")]
public class RefreshTokenEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do refresh token
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do usuário proprietário do token
    /// </summary>
    [Required]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Token criptográfico único
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Hash do token para verificação de integridade
    /// </summary>
    [Required]
    [StringLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// JTI (JWT ID) do access token relacionado
    /// </summary>
    [StringLength(255)]
    public string? JwtId { get; set; }

    /// <summary>
    /// Data de criação do token
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de expiração do token
    /// </summary>
    public DateTime DataExpiracao { get; set; }

    /// <summary>
    /// Data de uso do token (quando foi usado para refresh)
    /// </summary>
    public DateTime? DataUso { get; set; }

    /// <summary>
    /// Data de revogação do token
    /// </summary>
    public DateTime? DataRevogacao { get; set; }

    /// <summary>
    /// Motivo da revogação
    /// </summary>
    [StringLength(200)]
    public string? MotivoRevogacao { get; set; }

    /// <summary>
    /// Se o token está ativo (não revogado nem usado)
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// IP de origem da criação do token
    /// </summary>
    [StringLength(45)] // IPv6
    public string? IpOrigem { get; set; }

    /// <summary>
    /// User agent do client que criou o token
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Dados adicionais do dispositivo/sessão
    /// </summary>
    [StringLength(1000)]
    public string? DadosDispositivo { get; set; }

    /// <summary>
    /// Tipo de token (WEB, MOBILE, API)
    /// </summary>
    [StringLength(20)]
    public string TipoToken { get; set; } = "WEB";

    /// <summary>
    /// Token que foi usado para gerar este (cadeia de refresh)
    /// </summary>
    public Guid? TokenPaiId { get; set; }

    /// <summary>
    /// Número de usos permitidos (padrão: 1 - single use)
    /// </summary>
    public int UsosPermitidos { get; set; } = 1;

    /// <summary>
    /// Número de usos realizados
    /// </summary>
    public int UsosRealizados { get; set; } = 0;

    // Navegação

    /// <summary>
    /// Usuário proprietário do token
    /// </summary>
    public virtual UsuarioEntity? Usuario { get; set; }

    /// <summary>
    /// Token pai na cadeia de refresh
    /// </summary>
    public virtual RefreshTokenEntity? TokenPai { get; set; }

    /// <summary>
    /// Tokens filhos gerados a partir deste
    /// </summary>
    public virtual ICollection<RefreshTokenEntity> TokensFilhos { get; set; } = new List<RefreshTokenEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se o token é válido para uso
    /// </summary>
    /// <returns>True se token pode ser usado</returns>
    public bool EhValido()
    {
        return Ativo &&
               !EstaExpirado() &&
               !EstaRevogado() &&
               UsosRealizados < UsosPermitidos;
    }

    /// <summary>
    /// Verifica se o token está expirado
    /// </summary>
    /// <returns>True se token expirou</returns>
    public bool EstaExpirado()
    {
        return DateTime.UtcNow > DataExpiracao;
    }

    /// <summary>
    /// Verifica se o token foi revogado
    /// </summary>
    /// <returns>True se token foi revogado</returns>
    public bool EstaRevogado()
    {
        return DataRevogacao != null;
    }

    /// <summary>
    /// Verifica se o token já foi usado
    /// </summary>
    /// <returns>True se token foi usado</returns>
    public bool FoiUsado()
    {
        return DataUso != null || UsosRealizados > 0;
    }

    /// <summary>
    /// Marca token como usado para refresh
    /// </summary>
    public void MarcarComoUsado()
    {
        DataUso = DateTime.UtcNow;
        UsosRealizados++;

        // Se atingiu limite de usos, desativar
        if (UsosRealizados >= UsosPermitidos)
        {
            Ativo = false;
        }
    }

    /// <summary>
    /// Revoga o token com motivo específico
    /// </summary>
    /// <param name="motivo">Motivo da revogação</param>
    public void Revogar(string motivo)
    {
        if (EstaRevogado())
            return;

        DataRevogacao = DateTime.UtcNow;
        MotivoRevogacao = motivo;
        Ativo = false;
    }

    /// <summary>
    /// Valida se o token apresentado confere com o hash armazenado
    /// </summary>
    /// <param name="tokenParaValidar">Token a ser validado</param>
    /// <returns>True se token é autêntico</returns>
    public bool ValidarToken(string tokenParaValidar)
    {
        if (string.IsNullOrEmpty(tokenParaValidar))
            return false;

        var hashCalculado = CalcularHashToken(tokenParaValidar);
        return hashCalculado == TokenHash;
    }

    /// <summary>
    /// Gera um novo token refresh criptograficamente seguro
    /// </summary>
    /// <returns>Token gerado</returns>
    public static string GerarToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[64];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    /// <summary>
    /// Calcula hash SHA256 do token para armazenamento seguro
    /// </summary>
    /// <param name="token">Token a ser hasheado</param>
    /// <returns>Hash SHA256 do token</returns>
    public static string CalcularHashToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Cria novo refresh token para usuário específico
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="expirationDays">Dias para expiração</param>
    /// <param name="jwtId">JTI do JWT relacionado</param>
    /// <param name="ipOrigem">IP de origem</param>
    /// <param name="userAgent">User agent do client</param>
    /// <param name="tipoToken">Tipo do token</param>
    /// <returns>Nova instância de RefreshTokenEntity</returns>
    public static RefreshTokenEntity CriarNovo(
        Guid usuarioId,
        string tenantId,
        int expirationDays = 7,
        string? jwtId = null,
        string? ipOrigem = null,
        string? userAgent = null,
        string tipoToken = "WEB")
    {
        var token = GerarToken();
        var tokenHash = CalcularHashToken(token);

        return new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TenantId = tenantId,
            Token = token,
            TokenHash = tokenHash,
            JwtId = jwtId,
            DataCriacao = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddDays(expirationDays),
            IpOrigem = ipOrigem,
            UserAgent = userAgent,
            TipoToken = tipoToken,
            Ativo = true,
            UsosPermitidos = 1,
            UsosRealizados = 0
        };
    }

    /// <summary>
    /// Obtém informações de segurança do token para logs
    /// </summary>
    /// <returns>Informações de segurança sanitizadas</returns>
    public object ObterInformacoesSeguranca()
    {
        return new
        {
            Id = Id,
            UsuarioId = UsuarioId,
            TenantId = TenantId,
            DataCriacao = DataCriacao,
            DataExpiracao = DataExpiracao,
            TipoToken = TipoToken,
            EstaValido = EhValido(),
            EstaExpirado = EstaExpirado(),
            EstaRevogado = EstaRevogado(),
            FoiUsado = FoiUsado(),
            UsosRealizados = UsosRealizados,
            UsosPermitidos = UsosPermitidos,
            IpOrigem = IpOrigem,
            // Não incluir dados sensíveis como Token, TokenHash, UserAgent completo
            UserAgentTruncado = UserAgent?.Length > 50 ? UserAgent.Substring(0, 50) + "..." : UserAgent
        };
    }

    /// <summary>
    /// Limpa dados sensíveis do token (para logs de auditoria)
    /// </summary>
    public void LimparDadosSensiveis()
    {
        Token = "[REMOVIDO]";
        TokenHash = "[REMOVIDO]";
        UserAgent = UserAgent?.Length > 100 ? UserAgent.Substring(0, 100) + "..." : UserAgent;
        DadosDispositivo = DadosDispositivo?.Length > 200 ? DadosDispositivo.Substring(0, 200) + "..." : DadosDispositivo;
    }

    /// <summary>
    /// Verifica se token pertence ao mesmo usuário e tenant
    /// </summary>
    /// <param name="usuarioId">ID do usuário a verificar</param>
    /// <param name="tenantId">ID do tenant a verificar</param>
    /// <returns>True se token pertence ao usuário no tenant especificado</returns>
    public bool PertenceAoUsuario(Guid usuarioId, string tenantId)
    {
        return UsuarioId == usuarioId && TenantId == tenantId;
    }
}