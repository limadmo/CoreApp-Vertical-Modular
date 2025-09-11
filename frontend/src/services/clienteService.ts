/**
 * Service para comunicação com API de Clientes Brasileiros
 * Implementa todas as operações CRUD + business logic + LGPD
 * Usa dados gerados como fallback durante desenvolvimento
 */
import { api } from './api';
import { clientesGerados, clientesToResumo, calcularEstatisticas } from '../data/clientesGenerator';
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

const BASE_URL = '/api/clientes';

// Estado local dos dados gerados (para desenvolvimento)
class LocalClienteStore {
  private clientes: Cliente[] = [];
  private nextId = 21;

  constructor() {
    this.loadInitialData();
  }

  private loadInitialData() {
    const stored = localStorage.getItem('clientes-gerados');
    if (stored) {
      try {
        const data = JSON.parse(stored);
        this.clientes = data.clientes || [];
        this.nextId = data.nextId || 21;
      } catch (error) {
        console.warn('Erro ao carregar dados do localStorage, usando dados gerados');
        this.clientes = [...clientesGerados];
        this.saveData();
      }
    } else {
      this.clientes = [...clientesGerados];
      this.saveData();
    }
  }

  private saveData() {
    try {
      localStorage.setItem('clientes-gerados', JSON.stringify({
        clientes: this.clientes,
        nextId: this.nextId
      }));
    } catch (error) {
      console.warn('Erro ao salvar no localStorage:', error);
    }
  }

  async listar(request: BuscarClienteRequest): Promise<PagedResult<ClienteResumo>> {
    let clientesFiltrados = [...this.clientes];

    // Aplicar filtros
    if (request.termo) {
      const termo = request.termo.toLowerCase();
      clientesFiltrados = clientesFiltrados.filter(cliente =>
        cliente.nome.toLowerCase().includes(termo) ||
        cliente.cpf?.includes(termo.replace(/\D/g, '')) ||
        cliente.email?.toLowerCase().includes(termo) ||
        cliente.telefoneCelular?.includes(termo.replace(/\D/g, ''))
      );
    }

    if (request.apenasAtivos !== undefined) {
      clientesFiltrados = clientesFiltrados.filter(c => c.ativo === request.apenasAtivos);
    }

    if (request.categoriaCliente) {
      clientesFiltrados = clientesFiltrados.filter(c => c.categoriaCliente === request.categoriaCliente);
    }

    if (request.uf) {
      clientesFiltrados = clientesFiltrados.filter(c => c.endereco?.uf === request.uf);
    }

    if (request.cidade) {
      clientesFiltrados = clientesFiltrados.filter(c => 
        c.endereco?.cidade?.toLowerCase().includes(request.cidade!.toLowerCase())
      );
    }

    // Ordenação
    const ordenarPor = request.ordenarPor || 'nome';
    const direcao = request.direcaoOrdenacao || 'ASC';
    
    clientesFiltrados.sort((a, b) => {
      let valueA: any, valueB: any;
      
      switch (ordenarPor) {
        case 'nome':
          valueA = a.nome;
          valueB = b.nome;
          break;
        case 'dataCadastro':
          valueA = new Date(a.dataCadastro);
          valueB = new Date(b.dataCadastro);
          break;
        case 'dataUltimaCompra':
          valueA = a.dataUltimaCompra ? new Date(a.dataUltimaCompra) : new Date(0);
          valueB = b.dataUltimaCompra ? new Date(b.dataUltimaCompra) : new Date(0);
          break;
        case 'valorTotalCompras':
          valueA = a.valorTotalCompras;
          valueB = b.valorTotalCompras;
          break;
        default:
          valueA = a.nome;
          valueB = b.nome;
      }

      if (valueA < valueB) return direcao === 'ASC' ? -1 : 1;
      if (valueA > valueB) return direcao === 'ASC' ? 1 : -1;
      return 0;
    });

    // Paginação
    const page = request.pagina || 1;
    const limit = request.tamanhoPagina || 10;
    const startIndex = (page - 1) * limit;
    const endIndex = startIndex + limit;
    
    const clientesPaginados = clientesFiltrados.slice(startIndex, endIndex);
    const totalItems = clientesFiltrados.length;
    const totalPages = Math.ceil(totalItems / limit);

    return {
      items: clientesToResumo(clientesPaginados),
      totalItems,
      totalPages,
      currentPage: page,
      pageSize: limit,
      hasNext: page < totalPages,
      hasPrevious: page > 1
    };
  }

  async obterPorId(id: string): Promise<Cliente> {
    const cliente = this.clientes.find(c => c.id === id);
    if (!cliente) {
      throw new Error(`Cliente com ID ${id} não encontrado`);
    }
    return { ...cliente };
  }

  async criar(request: CriarClienteRequest): Promise<Cliente> {
    // Validações básicas
    if (request.cpf) {
      const cpfLimpo = request.cpf.replace(/\D/g, '');
      const cpfExistente = this.clientes.find(c => c.cpf === cpfLimpo);
      if (cpfExistente) {
        throw new Error('CPF já cadastrado para outro cliente');
      }
    }

    if (request.email) {
      const emailExistente = this.clientes.find(c => 
        c.email?.toLowerCase() === request.email!.toLowerCase()
      );
      if (emailExistente) {
        throw new Error('Email já cadastrado para outro cliente');
      }
    }

    const novoCliente: Cliente = {
      id: this.nextId.toString(),
      nome: request.nome,
      cpf: request.cpf?.replace(/\D/g, ''),
      cpfValidado: false,
      rg: request.rg,
      telefoneCelular: request.telefoneCelular?.replace(/\D/g, ''),
      telefoneValidado: false,
      email: request.email,
      emailValidado: false,
      dataNascimento: request.dataNascimento,
      genero: request.genero,
      profissao: request.profissao,
      estadoCivil: request.estadoCivil,
      categoriaCliente: (request.categoriaCliente as any) || 'Regular',
      ativo: true,
      endereco: request.endereco,
      limiteCredito: request.limiteCredito || 1000,
      descontoPadrao: request.descontoPadrao || 0,
      valorTotalCompras: 0,
      quantidadeCompras: 0,
      dataUltimaCompra: undefined,
      pontuacaoFidelidade: 0,
      observacoes: request.observacoes,
      consentimentoColeta: request.consentimentoColeta || false,
      consentimentoMarketing: request.consentimentoMarketing || false,
      consentimentoCompartilhamento: request.consentimentoCompartilhamento || false,
      direitoEsquecimento: false,
      finalidadeColeta: request.finalidadeColeta || 'Gestão de relacionamento',
      dataConsentimento: new Date().toISOString().split('T')[0],
      ipConsentimento: '192.168.1.100',
      dataCadastro: new Date().toISOString().split('T')[0],
      dataUltimaAtualizacao: new Date().toISOString().split('T')[0],
      tenantId: 'demo-padaria-123'
    };

    this.clientes.push(novoCliente);
    this.nextId++;
    this.saveData();

    return { ...novoCliente };
  }

  async atualizar(id: string, request: AtualizarClienteRequest): Promise<Cliente> {
    const index = this.clientes.findIndex(c => c.id === id);
    if (index === -1) {
      throw new Error(`Cliente com ID ${id} não encontrado`);
    }

    const clienteAtual = this.clientes[index];
    const clienteAtualizado: Cliente = {
      ...clienteAtual,
      ...request,
      cpf: request.cpf?.replace(/\D/g, '') || clienteAtual.cpf,
      telefoneCelular: request.telefoneCelular?.replace(/\D/g, '') || clienteAtual.telefoneCelular,
      dataUltimaAtualizacao: new Date().toISOString().split('T')[0]
    };

    this.clientes[index] = clienteAtualizado;
    this.saveData();

    return { ...clienteAtualizado };
  }

  async remover(id: string): Promise<void> {
    const index = this.clientes.findIndex(c => c.id === id);
    if (index === -1) {
      throw new Error(`Cliente com ID ${id} não encontrado`);
    }

    this.clientes[index] = {
      ...this.clientes[index],
      ativo: false,
      dataUltimaAtualizacao: new Date().toISOString().split('T')[0]
    };

    this.saveData();
  }

  async obterEstatisticas(): Promise<ClienteEstatisticas> {
    const stats = calcularEstatisticas(this.clientes);
    
    return {
      totalClientes: stats.totalClientes,
      clientesAtivos: stats.clientesAtivos,
      clientesInativos: stats.clientesInativos,
      clientesComConsentimento: this.clientes.filter(c => c.consentimentoColeta).length,
      clientesSemConsentimento: this.clientes.filter(c => !c.consentimentoColeta).length,
      clientesPorCategoria: stats.categorias,
      clientesPorUF: this.clientes.reduce((acc, c) => {
        if (c.endereco?.uf) {
          acc[c.endereco.uf] = (acc[c.endereco.uf] || 0) + 1;
        }
        return acc;
      }, {} as Record<string, number>),
      clientesPorMes: {},
      valorTotalCompras: stats.valorTotalVendas,
      ticketMedioGeral: stats.ticketMedio,
      totalPontosFidelidade: this.clientes.reduce((acc, c) => acc + c.pontuacaoFidelidade, 0)
    };
  }

  resetDados() {
    this.clientes = [...clientesGerados];
    this.nextId = 21;
    this.saveData();
  }
}

// Instância do store local
const localStore = new LocalClienteStore();

// Detecta se deve usar dados locais (desenvolvimento)
const useLocalData = import.meta.env.DEV || !import.meta.env.VITE_API_URL;

export const clienteService = {
  // === CRUD BÁSICO ===
  
  /**
   * Lista clientes com paginação e filtros
   */
  async listar(request: BuscarClienteRequest): Promise<PagedResult<ClienteResumo>> {
    if (useLocalData) {
      console.log('📊 [LOCAL] Listando clientes com dados gerados');
      return localStore.listar(request);
    }

    try {
      const response = await api.get(`${BASE_URL}`, { params: request });
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      return localStore.listar(request);
    }
  },

  /**
   * Obtém um cliente por ID
   */
  async obterPorId(id: string): Promise<Cliente> {
    if (useLocalData) {
      console.log(`📋 [LOCAL] Obtendo cliente ${id}`);
      return localStore.obterPorId(id);
    }

    try {
      const response = await api.get(`${BASE_URL}/${id}`);
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      return localStore.obterPorId(id);
    }
  },

  /**
   * Cria um novo cliente
   */
  async criar(request: CriarClienteRequest, ipOrigem?: string): Promise<Cliente> {
    if (useLocalData) {
      console.log('✨ [LOCAL] Criando novo cliente');
      return localStore.criar(request);
    }

    try {
      const headers = ipOrigem ? { 'X-Client-IP': ipOrigem } : {};
      const response = await api.post(`${BASE_URL}`, request, { headers });
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      return localStore.criar(request);
    }
  },

  /**
   * Atualiza um cliente existente
   */
  async atualizar(id: string, request: AtualizarClienteRequest): Promise<Cliente> {
    if (useLocalData) {
      console.log(`🔄 [LOCAL] Atualizando cliente ${id}`);
      return localStore.atualizar(id, request);
    }

    try {
      const response = await api.put(`${BASE_URL}/${id}`, request);
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      return localStore.atualizar(id, request);
    }
  },

  /**
   * Remove um cliente (soft delete)
   */
  async remover(id: string, motivo?: string): Promise<boolean> {
    if (useLocalData) {
      console.log(`🗑️ [LOCAL] Removendo cliente ${id}`);
      await localStore.remover(id);
      return true;
    }

    try {
      const params = motivo ? { motivo } : {};
      const response = await api.delete(`${BASE_URL}/${id}`, { params });
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      await localStore.remover(id);
      return true;
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
    if (useLocalData) {
      const cpfLimpo = cpf.replace(/\D/g, '');
      const cliente = localStore['clientes'].find((c: Cliente) => c.cpf === cpfLimpo);
      return cliente || null;
    }

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
    if (useLocalData) {
      console.log('📈 [LOCAL] Calculando estatísticas dos dados gerados');
      return localStore.obterEstatisticas();
    }

    try {
      const response = await api.get(`${BASE_URL}/estatisticas`);
      return response.data;
    } catch (error) {
      console.warn('⚠️ API indisponível, usando dados locais');
      return localStore.obterEstatisticas();
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
  },

  // === UTILITÁRIOS ===

  /**
   * Reset dados para dados gerados iniciais (desenvolvimento)
   */
  resetarDados(): void {
    if (useLocalData) {
      localStore.resetDados();
    }
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