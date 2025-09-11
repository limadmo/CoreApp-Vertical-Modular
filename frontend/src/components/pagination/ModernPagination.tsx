/**
 * Componente de paginação moderna e minimalista
 * Posicionado no topo direito da página para facilitar acesso
 */
import React from 'react';
import {
  Group,
  ActionIcon,
  Text,
  Select,
  Paper,
  UnstyledButton,
  Box,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconChevronRight,
  IconChevronsLeft,
  IconChevronsRight,
} from '@tabler/icons-react';

interface ModernPaginationProps {
  /** Página atual (1-indexed) */
  currentPage: number;
  /** Total de páginas */
  totalPages: number;
  /** Total de itens */
  totalItems: number;
  /** Itens por página */
  itemsPerPage: number;
  /** Callback quando página muda */
  onPageChange: (page: number) => void;
  /** Callback quando itens por página muda */
  onItemsPerPageChange: (itemsPerPage: number) => void;
  /** Opções para itens por página */
  itemsPerPageOptions?: number[];
  /** Se está carregando */
  loading?: boolean;
}

/**
 * Componente de paginação moderna
 */
export const ModernPagination: React.FC<ModernPaginationProps> = ({
  currentPage,
  totalPages,
  totalItems,
  itemsPerPage,
  onPageChange,
  onItemsPerPageChange,
  itemsPerPageOptions = [10, 20, 50, 100],
  loading = false,
}) => {
  // Calcular range de itens atuais
  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, totalItems);

  // Gerar números de páginas para exibir
  const getVisiblePages = () => {
    const delta = 2; // Páginas antes e depois da atual
    const range = [];
    const rangeWithDots = [];

    // Calcular range
    for (
      let i = Math.max(2, currentPage - delta);
      i <= Math.min(totalPages - 1, currentPage + delta);
      i++
    ) {
      range.push(i);
    }

    // Adicionar primeira página
    if (currentPage - delta > 2) {
      rangeWithDots.push(1, '...');
    } else {
      rangeWithDots.push(1);
    }

    // Adicionar páginas do range
    rangeWithDots.push(...range);

    // Adicionar última página
    if (currentPage + delta < totalPages - 1) {
      rangeWithDots.push('...', totalPages);
    } else if (totalPages > 1) {
      rangeWithDots.push(totalPages);
    }

    return rangeWithDots;
  };

  const visiblePages = getVisiblePages();

  if (totalItems === 0) {
    return null;
  }

  return (
    <Paper withBorder p="sm" radius="md" style={{ backgroundColor: '#fafafa' }}>
      <Group justify="space-between" align="center" wrap="nowrap">
        
        {/* Info dos itens */}
        <Group gap="sm">
          <Text size="sm" fw={500} c="dark">
            {startItem}–{endItem} de {totalItems.toLocaleString('pt-BR')}
          </Text>
          
          <Select
            size="xs"
            data={itemsPerPageOptions.map(option => ({
              value: option.toString(),
              label: `${option} por página`
            }))}
            value={itemsPerPage.toString()}
            onChange={(value) => onItemsPerPageChange(parseInt(value || '20'))}
            disabled={loading}
            style={{ width: 130 }}
          />
        </Group>

        {/* Controles de navegação */}
        <Group gap="xs">
          {/* Primeira página */}
          <ActionIcon
            variant="subtle"
            size="sm"
            disabled={currentPage === 1 || loading}
            onClick={() => onPageChange(1)}
            title="Primeira página"
          >
            <IconChevronsLeft size="1rem" />
          </ActionIcon>

          {/* Página anterior */}
          <ActionIcon
            variant="subtle"
            size="sm"
            disabled={currentPage === 1 || loading}
            onClick={() => onPageChange(currentPage - 1)}
            title="Página anterior"
          >
            <IconChevronLeft size="1rem" />
          </ActionIcon>

          {/* Números das páginas */}
          <Group gap={4}>
            {visiblePages.map((page, index) => {
              if (page === '...') {
                return (
                  <Text key={`dots-${index}`} size="sm" c="dimmed" px="xs">
                    ...
                  </Text>
                );
              }

              const pageNumber = page as number;
              const isActive = pageNumber === currentPage;

              return (
                <UnstyledButton
                  key={pageNumber}
                  onClick={() => onPageChange(pageNumber)}
                  disabled={loading}
                  style={{
                    width: 32,
                    height: 32,
                    borderRadius: 6,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: isActive ? '#228be6' : 'transparent',
                    color: isActive ? 'white' : '#495057',
                    fontWeight: isActive ? 600 : 500,
                    fontSize: '0.875rem',
                    transition: 'all 0.2s ease',
                    border: '1px solid',
                    borderColor: isActive ? '#228be6' : '#dee2e6',
                  }}
                  onMouseEnter={(e) => {
                    if (!isActive && !loading) {
                      e.currentTarget.style.backgroundColor = '#f8f9fa';
                      e.currentTarget.style.borderColor = '#228be6';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (!isActive) {
                      e.currentTarget.style.backgroundColor = 'transparent';
                      e.currentTarget.style.borderColor = '#dee2e6';
                    }
                  }}
                >
                  {pageNumber}
                </UnstyledButton>
              );
            })}
          </Group>

          {/* Próxima página */}
          <ActionIcon
            variant="subtle"
            size="sm"
            disabled={currentPage === totalPages || loading}
            onClick={() => onPageChange(currentPage + 1)}
            title="Próxima página"
          >
            <IconChevronRight size="1rem" />
          </ActionIcon>

          {/* Última página */}
          <ActionIcon
            variant="subtle"
            size="sm"
            disabled={currentPage === totalPages || loading}
            onClick={() => onPageChange(totalPages)}
            title="Última página"
          >
            <IconChevronsRight size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Paper>
  );
};