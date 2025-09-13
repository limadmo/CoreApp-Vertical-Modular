/**
 * Serviço JWT - Geração e Validação de Tokens
 * Sistema completo de autenticação com refresh tokens
 */

import jwt from 'jsonwebtoken';
import crypto from 'crypto';
import { 
  JWT_CONFIG,
  AccessTokenPayload,
  RefreshTokenPayload,
  TokenValidationResult,
  AuthContext
} from '../config/jwtConfig';

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  accessTokenExpires: Date;
  refreshTokenExpires: Date;
  sessionId: string;
}

/**
 * Gera par de tokens (access + refresh)
 * @param userData - Dados do usuário para o token
 * @returns Par de tokens com metadata
 */
export function generateTokenPair(userData: {
  userId: string;
  login: string;
  tenantId: string;
  roleId: string;
  roleName: string;
  permissions: string[];
  isSuperAdmin: boolean;
}): TokenPair {
  
  const sessionId = crypto.randomUUID();
  const now = new Date();
  
  // Calcular expirações
  const accessTokenExpires = new Date(now.getTime() + (15 * 60 * 1000)); // 15 minutos
  const refreshTokenExpires = new Date(now.getTime() + (7 * 24 * 60 * 60 * 1000)); // 7 dias

  // Payload do Access Token
  const accessPayload: AccessTokenPayload = {
    userId: userData.userId,
    login: userData.login,
    tenantId: userData.tenantId,
    roleId: userData.roleId,
    roleName: userData.roleName,
    permissions: userData.permissions,
    isSuperAdmin: userData.isSuperAdmin,
    sessionId
  };

  // Payload do Refresh Token
  const refreshPayload: RefreshTokenPayload = {
    userId: userData.userId,
    sessionId,
    tenantId: userData.tenantId
  };

  // Gerar tokens
  const accessToken = jwt.sign(
    accessPayload,
    JWT_CONFIG.accessTokenSecret,
    {
      expiresIn: JWT_CONFIG.accessTokenExpiry,
      issuer: JWT_CONFIG.issuer,
      audience: JWT_CONFIG.audience
    }
  );

  const refreshToken = jwt.sign(
    refreshPayload,
    JWT_CONFIG.refreshTokenSecret,
    {
      expiresIn: JWT_CONFIG.refreshTokenExpiry,
      issuer: JWT_CONFIG.issuer,
      audience: JWT_CONFIG.audience
    }
  );

  return {
    accessToken,
    refreshToken,
    accessTokenExpires,
    refreshTokenExpires,
    sessionId
  };
}

/**
 * Valida access token
 * @param token - Token a ser validado
 * @returns Resultado da validação
 */
export function validateAccessToken(token: string): TokenValidationResult {
  try {
    const payload = jwt.verify(
      token, 
      JWT_CONFIG.accessTokenSecret,
      {
        issuer: JWT_CONFIG.issuer,
        audience: JWT_CONFIG.audience
      }
    ) as AccessTokenPayload;

    return {
      valid: true,
      payload
    };
    
  } catch (error: any) {
    if (error.name === 'TokenExpiredError') {
      return {
        valid: false,
        error: 'Token expirado',
        expired: true
      };
    }
    
    if (error.name === 'JsonWebTokenError') {
      return {
        valid: false,
        error: 'Token inválido'
      };
    }

    return {
      valid: false,
      error: 'Erro na validação do token'
    };
  }
}

/**
 * Valida refresh token
 * @param token - Refresh token a ser validado
 * @returns Payload do refresh token ou null se inválido
 */
export function validateRefreshToken(token: string): RefreshTokenPayload | null {
  try {
    const payload = jwt.verify(
      token,
      JWT_CONFIG.refreshTokenSecret,
      {
        issuer: JWT_CONFIG.issuer,
        audience: JWT_CONFIG.audience
      }
    ) as RefreshTokenPayload;

    return payload;
    
  } catch (error) {
    return null;
  }
}

/**
 * Extrai token do header Authorization
 * @param authHeader - Header Authorization da requisição
 * @returns Token limpo ou null
 */
export function extractTokenFromHeader(authHeader: string | undefined): string | null {
  if (!authHeader) {
    return null;
  }

  const parts = authHeader.split(' ');
  if (parts.length !== 2 || parts[0] !== 'Bearer') {
    return null;
  }

  return parts[1];
}

/**
 * Cria contexto de autenticação a partir do payload do token
 * @param payload - Payload do access token
 * @returns Contexto de autenticação
 */
export function createAuthContext(payload: AccessTokenPayload): AuthContext {
  return {
    userId: payload.userId,
    login: payload.login,
    tenantId: payload.tenantId,
    roleId: payload.roleId,
    roleName: payload.roleName,
    permissions: payload.permissions,
    isSuperAdmin: payload.isSuperAdmin,
    sessionId: payload.sessionId
  };
}

/**
 * Verifica se o usuário tem uma permissão específica
 * @param context - Contexto de autenticação
 * @param requiredPermission - Permissão necessária (ex: "vendas:criar")
 * @returns Boolean indicando se tem permissão
 */
export function hasPermission(
  context: AuthContext, 
  requiredPermission: string
): boolean {
  // SuperAdmin tem todas as permissões
  if (context.isSuperAdmin) {
    return true;
  }

  // Verificar se tem a permissão específica
  return context.permissions.includes(requiredPermission);
}

/**
 * Verifica se o usuário tem pelo menos uma das permissões
 * @param context - Contexto de autenticação
 * @param requiredPermissions - Array de permissões (OR logic)
 * @returns Boolean indicando se tem pelo menos uma permissão
 */
export function hasAnyPermission(
  context: AuthContext,
  requiredPermissions: string[]
): boolean {
  // SuperAdmin tem todas as permissões
  if (context.isSuperAdmin) {
    return true;
  }

  // Verificar se tem pelo menos uma das permissões
  return requiredPermissions.some(permission => 
    context.permissions.includes(permission)
  );
}

/**
 * Verifica se o usuário tem todas as permissões necessárias
 * @param context - Contexto de autenticação
 * @param requiredPermissions - Array de permissões (AND logic)
 * @returns Boolean indicando se tem todas as permissões
 */
export function hasAllPermissions(
  context: AuthContext,
  requiredPermissions: string[]
): boolean {
  // SuperAdmin tem todas as permissões
  if (context.isSuperAdmin) {
    return true;
  }

  // Verificar se tem todas as permissões
  return requiredPermissions.every(permission => 
    context.permissions.includes(permission)
  );
}

/**
 * Decodifica token sem validar (para debug/logs)
 * ATENÇÃO: Apenas para debug, não usar para validação
 * @param token - Token a ser decodificado
 * @returns Payload decodificado ou null
 */
export function decodeTokenUnsafe(token: string): any | null {
  try {
    return jwt.decode(token);
  } catch (error) {
    return null;
  }
}

/**
 * Gera um novo access token usando refresh token válido
 * @param refreshToken - Refresh token válido
 * @param userData - Dados atualizados do usuário
 * @returns Novo access token ou null se refresh inválido
 */
export function refreshAccessToken(
  refreshToken: string,
  userData: {
    roleId: string;
    roleName: string;
    permissions: string[];
    login: string;
    isSuperAdmin: boolean;
  }
): string | null {
  
  const refreshPayload = validateRefreshToken(refreshToken);
  if (!refreshPayload) {
    return null;
  }

  // Criar novo access token com dados atualizados
  const newAccessPayload: AccessTokenPayload = {
    userId: refreshPayload.userId,
    login: userData.login,
    tenantId: refreshPayload.tenantId,
    roleId: userData.roleId,
    roleName: userData.roleName,
    permissions: userData.permissions,
    isSuperAdmin: userData.isSuperAdmin,
    sessionId: refreshPayload.sessionId
  };

  return jwt.sign(
    newAccessPayload,
    JWT_CONFIG.accessTokenSecret,
    {
      expiresIn: JWT_CONFIG.accessTokenExpiry,
      issuer: JWT_CONFIG.issuer,
      audience: JWT_CONFIG.audience
    }
  );
}