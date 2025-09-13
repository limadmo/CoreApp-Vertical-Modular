/**
 * Hook para gerenciamento de permissões por perfil
 * Controla acesso aos módulos F2-F6 baseado no usuário logado
 */

'use client';

import { useCurrentUser } from '@/stores/useAuth';
import { MODULES_BY_PROFILE, MODULE_NAMES } from '@/types/auth';

export const usePermissions = () => {
  const user = useCurrentUser();

  // Verificar se usuário tem acesso a um módulo específico
  const hasModuleAccess = (moduleKey: keyof typeof MODULE_NAMES): boolean => {
    if (!user || !user.perfil) return false;

    const allowedModules = MODULES_BY_PROFILE[user.perfil] || [];
    return allowedModules.includes(moduleKey);
  };

  // Verificar se usuário tem uma permissão específica
  const hasPermission = (permission: string): boolean => {
    if (!user || !user.permissions) return false;

    // Super admin tem acesso total
    if (user.permissions.includes('*')) return true;

    return user.permissions.includes(permission);
  };

  // Obter todos os módulos acessíveis pelo usuário
  const getAccessibleModules = () => {
    if (!user || !user.perfil) return [];

    const allowedModuleKeys = MODULES_BY_PROFILE[user.perfil] || [];

    return allowedModuleKeys.map(key => ({
      key,
      name: MODULE_NAMES[key as keyof typeof MODULE_NAMES],
      accessible: true
    }));
  };

  // Obter todos os módulos com status de acesso
  const getAllModulesWithStatus = () => {
    const allModules = Object.entries(MODULE_NAMES).map(([key, name]) => ({
      key: key as keyof typeof MODULE_NAMES,
      name,
      accessible: hasModuleAccess(key as keyof typeof MODULE_NAMES)
    }));

    return allModules;
  };

  // Verificar se pode navegar para um módulo
  const canNavigateToModule = (moduleKey: keyof typeof MODULE_NAMES): {
    allowed: boolean;
    reason?: string;
  } => {
    if (!user) {
      return { allowed: false, reason: 'Usuário não autenticado' };
    }

    if (!hasModuleAccess(moduleKey)) {
      const moduleName = MODULE_NAMES[moduleKey];
      return {
        allowed: false,
        reason: `Acesso negado ao módulo ${moduleName}. Perfil: ${user.perfil}`
      };
    }

    return { allowed: true };
  };

  // Log de tentativa de acesso (para auditoria)
  const logAccessAttempt = (moduleKey: keyof typeof MODULE_NAMES, success: boolean) => {
    if (typeof window !== 'undefined' && console && console.log) {
      const moduleName = MODULE_NAMES[moduleKey];
      const status = success ? '✅' : '❌';
      console.log(
        `${status} Acesso ao módulo ${moduleName} (${moduleKey}) - Usuário: ${user?.login || 'N/A'} - Perfil: ${user?.perfil || 'N/A'}`
      );
    }
  };

  return {
    user,
    hasModuleAccess,
    hasPermission,
    getAccessibleModules,
    getAllModulesWithStatus,
    canNavigateToModule,
    logAccessAttempt,

    // Shortcuts para verificações comuns
    isAdmin: user?.perfil === 'SUPER_ADMIN' || user?.perfil === 'ADMIN_LOJA',
    isManager: user?.perfil === 'GERENTE',
    isSeller: user?.perfil === 'VENDEDOR',
    isCashier: user?.perfil === 'CAIXA',

    // Módulos específicos
    canAccessSales: hasModuleAccess('F2'),
    canAccessClients: hasModuleAccess('F3'),
    canAccessProducts: hasModuleAccess('F4'),
    canAccessStock: hasModuleAccess('F5'),
    canAccessSuppliers: hasModuleAccess('F6')
  };
};