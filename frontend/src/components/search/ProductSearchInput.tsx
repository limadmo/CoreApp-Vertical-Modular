/**
 * Componente de busca de produtos para PDV
 * Interface profissional com dropdown navegável e suporte a scanner
 */
import React, { useRef, useEffect } from 'react';
import '../../styles/verticals/padaria.css';
import {
  TextInput,
  Paper,
  Stack,
  Group,
  Text,
  Badge,
  Loader,
  ActionIcon,
  Highlight,
} from '@mantine/core';
import {
  IconSearch,
  IconScan,
  IconAlertCircle,
  IconShoppingCart,
} from '@tabler/icons-react';
import { useProductSearch } from '@hooks/useProductSearch';

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

interface ProductSearchInputProps {
  products: Product[];
  onProductSelect: (product: Product) => void;
  placeholder?: string;
  autoFocus?: boolean;
}

/**
 * Componente principal de busca de produtos
 */
export const ProductSearchInput: React.FC<ProductSearchInputProps> = ({
  products,
  onProductSelect,
  placeholder = "Digite o código de barras ou nome do produto...",
  autoFocus = true,
}) => {
  const inputRef = useRef<HTMLInputElement>(null);
  
  const {
    searchTerm,
    setSearchTerm,
    searchResults,
    isSearching,
    selectedIndex,
    selectedProduct,
    handleKeyDown,
    handleProductSelect,
    showDropdown,
    setShowDropdown,
  } = useProductSearch({
    products,
    onProductSelect,
    maxResults: 8,
    debounceMs: 50,
  });

  /**
   * Auto focus no campo ao carregar
   */
  useEffect(() => {
    if (autoFocus && inputRef.current) {
      inputRef.current.focus();
    }
  }, [autoFocus]);

  /**
   * Detectar possível scanner de código de barras
   * Scanner geralmente digita muito rápido
   */
  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setSearchTerm(value);
    
    // Se parece com código de barras (13 dígitos)
    if (value.length === 13 && /^\d+$/.test(value)) {
      // Pequeno delay para detectar scanner
      setTimeout(() => {
        const exactMatch = products.find(p => p.codigoBarras === value);
        if (exactMatch && exactMatch.estoque > 0) {
          handleProductSelect(exactMatch);
        }
      }, 100);
    }
  };

  /**
   * Obter classe CSS da categoria para PADARIA
   */
  const getCategoryClass = (categoria: string) => {
    const classes: Record<string, string> = {
      'PAES': 'badge-categoria-paes',
      'DOCES': 'badge-categoria-doces',
      'SALGADOS': 'badge-categoria-salgados',
      'BOLOS': 'badge-categoria-bolos',
      'BEBIDAS': 'badge-categoria-bebidas',
    };
    return classes[categoria] || 'badge-categoria-paes';
  };

  /**
   * Simular leitura de scanner
   */
  const handleScanClick = () => {
    // Simular código de barras para demo
    const randomProduct = products[Math.floor(Math.random() * products.length)];
    if (randomProduct && inputRef.current) {
      inputRef.current.value = randomProduct.codigoBarras;
      setSearchTerm(randomProduct.codigoBarras);
      setTimeout(() => {
        if (randomProduct.estoque > 0) {
          handleProductSelect(randomProduct);
        }
      }, 300);
    }
  };

  return (
    <div style={{ position: 'relative' }} data-search-container>
      <TextInput
        ref={inputRef}
        data-search-input
        size="lg"
        placeholder={placeholder}
        value={searchTerm}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onFocus={() => searchTerm && setShowDropdown(true)}
        leftSection={<IconSearch size={20} />}
        rightSection={
          <Group gap="xs">
            {isSearching && <Loader size={16} />}
            <ActionIcon
              variant="subtle"
              onClick={handleScanClick}
              title="Simular Scanner (Demo)"
            >
              <IconScan size={16} />
            </ActionIcon>
          </Group>
        }
        styles={{
          input: {
            fontSize: '16px',
            fontWeight: 500,
            backgroundColor: 'white',
          },
        }}
      />

      {/* Dropdown de Resultados */}
      {showDropdown && searchResults.length > 0 && (
        <Paper
          data-search-dropdown
          shadow="lg"
          radius="md"
          withBorder
          style={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            zIndex: 1000,
            marginTop: '4px',
            maxHeight: '400px',
            overflowY: 'auto',
          }}
        >
          <Stack gap={0}>
            {searchResults.map((product, index) => {
              const isSelected = index === selectedIndex;
              const isOutOfStock = product.estoque === 0;
              
              return (
                <div
                  key={product.id}
                  onClick={() => !isOutOfStock && handleProductSelect(product)}
                  style={{
                    padding: '12px 16px',
                    backgroundColor: isSelected ? '#f8f9fa' : 'white',
                    borderLeft: isSelected ? '3px solid #228be6' : '3px solid transparent',
                    cursor: isOutOfStock ? 'not-allowed' : 'pointer',
                    opacity: isOutOfStock ? 0.6 : 1,
                  }}
                  onMouseEnter={() => !isOutOfStock && setShowDropdown(true)}
                >
                  <Group justify="space-between" mb="xs">
                    <Group gap="sm">
                      <div>
                        <Highlight
                          highlight={searchTerm}
                          fw={600}
                          size="md"
                          c={isOutOfStock ? '#6c757d' : '#1a1b1e'}
                        >
                          {product.nome}
                        </Highlight>
                        <Text size="sm" c="#495057" fw={500}>
                          {product.codigoBarras}
                        </Text>
                      </div>
                    </Group>
                    
                    <Group gap="sm">
                      <Badge 
                        className={getCategoryClass(product.categoria)}
                        variant="filled"
                        size="sm"
                      >
                        {product.categoria}
                      </Badge>
                      <Text fw={700} size="md" c="#1864ab">
                        R$ {product.preco.toFixed(2)}
                      </Text>
                    </Group>
                  </Group>

                  <Group justify="space-between" align="center">
                    <Group gap="xs">
                      {isOutOfStock ? (
                        <Group gap="xs">
                          <IconAlertCircle size={12} color="red" />
                          <Text size="sm" c="#d63384" fw={600}>
                            Sem estoque
                          </Text>
                        </Group>
                      ) : (
                        <Text size="sm" c="#495057" fw={500}>
                          Estoque: {product.estoque}
                        </Text>
                      )}

                      {product.alergenos.length > 0 && (
                        <Group gap="xs">
                          <IconAlertCircle size={14} color="#fd7e14" />
                          <Text size="sm" c="#fd7e14" fw={600}>
                            Alérgenos: {product.alergenos.slice(0, 2).join(', ')}
                            {product.alergenos.length > 2 && '...'}
                          </Text>
                        </Group>
                      )}
                    </Group>

                    {isSelected && !isOutOfStock && (
                      <Group gap="xs">
                        <IconShoppingCart size={14} color="#1864ab" />
                        <Text size="sm" fw={700} c="#1864ab">
                          Enter para adicionar
                        </Text>
                      </Group>
                    )}
                  </Group>
                </div>
              );
            })}
          </Stack>
        </Paper>
      )}

      {/* Mensagem quando não encontrar resultados */}
      {showDropdown && searchTerm && searchResults.length === 0 && !isSearching && (
        <Paper
          data-search-dropdown
          shadow="lg"
          radius="md"
          withBorder
          p="md"
          style={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            zIndex: 1000,
            marginTop: '4px',
          }}
        >
          <Group gap="sm">
            <IconSearch size={18} color="#6c757d" />
            <Text size="md" c="#495057" fw={500}>
              Nenhum produto encontrado para "{searchTerm}"
            </Text>
          </Group>
        </Paper>
      )}
    </div>
  );
};