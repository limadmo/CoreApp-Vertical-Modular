using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.Data.Context;
using Farmacia.Infrastructure.MultiTenant;
using BCrypt.Net;
using StackExchange.Redis;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço de autenticação multi-tenant para sistema farmacêutico brasileiro
/// Implementa JWT com refresh tokens e validação de módulos comerciais
/// </summary>
/// <remarks>
/// Este serviço garante autenticação segura e isolamento por tenant,
/// integrando com sistema de módulos comerciais brasileiros
/// </remarks>
public class AuthService : IAuthService
{
    private readonly FarmaciaDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITenantService _tenantService;
    private readonly IDatabase _redisDatabase;
    private readonly JwtConfiguration _jwtConfig;

    public AuthService(
        FarmaciaDbContext context,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        ITenantService tenantService,
        IDatabase redisDatabase)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _tenantService = tenantService;
        _redisDatabase = redisDatabase;

        // Carregar configurações JWT
        _jwtConfig = new JwtConfiguration
        {
            Secret = configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret não configurado"),
            Issuer = configuration["JWT:Issuer"] ?? "FarmaciaAPIBrasil",
            Audience = configuration["JWT:Audience"] ?? "FarmaciaClientsBrasil",
            ExpirationHours = configuration.GetValue<int>("JWT:ExpirationHours", 24),
            RefreshExpirationDays = configuration.GetValue<int>("JWT:RefreshExpirationDays", 7)
        };
    }

    /// <summary>
    /// Realiza autenticação de usuário no contexto de um tenant específico
    /// </summary>
    public async Task<AuthResult> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Tentativa de login para {Email} no tenant {TenantId}", 
                loginRequest.EmailOrUsername, loginRequest.TenantId);

            // Buscar usuário no tenant específico
            var usuario = await _context.Usuarios
                .Include(u => u.Roles).ThenInclude(ur => ur.Role)
                .Include(u => u.Permissoes).ThenInclude(up => up.Permissao)
                .FirstOrDefaultAsync(u => 
                    u.TenantId == loginRequest.TenantId &&
                    (u.Email == loginRequest.EmailOrUsername || u.Username == loginRequest.EmailOrUsername) &&
                    !u.IsDeleted,
                    cancellationToken);

            if (usuario == null)
            {
                _logger.LogWarning("Usuário {Email} não encontrado no tenant {TenantId}", 
                    loginRequest.EmailOrUsername, loginRequest.TenantId);
                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            // Verificar se usuário pode fazer login
            if (!usuario.PodeRealizarLogin())
            {
                _logger.LogWarning("Usuário {UserId} bloqueado ou inativo no tenant {TenantId}", 
                    usuario.Id, loginRequest.TenantId);
                
                var motivo = usuario.Status != StatusUsuario.Ativo ? "Usuário inativo" : 
                           usuario.DataBloqueio != null ? "Usuário bloqueado por tentativas excessivas" : 
                           "Usuário não pode realizar login";
                
                throw new UnauthorizedAccessException(motivo);
            }

            // Verificar senha
            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, usuario.PasswordHash))
            {
                _logger.LogWarning("Senha incorreta para usuário {UserId} no tenant {TenantId}", 
                    usuario.Id, loginRequest.TenantId);

                // Registrar tentativa de login falha
                usuario.RegistrarTentativaLoginFalha();
                await _context.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            // Buscar informações do tenant
            var tenant = await ObterInformacoesTenantAsync(loginRequest.TenantId, cancellationToken);
            
            if (tenant == null || !tenant.IsActive)
            {
                _logger.LogWarning("Tenant {TenantId} não encontrado ou inativo", loginRequest.TenantId);
                throw new UnauthorizedAccessException("Tenant inativo ou não encontrado");
            }

            // Login bem-sucedido - registrar acesso
            var ipAcesso = _tenantService.GetCurrentUserIP();
            usuario.RegistrarLoginSucesso(ipAcesso);

            // Obter módulos disponíveis para o usuário
            var modulosDisponiveis = await ObterModulosUsuarioAsync(usuario.Id.ToString(), loginRequest.TenantId);

            // Gerar tokens JWT
            var (accessToken, jwtId) = await GerarAccessTokenAsync(usuario, tenant, modulosDisponiveis);
            var refreshToken = await GerarRefreshTokenAsync(usuario, jwtId, ipAcesso);

            // Salvar mudanças
            await _context.SaveChangesAsync(cancellationToken);

            // Log de auditoria
            _logger.LogInformation("Login realizado com sucesso para usuário {UserId} no tenant {TenantId}", 
                usuario.Id, loginRequest.TenantId);

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtConfig.ExpirationHours * 3600,
                TokenType = "Bearer",
                User = new AuthUserInfo
                {
                    Id = usuario.Id.ToString(),
                    Name = usuario.Nome,
                    Email = usuario.Email,
                    Username = usuario.Username,
                    Roles = usuario.Roles.Where(ur => ur.EstaValida()).Select(ur => ur.Role?.Nome ?? string.Empty),
                    Permissions = ObterPermissoesUsuario(usuario)
                },
                Tenant = tenant,
                AvailableModules = modulosDisponiveis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante processo de login para {Email} no tenant {TenantId}", 
                loginRequest.EmailOrUsername, loginRequest.TenantId);
            throw;
        }
    }

    /// <summary>
    /// Atualiza token JWT usando refresh token válido
    /// </summary>
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Tentativa de refresh token");

            // Buscar refresh token no banco
            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.Usuario)
                    .ThenInclude(u => u!.Roles).ThenInclude(ur => ur.Role)
                .Include(rt => rt.Usuario)
                    .ThenInclude(u => u!.Permissoes).ThenInclude(up => up.Permissao)
                .FirstOrDefaultAsync(rt => rt.TokenHash == RefreshTokenEntity.CalcularHashToken(refreshToken) && 
                                          rt.Ativo, 
                                    cancellationToken);

            if (tokenEntity == null || !tokenEntity.EhValido())
            {
                _logger.LogWarning("Refresh token inválido ou não encontrado");
                throw new UnauthorizedAccessException("Refresh token inválido");
            }

            var usuario = tokenEntity.Usuario!;
            
            // Verificar se usuário ainda pode fazer login
            if (!usuario.PodeRealizarLogin())
            {
                _logger.LogWarning("Usuário {UserId} não pode mais realizar login", usuario.Id);
                
                // Revogar todos os tokens do usuário
                await RevogarTodosTokensUsuarioAsync(usuario.Id, "Usuário inativo");
                
                throw new UnauthorizedAccessException("Usuário inativo");
            }

            // Marcar token como usado
            tokenEntity.MarcarComoUsado();

            // Buscar informações do tenant
            var tenant = await ObterInformacoesTenantAsync(usuario.TenantId, cancellationToken);
            
            if (tenant == null || !tenant.IsActive)
            {
                throw new UnauthorizedAccessException("Tenant inativo");
            }

            // Obter módulos atualizados
            var modulosDisponiveis = await ObterModulosUsuarioAsync(usuario.Id.ToString(), usuario.TenantId);

            // Gerar novos tokens
            var (accessToken, jwtId) = await GerarAccessTokenAsync(usuario, tenant, modulosDisponiveis);
            var novoRefreshToken = await GerarRefreshTokenAsync(usuario, jwtId, tokenEntity.IpOrigem, tokenEntity.Id);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refresh realizado com sucesso para usuário {UserId}", usuario.Id);

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = novoRefreshToken.Token,
                ExpiresIn = _jwtConfig.ExpirationHours * 3600,
                TokenType = "Bearer",
                User = new AuthUserInfo
                {
                    Id = usuario.Id.ToString(),
                    Name = usuario.Nome,
                    Email = usuario.Email,
                    Username = usuario.Username,
                    Roles = usuario.Roles.Where(ur => ur.EstaValida()).Select(ur => ur.Role?.Nome ?? string.Empty),
                    Permissions = ObterPermissoesUsuario(usuario)
                },
                Tenant = tenant,
                AvailableModules = modulosDisponiveis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante refresh token");
            throw;
        }
    }

    /// <summary>
    /// Invalida token de usuário (logout)
    /// </summary>
    public async Task RevokeTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extrair JTI do access token
            var jwtId = ExtrairJwtId(accessToken);
            
            if (string.IsNullOrEmpty(jwtId))
            {
                _logger.LogWarning("Não foi possível extrair JTI do access token");
                return;
            }

            // Revogar refresh tokens relacionados
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.JwtId == jwtId && rt.Ativo)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.Revogar("Logout explícito");
            }

            // Adicionar JTI à blacklist no Redis (com TTL = expiração do JWT)
            var blacklistKey = $"blacklist:jwt:{jwtId}";
            var ttl = TimeSpan.FromHours(_jwtConfig.ExpirationHours);
            await _redisDatabase.StringSetAsync(blacklistKey, "revoked", ttl);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token {JwtId} revogado com sucesso", jwtId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar token");
            throw;
        }
    }

    /// <summary>
    /// Valida se token JWT é válido e extrai informações do usuário
    /// </summary>
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);
            
            // Verificar se token está na blacklist
            var jwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jwtId))
            {
                var blacklistKey = $"blacklist:jwt:{jwtId}";
                var isBlacklisted = await _redisDatabase.KeyExistsAsync(blacklistKey);
                
                if (isBlacklisted)
                {
                    _logger.LogWarning("Token {JwtId} está na blacklist", jwtId);
                    return null;
                }
            }

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token expirado");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token JWT");
            return null;
        }
    }

    /// <summary>
    /// Verifica se usuário tem permissão para acessar módulo comercial específico
    /// </summary>
    public async Task<bool> HasModuleAccessAsync(string userId, string tenantId, string moduleCode)
    {
        try
        {
            // Buscar no cache Redis primeiro
            var cacheKey = $"modules:tenant:{tenantId}:user:{userId}";
            var cachedModules = await _redisDatabase.StringGetAsync(cacheKey);
            
            if (cachedModules.HasValue)
            {
                var modules = cachedModules.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                return modules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
            }

            // Se não estiver no cache, buscar do banco e cachear
            var modulosUsuario = await ObterModulosUsuarioAsync(userId, tenantId);
            
            // Cachear por 15 minutos
            await _redisDatabase.StringSetAsync(cacheKey, string.Join(",", modulosUsuario), TimeSpan.FromMinutes(15));
            
            return modulosUsuario.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar acesso ao módulo {ModuleCode} para usuário {UserId}", moduleCode, userId);
            return false;
        }
    }

    /// <summary>
    /// Obtém todos os módulos disponíveis para o usuário no tenant
    /// </summary>
    public async Task<IEnumerable<string>> GetUserModulesAsync(string userId, string tenantId)
    {
        return await ObterModulosUsuarioAsync(userId, tenantId);
    }

    #region Métodos Auxiliares

    /// <summary>
    /// Gera access token JWT para usuário autenticado
    /// </summary>
    private async Task<(string token, string jwtId)> GerarAccessTokenAsync(
        UsuarioEntity usuario, 
        AuthTenantInfo tenant, 
        IEnumerable<string> modulos)
    {
        var jwtId = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new(JwtRegisteredClaimNames.GivenName, usuario.Nome),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
            // Claims específicos do sistema farmacêutico
            new("tenant_id", usuario.TenantId),
            new("tenant_name", tenant.Name),
            new("tenant_plan", tenant.Plan),
            new("user_status", usuario.Status.ToString()),
            new("is_pharmacist", usuario.EhFarmaceutico().ToString()),
        };

        // Adicionar roles
        var roles = usuario.Roles.Where(ur => ur.EstaValida()).Select(ur => ur.Role?.Nome ?? string.Empty);
        foreach (var role in roles.Where(r => !string.IsNullOrEmpty(r)))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Adicionar permissões principais
        var permissoes = ObterPermissoesUsuario(usuario);
        foreach (var permissao in permissoes.Take(20)) // Limitar para evitar token muito grande
        {
            claims.Add(new Claim("permission", permissao));
        }

        // Adicionar módulos se configurado
        if (_jwtConfig.IncludeModuleClaims)
        {
            foreach (var modulo in modulos)
            {
                claims.Add(new Claim("module", modulo));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationHours),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return (tokenHandler.WriteToken(token), jwtId);
    }

    /// <summary>
    /// Gera refresh token para usuário
    /// </summary>
    private async Task<RefreshTokenEntity> GerarRefreshTokenAsync(
        UsuarioEntity usuario, 
        string jwtId, 
        string? ipOrigem = null, 
        Guid? tokenPaiId = null)
    {
        var userAgent = _tenantService.GetCurrentUserAgent();
        
        var refreshToken = RefreshTokenEntity.CriarNovo(
            usuario.Id,
            usuario.TenantId,
            _jwtConfig.RefreshExpirationDays,
            jwtId,
            ipOrigem,
            userAgent);

        if (tokenPaiId.HasValue)
        {
            refreshToken.TokenPaiId = tokenPaiId.Value;
        }

        _context.RefreshTokens.Add(refreshToken);
        
        return refreshToken;
    }

    /// <summary>
    /// Obtém informações do tenant para autenticação
    /// </summary>
    private async Task<AuthTenantInfo?> ObterInformacoesTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        // TODO: Buscar de uma tabela de tenants quando implementada
        // Por enquanto, retornar informações básicas
        return new AuthTenantInfo
        {
            Id = tenantId,
            Name = $"Farmácia {tenantId}",
            Document = "00.000.000/0001-00", // TODO: buscar CNPJ real
            Plan = "PROFESSIONAL", // TODO: buscar plano real
            IsActive = true
        };
    }

    /// <summary>
    /// Obtém módulos disponíveis para usuário no tenant
    /// </summary>
    private async Task<IEnumerable<string>> ObterModulosUsuarioAsync(string userId, string tenantId)
    {
        // TODO: Implementar lógica real baseada no plano do tenant
        // Por enquanto, retornar módulos básicos
        
        // Buscar plano do tenant (simulado)
        var plano = "PROFESSIONAL"; // TODO: buscar plano real do banco
        
        return plano switch
        {
            "STARTER" => new[] { "PRODUCTS", "SALES", "STOCK", "USERS" },
            "PROFESSIONAL" => new[] { "PRODUCTS", "SALES", "STOCK", "USERS", "CUSTOMERS", "PROMOTIONS", "BASIC_REPORTS" },
            "ENTERPRISE" => new[] { "PRODUCTS", "SALES", "STOCK", "USERS", "CUSTOMERS", "PROMOTIONS", "BASIC_REPORTS", 
                                   "ADVANCED_REPORTS", "AUDIT", "SUPPLIERS", "MOBILE" },
            _ => new[] { "PRODUCTS", "SALES" }
        };
    }

    /// <summary>
    /// Obtém todas as permissões do usuário (roles + permissões diretas)
    /// </summary>
    private IEnumerable<string> ObterPermissoesUsuario(UsuarioEntity usuario)
    {
        var permissoes = new HashSet<string>();

        // Permissões via roles
        foreach (var userRole in usuario.Roles.Where(ur => ur.EstaValida()))
        {
            if (userRole.Role != null)
            {
                foreach (var permissao in userRole.Role.ObterPermissoesAtivas())
                {
                    permissoes.Add(permissao);
                }
            }
        }

        // Permissões diretas do usuário
        foreach (var userPermissao in usuario.Permissoes.Where(up => up.EstaValida()))
        {
            if (userPermissao.Permissao != null)
            {
                if (userPermissao.ConcederPermissao())
                {
                    permissoes.Add(userPermissao.Permissao.Codigo);
                }
                else if (userPermissao.NegarPermissao())
                {
                    permissoes.Remove(userPermissao.Permissao.Codigo);
                }
            }
        }

        return permissoes;
    }

    /// <summary>
    /// Extrai JTI (JWT ID) de um token JWT
    /// </summary>
    private string? ExtrairJwtId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            return jsonToken.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Revoga todos os refresh tokens de um usuário
    /// </summary>
    private async Task RevogarTodosTokensUsuarioAsync(Guid usuarioId, string motivo)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UsuarioId == usuarioId && rt.Ativo)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revogar(motivo);
        }
    }

    #endregion
}