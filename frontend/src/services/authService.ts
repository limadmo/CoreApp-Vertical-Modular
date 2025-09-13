/**
 * Serviço de autenticação real para Express.js + Prisma
 * Conecta ao backend em localhost:5000 - SEM MOCKS
 */

import { AuthResponse, LoginCredentials, User } from '@/types/auth';

class AuthService {
  private readonly API_BASE_URL = 'http://localhost:3000/api';
  private readonly DEFAULT_TENANT_ID = 'padaria-demo';

  private getTenantId(): string {
    // Para este sistema, sempre usar o tenant demo criado no seed
    if (typeof window !== 'undefined') {
      return localStorage.getItem('tenantId') || this.DEFAULT_TENANT_ID;
    }
    return this.DEFAULT_TENANT_ID;
  }

  private getHeaders(includeAuth = false): Record<string, string> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json'
    };

    if (includeAuth && typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        headers.Authorization = `Bearer ${token}`;
      }
    }

    return headers;
  }

  private mapBackendUserToFrontend(backendUser: any): User {
    // Mapear roles do backend para perfis do frontend
    const roleToPerfilMap: Record<string, User['perfil']> = {
      'Admin': 'SUPER_ADMIN',
      'Gerente': 'GERENTE',
      'Vendedor': 'VENDEDOR',
      'Caixa': 'CAIXA'
    };

    // Mapear permissões para módulos F
    const getModulesFromPermissions = (permissions: string[]): string[] => {
      const modules: string[] = [];

      // Admin tem todos os módulos
      if (permissions.includes('admin') || backendUser.role?.nome === 'Admin') {
        return ['F2', 'F3', 'F4', 'F5', 'F6'];
      }

      // Baseado nas permissões
      if (permissions.some(p => p.includes('vendas'))) modules.push('F2');
      if (permissions.some(p => p.includes('clientes'))) modules.push('F3');
      if (permissions.some(p => p.includes('produtos'))) modules.push('F4');
      if (permissions.some(p => p.includes('estoque'))) modules.push('F5');
      if (permissions.some(p => p.includes('fornecedores'))) modules.push('F6');

      return modules;
    };

    const permissions = backendUser.permissions || [];
    const roleName = backendUser.roleName || backendUser.role?.nome || 'Caixa';

    return {
      id: backendUser.id || backendUser.userId,
      login: backendUser.login,
      nome: backendUser.nome || backendUser.name,
      perfil: roleToPerfilMap[roleName] || 'CAIXA',
      tenantId: backendUser.tenantId || this.DEFAULT_TENANT_ID,
      modulos: getModulesFromPermissions(permissions),
      permissions,
      createdAt: backendUser.createdAt || new Date().toISOString(),
      updatedAt: backendUser.updatedAt || new Date().toISOString()
    };
  }

  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    try {
      const response = await fetch(`${this.API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: this.getHeaders(),
        body: JSON.stringify({
          login: credentials.login,
          password: credentials.password,
          tenantId: this.getTenantId()
        }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error || `Erro de login: ${response.status}`);
      }

      const backendData = await response.json();

      // Mapear resposta do backend para formato do frontend
      const user = this.mapBackendUserToFrontend(backendData.user);

      const authResponse: AuthResponse = {
        user,
        token: backendData.tokens?.accessToken || backendData.accessToken,
        refreshToken: backendData.tokens?.refreshToken || backendData.refreshToken,
        expiresIn: backendData.tokens?.expiresIn || backendData.expiresIn || 3600
      };

      // Salvar dados no localStorage e cookies
      if (typeof window !== 'undefined') {
        localStorage.setItem('token', authResponse.token);
        localStorage.setItem('refreshToken', authResponse.refreshToken);
        localStorage.setItem('user', JSON.stringify(authResponse.user));
        localStorage.setItem('tenantId', user.tenantId);

        // Salvar token em cookie para o middleware
        document.cookie = `token=${authResponse.token}; path=/; max-age=3600; SameSite=Strict`;
      }

      return authResponse;

    } catch (error) {
      console.error('Erro no login:', error);
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('Erro de conexão com o servidor');
    }
  }

  async logout(): Promise<void> {
    try {
      // Tentar fazer logout no servidor (se houver endpoint)
      await fetch(`${this.API_BASE_URL}/auth/logout`, {
        method: 'POST',
        headers: this.getHeaders(true),
      }).catch(() => {
        // Ignorar erros de logout - o importante é limpar o localStorage
      });
    } catch (error) {
      console.warn('Erro ao fazer logout no servidor:', error);
    } finally {
      // Limpar localStorage e cookies independentemente do resultado da API
      if (typeof window !== 'undefined') {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        localStorage.removeItem('tenantId');

        // Limpar cookie do token
        document.cookie = 'token=; path=/; max-age=0; SameSite=Strict';
      }
    }
  }

  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = typeof window !== 'undefined'
      ? localStorage.getItem('refreshToken')
      : null;

    if (!refreshToken) {
      throw new Error('Refresh token não encontrado');
    }

    try {
      const response = await fetch(`${this.API_BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: this.getHeaders(),
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        throw new Error('Erro ao renovar token');
      }

      const backendData = await response.json();

      // Mapear resposta do backend
      const user = this.mapBackendUserToFrontend(backendData.user);

      const authResponse: AuthResponse = {
        user,
        token: backendData.accessToken,
        refreshToken: backendData.refreshToken || refreshToken,
        expiresIn: backendData.expiresIn || 3600
      };

      // Atualizar localStorage
      if (typeof window !== 'undefined') {
        localStorage.setItem('token', authResponse.token);
        localStorage.setItem('refreshToken', authResponse.refreshToken);
        localStorage.setItem('user', JSON.stringify(authResponse.user));
      }

      return authResponse;

    } catch (error) {
      console.error('Erro ao renovar token:', error);
      this.logout();
      throw error;
    }
  }

  async checkAuth(): Promise<User | null> {
    if (typeof window === 'undefined') return null;

    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (!token || !userStr) return null;

    try {
      // Verificar se o token ainda é válido via profile endpoint
      const response = await fetch(`${this.API_BASE_URL}/auth/profile`, {
        method: 'GET',
        headers: this.getHeaders(true),
      });

      if (!response.ok) {
        // Tentar renovar o token
        try {
          const refreshData = await this.refreshToken();
          return refreshData.user;
        } catch {
          // Token inválido, limpar dados
          this.logout();
          return null;
        }
      }

      // Token válido, retornar usuário do localStorage
      return JSON.parse(userStr) as User;

    } catch (error) {
      console.error('Erro ao verificar autenticação:', error);
      this.logout();
      return null;
    }
  }

  // Método utilitário para obter dados do usuário do localStorage
  getCurrentUser(): User | null {
    if (typeof window === 'undefined') return null;

    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) as User : null;
  }

  // Método utilitário para verificar se está autenticado
  isAuthenticated(): boolean {
    if (typeof window === 'undefined') return false;

    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');

    return !!(token && user);
  }
}

export const authService = new AuthService();