/**
 * Cliente HTTP para comunicação com o backend CoreApp
 * Configurado com interceptors para tenant e autenticação
 */
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { notifications } from '@mantine/notifications';

export interface ApiResponse<T = any> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T = any> {
  data: T[];
  total: number;
  page: number;
  limit: number;
  totalPages: number;
}

/**
 * Configuração base da API
 */
const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080/api',
  timeout: 30000, // 30 segundos
  headers: {
    'Content-Type': 'application/json',
  },
};

/**
 * Instância principal do Axios
 */
export const api: AxiosInstance = axios.create(API_CONFIG);

/**
 * Interceptor de request para adicionar tenant e auth
 */
api.interceptors.request.use(
  (config) => {
    // Adicionar tenant ID (pode vir de localStorage, context, etc.)
    const tenantId = getCurrentTenantId();
    if (tenantId) {
      config.headers['X-Tenant-ID'] = tenantId;
    }

    // Adicionar token de autenticação
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Log da requisição em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`🚀 ${config.method?.toUpperCase()} ${config.url}`, {
        headers: config.headers,
        data: config.data,
      });
    }

    return config;
  },
  (error) => {
    console.error('Erro no interceptor de request:', error);
    return Promise.reject(error);
  }
);

/**
 * Interceptor de response para tratamento global de erros
 */
api.interceptors.response.use(
  (response: AxiosResponse) => {
    // Log da resposta em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`✅ ${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`, response.data);
    }

    return response;
  },
  (error) => {
    // Log do erro
    console.error('❌ Erro na API:', error);

    // Tratamento por status code
    if (error.response) {
      const { status, data } = error.response;

      switch (status) {
        case 401:
          notifications.show({
            title: 'Não Autorizado',
            message: 'Sua sessão expirou. Faça login novamente.',
            color: 'red',
            autoClose: 5000,
          });
          // TODO: Redirect para login
          break;

        case 403:
          notifications.show({
            title: 'Acesso Negado',
            message: 'Você não tem permissão para realizar esta ação.',
            color: 'orange',
            autoClose: 5000,
          });
          break;

        case 404:
          notifications.show({
            title: 'Não Encontrado',
            message: 'O recurso solicitado não foi encontrado.',
            color: 'yellow',
            autoClose: 3000,
          });
          break;

        case 422:
          // Erros de validação
          if (data?.errors) {
            data.errors.forEach((errorMsg: string) => {
              notifications.show({
                title: 'Erro de Validação',
                message: errorMsg,
                color: 'red',
                autoClose: 4000,
              });
            });
          }
          break;

        case 429:
          notifications.show({
            title: 'Muitas Requisições',
            message: 'Você está fazendo muitas requisições. Tente novamente em alguns minutos.',
            color: 'orange',
            autoClose: 5000,
          });
          break;

        case 500:
          notifications.show({
            title: 'Erro do Servidor',
            message: 'Ocorreu um erro interno. Tente novamente mais tarde.',
            color: 'red',
            autoClose: 5000,
          });
          break;

        default:
          notifications.show({
            title: 'Erro',
            message: data?.message || 'Ocorreu um erro inesperado.',
            color: 'red',
            autoClose: 5000,
          });
      }
    } else if (error.request) {
      // Erro de rede
      notifications.show({
        title: 'Erro de Conexão',
        message: 'Não foi possível conectar ao servidor. Verifique sua conexão.',
        color: 'red',
        autoClose: 5000,
      });
    }

    return Promise.reject(error);
  }
);

/**
 * Obter tenant atual (implementar conforme contexto)
 */
function getCurrentTenantId(): string | null {
  // Em desenvolvimento, usar tenant demo
  if (import.meta.env.DEV) {
    return 'demo-padaria-123';
  }
  
  // TODO: Implementar obtenção do tenant do contexto
  return null;
}

/**
 * Obter token de autenticação
 */
function getAuthToken(): string | null {
  return localStorage.getItem('auth_token');
}

/**
 * Definir token de autenticação
 */
export function setAuthToken(token: string): void {
  localStorage.setItem('auth_token', token);
}

/**
 * Remover token de autenticação
 */
export function removeAuthToken(): void {
  localStorage.removeItem('auth_token');
}

/**
 * Cliente API tipado para respostas padrão
 */
export const apiClient = {
  /**
   * GET request
   */
  async get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await api.get<ApiResponse<T>>(url, config);
    return response.data.data;
  },

  /**
   * POST request
   */
  async post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await api.post<ApiResponse<T>>(url, data, config);
    return response.data.data;
  },

  /**
   * PUT request
   */
  async put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await api.put<ApiResponse<T>>(url, data, config);
    return response.data.data;
  },

  /**
   * DELETE request
   */
  async delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await api.delete<ApiResponse<T>>(url, config);
    return response.data.data;
  },

  /**
   * GET request paginado
   */
  async getPaginated<T = any>(
    url: string, 
    params?: { page?: number; limit?: number; [key: string]: any }
  ): Promise<PaginatedResponse<T>> {
    const response = await api.get<ApiResponse<PaginatedResponse<T>>>(url, { params });
    return response.data.data;
  },
};

export default api;