/**
 * API Client - Configuração para conectar com backend Express.js
 * Axios configurado com interceptors para multi-tenant e auth
 */
import axios from 'axios';

// URL do backend (será configurado via env)
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080';

// Criar instância do axios
export const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar headers necessários
api.interceptors.request.use(
  (config) => {
    // TODO: Adicionar token JWT quando auth estiver implementado
    // const token = localStorage.getItem('auth_token');
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`;
    // }

    // Adicionar tenant ID (por enquanto fixo para demo)
    config.headers['X-Tenant-ID'] = 'demo';

    console.log(`🔄 API Request: ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('❌ Request Error:', error);
    return Promise.reject(error);
  }
);

// Interceptor para tratar respostas
api.interceptors.response.use(
  (response) => {
    console.log(`✅ API Response: ${response.status} ${response.config.url}`);
    return response;
  },
  (error) => {
    console.error('❌ Response Error:', error.response?.data || error.message);

    // Tratar erros específicos
    if (error.response?.status === 401) {
      // TODO: Redirect para login quando auth estiver implementado
      console.warn('🔐 Não autorizado - redirect para login');
    }

    if (error.response?.status === 403) {
      console.warn('🚫 Sem permissão para esta operação');
    }

    return Promise.reject(error);
  }
);

export default api;