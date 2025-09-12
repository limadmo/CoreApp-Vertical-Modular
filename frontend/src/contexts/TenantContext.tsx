/**
 * Context para gerenciar estado do tenant atual
 * Resolve automaticamente tenant via URL ou header
 */
import React, { createContext, useContext, useState, useEffect, useMemo } from 'react';
import { Tenant, TenantContextType, Module, ModuleCode, VerticalType } from '../types/index';

const TenantContext = createContext<TenantContextType | undefined>(undefined);

/**
 * Módulos disponíveis no sistema CoreApp
 * Configuração baseada em F1-F12 conforme CLAUDE.md
 */
const AVAILABLE_MODULES: Module[] = [
  // Módulos Starter (F1-F4)
  {
    code: 'VENDAS',
    name: 'Vendas',
    description: 'Sistema PDV e gestão de vendas',
    icon: 'cash',
    shortcut: 'F1',
    isActive: true,
    path: '/vendas',
    order: 1,
  },
  {
    code: 'CLIENTES',
    name: 'Clientes',
    description: 'Gestão de clientes e CRM',
    icon: 'users',
    shortcut: 'F2',
    isActive: true,
    path: '/clientes',
    order: 2,
  },
  {
    code: 'PRODUTOS',
    name: 'Produtos',
    description: 'Catálogo e gestão de produtos',
    icon: 'package',
    shortcut: 'F3',
    isActive: true,
    path: '/produtos',
    order: 3,
  },
  {
    code: 'ESTOQUE',
    name: 'Estoque',
    description: 'Controle de estoque e movimentações',
    icon: 'warehouse',
    shortcut: 'F4',
    isActive: true,
    path: '/estoque',
    order: 4,
  },
  // Módulos Adicionais (F5-F8)
  {
    code: 'FORNECEDORES',
    name: 'Fornecedores',
    description: 'Gestão de fornecedores e compras',
    icon: 'truck',
    shortcut: 'F5',
    isActive: false,
    path: '/fornecedores',
    order: 5,
  },
  {
    code: 'PROMOCOES',
    name: 'Promoções',
    description: 'Engine de descontos e campanhas',
    icon: 'percentage',
    shortcut: 'F6',
    isActive: false,
    path: '/promocoes',
    order: 6,
  },
  {
    code: 'RELATORIOS_BASICOS',
    name: 'Relatórios',
    description: 'Relatórios básicos e análises',
    icon: 'chart-bar',
    shortcut: 'F7',
    isActive: false,
    path: '/relatorios',
    order: 7,
  },
  {
    code: 'AUDITORIA',
    name: 'Auditoria',
    description: 'Compliance LGPD e auditoria',
    icon: 'shield-check',
    shortcut: 'F8',
    isActive: false,
    path: '/auditoria',
    order: 8,
  },
  // Módulos Sistema (F9-F12)
  {
    code: 'CONFIGURACOES',
    name: 'Configurações',
    description: 'Configurações do sistema',
    icon: 'settings',
    shortcut: 'F9',
    isActive: true,
    path: '/configuracoes',
    order: 9,
  },
  {
    code: 'DASHBOARD',
    name: 'Dashboard',
    description: 'Painel principal do sistema',
    icon: 'dashboard',
    shortcut: 'F10',
    isActive: true,
    path: '/',
    order: 10,
  },
  {
    code: 'HELP',
    name: 'Ajuda',
    description: 'Sistema de ajuda contextual',
    icon: 'help',
    shortcut: 'F11',
    isActive: true,
    path: '/ajuda',
    order: 11,
  },
  {
    code: 'ADMIN',
    name: 'Administração',
    description: 'Ferramentas administrativas',
    icon: 'shield',
    shortcut: 'Ctrl+F12', // F12 livre para Dev Tools, Ctrl+F12 para admin
    isActive: false,
    path: '/admin',
    order: 12,
  },
];

/**
 * Tenant de desenvolvimento/demo para testes
 */
const DEMO_TENANT: Tenant = {
  id: 'demo-padaria-123',
  nome: 'Padaria São José Demo',
  subdomain: 'padaria-demo',
  verticalType: 'PADARIA',
  activeModules: ['VENDAS', 'CLIENTES', 'PRODUTOS', 'ESTOQUE'],
  theme: {
    primaryColor: '#d97706', // Laranja quente para padarias
    brandColors: [
      '#fff7ed', '#ffedd5', '#fed7aa', '#fdba74', 
      '#fb923c', '#f97316', '#ea580c', '#dc2626', 
      '#c2410c', '#9a3412'
    ],
  },
  verticalConfig: {
    alertasValidadeHoras: 2,
    categoriasEspeciais: ['Pães', 'Bolos', 'Salgados', 'Doces'],
    gerenciarAlergenos: true,
    clienteFidelidade: true,
  },
  status: 'ACTIVE',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: new Date().toISOString(),
};

interface TenantProviderProps {
  children: React.ReactNode;
}

/**
 * Provider do contexto de tenant
 * Gerencia estado, resolução e validação do tenant atual
 */
export const TenantProvider: React.FC<TenantProviderProps> = ({ children }) => {
  const [currentTenant, setCurrentTenant] = useState<Tenant | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  /**
   * Resolve tenant atual baseado na URL ou configuração
   */
  const resolveTenant = async (): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);

      // Em desenvolvimento, usar tenant demo
      if (process.env.NODE_ENV === 'development') {
        await new Promise(resolve => setTimeout(resolve, 500)); // Simular loading
        setCurrentTenant(DEMO_TENANT);
        return;
      }

      // Em produção, resolver via API baseado no subdomínio
      const subdomain = window.location.hostname.split('.')[0];
      
      if (subdomain === 'www' || subdomain === window.location.hostname) {
        throw new Error('Subdomínio não identificado');
      }

      // TODO: Implementar chamada à API do backend para resolver tenant
      // const tenant = await tenantService.resolveBySubdomain(subdomain);
      
      // Por enquanto, usar demo
      setCurrentTenant(DEMO_TENANT);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao resolver tenant');
      console.error('Erro na resolução do tenant:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Módulos disponíveis filtrados pelos ativos no tenant
   */
  const availableModules = useMemo(() => {
    if (!currentTenant) return [];
    
    return AVAILABLE_MODULES.map(module => ({
      ...module,
      isActive: currentTenant.activeModules.includes(module.code),
    }));
  }, [currentTenant]);

  /**
   * Verifica se um módulo está ativo para o tenant atual
   */
  const hasModule = (moduleCode: ModuleCode): boolean => {
    return currentTenant?.activeModules.includes(moduleCode) ?? false;
  };

  /**
   * Obtém informações detalhadas de um módulo
   */
  const getModule = (code: ModuleCode): Module | undefined => {
    return availableModules.find(module => module.code === code);
  };

  /**
   * Força recarregamento dos dados do tenant
   */
  const refresh = async (): Promise<void> => {
    await resolveTenant();
  };

  // Resolver tenant na inicialização
  useEffect(() => {
    resolveTenant();
  }, []);

  const contextValue: TenantContextType = {
    currentTenant,
    availableModules,
    hasModule,
    getModule,
    isLoading,
    error,
    refresh,
  };

  return (
    <TenantContext.Provider value={contextValue}>
      {children}
    </TenantContext.Provider>
  );
};

/**
 * Hook para acessar contexto de tenant
 * @returns Contexto do tenant com validação
 * @throws Error se usado fora do provider
 */
export const useTenant = (): TenantContextType => {
  const context = useContext(TenantContext);
  
  if (context === undefined) {
    throw new Error('useTenant deve ser usado dentro de TenantProvider');
  }
  
  return context;
};