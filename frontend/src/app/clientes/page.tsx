/**
 * Página de Clientes - Next.js 15 + API Real
 * Interface CRM completa com CRUD e navegação por teclado
 */
'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Container,
  Title,
  Button,
  Table,
  Card,
  Group,
  TextInput,
  Text,
  Badge,
  Stack,
  LoadingOverlay,
  ActionIcon,
  Tooltip,
  Pagination,
  Select,
  Flex,
  Alert,
  Menu,
} from '@mantine/core';
import {
  IconSearch,
  IconPlus,
  IconEdit,
  IconTrash,
  IconRestore,
  IconEye,
  IconDots,
  IconRefresh,
  IconUser,
  IconFilter,
  IconChartBar,
} from '@tabler/icons-react';
import { useDisclosure } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  useClientes,
  useDeleteCliente,
  useRestoreCliente,
  useClienteEstatisticas
} from '@/hooks/useClientes';
import { Cliente, formatarNomeCompleto } from '@/services/clienteService';
import ClienteModal from '@/components/ClienteModal';
import ClienteDashboard from '@/components/ClienteDashboard';

export default function ClientesPage() {
  const router = useRouter();

  // Estado local
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [limit, setLimit] = useState(20);
  const [showInactive, setShowInactive] = useState(false);
  const [selectedCliente, setSelectedCliente] = useState<Cliente | null>(null);
  const [showDashboard, setShowDashboard] = useState(false);

  // Modals
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);

  // Queries
  const { data: clientesData, isLoading, error, refetch } = useClientes({
    page,
    limit,
    search: search.trim(),
    ativo: !showInactive
  });

  const { data: estatisticas } = useClienteEstatisticas();

  // Mutations
  const deleteCliente = useDeleteCliente();
  const restoreCliente = useRestoreCliente();

  // Navegação por teclado - F2 para clientes
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'F2') {
        event.preventDefault();
        console.log('🎯 F2 - Foco em Clientes');
        // Já estamos na página de clientes
      }

      if (event.ctrlKey && event.key === 'n') {
        event.preventDefault();
        handleNovoCliente();
      }

      if (event.key === 'F5') {
        event.preventDefault();
        refetch();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [refetch]);

  // Handlers
  const handleNovoCliente = () => {
    setSelectedCliente(null);
    openModal();
  };

  const handleEditarCliente = (cliente: Cliente) => {
    setSelectedCliente(cliente);
    openModal();
  };

  const handleVisualizarCliente = (cliente: Cliente) => {
    router.push(`/clientes/${cliente.id}`);
  };

  const handleDeletarCliente = (cliente: Cliente) => {
    modals.openConfirmModal({
      title: 'Confirmar Remoção',
      children: (
        <Text size="sm">
          Tem certeza que deseja remover <strong>{formatarNomeCompleto(cliente)}</strong>?
          Esta ação pode ser desfeita posteriormente.
        </Text>
      ),
      labels: { confirm: 'Remover', cancel: 'Cancelar' },
      confirmProps: { color: 'red' },
      onConfirm: () => deleteCliente.mutate({
        id: cliente.id,
        motivo: 'Removido pela interface'
      }),
    });
  };

  const handleRestaurarCliente = (cliente: Cliente) => {
    restoreCliente.mutate(cliente.id);
  };

  // Renderização das linhas da tabela
  const rows = clientesData?.data.items.map((cliente) => (
    <Table.Tr key={cliente.id} style={{ cursor: 'pointer' }}>
      <Table.Td onClick={() => handleVisualizarCliente(cliente)}>
        <Group gap="sm">
          <IconUser size={16} color="var(--mantine-color-blue-6)" />
          <div>
            <Text fw={500} size="sm">
              {formatarNomeCompleto(cliente)}
            </Text>
            {cliente.cpf && (
              <Text size="xs" c="dimmed">
                CPF: {cliente.cpf}
              </Text>
            )}
          </div>
        </Group>
      </Table.Td>

      <Table.Td>
        {cliente.dataNascimento ? (
          <Text size="sm">
            {new Date(cliente.dataNascimento).toLocaleDateString('pt-BR')}
          </Text>
        ) : (
          <Text size="sm" c="dimmed">Não informado</Text>
        )}
      </Table.Td>

      <Table.Td>
        <Badge
          color={cliente.ativo ? 'green' : 'red'}
          variant="light"
          size="sm"
        >
          {cliente.ativo ? 'Ativo' : 'Inativo'}
        </Badge>
      </Table.Td>

      <Table.Td>
        <Text size="sm" c="dimmed">
          {new Date(cliente.dataCadastro).toLocaleDateString('pt-BR')}
        </Text>
      </Table.Td>

      <Table.Td>
        <Group gap="xs" justify="flex-end">
          <Tooltip label="Visualizar">
            <ActionIcon
              variant="light"
              color="blue"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                handleVisualizarCliente(cliente);
              }}
            >
              <IconEye size={14} />
            </ActionIcon>
          </Tooltip>

          <Tooltip label="Editar">
            <ActionIcon
              variant="light"
              color="yellow"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                handleEditarCliente(cliente);
              }}
            >
              <IconEdit size={14} />
            </ActionIcon>
          </Tooltip>

          {cliente.ativo ? (
            <Tooltip label="Remover">
              <ActionIcon
                variant="light"
                color="red"
                size="sm"
                loading={deleteCliente.isPending}
                onClick={(e) => {
                  e.stopPropagation();
                  handleDeletarCliente(cliente);
                }}
              >
                <IconTrash size={14} />
              </ActionIcon>
            </Tooltip>
          ) : (
            <Tooltip label="Restaurar">
              <ActionIcon
                variant="light"
                color="green"
                size="sm"
                loading={restoreCliente.isPending}
                onClick={(e) => {
                  e.stopPropagation();
                  handleRestaurarCliente(cliente);
                }}
              >
                <IconRestore size={14} />
              </ActionIcon>
            </Tooltip>
          )}
        </Group>
      </Table.Td>
    </Table.Tr>
  ));

  return (
    <Container size="xl" py="xl">
      <Stack gap="xl">
        {/* Cabeçalho */}
        <Group justify="space-between">
          <div>
            <Title order={1} c="dark.7">👥 Gestão de Clientes</Title>
            <Text c="dimmed" size="lg">
              Sistema CRM integrado com API real
            </Text>
            {estatisticas && (
              <Group gap="lg" mt="xs">
                <Text size="sm" c="blue">
                  <strong>{estatisticas.data.ativos}</strong> ativos
                </Text>
                <Text size="sm" c="orange">
                  <strong>{estatisticas.data.inativos}</strong> inativos
                </Text>
                <Text size="sm" c="green">
                  <strong>{estatisticas.data.novosMes}</strong> novos este mês
                </Text>
              </Group>
            )}
          </div>

          <Group>
            <Button
              leftSection={<IconChartBar size={16} />}
              variant={showDashboard ? 'filled' : 'light'}
              onClick={() => setShowDashboard(!showDashboard)}
            >
              {showDashboard ? 'Ocultar Dashboard' : 'Dashboard'}
            </Button>

            <Button
              leftSection={<IconRefresh size={16} />}
              variant="light"
              onClick={() => refetch()}
              loading={isLoading}
            >
              Atualizar
            </Button>

            <Button
              leftSection={<IconPlus size={16} />}
              onClick={handleNovoCliente}
            >
              Novo Cliente
            </Button>
          </Group>
        </Group>

        {/* Dashboard de Métricas (condicional) */}
        {showDashboard && (
          <ClienteDashboard />
        )}

        {/* Filtros e Busca */}
        <Card shadow="sm" padding="lg" radius="md" withBorder>
          <Stack gap="md">
            <Group>
              <TextInput
                placeholder="Buscar por nome, sobrenome ou CPF..."
                leftSection={<IconSearch size={16} />}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                style={{ flex: 1 }}
                size="sm"
              />

              <Select
                data={[
                  { value: 'false', label: 'Apenas Ativos' },
                  { value: 'true', label: 'Apenas Inativos' },
                ]}
                value={showInactive.toString()}
                onChange={(value) => setShowInactive(value === 'true')}
                placeholder="Filtrar por status"
                leftSection={<IconFilter size={16} />}
                w={150}
                size="sm"
              />

              <Select
                data={[
                  { value: '10', label: '10 por página' },
                  { value: '20', label: '20 por página' },
                  { value: '50', label: '50 por página' },
                ]}
                value={limit.toString()}
                onChange={(value) => setLimit(parseInt(value || '20'))}
                w={140}
                size="sm"
              />
            </Group>

            {/* Alert de navegação por teclado */}
            <Alert color="blue" variant="light" style={{ fontSize: '14px' }}>
              <Group gap="lg">
                <Text size="sm">
                  <kbd>F2</kbd> Clientes • <kbd>Ctrl+N</kbd> Novo • <kbd>F5</kbd> Atualizar
                </Text>
              </Group>
            </Alert>
          </Stack>
        </Card>

        {/* Tabela de Clientes */}
        <Card shadow="sm" padding="lg" radius="md" withBorder>
          <div style={{ position: 'relative' }}>
            <LoadingOverlay visible={isLoading} />

            {error ? (
              <Alert color="red" title="Erro ao carregar clientes">
                Não foi possível conectar com a API. Verifique se o backend está rodando.
              </Alert>
            ) : (
              <>
                <Table striped highlightOnHover>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Cliente</Table.Th>
                      <Table.Th>Data Nascimento</Table.Th>
                      <Table.Th>Status</Table.Th>
                      <Table.Th>Cadastro</Table.Th>
                      <Table.Th width={120}>Ações</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {rows && rows.length > 0 ? rows : (
                      <Table.Tr>
                        <Table.Td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                          <Stack align="center" gap="sm">
                            <IconUser size={48} color="var(--mantine-color-gray-4)" />
                            <Text c="dimmed" size="lg">
                              {search ? 'Nenhum cliente encontrado' : 'Nenhum cliente cadastrado'}
                            </Text>
                            {!search && (
                              <Button
                                variant="light"
                                leftSection={<IconPlus size={16} />}
                                onClick={handleNovoCliente}
                              >
                                Cadastrar Primeiro Cliente
                              </Button>
                            )}
                          </Stack>
                        </Table.Td>
                      </Table.Tr>
                    )}
                  </Table.Tbody>
                </Table>

                {/* Paginação */}
                {clientesData?.data.pagination.totalPages > 1 && (
                  <Flex justify="space-between" align="center" mt="md">
                    <Text size="sm" c="dimmed">
                      {clientesData?.data.pagination.totalItems} clientes encontrados
                    </Text>
                    <Pagination
                      value={page}
                      onChange={setPage}
                      total={clientesData?.data.pagination.totalPages || 1}
                      size="sm"
                    />
                  </Flex>
                )}
              </>
            )}
          </div>
        </Card>

        {/* Rodapé */}
        <Text size="sm" c="dimmed" ta="center">
          ✅ Next.js 15 + Express.js + PostgreSQL + React Query
        </Text>
      </Stack>

      {/* Modal de Cliente */}
      <ClienteModal
        opened={modalOpened}
        onClose={closeModal}
        cliente={selectedCliente}
      />
    </Container>
  );
}