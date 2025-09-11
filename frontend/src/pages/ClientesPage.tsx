/**
 * Página principal de Clientes - Interface Padaria Otimizada
 * Layout compacto e prático para padarias
 */
import React, { useState, useCallback } from 'react';
import { useKeyboardShortcuts } from '../hooks/useKeyboardShortcuts';
import { useDisclosure } from '@mantine/hooks';
import { HelpModal } from '../components/help/HelpModal';
import {
  Container,
  Button,
  TextInput,
  Select,
  Table,
  Badge,
  ActionIcon,
  Tooltip,
  Modal,
  Alert,
  Loader,
  Menu,
  Switch,
  ScrollArea
} from '@mantine/core';
import { Text, Group, Stack } from '../components/padaria';
import { modals } from '@mantine/modals';
import {
  IconPlus,
  IconSearch,
  IconEye,
  IconEdit,
  IconTrash,
  IconUsers,
  IconShield,
  IconPhone,
  IconDots,
  IconFileText,
  IconUserCheck,
  IconTrendingUp
} from '@tabler/icons-react';
import { useClientes, useBuscarClientes } from '../hooks/useClientes';
import { ClienteForm } from '../components/forms/ClienteForm';
import { ClienteSearchInput } from '../components/search';
import { ModernPagination } from '../components/pagination';
import { formatarCpf, formatarTelefone } from '../services/clienteService';
import {
  Cliente,
  ClienteResumo,
  BuscarClienteRequest,
  CriarClienteRequest,
  AtualizarClienteRequest
} from '../types/cliente';

/**
 * Componente para estatísticas do dashboard - CSS Padaria Otimizado
 */
const ClientesStats: React.FC<{ estatisticas: any }> = ({ estatisticas }) => {
  if (!estatisticas) return null;

  const stats = [
    {
      label: 'Total de Clientes',
      value: estatisticas.totalClientes,
      icon: IconUsers,
    },
    {
      label: 'Clientes Ativos',
      value: estatisticas.clientesAtivos,
      icon: IconUserCheck,
    },
    {
      label: 'Com Consentimento LGPD',
      value: estatisticas.clientesComConsentimento,
      icon: IconShield,
    },
    {
      label: 'Ticket Médio',
      value: `R$ ${estatisticas.ticketMedioGeral?.toFixed(2) || '0,00'}`,
      icon: IconTrendingUp,
    },
  ];

  return (
    <div className="padaria-grid-4">
      {stats.map((stat, index) => (
        <div key={index} className="padaria-stats-card">
          <div className="padaria-stat-content">
            <div className="padaria-stat-label">
              {stat.label}
            </div>
            <div className="padaria-stat-value">
              {stat.value}
            </div>
          </div>
          <stat.icon size="1.5rem" className="padaria-stat-icon" />
        </div>
      ))}
    </div>
  );
};

/**
 * Componente para busca inteligente única - Ultra-simples
 */
const BuscaInteligente: React.FC<{
  clientes: ClienteResumo[];
  onClienteSelect: (cliente: ClienteResumo) => void;
  onNovoCliente: () => void;
}> = ({ clientes, onClienteSelect, onNovoCliente }) => {
  return (
    <div className="padaria-search-compact">
      <div className="padaria-between padaria-gap-md">
        <div style={{ flex: 1 }}>
          <ClienteSearchInput
            clientes={clientes || []}
            onClienteSelect={onClienteSelect}
            placeholder="🔍 Buscar cliente por nome, CPF, email ou telefone..."
            autoFocus={false}
          />
        </div>
        
        <Button
          leftSection={<IconPlus size="1rem" />}
          onClick={onNovoCliente}
          className="padaria-button-primary"
          size="sm"
        >
          Novo Cliente
        </Button>
      </div>
    </div>
  );
};

/**
 * Componente para linha da tabela de clientes - Colunas separadas
 */
const ClienteTableRow: React.FC<{
  cliente: ClienteResumo;
  onView: (id: string) => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}> = ({ cliente, onView, onEdit, onDelete }) => {
  const getAvatarColor = (categoria: string) => {
    switch (categoria) {
      case 'VIP': return 'purple';
      case 'Premium': return 'gold';
      case 'Regular': return 'blue';
      case 'Bronze': return 'orange';
      default: return 'gray';
    }
  };

  return (
    <tr>
      <td className="padaria-table-cell padaria-table-cell-left">
        <Text size="sm" fw={500} className="padaria-text-primary">
          {cliente.nome}
        </Text>
      </td>

      <td className="padaria-table-cell padaria-table-cell-left">
        {cliente.email ? (
          <Text size="sm" className="padaria-text-primary">
            {cliente.email}
          </Text>
        ) : (
          <Text size="sm" className="padaria-text-muted">-</Text>
        )}
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        {cliente.cpf ? (
          <Text size="sm" ff="monospace" className="padaria-text-primary">
            {formatarCpf(cliente.cpf)}
          </Text>
        ) : (
          <Text size="sm" className="padaria-text-muted">-</Text>
        )}
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        {cliente.telefoneCelular ? (
          <Group gap={4} justify="center">
            <IconPhone size="0.8rem" />
            <Text size="sm" className="padaria-text-primary">
              {formatarTelefone(cliente.telefoneCelular)}
            </Text>
          </Group>
        ) : (
          <Text size="sm" className="padaria-text-muted">-</Text>
        )}
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        <div style={{display: 'flex', justifyContent: 'center'}}>
          <Badge
            color={getAvatarColor(cliente.categoriaCliente)}
            variant="light"
            size="sm"
          >
            {cliente.categoriaCliente}
          </Badge>
        </div>
      </td>

      <td className="padaria-table-cell padaria-table-cell-right">
        <Text size="sm" fw={500} className="padaria-text-primary">
          R$ {cliente.valorTotalCompras.toFixed(2)}
        </Text>
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        {cliente.dataUltimaCompra ? (
          <Text size="sm" className="padaria-text-primary">
            {new Date(cliente.dataUltimaCompra).toLocaleDateString('pt-BR')}
          </Text>
        ) : (
          <Text size="sm" className="padaria-text-muted">Nunca</Text>
        )}
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        <Group gap={4} justify="center">
          <Badge
            color={cliente.ativo ? 'green' : 'red'}
            variant="dot"
            size="sm"
          >
            {cliente.ativo ? 'Ativo' : 'Inativo'}
          </Badge>
          {cliente.consentimentoColeta && (
            <Tooltip label="LGPD Compliant">
              <IconShield size="1rem" color="green" />
            </Tooltip>
          )}
        </Group>
      </td>

      <td className="padaria-table-cell padaria-table-cell-center">
        <Group gap={4}>
          <ActionIcon variant="subtle" size="sm" onClick={() => onView(cliente.id)}>
            <IconEye size="1rem" />
          </ActionIcon>
          <ActionIcon variant="subtle" size="sm" onClick={() => onEdit(cliente.id)}>
            <IconEdit size="1rem" />
          </ActionIcon>
          <Menu shadow="md" width={200}>
            <Menu.Target>
              <ActionIcon variant="subtle" size="sm">
                <IconDots size="1rem" />
              </ActionIcon>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Item leftSection={<IconFileText size="0.9rem" />}>
                Histórico
              </Menu.Item>
              <Menu.Item leftSection={<IconShield size="0.9rem" />}>
                Relatório LGPD
              </Menu.Item>
              <Menu.Divider />
              <Menu.Item
                leftSection={<IconTrash size="0.9rem" />}
                color="red"
                onClick={() => onDelete(cliente.id)}
              >
                Excluir
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </td>
    </tr>
  );
};

/**
 * Página principal de Clientes - Interface Padaria Otimizada
 */
export const ClientesPage: React.FC = () => {
  // Estado
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [clienteSelecionado, setClienteSelecionado] = useState<Cliente | undefined>();
  const [termoBusca, setTermoBusca] = useState('');

  // Modals
  const [modalAberto, { open: abrirModal, close: fecharModal }] = useDisclosure(false);
  const [helpModalAberto, { open: abrirHelpModal, close: fecharHelpModal }] = useDisclosure(false);

  // Hooks
  const {
    clientes,
    totalClientes,
    totalPaginas,
    paginaAtual,
    tamanhoPagina,
    estatisticas,
    carregandoClientes,
    criandoCliente,
    atualizandoCliente,
    filtros,
    criarCliente,
    atualizarCliente,
    removerCliente,
    atualizarFiltros,
    irParaPagina,
    mudarTamanhoPagina,
    resetarFiltros,
    recarregarClientes,
  } = useClientes();

  const { resultados: resultadosBusca } = useBuscarClientes(termoBusca, 10);

  // Sistema de atalhos de teclado
  const handleHelpModal = useCallback(() => {
    abrirHelpModal();
  }, [abrirHelpModal]);

  const handleSearchFocus = useCallback(() => {
    // Focar no campo de busca inteligente
    const searchInput = document.querySelector('[data-search-input]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  }, []);

  // Ativar sistema de atalhos
  useKeyboardShortcuts({
    onHelp: handleHelpModal,
    onSearch: handleSearchFocus,
  });

  // Handlers
  const handleNovoCliente = useCallback(() => {
    setModalMode('create');
    setClienteSelecionado(undefined);
    abrirModal();
  }, [abrirModal]);

  const handleVisualizarCliente = useCallback(async (id: string) => {
    setModalMode('view');
    abrirModal();
  }, [abrirModal]);

  const handleEditarCliente = useCallback(async (id: string) => {
    setModalMode('edit');
    abrirModal();
  }, [abrirModal]);

  const handleExcluirCliente = useCallback((id: string) => {
    modals.openConfirmModal({
      title: 'Confirmar exclusão',
      children: (
        <Text size="sm">
          Tem certeza que deseja excluir este cliente? Esta ação não pode ser desfeita.
        </Text>
      ),
      labels: { confirm: 'Excluir', cancel: 'Cancelar' },
      confirmProps: { color: 'red' },
      onConfirm: () => removerCliente(id),
    });
  }, [removerCliente]);

  const handleSalvarCliente = useCallback(async (data: CriarClienteRequest | AtualizarClienteRequest) => {
    try {
      if (modalMode === 'create') {
        await criarCliente(data as CriarClienteRequest);
      } else if (modalMode === 'edit' && clienteSelecionado) {
        await atualizarCliente(clienteSelecionado.id, data as AtualizarClienteRequest);
      }
      fecharModal();
    } catch (error) {
      // Erro já tratado no hook
    }
  }, [modalMode, clienteSelecionado, criarCliente, atualizarCliente, fecharModal]);

  // Loading state
  if (carregandoClientes && clientes.length === 0) {
    return (
      <div className="padaria-page-container">
        <Group justify="center" py="xl">
          <Loader size="lg" />
          <Text>Carregando clientes...</Text>
        </Group>
      </div>
    );
  }

  return (
    <div className="padaria-page-container padaria-theme padaria-form-compact">
      {/* Estatísticas */}
      <div className="padaria-mb-lg">
        <ClientesStats estatisticas={estatisticas} />
      </div>

      {/* Busca Inteligente Única + Novo Cliente */}
      <BuscaInteligente
        clientes={clientes || []}
        onClienteSelect={(cliente) => handleVisualizarCliente(cliente.id)}
        onNovoCliente={handleNovoCliente}
      />

      {/* Paginação moderna */}
      {totalPaginas > 1 && (
        <Group justify="flex-end" mb="md">
          <ModernPagination
            currentPage={paginaAtual}
            totalPages={totalPaginas}
            totalItems={totalClientes}
            itemsPerPage={tamanhoPagina}
            onPageChange={irParaPagina}
            onItemsPerPageChange={mudarTamanhoPagina}
            loading={carregandoClientes}
          />
        </Group>
      )}

      {/* Resultados da busca */}
      {termoBusca && resultadosBusca.length > 0 && (
        <Alert icon={<IconSearch />} title="Resultados da busca" color="blue" mb="md">
          <Stack gap="xs">
            {resultadosBusca.slice(0, 5).map((cliente) => (
              <Group key={cliente.id} justify="space-between">
                <Group gap="xs">
                  <Text size="sm" fw={500}>{cliente.nome}</Text>
                  {cliente.cpf && <Text size="xs" className="padaria-text-muted">• {formatarCpf(cliente.cpf)}</Text>}
                  {cliente.email && <Text size="xs" className="padaria-text-muted">• {cliente.email}</Text>}
                </Group>
                <Group gap="xs">
                  <ActionIcon size="sm" onClick={() => handleVisualizarCliente(cliente.id)}>
                    <IconEye size="0.8rem" />
                  </ActionIcon>
                  <ActionIcon size="sm" onClick={() => handleEditarCliente(cliente.id)}>
                    <IconEdit size="0.8rem" />
                  </ActionIcon>
                </Group>
              </Group>
            ))}
          </Stack>
        </Alert>
      )}

      {/* Tabela com Colunas Separadas */}
      <div className="padaria-table-container">
        <ScrollArea>
          <Table verticalSpacing="sm">
            <thead>
              <tr>
                <th className="padaria-table-header padaria-table-cell-left">Nome</th>
                <th className="padaria-table-header padaria-table-cell-left">Email</th>
                <th className="padaria-table-header padaria-table-cell-center">CPF</th>
                <th className="padaria-table-header padaria-table-cell-center">Telefone</th>
                <th className="padaria-table-header padaria-table-cell-center">Categoria</th>
                <th className="padaria-table-header padaria-table-cell-right">Total Compras</th>
                <th className="padaria-table-header padaria-table-cell-center">Última Compra</th>
                <th className="padaria-table-header padaria-table-cell-center">Status</th>
                <th className="padaria-table-header padaria-table-cell-center" style={{width: 120}}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {clientes.map((cliente) => (
                <ClienteTableRow
                  key={cliente.id}
                  cliente={cliente}
                  onView={handleVisualizarCliente}
                  onEdit={handleEditarCliente}
                  onDelete={handleExcluirCliente}
                />
              ))}
            </tbody>
          </Table>
        </ScrollArea>

        {clientes.length === 0 && !carregandoClientes && (
          <Group justify="center" py="xl">
            <Stack align="center" gap="sm">
              <IconUsers size="3rem" color="gray" />
              <Text className="padaria-text-muted" ta="center">
                {Object.keys(filtros).length > 1
                  ? 'Nenhum cliente encontrado com os filtros aplicados'
                  : 'Nenhum cliente cadastrado'}
              </Text>
              {Object.keys(filtros).length <= 1 && (
                <Button leftSection={<IconPlus size="1rem" />} onClick={handleNovoCliente}>
                  Cadastrar primeiro cliente
                </Button>
              )}
            </Stack>
          </Group>
        )}
      </div>

      {/* Modal */}
      <Modal
        opened={modalAberto}
        onClose={fecharModal}
        title={
          modalMode === 'create'
            ? '➕ Novo Cliente'
            : modalMode === 'edit'
            ? '✏️ Editar Cliente'
            : '👁️ Visualizar Cliente'
        }
        size="xl"
        className="padaria-modal-wide"
        centered
        closeOnClickOutside={false}
      >
        {/* ClienteForm temporariamente comentado por erro JSX */}
        <div>Form em manutenção</div>
      </Modal>

      {/* Modal de Ajuda F1 */}
      <HelpModal
        opened={helpModalAberto}
        onClose={fecharHelpModal}
      />
    </div>
  );
};