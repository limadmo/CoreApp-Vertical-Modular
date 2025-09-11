/**
 * Layout principal da aplicação multi-tenant
 * Inclui sidebar, header, breadcrumbs e área de conteúdo
 */
import React, { useState } from 'react';
import {
  AppShell,
  Burger,
  Group,
  Text,
  NavLink,
  ScrollArea,
  Badge,
  Avatar,
  Menu,
  ActionIcon,
  Divider,
  Stack,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconHome, IconSettings, IconHelp, IconLogout, IconUser } from '@tabler/icons-react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTenant } from '@hooks/useTenant';
import { NavigationItem } from '../navigation';
import { useKeyboardShortcuts } from '../../hooks/useKeyboardShortcuts';
import { HelpModal } from '../help/HelpModal';
import { ConfigurableUserAvatar } from '../user/ConfigurableUserAvatar';
import { useCurrentUser } from '@hooks/useCurrentUser';

interface AppLayoutProps {
  children: React.ReactNode;
}

/**
 * Layout principal da aplicação CoreApp SAAS
 * Implementa navegação acessível e responsiva
 * 
 * @param children - Conteúdo da página atual
 */
export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const [opened, { toggle }] = useDisclosure();
  const [helpModalAberto, { open: abrirHelpModal, close: fecharHelpModal }] = useDisclosure(false);
  const { currentTenant, availableModules } = useTenant();
  const { currentUser, isLoading: isLoadingUser } = useCurrentUser();
  const navigate = useNavigate();
  const location = useLocation();


  /**
   * Verifica se a rota atual está ativa
   */
  const isActive = (path: string): boolean => {
    return location.pathname === path;
  };

  /**
   * Navega para uma rota e fecha sidebar em mobile
   */
  const handleNavigation = (path: string): void => {
    navigate(path);
    if (opened) {
      toggle(); // Fechar sidebar em mobile após navegação
    }
  };

  /**
   * Handler para sistema de ajuda F1
   */
  const handleHelpModal = (): void => {
    abrirHelpModal();
  };

  /**
   * Handler para busca Ctrl+F - fallback global
   */
  const handleSearchFocus = (): void => {
    // Fallback: tentar focar em qualquer campo de busca na página
    const searchInput = document.querySelector('input[type="search"], input[placeholder*="buscar"], input[placeholder*="pesquisar"]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  };

  // Ativar sistema de atalhos globais
  useKeyboardShortcuts({
    onHelp: handleHelpModal,
    onSearch: handleSearchFocus,
  });

  return (
    <AppShell
      header={{ height: 70 }}
      navbar={{
        width: 300,
        breakpoint: 'sm',
        collapsed: { mobile: !opened },
      }}
      padding="md"
    >
      {/* Header */}
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group>
            <Burger
              opened={opened}
              onClick={toggle}
              hiddenFrom="sm"
              size="sm"
              aria-label="Abrir navegação"
            />
            
            <div>
              <Text size="lg" fw={600} c="gray.9">
                {currentTenant?.nome || 'CoreApp'}
              </Text>
              <Text size="xs" c="gray.6">
                {currentTenant?.verticalType}
              </Text>
            </div>
          </Group>

          {/* Informações do Funcionário + Menu */}
          <Group gap="md">
            {/* Informações básicas do funcionário logado */}
            <div className="text-right">
              <Text size="sm" fw={600} c="gray.9" className="leading-tight">
                {currentUser?.name?.split(' ').slice(0, 2).join(' ') || 'João Silva'} {/* Nome + Sobrenome */}
              </Text>
              <Text size="xs" c="gray.6" className="leading-tight">
                {currentUser?.login?.toLowerCase() || 'js01234a'} • {currentUser?.role || 'Gerente'}
              </Text>
            </div>
            
            {/* Avatar do Funcionário com Menu */}
            <Menu shadow="md" width={220}>
              <Menu.Target>
                <div style={{ cursor: 'pointer' }}>
                  <ConfigurableUserAvatar
                    userName={currentUser?.name || 'João Silva Santos'}
                    userLogin={currentUser?.login || 'JS01234A'}
                    userRole={currentUser?.role || 'Gerente Geral'}
                    photoUrl={currentUser?.photoUrl}
                    isOnline={currentUser?.isOnline ?? true}
                    size="md"
                  />
                </div>
              </Menu.Target>

              <Menu.Dropdown>
                <Menu.Label>
                  {currentUser?.name || 'Usuário'}
                </Menu.Label>
                <Menu.Item 
                  leftSection={<IconUser size={14} />}
                  onClick={() => handleNavigation('/perfil')}
                >
                  Meu Perfil
                </Menu.Item>
                <Menu.Item 
                  leftSection={<IconSettings size={14} />}
                  onClick={() => handleNavigation('/configuracoes')}
                >
                  Configurações
                </Menu.Item>
                
                <Menu.Divider />
                
                <Menu.Item 
                  leftSection={<IconHelp size={14} />}
                  onClick={() => handleNavigation('/ajuda')}
                >
                  Ajuda & Suporte
                </Menu.Item>
                
                <Menu.Divider />
                
                <Menu.Item
                  color="red"
                  leftSection={<IconLogout size={14} />}
                  onClick={() => {
                    // TODO: Implementar logout real
                    console.log('Logout do usuário:', currentUser?.login);
                  }}
                >
                  Sair do Sistema
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Group>
        </Group>
      </AppShell.Header>

      {/* Sidebar Navigation */}
      <AppShell.Navbar p="md">
        <div className="padaria-navigation-container">
          <Stack gap="xs">
            <Divider label="Módulos F1-F12" labelPosition="center" my="xs" />

            {/* Layout Dinâmico - Apenas módulos ativos/contratados */}
            <div className="padaria-f-keys-grid">
              {availableModules
                .filter(module => module.isActive)
                .sort((a, b) => a.order - b.order)
                .map((module) => {
                  // Apenas o nome do módulo sem letra duplicada
                  const getCompactLabel = (name: string, code: string) => {
                    const nomeSimples = name.split(' ')[0]; // Primeira palavra apenas
                    return nomeSimples; // Só "Vendas", "Clientes", etc.
                  };

                  // Tratamento especial para módulos específicos
                  const handleClick = () => {
                    if (module.code === 'HELP') {
                      handleHelpModal(); // F11 = Modal de ajuda
                    } else {
                      handleNavigation(module.path);
                    }
                  };

                  return (
                    <NavigationItem
                      key={module.code}
                      icon={
                        <div className="w-12 h-12 bg-brand-100 rounded flex items-center justify-center">
                          <Text size="28px" fw={700} c="brand.7">
                            {module.name.charAt(0)}
                          </Text>
                        </div>
                      }
                      label={getCompactLabel(module.name, module.code)}
                      description=""
                      shortcut={module.shortcut}
                      isActive={isActive(module.path)}
                      onClick={handleClick}
                    />
                  );
                })}
            </div>

          </Stack>

          {/* Tenant Info Expandido */}
          <div style={{ position: 'absolute', bottom: '12px', left: '12px', right: '12px' }}>
            <div className="p-3 bg-brand-50 rounded-md border border-brand-100">
              <Text size="11px" fw={700} c="brand.7" className="mb-1">
                {currentTenant?.nome}
              </Text>
              <Text size="9px" fw={500} c="gray.6">
                {currentTenant?.verticalType} • {currentTenant?.status}
              </Text>
              <Text size="8px" fw={400} c="gray.5" className="mt-1">
                ID: {currentTenant?.id?.slice(-8).toUpperCase()}
              </Text>
            </div>
          </div>
        </div>
      </AppShell.Navbar>

      {/* Main Content */}
      <AppShell.Main>
        <div className="min-h-full">
          {children}
        </div>
      </AppShell.Main>

      {/* Modal de Ajuda F1 Global */}
      <HelpModal
        opened={helpModalAberto}
        onClose={fecharHelpModal}
      />
    </AppShell>
  );
};