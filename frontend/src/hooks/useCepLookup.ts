/**
 * Hook para integração com ViaCEP
 * Busca automática de endereço por CEP com validação brasileira
 */
import { useState, useCallback } from 'react';
import { useMutation } from '@tanstack/react-query';
import { clienteService, formatarCep, limparCep } from '../services/clienteService';
import {
  EnderecoViaCep,
  BuscarCepRequest,
  BuscarEnderecoRequest,
  CepValidationResult,
  Endereco
} from '../types/cliente';

interface CepLookupState {
  endereco: Endereco | null;
  loading: boolean;
  error: string | null;
  validacao: CepValidationResult | null;
}

/**
 * Hook principal para busca de CEP
 */
export const useCepLookup = () => {
  const [state, setState] = useState<CepLookupState>({
    endereco: null,
    loading: false,
    error: null,
    validacao: null
  });

  // Mutation para buscar endereço por CEP
  const buscarCepMutation = useMutation({
    mutationFn: (request: BuscarCepRequest) => clienteService.buscarEnderecoPorCep(request),
    onMutate: () => {
      setState(prev => ({ ...prev, loading: true, error: null }));
    },
    onSuccess: (data: EnderecoViaCep) => {
      if (data.erro) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'CEP não encontrado',
          endereco: null
        }));
        return;
      }

      const endereco: Endereco = {
        cep: formatarCep(data.cep),
        uf: data.uf,
        cidade: data.localidade,
        logradouro: data.logradouro,
        bairro: data.bairro,
        complemento: data.complemento,
        codigoIBGE: data.ibge
      };

      setState(prev => ({
        ...prev,
        loading: false,
        error: null,
        endereco
      }));
    },
    onError: (error: any) => {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error.message || 'Erro ao buscar CEP',
        endereco: null
      }));
    }
  });

  // Mutation para validar CEP
  const validarCepMutation = useMutation({
    mutationFn: (cep: string) => clienteService.validarCep(cep),
    onSuccess: (validacao: CepValidationResult) => {
      setState(prev => ({ ...prev, validacao }));
      
      // Se válido, busca o endereço automaticamente
      if (validacao.isValid && validacao.endereco) {
        const endereco: Endereco = {
          cep: validacao.formattedCep,
          uf: validacao.endereco.uf,
          cidade: validacao.endereco.localidade,
          logradouro: validacao.endereco.logradouro,
          bairro: validacao.endereco.bairro,
          complemento: validacao.endereco.complemento,
          codigoIBGE: validacao.endereco.ibge
        };

        setState(prev => ({ ...prev, endereco }));
      }
    }
  });

  // Mutation para busca reversa (endereço → CEP)
  const buscarEnderecoMutation = useMutation({
    mutationFn: (request: BuscarEnderecoRequest) => clienteService.buscarCepPorEndereco(request)
  });

  /**
   * Busca endereço por CEP
   */
  const buscarPorCep = useCallback(async (cep: string): Promise<Endereco | null> => {
    const cepLimpo = limparCep(cep);
    
    if (cepLimpo.length !== 8) {
      setState(prev => ({
        ...prev,
        error: 'CEP deve ter 8 dígitos',
        endereco: null
      }));
      return null;
    }

    try {
      const resultado = await buscarCepMutation.mutateAsync({ cep: cepLimpo });
      return state.endereco;
    } catch (error) {
      return null;
    }
  }, [buscarCepMutation, state.endereco]);

  /**
   * Valida CEP em tempo real
   */
  const validarCep = useCallback(async (cep: string): Promise<CepValidationResult | null> => {
    if (!cep || cep.length < 8) {
      return null;
    }

    try {
      return await validarCepMutation.mutateAsync(cep);
    } catch (error) {
      return null;
    }
  }, [validarCepMutation]);

  /**
   * Busca CEPs por endereço (busca reversa)
   */
  const buscarPorEndereco = useCallback(async (request: BuscarEnderecoRequest): Promise<EnderecoViaCep[]> => {
    try {
      return await buscarEnderecoMutation.mutateAsync(request);
    } catch (error) {
      throw error;
    }
  }, [buscarEnderecoMutation]);

  /**
   * Limpa o estado atual
   */
  const limpar = useCallback(() => {
    setState({
      endereco: null,
      loading: false,
      error: null,
      validacao: null
    });
  }, []);

  /**
   * Define endereço manualmente
   */
  const definirEndereco = useCallback((endereco: Endereco | null) => {
    setState(prev => ({ ...prev, endereco, error: null }));
  }, []);

  return {
    // Estado
    endereco: state.endereco,
    loading: state.loading || buscarCepMutation.isPending || validarCepMutation.isPending,
    error: state.error,
    validacao: state.validacao,
    
    // Estados de busca reversa
    buscandoEndereco: buscarEnderecoMutation.isPending,
    resultadosBuscaEndereco: buscarEnderecoMutation.data || [],
    erroBuscaEndereco: buscarEnderecoMutation.error,

    // Ações
    buscarPorCep,
    validarCep,
    buscarPorEndereco,
    limpar,
    definirEndereco,

    // Reset mutations
    resetBuscarCep: buscarCepMutation.reset,
    resetValidarCep: validarCepMutation.reset,
    resetBuscarEndereco: buscarEnderecoMutation.reset,
  };
};

/**
 * Hook para auto-complete de endereço
 * Dispara busca automática quando CEP é alterado
 */
export const useCepAutoComplete = (onEnderecoChange?: (endereco: Endereco | null) => void) => {
  const { buscarPorCep, endereco, loading, error, validacao } = useCepLookup();
  const [cepValue, setCepValue] = useState('');

  /**
   * Manipula mudança do CEP
   */
  const handleCepChange = useCallback(async (novoCep: string) => {
    const cepFormatado = formatarCep(novoCep);
    setCepValue(cepFormatado);

    const cepLimpo = limparCep(novoCep);
    
    // Busca automaticamente quando CEP tem 8 dígitos
    if (cepLimpo.length === 8) {
      const endereco = await buscarPorCep(cepLimpo);
      if (endereco && onEnderecoChange) {
        onEnderecoChange(endereco);
      }
    } else if (cepLimpo.length === 0 && onEnderecoChange) {
      onEnderecoChange(null);
    }
  }, [buscarPorCep, onEnderecoChange]);

  /**
   * Verifica se CEP é válido
   */
  const isCepValido = useCallback((cep: string): boolean => {
    const cepLimpo = limparCep(cep);
    return cepLimpo.length === 8 && /^\d{8}$/.test(cepLimpo);
  }, []);

  return {
    cepValue,
    endereco,
    loading,
    error,
    validacao,
    isCepValido: isCepValido(cepValue),
    handleCepChange,
    isCepValidoFn: isCepValido,
  };
};

/**
 * Hook para máscara de CEP
 */
export const useCepMask = (value: string = '') => {
  const [maskedValue, setMaskedValue] = useState(formatarCep(value));

  const handleChange = useCallback((newValue: string) => {
    const cleaned = limparCep(newValue);
    const formatted = formatarCep(cleaned);
    setMaskedValue(formatted);
    return formatted;
  }, []);

  const setValue = useCallback((newValue: string) => {
    const formatted = formatarCep(newValue);
    setMaskedValue(formatted);
  }, []);

  const getRawValue = useCallback(() => {
    return limparCep(maskedValue);
  }, [maskedValue]);

  const isValid = useCallback(() => {
    const raw = getRawValue();
    return raw.length === 8 && /^\d{8}$/.test(raw);
  }, [getRawValue]);

  return {
    value: maskedValue,
    rawValue: getRawValue(),
    isValid: isValid(),
    handleChange,
    setValue,
    clear: () => setMaskedValue(''),
  };
};