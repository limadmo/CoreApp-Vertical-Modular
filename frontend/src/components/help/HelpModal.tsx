/**
 * Modal de ajuda contextual ativado pela tecla F1
 * Exibe informações específicas de cada tela/módulo
 */
import React from 'react';
import {
  Modal,
  Text,
  Stack,
  Group,
  Badge,
  Divider,
  Card,
  ScrollArea,
  List,
} from '@mantine/core';
import {
  IconKeyboard,
  IconSearch,
  IconHelp,
  IconBulb,
  IconUsers,
  IconShoppingCart,
  IconPackage,
  IconChartBar,
  IconSettings,
} from '@tabler/icons-react';
import { useLocation } from 'react-router-dom';

interface HelpModalProps {
  opened: boolean;
  onClose: () => void;
}

/**
 * Conteúdo de ajuda específico por rota
 */
const HELP_CONTENT = {
  '/': {
    title: 'Dashboard - Visão Geral',
    icon: <IconChartBar size="1.5rem" />,
    description: 'Painel principal com resumo do seu negócio',
    features: [
      'Vendas do dia, semana e mês',
      'Produtos em baixo estoque',
      'Clientes recentes',
      'Gráficos de performance',
    ],
    shortcuts: [
      { key: 'F2', action: 'Ir para Vendas' },
      { key: 'F3', action: 'Ir para Clientes' },
      { key: 'F4', action: 'Ir para Produtos' },
    ],
    tips: [
      'Use os cards coloridos para navegação rápida',
      'Gráficos são atualizados em tempo real',
      'Clique nos alertas para ações imediatas',
    ],
  },
  '/clientes': {
    title: 'Clientes - Gestão Completa',
    icon: <IconUsers size="1.5rem" />,
    description: 'Gerencie todos os seus clientes de forma prática',
    features: [
      'Busca inteligente por nome, CPF, email ou telefone',
      'Cadastro completo com dados de contato',
      'Histórico de compras e preferências',
      'Compliance LGPD automático',
    ],
    shortcuts: [
      { key: 'Ctrl+F', action: 'Focar na busca inteligente' },
      { key: 'Alt+N', action: 'Novo cliente' },
      { key: 'Enter', action: 'Visualizar cliente selecionado' },
    ],
    tips: [
      'A busca inteligente encontra clientes por qualquer campo',
      'Use Ctrl+F para buscar rapidamente',
      'O ícone 🛡️ indica compliance LGPD',
      'Categorias de cliente afetam promoções automáticas',
    ],
  },
  '/vendas': {
    title: 'Vendas - Ponto de Venda',
    icon: <IconShoppingCart size="1.5rem" />,
    description: 'Sistema de vendas otimizado para padarias',
    features: [
      'Busca rápida de produtos por código ou nome',
      'Cálculo automático de valores e descontos',
      'Integração com estoque em tempo real',
      'Múltiplas formas de pagamento',
    ],
    shortcuts: [
      { key: 'Ctrl+F', action: 'Buscar produtos' },
      { key: 'F8', action: 'Finalizar venda' },
      { key: 'Esc', action: 'Cancelar venda atual' },
    ],
    tips: [
      'Digite o código do produto diretamente',
      'Use * para buscar por parte do nome',
      'Promoções são aplicadas automaticamente',
      'Ctrl+F sempre foca na busca de produtos',
    ],
  },
  '/produtos': {
    title: 'Produtos - Catálogo Completo',
    icon: <IconPackage size="1.5rem" />,
    description: 'Gerencie seu catálogo de produtos',
    features: [
      'Cadastro por categorias (pães, doces, bebidas)',
      'Controle de preços e promoções',
      'Gestão de receitas e ingredientes',
      'Fotos e descrições detalhadas',
    ],
    shortcuts: [
      { key: 'Ctrl+F', action: 'Buscar produtos' },
      { key: 'Alt+N', action: 'Novo produto' },
      { key: 'Alt+E', action: 'Editar selecionado' },
    ],
    tips: [
      'Organize produtos por categorias lógicas',
      'Use códigos curtos para produtos frequentes',
      'Mantenha fotos atualizadas',
      'Configure alertas de estoque baixo',
    ],
  },
  '/configuracoes': {
    title: 'Configurações - Sistema',
    icon: <IconSettings size="1.5rem" />,
    description: 'Personalize o sistema para seu negócio',
    features: [
      'Dados da empresa e tributação',
      'Configuração de impressoras',
      'Usuários e permissões',
      'Backup automático',
    ],
    shortcuts: [
      { key: 'Ctrl+S', action: 'Salvar alterações' },
      { key: 'Esc', action: 'Cancelar edição' },
    ],
    tips: [
      'Configure a impressora antes da primeira venda',
      'Faça backup regularmente',
      'Defina permissões apropriadas por usuário',
      'Mantenha os dados da empresa atualizados',
    ],
  },
} as const;

/**
 * Modal de ajuda contextual para cada tela
 */
export const HelpModal: React.FC<HelpModalProps> = ({ opened, onClose }) => {
  const location = useLocation();
  
  // Obter conteúdo baseado na rota atual
  const currentPath = location.pathname;
  const helpContent = HELP_CONTENT[currentPath as keyof typeof HELP_CONTENT] || {
    title: 'Ajuda - CoreApp Padaria',
    icon: <IconHelp size="1.5rem" />,
    description: 'Sistema de gestão para padarias',
    features: [
      'Navegue pelo menu lateral ou use F2-F12',
      'Use Ctrl+F para buscar em qualquer tela',
      'Pressione F1 para ajuda contextual',
    ],
    shortcuts: [
      { key: 'F1', action: 'Ajuda contextual' },
      { key: 'F10', action: 'Dashboard' },
      { key: 'Esc', action: 'Fechar modais' },
    ],
    tips: [
      'Use atalhos de teclado para maior produtividade',
      'Mantenha dados sempre atualizados',
      'Configure o sistema conforme seu negócio',
    ],
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={
        <Group gap="sm">
          {helpContent.icon}
          <Text size="lg" fw={600}>
            {helpContent.title}
          </Text>
          <Badge color="blue" variant="light" size="sm">
            Pressione F1
          </Badge>
        </Group>
      }
      size="lg"
      centered
      scrollAreaComponent={ScrollArea.Autosize}
      className="padaria-modal-help"
    >
      <Stack gap="md">
        {/* Descrição */}
        <Text c="dimmed" size="sm">
          {helpContent.description}
        </Text>

        <Divider />

        {/* Recursos Principais */}
        <div>
          <Group gap="sm" mb="sm">
            <IconBulb size="1rem" color="orange" />
            <Text fw={600} size="sm">
              Recursos Principais
            </Text>
          </Group>
          <List
            spacing="xs"
            size="sm"
            icon={<Text c="blue" size="xs">•</Text>}
          >
            {helpContent.features.map((feature, index) => (
              <List.Item key={index}>{feature}</List.Item>
            ))}
          </List>
        </div>

        {/* Atalhos de Teclado */}
        <div>
          <Group gap="sm" mb="sm">
            <IconKeyboard size="1rem" color="indigo" />
            <Text fw={600} size="sm">
              Atalhos de Teclado
            </Text>
          </Group>
          <Stack gap="xs">
            {helpContent.shortcuts.map((shortcut, index) => (
              <Group key={index} justify="space-between">
                <Text size="sm">{shortcut.action}</Text>
                <Badge 
                  variant="light" 
                  color="gray" 
                  size="sm"
                  ff="monospace"
                  className="padaria-navigation-shortcut"
                >
                  {shortcut.key}
                </Badge>
              </Group>
            ))}
          </Stack>
        </div>

        {/* Dicas Importantes */}
        <div>
          <Group gap="sm" mb="sm">
            <IconSearch size="1rem" color="green" />
            <Text fw={600} size="sm">
              Dicas de Produtividade
            </Text>
          </Group>
          <Stack gap="xs">
            {helpContent.tips.map((tip, index) => (
              <Card key={index} p="xs" bg="gray.0" radius="sm">
                <Text size="sm" c="dark">
                  💡 {tip}
                </Text>
              </Card>
            ))}
          </Stack>
        </div>

        {/* Atalhos Globais */}
        <Divider />
        <div>
          <Text fw={600} size="sm" mb="sm">
            Navegação Rápida (Sempre Disponível)
          </Text>
          <div className="padaria-grid-2" style={{ gap: '0.5rem' }}>
            <Badge variant="light" size="sm">F2 - Vendas</Badge>
            <Badge variant="light" size="sm">F3 - Clientes</Badge>
            <Badge variant="light" size="sm">F4 - Produtos</Badge>
            <Badge variant="light" size="sm">F5 - Estoque</Badge>
            <Badge variant="light" size="sm">F9 - Configurações</Badge>
            <Badge variant="light" size="sm">F10 - Dashboard</Badge>
          </div>
        </div>

        {/* Rodapé */}
        <Card p="sm" bg="blue.0" radius="sm">
          <Text size="xs" c="blue.7" ta="center">
            <strong>💡 Dica Pro:</strong> Use <strong>Ctrl+F</strong> em qualquer tela para buscar rapidamente
          </Text>
        </Card>
      </Stack>
    </Modal>
  );
};