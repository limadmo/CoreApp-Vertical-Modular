/**
 * Configuração JWT - Token Management
 * Sistema seguro com chaves rotacionáveis
 */

export interface JwtConfig {
  accessTokenSecret: string;
  refreshTokenSecret: string;
  accessTokenExpiry: string;
  refreshTokenExpiry: string;
  issuer: string;
  audience: string;
}

/**
 * Configuração padrão JWT
 * Em produção, estes valores devem vir de variáveis de ambiente
 */
export const JWT_CONFIG: JwtConfig = {
  accessTokenSecret: process.env.JWT_ACCESS_SECRET || 'coreapp-access-secret-dev-2024',
  refreshTokenSecret: process.env.JWT_REFRESH_SECRET || 'coreapp-refresh-secret-dev-2024',
  accessTokenExpiry: process.env.JWT_ACCESS_EXPIRY || '15m', // 15 minutos
  refreshTokenExpiry: process.env.JWT_REFRESH_EXPIRY || '7d', // 7 dias
  issuer: process.env.JWT_ISSUER || 'coreapp-api',
  audience: process.env.JWT_AUDIENCE || 'coreapp-users'
};

/**
 * Payload do Token de Acesso
 */
export interface AccessTokenPayload {
  userId: string;
  login: string;
  tenantId: string;
  roleId: string;
  roleName: string;
  permissions: string[]; // Array de permissões: ["vendas:criar", "produtos:visualizar"]
  isSuperAdmin: boolean;
  sessionId: string; // Para invalidar sessões específicas
}

/**
 * Payload do Refresh Token
 */
export interface RefreshTokenPayload {
  userId: string;
  sessionId: string;
  tenantId: string;
}

/**
 * Informações de contexto do usuário logado
 */
export interface AuthContext {
  userId: string;
  login: string;
  tenantId: string;
  roleId: string;
  roleName: string;
  permissions: string[];
  isSuperAdmin: boolean;
  sessionId: string;
}

/**
 * Resultado da validação de token
 */
export interface TokenValidationResult {
  valid: boolean;
  payload?: AccessTokenPayload;
  error?: string;
  expired?: boolean;
}