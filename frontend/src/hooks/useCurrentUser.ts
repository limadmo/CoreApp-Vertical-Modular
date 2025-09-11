/**
 * Hook para gerenciar dados do usuário logado
 * Sistema enterprise com login configurável
 */
import { useState, useEffect } from 'react';

export interface CurrentUser {
  /** ID único do usuário */
  id: string;
  /** Nome completo */
  name: string;
  /** Login identificador configurável (ex: JS01234A) */
  login: string;
  /** Email do usuário */
  email: string;
  /** Cargo/função */
  role: string;
  /** URL da foto do usuário (opcional) */
  photoUrl?: string;
  /** Status online */
  isOnline: boolean;
  /** Tenant ID */
  tenantId: string;
  /** Permissões */
  permissions: string[];
}

/**
 * Gera login configurável baseado no padrão do tenant
 * Formato padrão: 2 letras + 5 dígitos + 1 letra (ex: JS01234A)
 */
const generateConfigurableLogin = (name: string, userId: string): string => {
  // Extrair iniciais do nome
  const nameParts = name.trim().split(' ').filter(part => part.length > 0);
  const firstInitial = nameParts[0]?.[0]?.toUpperCase() || 'U';
  const lastInitial = nameParts[nameParts.length - 1]?.[0]?.toUpperCase() || 'S';
  
  // Gerar 5 dígitos baseados no hash do userId
  let hash = 0;
  for (let i = 0; i < userId.length; i++) {
    hash = userId.charCodeAt(i) + ((hash << 5) - hash);
  }
  const digits = Math.abs(hash).toString().padStart(5, '0').slice(0, 5);
  
  // Letra final baseada na soma dos dígitos
  const digitSum = digits.split('').reduce((sum, digit) => sum + parseInt(digit), 0);
  const finalLetter = String.fromCharCode(65 + (digitSum % 26)); // A-Z
  
  return `${firstInitial}${lastInitial}${digits}${finalLetter}`;
};

/**
 * Simulação de usuário demo para desenvolvimento
 */
const generateDemoUser = (): CurrentUser => {
  const names = [
    'João Silva Santos',
    'Maria Oliveira Costa', 
    'Pedro Rodrigues Almeida',
    'Ana Carolina Ferreira',
    'Roberto Carlos Mendes',
    'Fernanda Lima Souza'
  ];
  
  const roles = [
    'Gerente Geral',
    'Supervisor de Vendas', 
    'Operador de Caixa',
    'Administrador do Sistema',
    'Coordenador de Estoque',
    'Analista Financeiro'
  ];
  
  const selectedName = names[Math.floor(Math.random() * names.length)];
  const selectedRole = roles[Math.floor(Math.random() * roles.length)];
  const userId = `user-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  
  // URLs de fotos demo (algumas com foto, outras sem)
  const demoPhotos = [
    'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face',
    'https://images.unsplash.com/photo-1494790108755-2616b9c95f3d?w=150&h=150&fit=crop&crop=face', 
    'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face',
    null, // Sem foto - usar iniciais
    null, // Sem foto - usar iniciais
    'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&h=150&fit=crop&crop=face',
  ];
  
  const selectedPhoto = demoPhotos[Math.floor(Math.random() * demoPhotos.length)];

  return {
    id: userId,
    name: selectedName,
    login: generateConfigurableLogin(selectedName, userId),
    email: `${selectedName.toLowerCase().replace(/\s+/g, '.')}@padariasaojose.com.br`,
    role: selectedRole,
    photoUrl: selectedPhoto || undefined,
    isOnline: Math.random() > 0.2, // 80% chance de estar online
    tenantId: 'demo-padaria-123',
    permissions: ['read', 'write', 'admin'], // Permissions demo
  };
};

/**
 * Hook para acessar dados do usuário atual
 * Em produção, integraria com sistema de autenticação real
 */
export const useCurrentUser = () => {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Simular carregamento de dados do usuário
    const loadUser = async () => {
      setIsLoading(true);
      
      // Simular delay de rede
      await new Promise(resolve => setTimeout(resolve, 500));
      
      // Em produção, aqui faria a chamada para API de autenticação
      const user = generateDemoUser();
      setCurrentUser(user);
      
      setIsLoading(false);
    };

    loadUser();
  }, []);

  /**
   * Atualizar status online/offline
   */
  const updateOnlineStatus = (isOnline: boolean) => {
    if (currentUser) {
      setCurrentUser({ ...currentUser, isOnline });
    }
  };

  /**
   * Fazer logout do usuário
   */
  const logout = () => {
    setCurrentUser(null);
    // Em produção, limparia tokens e redirecionaria para login
  };

  return {
    currentUser,
    isLoading,
    updateOnlineStatus,
    logout,
    /** Helper para verificar permissões */
    hasPermission: (permission: string) => 
      currentUser?.permissions.includes(permission) ?? false,
  };
};