/**
 * Cliente Service - Operações CRUD completas
 * Conecta frontend Next.js com API Express.js + Prisma
 */
import { api } from '@/lib/api';

// Interfaces TypeScript baseadas no backend
export interface Cliente {
  id: string;
  nome: string;
  sobrenome: string;
  cpf?: string | null;
  dataNascimento?: string | null;
  ativo: boolean;
  dataCadastro: string;
  dataUltimaAtualizacao: string;
  tenantId: string;
}

export interface ClienteCreateRequest {
  nome: string;
  sobrenome: string;
  cpf?: string;
  dataNascimento?: string;
}

export interface ClienteUpdateRequest extends ClienteCreateRequest {
  id: string;
}

export interface ClienteListResponse {
  success: boolean;
  data: {
    items: Cliente[];
    pagination: {
      currentPage: number;
      totalPages: number;
      totalItems: number;
      itemsPerPage: number;
      hasNext: boolean;
      hasPrevious: boolean;
    };
  };
}

export interface ClienteResponse {
  success: boolean;
  data: Cliente;
  message?: string;
}

export interface ClienteEstatisticas {
  total: number;
  ativos: number;
  inativos: number;
  comCpf: number;
  semCpf: number;
  novosMes: number;
}

export interface ClienteEstatisticasResponse {
  success: boolean;
  data: ClienteEstatisticas;
}

// Parâmetros de busca
export interface ClienteQueryParams {
  page?: number;
  limit?: number;
  search?: string;
  ativo?: boolean;
}

/**
 * Classe ClienteService - Todas operações de cliente
 */
export class ClienteService {
  private baseUrl = '/clientes';

  /**
   * Listar clientes com paginação e filtros
   */
  async listar(params: ClienteQueryParams = {}): Promise<ClienteListResponse> {
    const queryParams = new URLSearchParams();

    if (params.page) queryParams.append('page', params.page.toString());
    if (params.limit) queryParams.append('limit', params.limit.toString());
    if (params.search) queryParams.append('search', params.search);
    if (params.ativo !== undefined) queryParams.append('ativo', params.ativo.toString());

    const response = await api.get(`${this.baseUrl}?${queryParams}`);
    return response.data;
  }

  /**
   * Obter cliente por ID
   */
  async obterPorId(id: string): Promise<ClienteResponse> {
    const response = await api.get(`${this.baseUrl}/${id}`);
    return response.data;
  }

  /**
   * Criar novo cliente
   */
  async criar(dados: ClienteCreateRequest): Promise<ClienteResponse> {
    const response = await api.post(this.baseUrl, dados);
    return response.data;
  }

  /**
   * Atualizar cliente existente
   */
  async atualizar(id: string, dados: ClienteCreateRequest): Promise<ClienteResponse> {
    const response = await api.put(`${this.baseUrl}/${id}`, dados);
    return response.data;
  }

  /**
   * Remover cliente (soft delete)
   */
  async remover(id: string, motivo?: string): Promise<{ success: boolean; message: string }> {
    const response = await api.delete(`${this.baseUrl}/${id}`, {
      data: { motivo }
    });
    return response.data;
  }

  /**
   * Restaurar cliente removido
   */
  async restaurar(id: string): Promise<{ success: boolean; message: string }> {
    const response = await api.put(`${this.baseUrl}/${id}/restaurar`);
    return response.data;
  }

  /**
   * Buscar cliente por CPF
   */
  async buscarPorCpf(cpf: string): Promise<ClienteResponse> {
    const response = await api.get(`${this.baseUrl}/cpf/${cpf}`);
    return response.data;
  }

  /**
   * Buscar clientes por faixa etária
   */
  async buscarPorIdade(idadeMin: number, idadeMax: number): Promise<ClienteListResponse> {
    const response = await api.get(`${this.baseUrl}/idade/${idadeMin}/${idadeMax}`);
    return { success: true, data: { items: response.data.data, pagination: {} as any } };
  }

  /**
   * Obter estatísticas de clientes
   */
  async obterEstatisticas(): Promise<ClienteEstatisticasResponse> {
    const response = await api.get(`${this.baseUrl}/estatisticas`);
    return response.data;
  }
}

// Instância singleton para uso em toda aplicação
export const clienteService = new ClienteService();

// Funções utilitárias para formatação
export const formatarCpf = (cpf: string): string => {
  const cleaned = cpf.replace(/\D/g, '');
  return cleaned.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
};

export const validarCpf = (cpf: string): boolean => {
  const cleaned = cpf.replace(/\D/g, '');
  if (cleaned.length !== 11 || /^(\d)\1+$/.test(cleaned)) return false;

  let soma = 0;
  for (let i = 0; i < 9; i++) {
    soma += parseInt(cleaned[i]) * (10 - i);
  }
  let digito = 11 - (soma % 11);
  if (digito >= 10) digito = 0;
  if (parseInt(cleaned[9]) !== digito) return false;

  soma = 0;
  for (let i = 0; i < 10; i++) {
    soma += parseInt(cleaned[i]) * (11 - i);
  }
  digito = 11 - (soma % 11);
  if (digito >= 10) digito = 0;
  return parseInt(cleaned[10]) === digito;
};

export const formatarNomeCompleto = (cliente: Cliente): string => {
  return `${cliente.nome} ${cliente.sobrenome}`;
};