/**
 * Middleware de Autenticação
 * Sistema completo de autenticação JWT com permissões
 */

import { Request, Response, NextFunction } from 'express';
import { 
  validateAccessToken, 
  extractTokenFromHeader, 
  createAuthContext,
  hasPermission,
  hasAnyPermission,
  hasAllPermissions
} from '../services/jwtService';
import { AuthContext } from '../config/jwtConfig';

/**
 * Estende Request do Express com contexto de autenticação
 */
declare global {
  namespace Express {
    interface Request {
      auth?: AuthContext;
      tenant?: {
        id: string;
      };
    }
  }
}

/**
 * Middleware básico de autenticação JWT
 * Valida token e adiciona contexto ao request
 */
export function authenticateToken(req: Request, res: Response, next: NextFunction) {
  const authHeader = req.headers.authorization;
  const token = extractTokenFromHeader(authHeader);

  if (!token) {
    return res.status(401).json({
      error: 'Token de acesso não fornecido',
      code: 'MISSING_TOKEN'
    });
  }

  const validation = validateAccessToken(token);
  
  if (!validation.valid) {
    const statusCode = validation.expired ? 401 : 403;
    return res.status(statusCode).json({
      error: validation.error,
      code: validation.expired ? 'TOKEN_EXPIRED' : 'INVALID_TOKEN',
      expired: validation.expired
    });
  }

  // Adicionar contexto de autenticação ao request
  req.auth = createAuthContext(validation.payload!);
  req.tenant = { id: validation.payload!.tenantId };

  next();
}

/**
 * Middleware opcional de autenticação
 * Adiciona contexto se token válido, mas não bloqueia se não tiver
 */
export function optionalAuth(req: Request, res: Response, next: NextFunction) {
  const authHeader = req.headers.authorization;
  const token = extractTokenFromHeader(authHeader);

  if (token) {
    const validation = validateAccessToken(token);
    if (validation.valid) {
      req.auth = createAuthContext(validation.payload!);
      req.tenant = { id: validation.payload!.tenantId };
    }
  }

  next();
}

/**
 * Cria middleware que requer permissão específica
 * @param permission - Permissão necessária (ex: "vendas:criar")
 * @returns Middleware function
 */
export function requirePermission(permission: string) {
  return (req: Request, res: Response, next: NextFunction) => {
    if (!req.auth) {
      return res.status(401).json({
        error: 'Autenticação necessária',
        code: 'AUTHENTICATION_REQUIRED'
      });
    }

    if (!hasPermission(req.auth, permission)) {
      return res.status(403).json({
        error: `Permissão necessária: ${permission}`,
        code: 'INSUFFICIENT_PERMISSIONS',
        required: permission,
        userPermissions: req.auth.permissions
      });
    }

    next();
  };
}

/**
 * Cria middleware que requer pelo menos uma das permissões
 * @param permissions - Array de permissões (OR logic)
 * @returns Middleware function
 */
export function requireAnyPermission(permissions: string[]) {
  return (req: Request, res: Response, next: NextFunction) => {
    if (!req.auth) {
      return res.status(401).json({
        error: 'Autenticação necessária',
        code: 'AUTHENTICATION_REQUIRED'
      });
    }

    if (!hasAnyPermission(req.auth, permissions)) {
      return res.status(403).json({
        error: `Uma das permissões necessária: ${permissions.join(', ')}`,
        code: 'INSUFFICIENT_PERMISSIONS',
        required: permissions,
        userPermissions: req.auth.permissions
      });
    }

    next();
  };
}

/**
 * Cria middleware que requer todas as permissões
 * @param permissions - Array de permissões (AND logic)
 * @returns Middleware function
 */
export function requireAllPermissions(permissions: string[]) {
  return (req: Request, res: Response, next: NextFunction) => {
    if (!req.auth) {
      return res.status(401).json({
        error: 'Autenticação necessária',
        code: 'AUTHENTICATION_REQUIRED'
      });
    }

    if (!hasAllPermissions(req.auth, permissions)) {
      return res.status(403).json({
        error: `Todas as permissões necessárias: ${permissions.join(', ')}`,
        code: 'INSUFFICIENT_PERMISSIONS',
        required: permissions,
        userPermissions: req.auth.permissions
      });
    }

    next();
  };
}

/**
 * Middleware que requer SuperAdmin
 */
export function requireSuperAdmin(req: Request, res: Response, next: NextFunction) {
  if (!req.auth) {
    return res.status(401).json({
      error: 'Autenticação necessária',
      code: 'AUTHENTICATION_REQUIRED'
    });
  }

  if (!req.auth.isSuperAdmin) {
    return res.status(403).json({
      error: 'Acesso restrito a Super Administradores',
      code: 'SUPER_ADMIN_REQUIRED'
    });
  }

  next();
}

/**
 * Middleware de isolamento de tenant
 * Garante que usuários só acessem dados do próprio tenant
 */
export function tenantIsolation(req: Request, res: Response, next: NextFunction) {
  if (!req.auth) {
    return res.status(401).json({
      error: 'Autenticação necessária',
      code: 'AUTHENTICATION_REQUIRED'
    });
  }

  // SuperAdmin pode acessar qualquer tenant
  if (req.auth.isSuperAdmin) {
    return next();
  }

  // Verificar se o tenantId da requisição corresponde ao do usuário
  const requestedTenantId = req.headers['x-tenant-id'] || req.query.tenantId || req.body.tenantId;
  
  if (requestedTenantId && requestedTenantId !== req.auth.tenantId) {
    return res.status(403).json({
      error: 'Acesso negado: tenant diferente do usuário',
      code: 'TENANT_ACCESS_DENIED',
      userTenant: req.auth.tenantId,
      requestedTenant: requestedTenantId
    });
  }

  // Definir tenant no request se não foi especificado
  if (!req.tenant) {
    req.tenant = { id: req.auth.tenantId };
  }

  next();
}

/**
 * Middleware combinado: autenticação + isolamento de tenant
 * Mais comum para endpoints protegidos
 */
export function authenticateAndIsolateTenant(req: Request, res: Response, next: NextFunction) {
  authenticateToken(req, res, (error) => {
    if (error) return next(error);
    
    tenantIsolation(req, res, next);
  });
}

/**
 * Middleware para logs de auditoria
 * Registra todas as ações autenticadas
 */
export function auditLog(req: Request, res: Response, next: NextFunction) {
  if (req.auth) {
    const logData = {
      timestamp: new Date().toISOString(),
      userId: req.auth.userId,
      login: req.auth.login,
      tenantId: req.auth.tenantId,
      method: req.method,
      path: req.path,
      ip: req.ip || req.connection.remoteAddress,
      userAgent: req.headers['user-agent']
    };

    // Em produção, isso iria para um sistema de logs centralizado
    console.log(`[AUDIT] ${JSON.stringify(logData)}`);
  }

  next();
}

/**
 * Middleware de rate limiting baseado em usuário
 * Previne abuso por usuário específico
 */
const userRateLimits = new Map<string, { count: number; resetTime: number }>();
const RATE_LIMIT_WINDOW = 60000; // 1 minuto
const RATE_LIMIT_MAX_REQUESTS = 100; // 100 requests por minuto

export function userRateLimit(req: Request, res: Response, next: NextFunction) {
  if (!req.auth) {
    return next(); // Sem autenticação, sem rate limit
  }

  const userId = req.auth.userId;
  const now = Date.now();
  const userLimit = userRateLimits.get(userId);

  if (!userLimit || now > userLimit.resetTime) {
    // Primeiro request ou janela expirada
    userRateLimits.set(userId, {
      count: 1,
      resetTime: now + RATE_LIMIT_WINDOW
    });
    return next();
  }

  if (userLimit.count >= RATE_LIMIT_MAX_REQUESTS) {
    const resetInSeconds = Math.ceil((userLimit.resetTime - now) / 1000);
    return res.status(429).json({
      error: 'Muitas requisições, tente novamente em alguns segundos',
      code: 'RATE_LIMIT_EXCEEDED',
      resetInSeconds
    });
  }

  // Incrementar contador
  userLimit.count++;
  userRateLimits.set(userId, userLimit);
  
  // Headers informativos
  res.setHeader('X-RateLimit-Limit', RATE_LIMIT_MAX_REQUESTS);
  res.setHeader('X-RateLimit-Remaining', RATE_LIMIT_MAX_REQUESTS - userLimit.count);
  res.setHeader('X-RateLimit-Reset', Math.ceil(userLimit.resetTime / 1000));

  next();
}