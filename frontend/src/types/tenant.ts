/**
 * Tipos para sistema multi-tenant CoreApp
 */

/** Códigos dos módulos comerciais */
export type ModuleCode = 
  // Módulos Starter (inclusos)
  | 'VENDAS' 
  | 'CLIENTES' 
  | 'PRODUTOS' 
  | 'ESTOQUE' 
  | 'USUARIOS'
  // Módulos Adicionais (opcionais)
  | 'FORNECEDORES'
  | 'PROMOCOES'
  | 'RELATORIOS_BASICOS'
  | 'RELATORIOS_AVANCADOS'
  | 'AUDITORIA'
  | 'PAGAMENTOS'
  | 'MOBILE'
  | 'PRECIFICACAO';

/** Tipos de verticais suportados */
export type VerticalType = 
  | 'PADARIA'
  | 'FARMACIA' 
  | 'SUPERMERCADO'
  | 'OTICA'
  | 'DELIVERY'
  | 'AUTOPECAS'
  | 'PETSHOP'
  | 'MATERIAL_CONSTRUCAO';

/** Informações do módulo comercial */
export interface Module {
  /** Código único do módulo */
  code: ModuleCode;
  /** Nome para exibição */
  name: string;
  /** Descrição do módulo */
  description: string;
  /** Ícone do módulo */
  icon: string;
  /** Atalho de teclado (F1-F12) */
  shortcut: string;
  /** Se o módulo está ativo para o tenant */
  isActive: boolean;
  /** URL de roteamento */
  path: string;
  /** Ordem no menu */
  order: number;
}

/** Configurações do tema por tenant */
export interface TenantTheme {
  /** Cores primárias customizadas */
  primaryColor: string;
  /** Logo da empresa */
  logoUrl?: string;
  /** Favicon customizado */
  faviconUrl?: string;
  /** Cores do brand (array de 10 tons) */
  brandColors?: string[];
}

/** Dados do tenant/loja */
export interface Tenant {
  /** ID único do tenant */
  id: string;
  /** Nome da empresa/loja */
  nome: string;
  /** Subdomínio (ex: padaria123) */
  subdomain: string;
  /** Tipo de vertical da empresa */
  verticalType: VerticalType;
  /** Módulos ativos para este tenant */
  activeModules: ModuleCode[];
  /** Tema customizado */
  theme?: TenantTheme;
  /** Configurações específicas do vertical */
  verticalConfig?: Record<string, any>;
  /** Status do tenant */
  status: 'ACTIVE' | 'INACTIVE' | 'SUSPENDED';
  /** Data de criação */
  createdAt: string;
  /** Data de atualização */
  updatedAt: string;
}

/** Context do tenant atual */
export interface TenantContextType {
  /** Tenant atual identificado */
  currentTenant: Tenant | null;
  /** Módulos disponíveis para o tenant */
  availableModules: Module[];
  /** Verifica se módulo está ativo */
  hasModule: (module: ModuleCode) => boolean;
  /** Obtém informações de um módulo */
  getModule: (code: ModuleCode) => Module | undefined;
  /** Loading state */
  isLoading: boolean;
  /** Erro de resolução */
  error: string | null;
  /** Recarregar dados do tenant */
  refresh: () => Promise<void>;
}