/**
 * Service para comunicação com API de Clientes Brasileiros
 * Usa APENAS dados reais da base de dados via API
 */
import { api } from './api';
import {
  Cliente,
  ClienteResumo,
  CriarClienteRequest,
  AtualizarClienteRequest,
  BuscarClienteRequest,
  PagedResult,
  EnderecoViaCep,
  BuscarCepRequest,
  BuscarEnderecoRequest,
  CpfValidationResult,
  TelefoneValidationResult,
  CepValidationResult,
  DireitoEsquecimentoRequest,
  ClienteLgpd,
  ClienteEstatisticas,
  ClienteHistorico,
  HistoricoClienteRequest
} from '../types/cliente';

const BASE_URL = '/clientes';

export const clienteService = {
  // === CRUD BÁSICO ===
  
  /**
   * Lista clientes com paginação e filtros
   */
  async listar(request: BuscarClienteRequest): Promise<PagedResult<ClienteResumo>> {
    try {
      console.log('📊 Buscando clientes da API real:', request);
      const response = await api.get(`${BASE_URL}`, { params: request });
      console.log('✅ Clientes obtidos da API:', response.data);
      return response.data;
    } catch (error) {
      console.error('❌ Erro ao buscar clientes da API:', error);
      throw error;
    }
  },

  /**
   * Obtém um cliente por ID
   */
  async obterPorId(id: string): Promise<Cliente> {
    try {
      console.log(`📋 Buscando cliente ${id} da API real`);
      const response = await api.get(`${BASE_URL}/${id}`);
      return response.data;
    } catch (error) {
      console.error(`❌ Erro ao buscar cliente ${id}:`, error);
      throw error;
    }
  },

  /**
   * Cria um novo cliente
   */
  async criar(request: CriarClienteRequest, ipOrigem?: string): Promise<Cliente> {
    try {
      console.log('✨ Criando novo cliente via API real');
      const headers = ipOrigem ? { 'X-Client-IP': ipOrigem } : {};
      const response = await api.post(`${BASE_URL}`, request, { headers });
      return response.data;
    } catch (error) {
      console.error('❌ Erro ao criar cliente:', error);
      throw error;
    }
  },

  /**
   * Atualiza um cliente existente
   */
  async atualizar(id: string, request: AtualizarClienteRequest): Promise<Cliente> {
    try {
      console.log(`🔄 Atualizando cliente ${id} via API real`);
      const response = await api.put(`${BASE_URL}/${id}`, request);
      return response.data;
    } catch (error) {
      console.error(`❌ Erro ao atualizar cliente ${id}:`, error);
      throw error;
    }
  },

  /**
   * Remove um cliente (soft delete)
   */
  async remover(id: string, motivo?: string): Promise<boolean> {
    try {
      console.log(`🗑️ Removendo cliente ${id} via API real`);
      const params = motivo ? { motivo } : {};
      const response = await api.delete(`${BASE_URL}/${id}`, { params });
      return response.data;
    } catch (error) {
      console.error(`❌ Erro ao remover cliente ${id}:`, error);
      throw error;
    }
  },

  // === BUSCA AVANÇADA ===

  /**
   * Busca clientes por termo geral
   */
  async buscarPorTermo(termo: string, limite: number = 20): Promise<ClienteResumo[]> {
    const request: BuscarClienteRequest = { 
      termo, 
      tamanhoPagina: limite,
      pagina: 1,
      direcaoOrdenacao: 'ASC'
    };
    const result = await this.listar(request);
    return result.items;
  },

  /**
   * Busca cliente por CPF
   */
  async buscarPorCpf(cpf: string): Promise<Cliente | null> {
    try {
      const response = await api.get(`${BASE_URL}/cpf/${encodeURIComponent(cpf)}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  /**
   * Verifica se CPF já existe
   */
  async cpfJaExiste(cpf: string, excluirId?: string): Promise<boolean> {
    try {
      const params = excluirId ? { excluirId } : {};
      const response = await api.get(`${BASE_URL}/cpf/${encodeURIComponent(cpf)}/exists`, { params });
      return response.data;
    } catch (error) {
      console.error('❌ Erro ao verificar CPF:', error);
      return false;
    }
  },

  /**
   * Verifica se email já existe
   */
  async emailJaExiste(email: string, excluirId?: string): Promise<boolean> {
    try {
      const params = excluirId ? { excluirId } : {};
      const response = await api.get(`${BASE_URL}/email/${encodeURIComponent(email)}/exists`, { params });
      return response.data;
    } catch (error) {
      console.error('❌ Erro ao verificar email:', error);
      return false;
    }
  },

  // === VALIDAÇÕES ===

  /**
   * Valida CPF
   */
  async validarCpf(cpf: string): Promise<CpfValidationResult> {
    const response = await api.post(`${BASE_URL}/validar/cpf`, { cpf });
    return response.data;
  },

  /**
   * Valida telefone
   */
  async validarTelefone(telefone: string): Promise<TelefoneValidationResult> {
    const response = await api.post(`${BASE_URL}/validar/telefone`, { telefone });
    return response.data;
  },

  /**
   * Valida CEP e consulta endereço
   */
  async validarCep(cep: string): Promise<CepValidationResult> {
    const response = await api.post(`${BASE_URL}/validar/cep`, { cep });
    return response.data;
  },

  /**
   * Busca endereço por CEP via ViaCEP
   */
  async buscarEnderecoPorCep(request: BuscarCepRequest): Promise<EnderecoViaCep> {
    const response = await api.post(`${BASE_URL}/endereco/cep`, request);
    return response.data;
  },

  /**
   * Busca CEPs por endereço (busca reversa)
   */
  async buscarCepPorEndereco(request: BuscarEnderecoRequest): Promise<EnderecoViaCep[]> {
    const response = await api.post(`${BASE_URL}/endereco/buscar`, request);
    return response.data;
  },

  // === BUSINESS LOGIC ===

  /**
   * Atualiza dados comerciais após compra
   */
  async atualizarDadosComerciais(clienteId: string, valorCompra: number): Promise<Cliente> {
    const response = await api.post(`${BASE_URL}/${clienteId}/comercial`, { valorCompra });
    return response.data;
  },

  /**
   * Calcula desconto aplicável para o cliente
   */
  async calcularDesconto(clienteId: string, valorCompra: number): Promise<number> {
    const response = await api.get(`${BASE_URL}/${clienteId}/desconto`, {
      params: { valorCompra }
    });
    return response.data;
  },

  /**
   * Obtém histórico do cliente
   */
  async obterHistorico(clienteId: string, request: HistoricoClienteRequest): Promise<ClienteHistorico> {
    const response = await api.post(`${BASE_URL}/${clienteId}/historico`, request);
    return response.data;
  },

  // === LGPD COMPLIANCE ===

  /**
   * Registra consentimento LGPD
   */
  async registrarConsentimento(
    clienteId: string,
    ip: string,
    finalidade: string,
    marketing: boolean = false,
    compartilhamento: boolean = false
  ): Promise<Cliente> {
    const response = await api.post(`${BASE_URL}/${clienteId}/lgpd/consentimento`, {
      ip,
      finalidade,
      marketing,
      compartilhamento
    });
    return response.data;
  },

  /**
   * Processa direito ao esquecimento
   */
  async processarDireitoEsquecimento(
    clienteId: string,
    request: DireitoEsquecimentoRequest
  ): Promise<boolean> {
    const response = await api.post(`${BASE_URL}/${clienteId}/lgpd/esquecimento`, request);
    return response.data;
  },

  /**
   * Obtém relatório LGPD do cliente
   */
  async obterRelatorioLgpd(clienteId: string): Promise<ClienteLgpd> {
    const response = await api.get(`${BASE_URL}/${clienteId}/lgpd/relatorio`);
    return response.data;
  },

  // === ESTATÍSTICAS E RELATÓRIOS ===

  /**
   * Obtém estatísticas gerais dos clientes
   */
  async obterEstatisticas(): Promise<ClienteEstatisticas> {
    try {
      console.log('📈 Buscando estatísticas da API real');
      const response = await api.get(`${BASE_URL}/estatisticas`);
      return response.data;
    } catch (error) {
      console.error('❌ Erro ao buscar estatísticas:', error);
      throw error;
    }
  },

  /**
   * Exporta clientes para diferentes formatos
   */
  async exportarClientes(
    request: BuscarClienteRequest,
    formato: 'CSV' | 'Excel' | 'PDF' = 'CSV'
  ): Promise<Blob> {
    const response = await api.post(`${BASE_URL}/exportar`, request, {
      params: { formato },
      responseType: 'blob'
    });
    return response.data;
  }
};

// === UTILITY FUNCTIONS ===

/**
 * Formata CPF para exibição
 */
export const formatarCpf = (cpf: string): string => {
  const cleanCpf = cpf.replace(/\D/g, '');
  if (cleanCpf.length === 11) {
    return cleanCpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }
  return cpf;
};

/**
 * Limpa CPF removendo formatação
 */
export const limparCpf = (cpf: string): string => {
  return cpf.replace(/\D/g, '');
};

/**
 * Formata telefone brasileiro
 */
export const formatarTelefone = (telefone: string): string => {
  const cleanPhone = telefone.replace(/\D/g, '');
  
  if (cleanPhone.length === 11) {
    return cleanPhone.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  } else if (cleanPhone.length === 10) {
    return cleanPhone.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
  }
  
  return telefone;
};

/**
 * Formata CEP para exibição
 */
export const formatarCep = (cep: string): string => {
  const cleanCep = cep.replace(/\D/g, '');
  if (cleanCep.length === 8) {
    return cleanCep.replace(/(\d{5})(\d{3})/, '$1-$2');
  }
  return cep;
};

/**
 * Limpa CEP removendo formatação
 */
export const limparCep = (cep: string): string => {
  return cep.replace(/\D/g, '');
};

/**
 * Obtém IP do cliente (para LGPD)
 */
export const obterIpCliente = async (): Promise<string> => {
  try {
    const response = await fetch('https://api.ipify.org?format=json');
    const data = await response.json();
    return data.ip;
  } catch {
    return 'unknown';
  }
};