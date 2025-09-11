/**
 * Hook para busca de clientes com navegação por teclado
 * Similar ao useProductSearch para consistência
 */
import { useState, useEffect, useCallback, useMemo } from 'react';
import { ClienteResumo } from '../types/cliente';

interface UseClienteSearchOptions {
  clientes: ClienteResumo[];
  onClienteSelect: (cliente: ClienteResumo) => void;
  maxResults?: number;
  debounceMs?: number;
}

export const useClienteSearch = ({
  clientes,
  onClienteSelect,
  maxResults = 10,
  debounceMs = 300,
}: UseClienteSearchOptions) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isSearching, setIsSearching] = useState(false);

  /**
   * Função de busca otimizada
   */
  const searchResults = useMemo(() => {
    if (!searchTerm.trim()) return [];

    const term = searchTerm.toLowerCase().trim();
    const cpfTerm = searchTerm.replace(/\D/g, ''); // Remove formatação para busca por CPF
    const phoneTerm = searchTerm.replace(/\D/g, ''); // Remove formatação para busca por telefone

    return clientes
      .filter(cliente => {
        // Busca por nome
        if (cliente.nome.toLowerCase().includes(term)) return true;
        
        // Busca por CPF (com ou sem formatação)
        if (cliente.cpf && (
          cliente.cpf.includes(cpfTerm) ||
          cliente.cpf.replace(/\D/g, '').includes(cpfTerm)
        )) return true;
        
        // Busca por email
        if (cliente.email && cliente.email.toLowerCase().includes(term)) return true;
        
        // Busca por telefone (com ou sem formatação)
        if (cliente.telefoneCelular && (
          cliente.telefoneCelular.includes(phoneTerm) ||
          cliente.telefoneCelular.replace(/\D/g, '').includes(phoneTerm)
        )) return true;
        
        // Busca por categoria
        if (cliente.categoriaCliente.toLowerCase().includes(term)) return true;
        
        return false;
      })
      .sort((a, b) => {
        // Priorizar clientes ativos
        if (a.ativo && !b.ativo) return -1;
        if (!a.ativo && b.ativo) return 1;
        
        // Priorizar matches exatos no nome
        const aNameMatch = a.nome.toLowerCase().startsWith(term);
        const bNameMatch = b.nome.toLowerCase().startsWith(term);
        if (aNameMatch && !bNameMatch) return -1;
        if (!aNameMatch && bNameMatch) return 1;
        
        // Priorizar por valor total de compras (clientes mais valiosos primeiro)
        return b.valorTotalCompras - a.valorTotalCompras;
      })
      .slice(0, maxResults);
  }, [searchTerm, clientes, maxResults]);

  /**
   * Debounce para simulação de loading
   */
  useEffect(() => {
    if (!searchTerm.trim()) {
      setIsSearching(false);
      setShowDropdown(false);
      return;
    }

    setIsSearching(true);
    const timeout = setTimeout(() => {
      setIsSearching(false);
      setShowDropdown(true);
    }, debounceMs);

    return () => clearTimeout(timeout);
  }, [searchTerm, debounceMs]);

  /**
   * Reset do índice selecionado quando resultados mudam
   */
  useEffect(() => {
    setSelectedIndex(0);
  }, [searchResults]);

  /**
   * Navegação por teclado
   */
  const handleKeyDown = useCallback(
    (event: React.KeyboardEvent) => {
      if (!showDropdown || searchResults.length === 0) return;

      switch (event.key) {
        case 'ArrowDown':
          event.preventDefault();
          setSelectedIndex(prev => 
            prev < searchResults.length - 1 ? prev + 1 : 0
          );
          break;

        case 'ArrowUp':
          event.preventDefault();
          setSelectedIndex(prev => 
            prev > 0 ? prev - 1 : searchResults.length - 1
          );
          break;

        case 'Enter':
          event.preventDefault();
          if (searchResults[selectedIndex]) {
            handleClienteSelect(searchResults[selectedIndex]);
          }
          break;

        case 'Escape':
          event.preventDefault();
          setShowDropdown(false);
          setSearchTerm('');
          setSelectedIndex(0);
          break;

        default:
          break;
      }
    },
    [showDropdown, searchResults, selectedIndex]
  );

  /**
   * Selecionar cliente e limpar busca
   */
  const handleClienteSelect = useCallback(
    (cliente: ClienteResumo) => {
      onClienteSelect(cliente);
      setSearchTerm('');
      setShowDropdown(false);
      setSelectedIndex(0);
    },
    [onClienteSelect]
  );

  /**
   * Cliente atualmente selecionado
   */
  const selectedCliente = useMemo(
    () => searchResults[selectedIndex] || null,
    [searchResults, selectedIndex]
  );

  return {
    searchTerm,
    setSearchTerm,
    searchResults,
    isSearching,
    selectedIndex,
    setSelectedIndex,
    selectedCliente,
    handleKeyDown,
    handleClienteSelect,
    showDropdown,
    setShowDropdown,
  };
};