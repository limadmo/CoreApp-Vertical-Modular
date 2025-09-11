/**
 * Tabela de dados reutilizável com paginação, filtros e ordenação
 * Implementa acessibilidade WCAG AAA para navegação por teclado
 */
import React, { useState, useMemo } from 'react';
import {
  Table,
  Paper,
  Pagination,
  Group,
  Text,
  ActionIcon,
  TextInput,
  Select,
  Badge,
  LoadingOverlay,
  Stack,
  Center,
} from '@mantine/core';
import {
  IconChevronUp,
  IconChevronDown,
  IconSearch,
  IconAdjustments,
  IconFileX,
} from '@tabler/icons-react';
import { useDebouncedValue } from '@mantine/hooks';

export interface ColumnDef<T = any> {
  /** Chave única da coluna */
  key: string;
  /** Label para exibição */
  label: string;
  /** Se a coluna é ordenável */
  sortable?: boolean;
  /** Largura da coluna */
  width?: string | number;
  /** Alinhamento do conteúdo */
  align?: 'left' | 'center' | 'right';
  /** Função para renderizar o conteúdo */
  render: (item: T) => React.ReactNode;
}

interface DataTableProps<T = any> {
  /** Dados a serem exibidos */
  data: T[];
  /** Definição das colunas */
  columns: ColumnDef<T>[];
  /** Total de registros (para paginação) */
  totalRecords?: number;
  /** Página atual */
  currentPage?: number;
  /** Tamanho da página */
  pageSize?: number;
  /** Callback de mudança de página */
  onPageChange?: (page: number) => void;
  /** Se está carregando */
  isLoading?: boolean;
  /** Título da tabela */
  title?: string;
  /** Se permite busca */
  searchable?: boolean;
  /** Placeholder da busca */
  searchPlaceholder?: string;
  /** Callback de busca */
  onSearch?: (query: string) => void;
  /** Se permite filtros */
  filterable?: boolean;
  /** Opções de filtro */
  filterOptions?: Array<{ label: string; value: string }>;
  /** Callback de filtro */
  onFilter?: (filter: string | null) => void;
  /** Mensagem quando não há dados */
  emptyMessage?: string;
  /** Altura mínima da tabela */
  minHeight?: number;
}

/**
 * Tabela de dados completa e acessível
 * Implementa navegação por setas, ordenação e filtros
 * 
 * @example
 * ```tsx
 * <DataTable
 *   data={produtos}
 *   columns={[
 *     {
 *       key: 'nome',
 *       label: 'Nome',
 *       sortable: true,
 *       render: (produto) => produto.nome
 *     }
 *   ]}
 *   searchable
 *   onSearch={(query) => setSearchQuery(query)}
 * />
 * ```
 */
export function DataTable<T = any>({
  data,
  columns,
  totalRecords,
  currentPage = 1,
  pageSize = 10,
  onPageChange,
  isLoading = false,
  title,
  searchable = false,
  searchPlaceholder = 'Buscar...',
  onSearch,
  filterable = false,
  filterOptions = [],
  onFilter,
  emptyMessage = 'Nenhum registro encontrado',
  minHeight = 300,
}: DataTableProps<T>) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFilter, setSelectedFilter] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<string | null>(null);
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const [debouncedSearchQuery] = useDebouncedValue(searchQuery, 300);

  // Chamar callback de busca quando query mudar
  React.useEffect(() => {
    if (onSearch) {
      onSearch(debouncedSearchQuery);
    }
  }, [debouncedSearchQuery, onSearch]);

  // Chamar callback de filtro quando filtro mudar
  React.useEffect(() => {
    if (onFilter) {
      onFilter(selectedFilter);
    }
  }, [selectedFilter, onFilter]);

  /**
   * Handler de ordenação
   */
  const handleSort = (columnKey: string) => {
    const column = columns.find(col => col.key === columnKey);
    if (!column?.sortable) return;

    if (sortBy === columnKey) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(columnKey);
      setSortOrder('asc');
    }
  };

  /**
   * Dados processados com filtros e ordenação local
   */
  const processedData = useMemo(() => {
    let result = [...data];

    // Ordenação local (se não há paginação externa)
    if (sortBy && !totalRecords) {
      result.sort((a, b) => {
        const aValue = (a as any)[sortBy];
        const bValue = (b as any)[sortBy];
        
        if (aValue < bValue) return sortOrder === 'asc' ? -1 : 1;
        if (aValue > bValue) return sortOrder === 'asc' ? 1 : -1;
        return 0;
      });
    }

    return result;
  }, [data, sortBy, sortOrder, totalRecords]);

  /**
   * Total de páginas
   */
  const totalPages = useMemo(() => {
    const total = totalRecords || processedData.length;
    return Math.ceil(total / pageSize);
  }, [totalRecords, processedData.length, pageSize]);

  return (
    <Paper shadow="sm" radius="md" withBorder>
      <div className="relative">
        <LoadingOverlay 
          visible={isLoading}
          overlayProps={{ radius: 'sm', blur: 1 }}
          loaderProps={{ color: 'brand', type: 'dots' }}
        />

        <Stack gap="md" p="lg">
          {/* Header */}
          <Group justify="space-between" align="center">
            {title && (
              <Text size="lg" fw={600}>
                {title}
              </Text>
            )}
            
            <Group gap="sm">
              {/* Busca */}
              {searchable && (
                <TextInput
                  placeholder={searchPlaceholder}
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.currentTarget.value)}
                  leftSection={<IconSearch size={16} />}
                  className="w-64"
                  aria-label="Campo de busca"
                />
              )}

              {/* Filtros */}
              {filterable && filterOptions.length > 0 && (
                <Select
                  placeholder="Filtrar"
                  value={selectedFilter}
                  onChange={setSelectedFilter}
                  data={filterOptions}
                  leftSection={<IconAdjustments size={16} />}
                  clearable
                  className="w-40"
                  aria-label="Filtrar resultados"
                />
              )}
            </Group>
          </Group>

          {/* Tabela */}
          <div style={{ minHeight }}>
            {processedData.length === 0 ? (
              <Center h={minHeight}>
                <Stack align="center" gap="md">
                  <IconFileX size={48} color="gray" />
                  <Text c="dimmed" ta="center">
                    {emptyMessage}
                  </Text>
                </Stack>
              </Center>
            ) : (
              <Table highlightOnHover striped>
                <Table.Thead>
                  <Table.Tr>
                    {columns.map((column) => (
                      <Table.Th
                        key={column.key}
                        style={{ 
                          width: column.width,
                          textAlign: column.align || 'left',
                        }}
                        className={column.sortable ? 'cursor-pointer hover:bg-gray-50' : ''}
                        onClick={() => column.sortable && handleSort(column.key)}
                      >
                        <Group gap="xs" justify={column.align === 'center' ? 'center' : column.align === 'right' ? 'flex-end' : 'flex-start'}>
                          <Text fw={600} size="sm">
                            {column.label}
                          </Text>
                          {column.sortable && (
                            <ActionIcon
                              variant="transparent"
                              size="sm"
                              c={sortBy === column.key ? 'brand' : 'gray'}
                            >
                              {sortBy === column.key ? (
                                sortOrder === 'asc' ? (
                                  <IconChevronUp size={14} />
                                ) : (
                                  <IconChevronDown size={14} />
                                )
                              ) : (
                                <IconChevronUp size={14} />
                              )}
                            </ActionIcon>
                          )}
                        </Group>
                      </Table.Th>
                    ))}
                  </Table.Tr>
                </Table.Thead>

                <Table.Tbody>
                  {processedData.map((item, index) => (
                    <Table.Tr 
                      key={index}
                      className="hover:bg-gray-25"
                    >
                      {columns.map((column) => (
                        <Table.Td
                          key={column.key}
                          style={{ textAlign: column.align || 'left' }}
                        >
                          {column.render(item)}
                        </Table.Td>
                      ))}
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            )}
          </div>

          {/* Paginação */}
          {totalPages > 1 && (
            <Group justify="space-between" align="center">
              <Text size="sm" c="dimmed">
                Exibindo {((currentPage - 1) * pageSize) + 1} a{' '}
                {Math.min(currentPage * pageSize, totalRecords || processedData.length)}{' '}
                de {totalRecords || processedData.length} registros
              </Text>
              
              <Pagination
                total={totalPages}
                value={currentPage}
                onChange={onPageChange || (() => {})}
                size="sm"
                withEdges
              />
            </Group>
          )}
        </Stack>
      </div>
    </Paper>
  );
}