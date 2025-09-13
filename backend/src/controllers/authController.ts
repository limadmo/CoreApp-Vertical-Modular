/**
 * Controller de Autenticação
 * Endpoints documentados com Swagger para auth completa
 * 
 * @swagger
 * components:
 *   schemas:
 *     LoginRequest:
 *       type: object
 *       required:
 *         - login
 *         - password
 *       properties:
 *         login:
 *           type: string
 *           pattern: "^[A-Z]{2}[0-9]{5}[A-Z]$"
 *           example: "AB12345C"
 *           description: "Login no formato XX00000X (2 letras + 5 números + 1 letra)"
 *         password:
 *           type: string
 *           pattern: "^[A-Z][0-9]{4}[A-Z]$"
 *           example: "A1234B"
 *           description: "Senha no formato X0000X (1 letra + 4 números + 1 letra)"
 *         tenantId:
 *           type: string
 *           example: "padaria-demo"
 *           description: "ID do tenant (opcional para super admin)"
 *     
 *     LoginResponse:
 *       type: object
 *       properties:
 *         success:
 *           type: boolean
 *           example: true
 *         user:
 *           type: object
 *           properties:
 *             id:
 *               type: string
 *               example: "clnx8k3r40000..."
 *             login:
 *               type: string
 *               example: "AB12345C"
 *             nome:
 *               type: string
 *               example: "João Silva"
 *             tenantId:
 *               type: string
 *               example: "padaria-demo"
 *             roleName:
 *               type: string
 *               example: "Gerente"
 *             isSuperAdmin:
 *               type: boolean
 *               example: false
 *         tokens:
 *           type: object
 *           properties:
 *             accessToken:
 *               type: string
 *               description: "JWT access token (válido por 15 minutos)"
 *             refreshToken:
 *               type: string
 *               description: "JWT refresh token (válido por 7 dias)"
 *             expiresAt:
 *               type: string
 *               format: date-time
 *               description: "Data de expiração do access token"
 *         forcePasswordChange:
 *           type: boolean
 *           example: false
 *           description: "Indica se usuário deve trocar senha"
 *         error:
 *           type: string
 *           example: "Credenciais inválidas"
 *         locked:
 *           type: boolean
 *           example: false
 *           description: "Conta bloqueada por muitas tentativas"
 *     
 *     RefreshRequest:
 *       type: object
 *       required:
 *         - refreshToken
 *       properties:
 *         refreshToken:
 *           type: string
 *           description: "JWT refresh token válido"
 *     
 *     CreateUserRequest:
 *       type: object
 *       required:
 *         - nome
 *         - roleId
 *         - tenantId
 *       properties:
 *         nome:
 *           type: string
 *           example: "Maria Santos"
 *           description: "Nome completo do usuário"
 *         email:
 *           type: string
 *           format: email
 *           example: "maria@exemplo.com"
 *           description: "Email do usuário (opcional)"
 *         roleId:
 *           type: string
 *           example: "clnx8k3r40001..."
 *           description: "ID da role do usuário"
 *         tenantId:
 *           type: string
 *           example: "padaria-demo"
 *           description: "ID do tenant"
 *         generateCredentials:
 *           type: boolean
 *           example: true
 *           description: "Se deve retornar credenciais geradas"
 * 
 *   securitySchemes:
 *     bearerAuth:
 *       type: http
 *       scheme: bearer
 *       bearerFormat: JWT
 *       description: "JWT access token no header Authorization: Bearer <token>"
 *     
 *     superAdminAuth:
 *       type: http
 *       scheme: bearer
 *       bearerFormat: JWT
 *       description: "JWT de Super Admin para operações administrativas"
 */

import { Request, Response } from 'express';
import { 
  authenticateUser, 
  authenticateSuperAdmin, 
  refreshUserToken, 
  createUser, 
  getUsersByTenant 
} from '../services/authService';
import { generateValidExamples } from '../config/credentialsRules';

/**
 * @swagger
 * /api/auth/login:
 *   post:
 *     summary: Login de usuário regular
 *     description: Autentica usuário regular de um tenant específico
 *     tags: [Autenticação]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/LoginRequest'
 *     responses:
 *       200:
 *         description: Login realizado com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/LoginResponse'
 *       400:
 *         description: Dados de entrada inválidos
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 error:
 *                   type: string
 *                   example: "Login deve ter exatamente 8 caracteres"
 *       401:
 *         description: Credenciais inválidas
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 error:
 *                   type: string
 *                   example: "Usuário não encontrado ou senha incorreta"
 *       423:
 *         description: Conta bloqueada por muitas tentativas
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 error:
 *                   type: string
 *                   example: "Conta bloqueada. Tente novamente em 30 minutos"
 *                 locked:
 *                   type: boolean
 *                   example: true
 *       500:
 *         description: Erro interno do servidor
 */
export async function login(req: Request, res: Response) {
  try {
    const { login, password, tenantId } = req.body;

    if (!login || !password) {
      return res.status(400).json({
        error: 'Login e senha são obrigatórios'
      });
    }

    const result = await authenticateUser({ login, password, tenantId });

    if (!result.success) {
      const statusCode = result.locked ? 423 : 401;
      return res.status(statusCode).json({
        error: result.error,
        locked: result.locked
      });
    }

    return res.status(200).json(result);

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro no login:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/super-admin/login:
 *   post:
 *     summary: Login de Super Administrador
 *     description: Autentica Super Administrador com acesso global
 *     tags: [Autenticação]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - login
 *               - password
 *             properties:
 *               login:
 *                 type: string
 *                 example: "SA12345A"
 *               password:
 *                 type: string
 *                 example: "S1234A"
 *     responses:
 *       200:
 *         description: Login de Super Admin realizado com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/LoginResponse'
 *       401:
 *         description: Credenciais de Super Admin inválidas
 *       500:
 *         description: Erro interno do servidor
 */
export async function superAdminLogin(req: Request, res: Response) {
  try {
    const { login, password } = req.body;

    if (!login || !password) {
      return res.status(400).json({
        error: 'Login e senha são obrigatórios'
      });
    }

    const result = await authenticateSuperAdmin({ login, password });

    if (!result.success) {
      return res.status(401).json({
        error: result.error
      });
    }

    return res.status(200).json(result);

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro no login super admin:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/refresh:
 *   post:
 *     summary: Renovar access token
 *     description: Gera novo access token usando refresh token válido
 *     tags: [Autenticação]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/RefreshRequest'
 *     responses:
 *       200:
 *         description: Access token renovado com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 accessToken:
 *                   type: string
 *                   description: "Novo access token"
 *                 expiresAt:
 *                   type: string
 *                   format: date-time
 *                   description: "Data de expiração do novo token"
 *       400:
 *         description: Refresh token não fornecido
 *       401:
 *         description: Refresh token inválido ou expirado
 *       500:
 *         description: Erro interno do servidor
 */
export async function refreshToken(req: Request, res: Response) {
  try {
    const { refreshToken } = req.body;

    if (!refreshToken) {
      return res.status(400).json({
        error: 'Refresh token é obrigatório'
      });
    }

    const result = await refreshUserToken(refreshToken);

    if (!result.success) {
      return res.status(401).json({
        error: result.error
      });
    }

    return res.status(200).json(result);

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro ao renovar token:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/users:
 *   post:
 *     summary: Criar novo usuário
 *     description: Cria novo usuário no tenant (apenas Super Admin)
 *     tags: [Gestão de Usuários]
 *     security:
 *       - superAdminAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/CreateUserRequest'
 *     responses:
 *       201:
 *         description: Usuário criado com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 user:
 *                   type: object
 *                   properties:
 *                     id:
 *                       type: string
 *                       example: "clnx8k3r40002..."
 *                     login:
 *                       type: string
 *                       example: "MS12345A"
 *                     temporaryPassword:
 *                       type: string
 *                       example: "T1234P"
 *                       description: "Senha temporária (apenas se generateCredentials=true)"
 *       400:
 *         description: Dados inválidos
 *       403:
 *         description: Apenas Super Admin pode criar usuários
 *       404:
 *         description: Tenant ou Role não encontrados
 *       500:
 *         description: Erro interno do servidor
 */
export async function createNewUser(req: Request, res: Response) {
  try {
    const { nome, email, roleId, tenantId, generateCredentials } = req.body;

    if (!nome || !roleId || !tenantId) {
      return res.status(400).json({
        error: 'Nome, roleId e tenantId são obrigatórios'
      });
    }

    // Verificar se é super admin (middleware já validou)
    if (!req.auth?.isSuperAdmin) {
      return res.status(403).json({
        error: 'Apenas Super Admin pode criar usuários'
      });
    }

    const result = await createUser({
      nome,
      email,
      roleId,
      tenantId,
      createdBy: req.auth.userId,
      generateCredentials: generateCredentials === true
    });

    if (!result.success) {
      const statusCode = result.error?.includes('não encontrado') ? 404 : 400;
      return res.status(statusCode).json({
        error: result.error
      });
    }

    return res.status(201).json(result);

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro ao criar usuário:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/users/{tenantId}:
 *   get:
 *     summary: Listar usuários do tenant
 *     description: Lista todos os usuários ativos de um tenant específico
 *     tags: [Gestão de Usuários]
 *     security:
 *       - bearerAuth: []
 *       - superAdminAuth: []
 *     parameters:
 *       - in: path
 *         name: tenantId
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do tenant
 *         example: "padaria-demo"
 *     responses:
 *       200:
 *         description: Lista de usuários retornada com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 users:
 *                   type: array
 *                   items:
 *                     type: object
 *                     properties:
 *                       id:
 *                         type: string
 *                       login:
 *                         type: string
 *                       nome:
 *                         type: string
 *                       email:
 *                         type: string
 *                       ultimoLogin:
 *                         type: string
 *                         format: date-time
 *                       role:
 *                         type: object
 *                         properties:
 *                           id:
 *                             type: string
 *                           nome:
 *                             type: string
 *       403:
 *         description: Sem permissão para acessar usuários deste tenant
 *       404:
 *         description: Tenant não encontrado
 *       500:
 *         description: Erro interno do servidor
 */
export async function listUsers(req: Request, res: Response) {
  try {
    const { tenantId } = req.params;

    // Verificar se pode acessar este tenant
    if (!req.auth?.isSuperAdmin && req.auth?.tenantId !== tenantId) {
      return res.status(403).json({
        error: 'Sem permissão para acessar usuários deste tenant'
      });
    }

    const users = await getUsersByTenant(tenantId);

    return res.status(200).json({
      success: true,
      users
    });

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro ao listar usuários:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/profile:
 *   get:
 *     summary: Obter perfil do usuário logado
 *     description: Retorna informações do usuário autenticado
 *     tags: [Perfil]
 *     security:
 *       - bearerAuth: []
 *       - superAdminAuth: []
 *     responses:
 *       200:
 *         description: Perfil do usuário retornado com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 profile:
 *                   type: object
 *                   properties:
 *                     userId:
 *                       type: string
 *                     login:
 *                       type: string
 *                     tenantId:
 *                       type: string
 *                     roleName:
 *                       type: string
 *                     permissions:
 *                       type: array
 *                       items:
 *                         type: string
 *                     isSuperAdmin:
 *                       type: boolean
 *       401:
 *         description: Token de autenticação necessário
 */
export async function getUserProfile(req: Request, res: Response) {
  try {
    if (!req.auth) {
      return res.status(401).json({
        error: 'Autenticação necessária'
      });
    }

    return res.status(200).json({
      success: true,
      profile: {
        userId: req.auth.userId,
        login: req.auth.login,
        tenantId: req.auth.tenantId,
        roleName: req.auth.roleName,
        permissions: req.auth.permissions,
        isSuperAdmin: req.auth.isSuperAdmin
      }
    });

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro ao obter perfil:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}

/**
 * @swagger
 * /api/auth/credential-rules:
 *   get:
 *     summary: Obter regras de credenciais
 *     description: Retorna as regras atuais para validação de login e senha, com exemplos
 *     tags: [Configuração]
 *     responses:
 *       200:
 *         description: Regras de credenciais retornadas com sucesso
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 rules:
 *                   type: object
 *                   properties:
 *                     loginPattern:
 *                       type: string
 *                       example: "XX00000X"
 *                     passwordPattern:
 *                       type: string
 *                       example: "X0000X"
 *                     loginDescription:
 *                       type: string
 *                       example: "Login deve ter 8 caracteres: 2 letras maiúsculas, 5 números, 1 letra maiúscula"
 *                     passwordDescription:
 *                       type: string
 *                       example: "Senha deve ter 6 caracteres: 1 letra maiúscula, 4 números, 1 letra maiúscula"
 *                 examples:
 *                   type: object
 *                   properties:
 *                     loginExamples:
 *                       type: array
 *                       items:
 *                         type: string
 *                       example: ["AB12345C", "XY98765Z", "QW11111A"]
 *                     passwordExamples:
 *                       type: array
 *                       items:
 *                         type: string
 *                       example: ["A1234B", "Z9876X", "M0000N"]
 */
export async function getCredentialRules(req: Request, res: Response) {
  try {
    const examples = generateValidExamples();

    return res.status(200).json({
      success: true,
      rules: examples.rules,
      examples: {
        loginExamples: examples.loginExamples,
        passwordExamples: examples.passwordExamples
      }
    });

  } catch (error) {
    console.error('[AUTH_CONTROLLER] Erro ao obter regras:', error);
    return res.status(500).json({
      error: 'Erro interno do servidor'
    });
  }
}