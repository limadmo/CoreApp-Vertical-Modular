/**
 * Hook useClientes - React Query + TypeScript
 * Gerenciamento completo de estado para clientes com cache inteligente
 */
'use client';

import {
  useQuery,
  useMutation,
  useQueryClient,
  UseQueryResult,
  UseMutationResult
} from '@tanstack/react-query';
import { notifications } from '@mantine/notifications';
import {
  clienteService,
  Cliente,
  ClienteCreateRequest,
  ClienteQueryParams,
  ClienteListResponse,
  ClienteEstatisticas
} from '@/services/clienteService';

// Chaves para React Query
export const CLIENTE_QUERY_KEYS = {
  all: ['clientes'] as const,
  lists: () => [...CLIENTE_QUERY_KEYS.all, 'list'] as const,
  list: (params: ClienteQueryParams) => [...CLIENTE_QUERY_KEYS.lists(), params] as const,
  details: () => [...CLIENTE_QUERY_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...CLIENTE_QUERY_KEYS.details(), id] as const,
  estatisticas: () => [...CLIENTE_QUERY_KEYS.all, 'estatisticas'] as const,
  cpf: (cpf: string) => [...CLIENTE_QUERY_KEYS.all, 'cpf', cpf] as const,
};

/**
 * Hook para listar clientes com paginação
 */
export function useClientes(params: ClienteQueryParams = {}) {
  return useQuery({
    queryKey: CLIENTE_QUERY_KEYS.list(params),
    queryFn: () => clienteService.listar(params),
    staleTime: 5 * 60 * 1000, // 5 minutos
    gcTime: 10 * 60 * 1000,   // 10 minutos (era cacheTime)
    retry: 2,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
}

/**
 * Hook para obter cliente por ID
 */
export function useCliente(id: string | undefined) {
  return useQuery({
    queryKey: CLIENTE_QUERY_KEYS.detail(id!),
    queryFn: () => clienteService.obterPorId(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}

/**
 * Hook para estatísticas de clientes
 */
export function useClienteEstatisticas() {
  return useQuery({
    queryKey: CLIENTE_QUERY_KEYS.estatisticas(),
    queryFn: () => clienteService.obterEstatisticas(),
    staleTime: 2 * 60 * 1000, // 2 minutos (dados mais voláteis)
    gcTime: 5 * 60 * 1000,
  });
}

/**
 * Hook para buscar cliente por CPF
 */
export function useClientePorCpf(cpf: string | undefined) {
  return useQuery({
    queryKey: CLIENTE_QUERY_KEYS.cpf(cpf!),
    queryFn: () => clienteService.buscarPorCpf(cpf!),
    enabled: !!cpf && cpf.length >= 11,
    staleTime: 10 * 60 * 1000, // 10 minutos (CPF é mais estável)
    gcTime: 30 * 60 * 1000,
  });
}

/**
 * Hook para criar cliente
 */
export function useCreateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dados: ClienteCreateRequest) => clienteService.criar(dados),
    onSuccess: (data) => {
      // Invalidar cache de listas
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      // Adicionar ao cache individual
      queryClient.setQueryData(CLIENTE_QUERY_KEYS.detail(data.data.id), data);

      notifications.show({
        title: 'Sucesso! 🎉',
        message: 'Cliente criado com sucesso',
        color: 'green',
      });
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Erro ao criar cliente';
      notifications.show({
        title: 'Erro ❌',
        message,
        color: 'red',
      });
    },
  });
}

/**
 * Hook para atualizar cliente
 */
export function useUpdateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: ClienteCreateRequest }) =>
      clienteService.atualizar(id, dados),
    onSuccess: (data, variables) => {
      // Invalidar cache de listas
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      // Atualizar cache individual
      queryClient.setQueryData(CLIENTE_QUERY_KEYS.detail(variables.id), data);

      notifications.show({
        title: 'Sucesso! ✅',
        message: 'Cliente atualizado com sucesso',
        color: 'green',
      });
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Erro ao atualizar cliente';
      notifications.show({
        title: 'Erro ❌',
        message,
        color: 'red',
      });
    },
  });
}

/**
 * Hook para remover cliente (soft delete)
 */
export function useDeleteCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, motivo }: { id: string; motivo?: string }) =>
      clienteService.remover(id, motivo),
    onSuccess: () => {
      // Invalidar cache de listas e estatísticas
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      notifications.show({
        title: 'Cliente Removido 🗑️',
        message: 'Cliente removido com sucesso',
        color: 'orange',
      });
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Erro ao remover cliente';
      notifications.show({
        title: 'Erro ❌',
        message,
        color: 'red',
      });
    },
  });
}

/**
 * Hook para restaurar cliente
 */
export function useRestoreCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => clienteService.restaurar(id),
    onSuccess: () => {
      // Invalidar cache
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      notifications.show({
        title: 'Cliente Restaurado ↩️',
        message: 'Cliente restaurado com sucesso',
        color: 'blue',
      });
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Erro ao restaurar cliente';
      notifications.show({
        title: 'Erro ❌',
        message,
        color: 'red',
      });
    },
  });
}

/**
 * Hook otimizado para busca com debounce
 */
export function useClientesSearch(search: string, delay = 500) {
  const [debouncedSearch, setDebouncedSearch] = useState(search);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
    }, delay);

    return () => clearTimeout(timer);
  }, [search, delay]);

  return useClientes({
    search: debouncedSearch,
    limit: 20
  });
}

// Importar useState e useEffect para o hook de busca
import { useState, useEffect } from 'react';

/**
 * Hook para operações em lote
 */
export function useBatchClienteOperations() {
  const queryClient = useQueryClient();
  const deleteCliente = useDeleteCliente();
  const restoreCliente = useRestoreCliente();

  const deleteBatch = async (ids: string[], motivo?: string) => {
    const promises = ids.map(id => clienteService.remover(id, motivo));

    try {
      await Promise.all(promises);
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      notifications.show({
        title: 'Operação Concluída 📦',
        message: `${ids.length} clientes removidos`,
        color: 'orange',
      });
    } catch (error) {
      notifications.show({
        title: 'Erro na Operação ❌',
        message: 'Alguns clientes não puderam ser removidos',
        color: 'red',
      });
    }
  };

  const restoreBatch = async (ids: string[]) => {
    const promises = ids.map(id => clienteService.restaurar(id));

    try {
      await Promise.all(promises);
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: CLIENTE_QUERY_KEYS.estatisticas() });

      notifications.show({
        title: 'Operação Concluída ↩️',
        message: `${ids.length} clientes restaurados`,
        color: 'blue',
      });
    } catch (error) {
      notifications.show({
        title: 'Erro na Operação ❌',
        message: 'Alguns clientes não puderam ser restaurados',
        color: 'red',
      });
    }
  };

  return {
    deleteBatch,
    restoreBatch,
    isDeleting: deleteCliente.isPending,
    isRestoring: restoreCliente.isPending,
  };
}