/**
 * Página de Detalhes do Cliente - CRM Completo
 * Histórico de compras, timeline e informações detalhadas
 */
'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Container,
  Title,
  Button,
  Card,
  Group,
  Text,
  Stack,
  LoadingOverlay,
  Badge,
  Grid,
  Alert,
  Timeline,
  ActionIcon,
  Tooltip,
  Divider,
  Paper,
  Avatar,
  Tabs,
  StatGroup,
  Stat,
} from '@mantine/core';
import {
  IconUser,
  IconArrowLeft,
  IconEdit,
  IconTrash,
  IconRestore,
  IconShoppingCart,
  IconCalendar,
  IconPhone,
  IconMail,
  IconMapPin,
  IconCoin,
  IconChartLine,
  IconNotes,
  IconHistory,
} from '@tabler/icons-react';
import { useDisclosure } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { useCliente, useDeleteCliente, useRestoreCliente } from '@/hooks/useClientes';
import { Cliente, formatarNomeCompleto } from '@/services/clienteService';
import ClienteModal from '@/components/ClienteModal';

interface ClienteDetalhesPageProps {
  params: { id: string };
}

export default function ClienteDetalhesPage({ params }: ClienteDetalhesPageProps) {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState('geral');
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);

  // Queries e Mutations
  const { data: clienteResponse, isLoading, error } = useCliente(params.id);
  const deleteCliente = useDeleteCliente();
  const restoreCliente = useRestoreCliente();

  const cliente = clienteResponse?.data;

  // Navegação por teclado
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault();
        router.back();
      }

      if (event.key === 'F2') {
        event.preventDefault();
        router.push('/clientes');
      }

      if (event.ctrlKey && event.key === 'e') {
        event.preventDefault();
        if (cliente) openModal();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [router, cliente, openModal]);

  // Handlers
  const handleVoltar = () => {
    router.back();
  };

  const handleEditar = () => {
    openModal();
  };

  const handleDeletar = () => {
    if (!cliente) return;

    // TODO: Implementar modal de confirmação
    notifications.show({
      title: 'Função em desenvolvimento',
      message: 'Modal de confirmação será implementado',
      color: 'blue',
    });
  };

  const handleRestaurar = () => {
    if (!cliente) return;
    restoreCliente.mutate(cliente.id);
  };

  if (isLoading) {
    return (
      <Container size="xl" py="xl">
        <LoadingOverlay visible />
      </Container>
    );
  }

  if (error || !cliente) {
    return (
      <Container size="xl" py="xl">
        <Alert color="red" title="Erro">
          Cliente não encontrado ou erro ao carregar dados.
          <Button variant="light" mt="sm" onClick={handleVoltar}>
            Voltar para listagem
          </Button>
        </Alert>
      </Container>
    );
  }

  const idade = cliente.dataNascimento
    ? new Date().getFullYear() - new Date(cliente.dataNascimento).getFullYear()
    : null;

  return (
    <Container size="xl" py="xl">
      <Stack gap="xl">
        {/* Cabeçalho */}
        <Group justify="space-between">
          <Group>
            <ActionIcon
              variant="light"
              size="lg"
              onClick={handleVoltar}
              aria-label="Voltar"
            >
              <IconArrowLeft size={20} />
            </ActionIcon>
            <div>
              <Title order={1} c="dark.7">
                {formatarNomeCompleto(cliente)}
              </Title>
              <Group gap="sm" mt={4}>
                <Badge
                  color={cliente.ativo ? 'green' : 'red'}
                  variant="light"
                  size="sm"
                >
                  {cliente.ativo ? 'Ativo' : 'Inativo'}
                </Badge>
                {cliente.cpf && (
                  <Text size="sm" c="dimmed">
                    CPF: {cliente.cpf}
                  </Text>
                )}
              </Group>
            </div>
          </Group>

          <Group>
            <Button
              variant="light"
              leftSection={<IconEdit size={16} />}
              onClick={handleEditar}
            >
              Editar
            </Button>

            {cliente.ativo ? (
              <Button
                variant="light"
                color="red"
                leftSection={<IconTrash size={16} />}
                onClick={handleDeletar}
                loading={deleteCliente.isPending}
              >
                Remover
              </Button>
            ) : (
              <Button
                variant="light"
                color="green"
                leftSection={<IconRestore size={16} />}
                onClick={handleRestaurar}
                loading={restoreCliente.isPending}
              >
                Restaurar
              </Button>
            )}
          </Group>
        </Group>

        {/* Alert de navegação */}
        <Alert color="blue" variant="light" style={{ fontSize: '14px' }}>
          <Text size="sm">
            <kbd>ESC</kbd> Voltar • <kbd>F2</kbd> Lista Clientes • <kbd>Ctrl+E</kbd> Editar
          </Text>
        </Alert>

        {/* Tabs de Conteúdo */}
        <Tabs value={activeTab} onChange={(value) => setActiveTab(value || 'geral')}>
          <Tabs.List>
            <Tabs.Tab value="geral" leftSection={<IconUser size={16} />}>
              Informações Gerais
            </Tabs.Tab>
            <Tabs.Tab value="vendas" leftSection={<IconShoppingCart size={16} />}>
              Histórico de Vendas
            </Tabs.Tab>
            <Tabs.Tab value="estatisticas" leftSection={<IconChartLine size={16} />}>
              Estatísticas
            </Tabs.Tab>
            <Tabs.Tab value="observacoes" leftSection={<IconNotes size={16} />}>
              Observações
            </Tabs.Tab>
          </Tabs.List>

          {/* Tab: Informações Gerais */}
          <Tabs.Panel value="geral" pt="lg">
            <Grid>
              <Grid.Col span={{ base: 12, md: 6 }}>
                <Card shadow="sm" padding="lg" radius="md" withBorder>
                  <Stack gap="md">
                    <Group gap="sm">
                      <IconUser size={20} color="var(--mantine-color-blue-6)" />
                      <Text fw={600} size="lg">Dados Pessoais</Text>
                    </Group>

                    <Stack gap="sm">
                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">Nome:</Text>
                        <Text fw={500}>{cliente.nome}</Text>
                      </Group>

                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">Sobrenome:</Text>
                        <Text fw={500}>{cliente.sobrenome}</Text>
                      </Group>

                      {cliente.cpf && (
                        <Group justify="space-between">
                          <Text c="dimmed" size="sm">CPF:</Text>
                          <Text fw={500}>{cliente.cpf}</Text>
                        </Group>
                      )}

                      {cliente.dataNascimento && (
                        <>
                          <Group justify="space-between">
                            <Text c="dimmed" size="sm">Data de Nascimento:</Text>
                            <Text fw={500}>
                              {new Date(cliente.dataNascimento).toLocaleDateString('pt-BR')}
                            </Text>
                          </Group>

                          <Group justify="space-between">
                            <Text c="dimmed" size="sm">Idade:</Text>
                            <Text fw={500}>{idade} anos</Text>
                          </Group>
                        </>
                      )}
                    </Stack>
                  </Stack>
                </Card>
              </Grid.Col>

              <Grid.Col span={{ base: 12, md: 6 }}>
                <Card shadow="sm" padding="lg" radius="md" withBorder>
                  <Stack gap="md">
                    <Group gap="sm">
                      <IconCalendar size={20} color="var(--mantine-color-green-6)" />
                      <Text fw={600} size="lg">Informações do Sistema</Text>
                    </Group>

                    <Stack gap="sm">
                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">Data de Cadastro:</Text>
                        <Text fw={500}>
                          {new Date(cliente.dataCadastro).toLocaleDateString('pt-BR')}
                        </Text>
                      </Group>

                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">Última Atualização:</Text>
                        <Text fw={500}>
                          {new Date(cliente.dataUltimaAtualizacao).toLocaleDateString('pt-BR')}
                        </Text>
                      </Group>

                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">Status:</Text>
                        <Badge
                          color={cliente.ativo ? 'green' : 'red'}
                          variant="light"
                        >
                          {cliente.ativo ? 'Ativo' : 'Inativo'}
                        </Badge>
                      </Group>

                      <Group justify="space-between">
                        <Text c="dimmed" size="sm">ID do Sistema:</Text>
                        <Text size="xs" c="dimmed" style={{ fontFamily: 'monospace' }}>
                          {cliente.id}
                        </Text>
                      </Group>
                    </Stack>
                  </Stack>
                </Card>
              </Grid.Col>
            </Grid>
          </Tabs.Panel>

          {/* Tab: Histórico de Vendas */}
          <Tabs.Panel value="vendas" pt="lg">
            <Card shadow="sm" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <Group gap="sm">
                  <IconShoppingCart size={20} color="var(--mantine-color-orange-6)" />
                  <Text fw={600} size="lg">Histórico de Compras</Text>
                </Group>

                <Alert color="blue" variant="light">
                  <Text size="sm">
                    📝 Funcionalidade em desenvolvimento. Em breve será possível visualizar:
                  </Text>
                  <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                    <li>Timeline de compras</li>
                    <li>Produtos mais comprados</li>
                    <li>Valor total gasto</li>
                    <li>Frequência de visitas</li>
                  </ul>
                </Alert>

                {/* Placeholder para futuras vendas */}
                <Paper p="xl" radius="md" style={{ backgroundColor: 'var(--mantine-color-gray-0)' }}>
                  <Stack align="center" gap="md">
                    <IconHistory size={48} color="var(--mantine-color-gray-4)" />
                    <Text c="dimmed" ta="center">
                      Nenhuma compra registrada ainda.<br />
                      As vendas aparecerão aqui quando integrarmos com o módulo de vendas.
                    </Text>
                  </Stack>
                </Paper>
              </Stack>
            </Card>
          </Tabs.Panel>

          {/* Tab: Estatísticas */}
          <Tabs.Panel value="estatisticas" pt="lg">
            <Card shadow="sm" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <Group gap="sm">
                  <IconChartLine size={20} color="var(--mantine-color-teal-6)" />
                  <Text fw={600} size="lg">Estatísticas do Cliente</Text>
                </Group>

                <Grid>
                  <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                    <Paper p="md" radius="md" style={{ backgroundColor: 'var(--mantine-color-blue-0)' }}>
                      <Stack align="center" gap="xs">
                        <IconShoppingCart size={32} color="var(--mantine-color-blue-6)" />
                        <Text size="xl" fw={700} c="blue">0</Text>
                        <Text size="sm" c="dimmed" ta="center">Total de Compras</Text>
                      </Stack>
                    </Paper>
                  </Grid.Col>

                  <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                    <Paper p="md" radius="md" style={{ backgroundColor: 'var(--mantine-color-green-0)' }}>
                      <Stack align="center" gap="xs">
                        <IconCoin size={32} color="var(--mantine-color-green-6)" />
                        <Text size="xl" fw={700} c="green">R$ 0,00</Text>
                        <Text size="sm" c="dimmed" ta="center">Valor Total Gasto</Text>
                      </Stack>
                    </Paper>
                  </Grid.Col>

                  <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                    <Paper p="md" radius="md" style={{ backgroundColor: 'var(--mantine-color-orange-0)' }}>
                      <Stack align="center" gap="xs">
                        <IconCalendar size={32} color="var(--mantine-color-orange-6)" />
                        <Text size="xl" fw={700} c="orange">-</Text>
                        <Text size="sm" c="dimmed" ta="center">Última Compra</Text>
                      </Stack>
                    </Paper>
                  </Grid.Col>

                  <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                    <Paper p="md" radius="md" style={{ backgroundColor: 'var(--mantine-color-purple-0)' }}>
                      <Stack align="center" gap="xs">
                        <IconChartLine size={32} color="var(--mantine-color-purple-6)" />
                        <Text size="xl" fw={700} c="purple">R$ 0,00</Text>
                        <Text size="sm" c="dimmed" ta="center">Ticket Médio</Text>
                      </Stack>
                    </Paper>
                  </Grid.Col>
                </Grid>

                <Alert color="blue" variant="light" mt="md">
                  📊 Estatísticas serão calculadas automaticamente conforme as vendas forem registradas.
                </Alert>
              </Stack>
            </Card>
          </Tabs.Panel>

          {/* Tab: Observações */}
          <Tabs.Panel value="observacoes" pt="lg">
            <Card shadow="sm" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <Group gap="sm">
                  <IconNotes size={20} color="var(--mantine-color-indigo-6)" />
                  <Text fw={600} size="lg">Observações e Notas</Text>
                </Group>

                <Alert color="blue" variant="light">
                  📝 Funcionalidade de observações será implementada em breve. Permitirá:
                  <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                    <li>Adicionar notas sobre o cliente</li>
                    <li>Registrar preferências</li>
                    <li>Histórico de contatos</li>
                    <li>Informações especiais</li>
                  </ul>
                </Alert>

                <Paper p="xl" radius="md" style={{ backgroundColor: 'var(--mantine-color-gray-0)' }}>
                  <Stack align="center" gap="md">
                    <IconNotes size={48} color="var(--mantine-color-gray-4)" />
                    <Text c="dimmed" ta="center">
                      Nenhuma observação cadastrada.<br />
                      Use este espaço para registrar informações importantes sobre o cliente.
                    </Text>
                  </Stack>
                </Paper>
              </Stack>
            </Card>
          </Tabs.Panel>
        </Tabs>
      </Stack>

      {/* Modal de Edição */}
      <ClienteModal
        opened={modalOpened}
        onClose={closeModal}
        cliente={cliente}
      />
    </Container>
  );
}