using System.Security.Claims;

namespace CoreApp.Domain.Interfaces;

/// <summary>
/// Interface para serviços de autenticação multi-tenant farmacêutica brasileira
/// Gerencia login, logout, refresh tokens e validações de módulos comerciais
/// </summary>
/// <remarks>
/// Este serviço implementa autenticação específica para farmácias brasileiras,
/// considerando isolamento por tenant e validação de módulos pagos
/// </remarks>
public interface IAuthService
{
    /// <summary>
    /// Realiza autenticação de usuário no contexto de um tenant específico
    /// </summary>
    /// <param name="loginRequest">Dados de login incluindo tenant</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token JWT e dados do usuário autenticado</returns>
    Task<AuthResult> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza token JWT usando refresh token válido
    /// </summary>
    /// <param name="refreshToken">Token de renovação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Novo token JWT</returns>
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalida token de usuário (logout)
    /// </summary>
    /// <param name="accessToken">Token JWT a ser invalidado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Valida se token JWT é válido e extrai informações do usuário
    /// </summary>
    /// <param name="accessToken">Token JWT a ser validado</param>
    /// <returns>Dados do usuário se token válido, null caso contrário</returns>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string accessToken);
    
    /// <summary>
    /// Verifica se usuário tem permissão para acessar módulo comercial específico
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="tenantId">ID do tenant (farmácia)</param>
    /// <param name="moduleCode">Código do módulo (ex: CUSTOMERS, REPORTS)</param>
    /// <returns>True se usuário tem acesso ao módulo</returns>
    Task<bool> HasModuleAccessAsync(string userId, string tenantId, string moduleCode);
    
    /// <summary>
    /// Obtém todos os módulos disponíveis para o usuário no tenant
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="tenantId">ID do tenant (farmácia)</param>
    /// <returns>Lista de códigos dos módulos disponíveis</returns>
    Task<IEnumerable<string>> GetUserModulesAsync(string userId, string tenantId);
}

/// <summary>
/// Request de login com informações específicas do tenant
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email ou username do usuário
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do tenant (farmácia) - pode vir do subdomain ou header
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Se deve manter login persistente (Remember Me)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Resultado da autenticação com tokens e informações do usuário
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Token JWT de acesso
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Token de renovação
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Tempo de expiração do access token (em segundos)
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Tipo do token (sempre "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Informações do usuário autenticado
    /// </summary>
    public AuthUserInfo User { get; set; } = new();
    
    /// <summary>
    /// Informações do tenant (farmácia)
    /// </summary>
    public AuthTenantInfo Tenant { get; set; } = new();
    
    /// <summary>
    /// Módulos comerciais disponíveis para o usuário
    /// </summary>
    public IEnumerable<string> AvailableModules { get; set; } = new List<string>();
}

/// <summary>
/// Informações do usuário autenticado
/// </summary>
public class AuthUserInfo
{
    /// <summary>
    /// ID único do usuário
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Username/login do usuário
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Roles do usuário no tenant atual
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// Permissões específicas do usuário
    /// </summary>
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}

/// <summary>
/// Informações do tenant (farmácia) para autenticação
/// </summary>
public class AuthTenantInfo
{
    /// <summary>
    /// ID único do tenant
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome fantasia da farmácia
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// CNPJ da farmácia
    /// </summary>
    public string Document { get; set; } = string.Empty;
    
    /// <summary>
    /// Plano comercial ativo (Starter, Professional, Enterprise)
    /// </summary>
    public string Plan { get; set; } = string.Empty;
    
    /// <summary>
    /// Se o tenant está ativo e pode acessar o sistema
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Configurações específicas de JWT para o sistema farmacêutico brasileiro
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Chave secreta para assinatura do token (mín. 256 bits)
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Emissor do token (farmacia.com.br)
    /// </summary>
    public string Issuer { get; set; } = "FarmaciaAPIBrasil";
    
    /// <summary>
    /// Audiência do token (clientes farmácias)
    /// </summary>
    public string Audience { get; set; } = "FarmaciaClientsBrasil";
    
    /// <summary>
    /// Tempo de expiração do access token em horas
    /// </summary>
    public int ExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// Tempo de expiração do refresh token em dias
    /// </summary>
    public int RefreshExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Se deve incluir claims de módulos no token
    /// </summary>
    public bool IncludeModuleClaims { get; set; } = true;
    
    /// <summary>
    /// Se deve incluir claims de tenant no token
    /// </summary>
    public bool IncludeTenantClaims { get; set; } = true;
}