/**
 * Tipos TypeScript para autenticação
 * Seguindo padrões do CoreApp multi-tenant
 */

export interface LoginCredentials {
  login: string;
  password: string;
}

export interface User {
  id: string;
  login: string;
  nome: string;
  perfil: 'SUPER_ADMIN' | 'ADMIN_LOJA' | 'GERENTE' | 'VENDEDOR' | 'CAIXA';
  tenantId: string;
  modulos: string[];
  permissions: string[];
  createdAt: string;
  updatedAt: string;
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken: string;
  expiresIn: number;
}

export interface AuthError {
  message: string;
  code: string;
  field?: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  error: AuthError | null;
  isAuthenticated: boolean;
}

export interface AuthActions {
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  clearError: () => void;
  setLoading: (loading: boolean) => void;
}

export type AuthStore = AuthState & AuthActions;

// Constantes para perfis de usuário
export const USER_PROFILES = {
  SUPER_ADMIN: 'Super Administrador',
  ADMIN_LOJA: 'Administrador da Loja',
  GERENTE: 'Gerente',
  VENDEDOR: 'Vendedor',
  CAIXA: 'Operador de Caixa'
} as const;

// Credenciais padrão do sistema
export const DEFAULT_CREDENTIALS = {
  admin: 'admin123',
  gerente: 'gerente123',
  vendedor: 'vendedor123',
  caixa: 'caixa123'
} as const;

// Módulos disponíveis por perfil (teclas F)
export const MODULES_BY_PROFILE = {
  SUPER_ADMIN: ['F2', 'F3', 'F4', 'F5', 'F6'], // Todos os módulos
  ADMIN_LOJA: ['F2', 'F3', 'F4', 'F5', 'F6'],  // Todos os módulos
  GERENTE: ['F2', 'F3', 'F4', 'F5'],           // Sem fornecedores
  VENDEDOR: ['F2', 'F3'],                      // Vendas + clientes
  CAIXA: ['F2']                                // Apenas vendas
} as const;

// Mapeamento de módulos F para nomes
export const MODULE_NAMES = {
  F2: 'Vendas',
  F3: 'Clientes',
  F4: 'Produtos',
  F5: 'Estoque',
  F6: 'Fornecedores'
} as const;