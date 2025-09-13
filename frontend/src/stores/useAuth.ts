/**
 * Store Zustand para gerenciamento de autenticação
 * Integrado com authService e tipos TypeScript
 */

import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { AuthStore, AuthError, LoginCredentials } from '@/types/auth';
import { authService } from '@/services/authService';

export const useAuth = create<AuthStore>()(
  devtools(
    (set, get) => ({
      // Estado inicial
      user: null,
      token: null,
      isLoading: false,
      error: null,
      isAuthenticated: false,

      // Ações
      login: async (credentials: LoginCredentials) => {
        set({ isLoading: true, error: null }, false, 'auth/login-start');

        try {
          const response = await authService.login(credentials);

          set({
            user: response.user,
            token: response.token,
            isLoading: false,
            isAuthenticated: true,
            error: null
          }, false, 'auth/login-success');

        } catch (error) {
          const authError: AuthError = {
            message: error instanceof Error ? error.message : 'Erro desconhecido',
            code: 'LOGIN_ERROR'
          };

          set({
            isLoading: false,
            error: authError,
            user: null,
            token: null,
            isAuthenticated: false
          }, false, 'auth/login-error');

          throw error;
        }
      },

      logout: () => {
        set({ isLoading: true }, false, 'auth/logout-start');

        authService.logout().finally(() => {
          set({
            user: null,
            token: null,
            isLoading: false,
            error: null,
            isAuthenticated: false
          }, false, 'auth/logout-complete');
        });
      },

      checkAuth: async () => {
        // Não mostrar loading para verificação silenciosa
        try {
          const user = await authService.checkAuth();

          if (user) {
            set({
              user,
              token: typeof window !== 'undefined'
                ? localStorage.getItem('token')
                : null,
              isAuthenticated: true,
              error: null
            }, false, 'auth/check-success');
          } else {
            set({
              user: null,
              token: null,
              isAuthenticated: false,
              error: null
            }, false, 'auth/check-not-authenticated');
          }
        } catch (error) {
          console.error('Erro ao verificar autenticação:', error);
          set({
            user: null,
            token: null,
            isAuthenticated: false,
            error: null // Não mostrar erro para verificação silenciosa
          }, false, 'auth/check-error');
        }
      },

      clearError: () => {
        set({ error: null }, false, 'auth/clear-error');
      },

      setLoading: (loading: boolean) => {
        set({ isLoading: loading }, false, 'auth/set-loading');
      }
    }),
    {
      name: 'auth-storage', // Nome para DevTools
      // Serializar apenas dados não sensíveis
      serialize: {
        options: {
          user: true,
          isAuthenticated: true,
          isLoading: true,
          error: true
        }
      }
    }
  )
);

// Hook para obter apenas o status de autenticação (otimização)
export const useAuthStatus = () => useAuth(state => ({
  isAuthenticated: state.isAuthenticated,
  isLoading: state.isLoading,
  user: state.user
}));

// Hook para obter apenas as ações de auth (otimização)
export const useAuthActions = () => useAuth(state => ({
  login: state.login,
  logout: state.logout,
  checkAuth: state.checkAuth,
  clearError: state.clearError,
  setLoading: state.setLoading
}));

// Hook para obter dados do usuário atual
export const useCurrentUser = () => useAuth(state => state.user);

// Hook para obter erros de autenticação
export const useAuthError = () => useAuth(state => state.error);

// Inicialização automática da verificação de auth
if (typeof window !== 'undefined') {
  // Verificar autenticação ao carregar a aplicação
  useAuth.getState().checkAuth();
}