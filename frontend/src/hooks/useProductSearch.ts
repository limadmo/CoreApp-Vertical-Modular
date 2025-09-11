/**
 * Hook para busca de produtos com debounce e navegação por teclado
 * Implementa funcionalidades avançadas de PDV profissional
 * Performance otimizada para digitação fluida + busca inteligente brasileira
 */
import { useState, useEffect, useCallback, useRef, useMemo } from 'react';

interface Product {
  id: string;
  nome: string;
  preco: number;
  categoria: 'PAES' | 'DOCES' | 'SALGADOS' | 'BOLOS' | 'BEBIDAS';
  estoque: number;
  codigoBarras: string;
  alergenos: string[];
  validadeHoras: number;
  pesoMedio: number;
  temperaturaArmazenamento: 'AMBIENTE' | 'REFRIGERADO' | 'CONGELADO';
}

interface UseProductSearchOptions {
  products: Product[];
  debounceMs?: number;
  maxResults?: number;
  onProductSelect?: (product: Product) => void;
}

interface UseProductSearchReturn {
  // Estado da busca
  searchTerm: string;
  setSearchTerm: (term: string) => void;
  searchResults: Product[];
  isSearching: boolean;
  
  // Navegação por teclado
  selectedIndex: number;
  setSelectedIndex: (index: number) => void;
  selectedProduct: Product | null;
  
  // Controles
  handleKeyDown: (event: React.KeyboardEvent) => void;
  handleProductSelect: (product: Product) => void;
  clearSearch: () => void;
  
  // Estados visuais
  showDropdown: boolean;
  setShowDropdown: (show: boolean) => void;
}

/**
 * Normaliza texto removendo acentos e convertendo para lowercase
 * Permite busca inteligente brasileira (café = cafe)
 */
const normalizeText = (text: string): string => {
  return text
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
};

/**
 * Busca aproximada por palavras para PDV inteligente
 * "fran" encontra "PÃO FRANCÊS", "choco" encontra "CROISSANT DE CHOCOLATE"
 */
const searchApproximate = (term: string, productName: string): boolean => {
  const normalizedTerm = normalizeText(term);
  const normalizedName = normalizeText(productName);
  
  // Busca exata primeiro
  if (normalizedName.includes(normalizedTerm)) return true;
  
  // Busca por palavras (aproximação inteligente)
  const words = normalizedName.split(' ');
  return words.some(word => 
    word.startsWith(normalizedTerm) || 
    normalizedTerm.startsWith(word.slice(0, Math.max(3, normalizedTerm.length)))
  );
};

/**
 * Hook de busca de produtos com funcionalidades avançadas
 * Performance otimizada para PDV (debounce 50ms)
 */
export const useProductSearch = ({
  products,
  debounceMs = 50,
  maxResults = 8,
  onProductSelect,
}: UseProductSearchOptions): UseProductSearchReturn => {
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  
  const debounceRef = useRef<NodeJS.Timeout | null>(null);
  const searchInputRef = useRef<HTMLInputElement | null>(null);

  /**
   * Busca produtos por termo com inteligência brasileira
   * Suporta busca por código de barras, nome exato e aproximado
   */
  const searchProducts = useCallback((term: string): Product[] => {
    if (!term || term.length < 1) {
      return [];
    }

    const originalTerm = term.trim();
    
    // 1. Busca por código de barras (exata, sem normalização)
    const barCodeMatch = products.find(
      product => product.codigoBarras === originalTerm
    );
    
    if (barCodeMatch && barCodeMatch.estoque > 0) {
      return [barCodeMatch];
    }

    // 2. Busca por nome (inteligente + aproximada)
    const nameMatches = products.filter(product => {
      if (product.estoque <= 0) return false;
      
      // Busca inteligente (sem acentos + aproximação)
      return searchApproximate(originalTerm, product.nome);
    });

    // 3. Ordenar por relevância (busca exata primeiro, depois aproximada)
    const normalizedTerm = normalizeText(originalTerm);
    const sortedResults = nameMatches.sort((a, b) => {
      const aNormalized = normalizeText(a.nome);
      const bNormalized = normalizeText(b.nome);
      
      // Prioridade 1: Começa com o termo
      const aStartsWith = aNormalized.startsWith(normalizedTerm);
      const bStartsWith = bNormalized.startsWith(normalizedTerm);
      
      if (aStartsWith && !bStartsWith) return -1;
      if (!aStartsWith && bStartsWith) return 1;
      
      // Prioridade 2: Contém o termo exato
      const aContainsExact = aNormalized.includes(normalizedTerm);
      const bContainsExact = bNormalized.includes(normalizedTerm);
      
      if (aContainsExact && !bContainsExact) return -1;
      if (!aContainsExact && bContainsExact) return 1;
      
      // Prioridade 3: Ordem alfabética
      return a.nome.localeCompare(b.nome);
    });

    return sortedResults.slice(0, maxResults);
  }, [products, maxResults]);

  /**
   * Debounce separado para evitar lag na digitação
   */
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    debounceRef.current = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
      setIsSearching(false);
    }, debounceMs);

    // Mostrar loading apenas se há termo de busca
    if (searchTerm) {
      setIsSearching(true);
    } else {
      setIsSearching(false);
      setDebouncedSearchTerm('');
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [searchTerm, debounceMs]);

  /**
   * Resultados da busca otimizados (apenas com termo debounced)
   */
  const searchResults = useMemo(() => {
    if (!debouncedSearchTerm) return [];
    return searchProducts(debouncedSearchTerm);
  }, [debouncedSearchTerm, searchProducts]);

  /**
   * Produto selecionado atual
   */
  const selectedProduct = useMemo(() => {
    if (selectedIndex >= 0 && selectedIndex < searchResults.length) {
      return searchResults[selectedIndex];
    }
    return null;
  }, [selectedIndex, searchResults]);

  /**
   * Atualizar termo de busca sem lag (input direto)
   */
  const updateSearchTerm = useCallback((term: string) => {
    setSearchTerm(term); // Input imediato sem delay
    setSelectedIndex(-1); // Reset seleção
    setShowDropdown(term.length > 0);
  }, []);

  /**
   * Handler para teclas de navegação
   */
  const handleKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (!searchResults.length) return;

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
        if (selectedProduct) {
          handleProductSelect(selectedProduct);
        } else if (searchResults.length === 1) {
          // Se há apenas um resultado, seleciona automaticamente
          handleProductSelect(searchResults[0]);
        }
        break;
        
      case 'Escape':
        event.preventDefault();
        clearSearch();
        break;
        
      case 'Tab':
        // Permitir navegação normal por tab
        setShowDropdown(false);
        break;
    }
  }, [searchResults, selectedProduct]);

  /**
   * Selecionar um produto
   */
  const handleProductSelect = useCallback((product: Product) => {
    onProductSelect?.(product);
    clearSearch();
    
    // Voltar foco para o campo de busca
    setTimeout(() => {
      searchInputRef.current?.focus();
    }, 100);
  }, [onProductSelect]);

  /**
   * Limpar busca
   */
  const clearSearch = useCallback(() => {
    setSearchTerm('');
    setSelectedIndex(-1);
    setShowDropdown(false);
    setIsSearching(false);
    
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
  }, []);

  /**
   * Detectar cliques fora do dropdown
   */
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Element;
      if (!target.closest('[data-search-dropdown]') && 
          !target.closest('[data-search-input]')) {
        setShowDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  /**
   * Limpar debounce ao desmontar
   */
  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  return {
    searchTerm,
    setSearchTerm: updateSearchTerm,
    searchResults,
    isSearching,
    selectedIndex,
    setSelectedIndex,
    selectedProduct,
    handleKeyDown,
    handleProductSelect,
    clearSearch,
    showDropdown,
    setShowDropdown,
  };
};