/**
 * Serviço de Autenticação - Business Logic
 * Orquestração completa de login, registro e gestão de usuários
 */

import { PrismaClient } from '@prisma/client';
import { 
  validatePasswordComplete, 
  validateLoginFormat, 
  generateLogin, 
  generateTemporaryPassword, 
  createInitialPassword 
} from './passwordService';
import { generateTokenPair, validateRefreshToken, refreshAccessToken } from './jwtService';
import { cacheService } from './cacheService';

const prisma = new PrismaClient();

export interface LoginRequest {
  login: string;
  password: string;
  tenantId?: string;
}

export interface LoginResponse {
  success: boolean;
  user?: {
    id: string;
    login: string;
    nome: string;
    tenantId: string;
    roleName: string;
    isSuperAdmin: boolean;
  };
  tokens?: {
    accessToken: string;
    refreshToken: string;
    expiresAt: Date;
  };
  forcePasswordChange?: boolean;
  error?: string;
  locked?: boolean;
}

export interface CreateUserRequest {
  nome: string;
  email?: string;
  roleId: string;
  tenantId: string;
  createdBy: string;
  generateCredentials?: boolean;
}

export interface CreateUserResponse {
  success: boolean;
  user?: {
    id: string;
    login: string;
    temporaryPassword?: string;
  };
  error?: string;
}

/**
 * Autentica usuário regular (não super admin)
 * @param loginData - Dados de login
 * @returns Resultado da autenticação
 */
export async function authenticateUser(loginData: LoginRequest): Promise<LoginResponse> {
  const { login, password, tenantId } = loginData;

  // Validar formato do login
  const loginValidation = validateLoginFormat(login);
  if (!loginValidation.valid) {
    return {
      success: false,
      error: loginValidation.message
    };
  }

  try {
    // Buscar usuário no banco
    const user = await prisma.usuario.findFirst({
      where: {
        login,
        tenantId: tenantId || undefined,
        ativo: true
      },
      include: {
        role: {
          include: {
            permissoes: {
              include: {
                permissao: true
              }
            }
          }
        },
        tenant: true
      }
    });

    if (!user) {
      return {
        success: false,
        error: 'Usuário não encontrado ou inativo'
      };
    }

    // Validar senha com todas as regras
    const passwordValidation = await validatePasswordComplete(
      password,
      user.senhaHash,
      user.senhaExpiraEm ? new Date(user.senhaExpiraEm) : null,
      user.tentativasLogin,
      user.bloqueadoEm ? new Date(user.bloqueadoEm) : null,
      user.forcarTrocaSenha
    );

    // Se conta bloqueada
    if (passwordValidation.accountLocked) {
      return {
        success: false,
        error: passwordValidation.message,
        locked: true
      };
    }

    // Se senha incorreta
    if (!passwordValidation.valid) {
      // Incrementar tentativas falhadas
      await prisma.usuario.update({
        where: { id: user.id },
        data: {
          tentativasLogin: user.tentativasLogin + 1,
          bloqueadoEm: user.tentativasLogin + 1 >= 5 ? new Date() : undefined
        }
      });

      return {
        success: false,
        error: passwordValidation.message
      };
    }

    // Extrair permissões
    const permissions = user.role.permissoes.map(rp => 
      `${rp.permissao.modulo}:${rp.permissao.acao}`
    );

    // Gerar tokens JWT
    const tokens = generateTokenPair({
      userId: user.id,
      login: user.login,
      tenantId: user.tenantId,
      roleId: user.roleId,
      roleName: user.role.nome,
      permissions,
      isSuperAdmin: false
    });

    // Atualizar dados de login no banco
    await prisma.usuario.update({
      where: { id: user.id },
      data: {
        ultimoLogin: new Date(),
        tentativasLogin: 0, // Reset tentativas
        bloqueadoEm: null   // Desbloquear se estava bloqueado
      }
    });

    // Cache dos dados do usuário
    cacheService.set(`user:${user.id}`, {
      id: user.id,
      login: user.login,
      nome: user.nome,
      roleId: user.roleId,
      roleName: user.role.nome,
      tenantId: user.tenantId,
      permissions
    }, 900); // 15 minutos

    return {
      success: true,
      user: {
        id: user.id,
        login: user.login,
        nome: user.nome,
        tenantId: user.tenantId,
        roleName: user.role.nome,
        isSuperAdmin: false
      },
      tokens: {
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        expiresAt: tokens.accessTokenExpires
      },
      forcePasswordChange: passwordValidation.shouldForceChange
    };

  } catch (error) {
    console.error('[AUTH] Erro na autenticação:', error);
    return {
      success: false,
      error: 'Erro interno do servidor'
    };
  }
}

/**
 * Autentica super administrador
 * @param loginData - Dados de login
 * @returns Resultado da autenticação
 */
export async function authenticateSuperAdmin(loginData: LoginRequest): Promise<LoginResponse> {
  const { login, password } = loginData;

  // Validar formato do login
  const loginValidation = validateLoginFormat(login);
  if (!loginValidation.valid) {
    return {
      success: false,
      error: loginValidation.message
    };
  }

  try {
    // Buscar super admin no banco
    const superAdmin = await prisma.superAdmin.findUnique({
      where: {
        login,
        ativo: true
      }
    });

    if (!superAdmin) {
      return {
        success: false,
        error: 'Super administrador não encontrado'
      };
    }

    // Validar senha
    const passwordValidation = await validatePasswordComplete(
      password,
      superAdmin.senhaHash,
      superAdmin.senhaExpiraEm ? new Date(superAdmin.senhaExpiraEm) : null,
      0, // Super admin não tem limite de tentativas
      null,
      superAdmin.forcarTrocaSenha
    );

    if (!passwordValidation.valid) {
      return {
        success: false,
        error: passwordValidation.message
      };
    }

    // Gerar tokens JWT para super admin
    const tokens = generateTokenPair({
      userId: superAdmin.id,
      login: superAdmin.login,
      tenantId: 'super-admin', // Tenant especial
      roleId: 'super-admin',
      roleName: 'Super Admin',
      permissions: ['*'], // Todas as permissões
      isSuperAdmin: true
    });

    // Atualizar último login
    await prisma.superAdmin.update({
      where: { id: superAdmin.id },
      data: {
        ultimoLogin: new Date()
      }
    });

    return {
      success: true,
      user: {
        id: superAdmin.id,
        login: superAdmin.login,
        nome: superAdmin.nome,
        tenantId: 'super-admin',
        roleName: 'Super Admin',
        isSuperAdmin: true
      },
      tokens: {
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        expiresAt: tokens.accessTokenExpires
      },
      forcePasswordChange: passwordValidation.shouldForceChange
    };

  } catch (error) {
    console.error('[AUTH] Erro na autenticação do super admin:', error);
    return {
      success: false,
      error: 'Erro interno do servidor'
    };
  }
}

/**
 * Renova access token usando refresh token
 * @param refreshToken - Refresh token válido
 * @returns Novo access token ou erro
 */
export async function refreshUserToken(refreshToken: string): Promise<{
  success: boolean;
  accessToken?: string;
  expiresAt?: Date;
  error?: string;
}> {
  const refreshPayload = validateRefreshToken(refreshToken);
  
  if (!refreshPayload) {
    return {
      success: false,
      error: 'Refresh token inválido ou expirado'
    };
  }

  try {
    // Buscar dados atualizados do usuário
    const user = await prisma.usuario.findUnique({
      where: { 
        id: refreshPayload.userId,
        ativo: true 
      },
      include: {
        role: {
          include: {
            permissoes: {
              include: {
                permissao: true
              }
            }
          }
        }
      }
    });

    if (!user) {
      return {
        success: false,
        error: 'Usuário não encontrado ou inativo'
      };
    }

    // Extrair permissões atualizadas
    const permissions = user.role.permissoes.map(rp => 
      `${rp.permissao.modulo}:${rp.permissao.acao}`
    );

    // Gerar novo access token
    const newAccessToken = refreshAccessToken(refreshToken, {
      roleId: user.roleId,
      roleName: user.role.nome,
      permissions,
      login: user.login,
      isSuperAdmin: false
    });

    if (!newAccessToken) {
      return {
        success: false,
        error: 'Falha ao gerar novo token'
      };
    }

    return {
      success: true,
      accessToken: newAccessToken,
      expiresAt: new Date(Date.now() + 15 * 60 * 1000) // 15 minutos
    };

  } catch (error) {
    console.error('[AUTH] Erro ao renovar token:', error);
    return {
      success: false,
      error: 'Erro interno do servidor'
    };
  }
}

/**
 * Cria novo usuário (apenas SuperAdmin pode)
 * @param userData - Dados do usuário
 * @returns Resultado da criação
 */
export async function createUser(userData: CreateUserRequest): Promise<CreateUserResponse> {
  try {
    // Verificar se o tenant existe
    const tenant = await prisma.tenant.findUnique({
      where: { id: userData.tenantId }
    });

    if (!tenant) {
      return {
        success: false,
        error: 'Tenant não encontrado'
      };
    }

    // Verificar se a role existe e pertence ao tenant
    const role = await prisma.role.findFirst({
      where: {
        id: userData.roleId,
        tenantId: userData.tenantId,
        ativo: true
      }
    });

    if (!role) {
      return {
        success: false,
        error: 'Role não encontrada ou inválida para este tenant'
      };
    }

    // Gerar login único
    let login = generateLogin(userData.nome);
    let attempts = 0;
    
    // Garantir que o login é único
    while (attempts < 10) {
      const existingUser = await prisma.usuario.findFirst({
        where: { login, tenantId: userData.tenantId }
      });

      if (!existingUser) break;
      
      // Gerar novo login se já existe
      login = generateLogin(userData.nome);
      attempts++;
    }

    if (attempts >= 10) {
      return {
        success: false,
        error: 'Não foi possível gerar login único'
      };
    }

    // Gerar senha temporária
    const temporaryPassword = generateTemporaryPassword();
    const passwordData = await createInitialPassword(temporaryPassword, true);

    // Criar usuário
    const newUser = await prisma.usuario.create({
      data: {
        login,
        nome: userData.nome,
        email: userData.email,
        roleId: userData.roleId,
        tenantId: userData.tenantId,
        senhaHash: passwordData.hashedPassword,
        forcarTrocaSenha: passwordData.forceChangeOnFirstLogin,
        usuarioCadastro: userData.createdBy
      }
    });

    return {
      success: true,
      user: {
        id: newUser.id,
        login: newUser.login,
        temporaryPassword: userData.generateCredentials ? temporaryPassword : undefined
      }
    };

  } catch (error) {
    console.error('[AUTH] Erro ao criar usuário:', error);
    return {
      success: false,
      error: 'Erro interno do servidor'
    };
  }
}

/**
 * Lista usuários de um tenant (com cache)
 * @param tenantId - ID do tenant
 * @returns Lista de usuários
 */
export async function getUsersByTenant(tenantId: string) {
  return cacheService.getOrSet(
    `users:${tenantId}`,
    async () => {
      return await prisma.usuario.findMany({
        where: {
          tenantId,
          ativo: true
        },
        select: {
          id: true,
          login: true,
          nome: true,
          email: true,
          ultimoLogin: true,
          role: {
            select: {
              id: true,
              nome: true
            }
          }
        },
        orderBy: {
          nome: 'asc'
        }
      });
    },
    600 // 10 minutos
  );
}