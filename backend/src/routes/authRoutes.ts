/**
 * Rotas de Autenticação
 * Configuração completa de endpoints com middlewares
 */

import { Router } from 'express';
import {
  login,
  superAdminLogin,
  refreshToken,
  createNewUser,
  listUsers,
  getUserProfile,
  getCredentialRules
} from '../controllers/authController';
import {
  authenticateToken,
  requireSuperAdmin,
  authenticateAndIsolateTenant,
  auditLog,
  userRateLimit
} from '../middleware/authMiddleware';

const router = Router();

// ====================================
// ROTAS PÚBLICAS (sem autenticação)
// ====================================

/**
 * Login de usuário regular
 * POST /api/auth/login
 */
router.post('/login', userRateLimit, login);

/**
 * Login de Super Administrador
 * POST /api/auth/super-admin/login
 */
router.post('/super-admin/login', userRateLimit, superAdminLogin);

/**
 * Renovar access token
 * POST /api/auth/refresh
 */
router.post('/refresh', userRateLimit, refreshToken);

/**
 * Obter regras de credenciais (público para frontend validar)
 * GET /api/auth/credential-rules
 */
router.get('/credential-rules', getCredentialRules);

// ====================================
// ROTAS PROTEGIDAS (com autenticação)
// ====================================

/**
 * Obter perfil do usuário logado
 * GET /api/auth/profile
 */
router.get('/profile', 
  authenticateToken, 
  auditLog, 
  getUserProfile
);

// ====================================
// ROTAS DE SUPER ADMIN
// ====================================

/**
 * Criar novo usuário (apenas Super Admin)
 * POST /api/auth/users
 */
router.post('/users', 
  authenticateToken,
  requireSuperAdmin,
  auditLog,
  createNewUser
);

/**
 * Listar usuários do tenant
 * GET /api/auth/users/:tenantId
 */
router.get('/users/:tenantId', 
  authenticateToken,
  auditLog,
  listUsers
);

export default router;