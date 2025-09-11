/**
 * Hook para gerenciar estado e operações de Clientes Brasileiros
 * Implementa todas as operações CRUD com React Query
 */
import { useState, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clienteService } from '../services/clienteService';
import {
  Cliente,
  ClienteResumo,
  CriarClienteRequest,
  AtualizarClienteRequest,
  BuscarClienteRequest,
  PagedResult,
  ClienteEstatisticas,
  ClienteHistorico,
  HistoricoClienteRequest
} from '../types/cliente';

// Query Keys para React Query
export const QUERY_KEYS = {
  clientes: ['clientes'] as const,
  cliente: (id: string) => ['clientes', id] as const,
  clientesPorTermo: (termo: string) => ['clientes', 'buscar', termo] as const,
  clientePorCpf: (cpf: string) => ['clientes', 'cpf', cpf] as const,
  clientePorEmail: (email: string) => ['clientes', 'email', email] as const,
  clientesPorRegiao: (uf: string, cidade?: string) => ['clientes', 'regiao', uf, cidade] as const,
  clientesEstatisticas: ['clientes', 'estatisticas'] as const,
  clienteHistorico: (id: string) => ['clientes', id, 'historico'] as const,
  clientesPorCategoria: (categoria: string) => ['clientes', 'categoria', categoria] as const,
  aniversariantes: (mes: number, ano?: number) => ['clientes', 'aniversariantes', mes, ano] as const,
} as const;

/**
 * Hook principal para operações de clientes
 */
export const useClientes = () => {
  const queryClient = useQueryClient();
  const [filtros, setFiltros] = useState<BuscarClienteRequest>({
    pagina: 1,
    tamanhoPagina: 20,
    direcaoOrdenacao: 'DESC',
    ordenarPor: 'DataCadastro',
    apenasAtivos: true
  });

  // === QUERIES ===

  /**
   * Lista clientes com paginação
   */
  const {
    data: clientes,
    isLoading: carregandoClientes,
    error: erroClientes,
    refetch: recarregarClientes
  } = useQuery({
    queryKey: [...QUERY_KEYS.clientes, filtros],
    queryFn: () => clienteService.listar(filtros),
    keepPreviousData: true,
    staleTime: 30000, // 30 segundos
  });

  /**
   * Estatísticas gerais
   */
  const {
    data: estatisticas,
    isLoading: carregandoEstatisticas
  } = useQuery({
    queryKey: QUERY_KEYS.clientesEstatisticas,
    queryFn: () => clienteService.obterEstatisticas(),
    staleTime: 300000, // 5 minutos
  });

  // === MUTATIONS ===

  /**
   * Criar cliente
   */
  const criarClienteMutation = useMutation({
    mutationFn: ({ request, ip }: { request: CriarClienteRequest; ip?: string }) =>
      clienteService.criar(request, ip),
    onSuccess: (novoCliente) => {
      // Invalida a lista de clientes
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.clientes });
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.clientesEstatisticas });
      
      // Adiciona o cliente ao cache
      queryClient.setQueryData(QUERY_KEYS.cliente(novoCliente.id), novoCliente);
    },
  });

  /**
   * Atualizar cliente
   */
  const atualizarClienteMutation = useMutation({
    mutationFn: ({ id, request }: { id: string; request: AtualizarClienteRequest }) =>
      clienteService.atualizar(id, request),
    onSuccess: (clienteAtualizado) => {
      // Atualiza o cache do cliente específico
      queryClient.setQueryData(QUERY_KEYS.cliente(clienteAtualizado.id), clienteAtualizado);
      
      // Invalida a lista de clientes
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.clientes });
    },
  });

  /**
   * Remover cliente
   */
  const removerClienteMutation = useMutation({
    mutationFn: ({ id, motivo }: { id: string; motivo?: string }) =>
      clienteService.remover(id, motivo),
    onSuccess: (_, variables) => {
      // Remove do cache
      queryClient.removeQueries({ queryKey: QUERY_KEYS.cliente(variables.id) });
      
      // Invalida a lista
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.clientes });
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.clientesEstatisticas });
    },
  });

  // === ACTIONS ===

  /**
   * Cria um novo cliente
   */
  const criarCliente = useCallback(async (request: CriarClienteRequest, ip?: string) => {
    return criarClienteMutation.mutateAsync({ request, ip });
  }, [criarClienteMutation]);

  /**
   * Atualiza um cliente
   */
  const atualizarCliente = useCallback(async (id: string, request: AtualizarClienteRequest) => {
    return atualizarClienteMutation.mutateAsync({ id, request });
  }, [atualizarClienteMutation]);

  /**
   * Remove um cliente
   */
  const removerCliente = useCallback(async (id: string, motivo?: string) => {
    return removerClienteMutation.mutateAsync({ id, motivo });
  }, [removerClienteMutation]);

  /**
   * Atualiza filtros de busca
   */
  const atualizarFiltros = useCallback((novosFiltros: Partial<BuscarClienteRequest>) => {
    setFiltros(prev => ({ ...prev, ...novosFiltros }));
  }, []);

  /**
   * Vai para uma página específica
   */
  const irParaPagina = useCallback((pagina: number) => {
    setFiltros(prev => ({ ...prev, pagina }));
  }, []);

  /**
   * Muda o tamanho da página
   */
  const mudarTamanhoPagina = useCallback((tamanhoPagina: number) => {
    setFiltros(prev => ({ ...prev, tamanhoPagina, pagina: 1 }));
  }, []);

  /**
   * Reseta filtros para o padrão
   */
  const resetarFiltros = useCallback(() => {
    setFiltros({
      pagina: 1,
      tamanhoPagina: 20,
      direcaoOrdenacao: 'DESC',
      ordenarPor: 'DataCadastro',
      apenasAtivos: true
    });
  }, []);

  return {
    // Dados
    clientes: clientes?.items || [],
    totalClientes: clientes?.totalItems || 0,
    totalPaginas: clientes?.totalPages || 0,
    paginaAtual: clientes?.currentPage || 1,
    tamanhoPagina: clientes?.pageSize || 20,
    temProxima: clientes?.hasNext || false,
    temAnterior: clientes?.hasPrevious || false,
    estatisticas,
    filtros,

    // Estados
    carregandoClientes,
    carregandoEstatisticas,
    criandoCliente: criarClienteMutation.isPending,
    atualizandoCliente: atualizarClienteMutation.isPending,
    removendoCliente: removerClienteMutation.isPending,

    // Erros
    erroClientes,
    erroCriarCliente: criarClienteMutation.error,
    erroAtualizarCliente: atualizarClienteMutation.error,
    erroRemoverCliente: removerClienteMutation.error,

    // Ações
    criarCliente,
    atualizarCliente,
    removerCliente,
    atualizarFiltros,
    irParaPagina,
    mudarTamanhoPagina,
    resetarFiltros,
    recarregarClientes,

    // Reset mutations
    resetCriarCliente: criarClienteMutation.reset,
    resetAtualizarCliente: atualizarClienteMutation.reset,
    resetRemoverCliente: removerClienteMutation.reset,
  };
};

/**
 * Hook para obter um cliente específico
 */
export const useCliente = (id: string, enabled: boolean = true) => {
  const queryClient = useQueryClient();

  const {
    data: cliente,
    isLoading: carregandoCliente,
    error: erroCliente,
    refetch: recarregarCliente
  } = useQuery({
    queryKey: QUERY_KEYS.cliente(id),
    queryFn: () => clienteService.obterPorId(id),
    enabled: enabled && !!id,
    staleTime: 300000, // 5 minutos
  });

  return {
    cliente,
    carregandoCliente,
    erroCliente,
    recarregarCliente,
  };
};

/**
 * Hook para buscar clientes por termo
 */
export const useBuscarClientes = (termo: string, limite: number = 20) => {
  const {
    data: resultados,
    isLoading: buscando,
    error: erroBusca
  } = useQuery({
    queryKey: QUERY_KEYS.clientesPorTermo(termo),
    queryFn: () => clienteService.buscarPorTermo(termo, limite),
    enabled: termo.length >= 2,
    staleTime: 60000, // 1 minuto
  });

  return {
    resultados: resultados || [],
    buscando,
    erroBusca,
  };
};

/**
 * Hook para verificar se CPF já existe
 */
export const useVerificarCpf = () => {
  const verificarCpfMutation = useMutation({
    mutationFn: ({ cpf, excluirId }: { cpf: string; excluirId?: string }) =>
      clienteService.cpfJaExiste(cpf, excluirId),
  });

  return {
    verificarCpf: verificarCpfMutation.mutateAsync,
    verificandoCpf: verificarCpfMutation.isPending,
    erroVerificarCpf: verificarCpfMutation.error,
  };
};

/**
 * Hook para verificar se email já existe
 */
export const useVerificarEmail = () => {
  const verificarEmailMutation = useMutation({
    mutationFn: ({ email, excluirId }: { email: string; excluirId?: string }) =>
      clienteService.emailJaExiste(email, excluirId),
  });

  return {
    verificarEmail: verificarEmailMutation.mutateAsync,
    verificandoEmail: verificarEmailMutation.isPending,
    erroVerificarEmail: verificarEmailMutation.error,
  };
};

/**
 * Hook para histórico do cliente
 */
export const useClienteHistorico = (clienteId: string, request: HistoricoClienteRequest) => {
  const {
    data: historico,
    isLoading: carregandoHistorico,
    error: erroHistorico
  } = useQuery({
    queryKey: [...QUERY_KEYS.clienteHistorico(clienteId), request],
    queryFn: () => clienteService.obterHistorico(clienteId, request),
    enabled: !!clienteId,
    staleTime: 300000, // 5 minutos
  });

  return {
    historico,
    carregandoHistorico,
    erroHistorico,
  };
};