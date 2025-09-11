/**
 * Componente de busca de clientes
 * Interface similar ao ProductSearchInput para consistência
 */
import React, { useRef, useEffect } from 'react';
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
  Avatar,
} from '@mantine/core';
import {
  IconSearch,
  IconUser,
  IconPhone,
  IconMail,
  IconMapPin,
  IconShield,
  IconUserCheck,
  IconUserX,
} from '@tabler/icons-react';
import { useClienteSearch } from '../../hooks/useClienteSearch';
import { ClienteResumo } from '../../types/cliente';
import { formatarCpf, formatarTelefone } from '../../services/clienteService';

interface ClienteSearchInputProps {
  clientes: ClienteResumo[];
  onClienteSelect: (cliente: ClienteResumo) => void;
  placeholder?: string;
  autoFocus?: boolean;
}

/**
 * Componente principal de busca de clientes
 */
export const ClienteSearchInput: React.FC<ClienteSearchInputProps> = ({
  clientes,
  onClienteSelect,
  placeholder = "Digite CPF, nome, email ou telefone do cliente...",
  autoFocus = true,
}) => {
  const inputRef = useRef<HTMLInputElement>(null);
  
  const {
    searchTerm,
    setSearchTerm,
    searchResults,
    isSearching,
    selectedIndex,
    handleKeyDown,
    handleClienteSelect,
    showDropdown,
    setShowDropdown,
  } = useClienteSearch({
    clientes,
    onClienteSelect,
    maxResults: 8,
    debounceMs: 200,
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
   * Detectar possível CPF (11 dígitos)
   */
  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setSearchTerm(value);
    
    // Se parece com CPF completo (11 dígitos)
    if (value.replace(/\D/g, '').length === 11) {
      const cpfLimpo = value.replace(/\D/g, '');
      const exactMatch = clientes.find(c => c.cpf === cpfLimpo);
      if (exactMatch && exactMatch.ativo) {
        setTimeout(() => {
          handleClienteSelect(exactMatch);
        }, 100);
      }
    }
  };

  /**
   * Obter cor do badge da categoria
   */
  const getCategoryColor = (categoria: string) => {
    const colors: Record<string, string> = {
      'VIP': 'yellow',
      'Premium': 'purple', 
      'Regular': 'blue',
      'Bronze': 'gray',
    };
    return colors[categoria] || 'blue';
  };

  /**
   * Gerar avatar com iniciais do nome
   */
  const getClienteInitials = (nome: string) => {
    return nome.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
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
              title="Buscar Clientes"
            >
              <IconUser size={16} />
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
            maxHeight: '250px',
            overflowY: 'auto',
          }}
        >
          <Stack gap={0}>
            {searchResults.map((cliente, index) => {
              const isSelected = index === selectedIndex;
              const isInactive = !cliente.ativo;
              
              return (
                <div
                  key={cliente.id}
                  onClick={() => handleClienteSelect(cliente)}
                  style={{
                    padding: '8px 12px',
                    backgroundColor: isSelected ? '#f8f9fa' : 'white',
                    borderLeft: isSelected ? '3px solid #228be6' : '3px solid transparent',
                    cursor: 'pointer',
                    opacity: isInactive ? 0.7 : 1,
                  }}
                  onMouseEnter={() => setShowDropdown(true)}
                >
                  <Group justify="space-between" align="center" wrap="nowrap">
                    <Group gap="xs" style={{ flex: 1, minWidth: 0 }}>
                      <Avatar color="blue" size="xs">
                        {getClienteInitials(cliente.nome)}
                      </Avatar>
                      
                      <Highlight
                        highlight={searchTerm}
                        fw={600}
                        size="sm"
                        c={isInactive ? '#6c757d' : '#1a1b1e'}
                        style={{ flex: 1, minWidth: 0 }}
                        truncate
                      >
                        {cliente.nome}
                      </Highlight>
                      
                      {cliente.cpf && (
                        <Text size="xs" c="#6c757d" fw={500}>
                          {formatarCpf(cliente.cpf)}
                        </Text>
                      )}
                    </Group>
                    
                    <Group gap="xs" wrap="nowrap">
                      <Badge 
                        color={getCategoryColor(cliente.categoriaCliente)}
                        variant="filled"
                        size="xs"
                      >
                        {cliente.categoriaCliente[0]} {/* Apenas primeira letra */}
                      </Badge>
                      
                      <Text size="xs" fw={700} c="#1864ab">
                        R$ {cliente.valorTotalCompras.toFixed(0)}
                      </Text>
                      
                      {isInactive ? (
                        <IconUserX size={14} color="red" />
                      ) : (
                        <IconUserCheck size={14} color="green" />
                      )}
                    </Group>
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
            <div>
              <Text size="md" c="#495057" fw={500}>
                Nenhum cliente encontrado para "{searchTerm}"
              </Text>
              <Text size="sm" c="#6c757d">
                Tente buscar por nome, CPF, email ou telefone
              </Text>
            </div>
          </Group>
        </Paper>
      )}
    </div>
  );
};