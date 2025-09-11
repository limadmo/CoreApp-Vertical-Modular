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
  const { currentTenant, availableModules } = useTenant();
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
            
            <Group gap="sm">
              <div className="w-8 h-8 bg-brand-500 rounded-lg flex items-center justify-center">
                <Text size="lg" fw={700} c="white">
                  C
                </Text>
              </div>
              <div>
                <Text size="lg" fw={600} c="gray.9">
                  {currentTenant?.nome || 'CoreApp'}
                </Text>
                <Text size="xs" c="gray.6">
                  {currentTenant?.verticalType}
                </Text>
              </div>
            </Group>
          </Group>

          {/* User Menu */}
          <Menu shadow="md" width={200}>
            <Menu.Target>
              <ActionIcon
                variant="light"
                size="lg"
                aria-label="Menu do usuário"
                className="hover:bg-gray-100"
              >
                <Avatar size="sm" color="brand">
                  <IconUser size={16} />
                </Avatar>
              </ActionIcon>
            </Menu.Target>

            <Menu.Dropdown>
              <Menu.Label>Usuário</Menu.Label>
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
                Ajuda
              </Menu.Item>
              
              <Menu.Divider />
              
              <Menu.Item
                color="red"
                leftSection={<IconLogout size={14} />}
                onClick={() => {
                  // TODO: Implementar logout
                  console.log('Logout clicked');
                }}
              >
                Sair
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </AppShell.Header>

      {/* Sidebar Navigation */}
      <AppShell.Navbar p="md">
        <ScrollArea>
          <Stack gap="xs">
            {/* Dashboard */}
            <NavLink
              href="#"
              label="Dashboard"
              description="Visão geral"
              leftSection={<IconHome size="1rem" />}
              rightSection={
                <kbd className="px-2 py-1 bg-gray-100 text-xs rounded">
                  F10
                </kbd>
              }
              active={isActive('/')}
              onClick={(e) => {
                e.preventDefault();
                handleNavigation('/');
              }}
              className="hover:bg-gray-50 rounded-md"
            />

            <Divider label="Módulos Ativos" labelPosition="center" my="sm" />

            {/* Módulos Disponíveis */}
            {availableModules
              .filter(module => module.isActive)
              .sort((a, b) => a.order - b.order)
              .map((module) => (
                <NavLink
                  key={module.code}
                  href="#"
                  label={module.name}
                  description={module.description}
                  leftSection={
                    <div className="w-6 h-6 bg-brand-100 rounded flex items-center justify-center">
                      <Text size="xs" fw={600} c="brand.7">
                        {module.code.charAt(0)}
                      </Text>
                    </div>
                  }
                  rightSection={
                    <Badge
                      size="xs"
                      variant="light"
                      color="gray"
                      className="font-mono"
                    >
                      {module.shortcut}
                    </Badge>
                  }
                  active={isActive(module.path)}
                  onClick={(e) => {
                    e.preventDefault();
                    handleNavigation(module.path);
                  }}
                  className="hover:bg-gray-50 rounded-md"
                />
              ))}

            <Divider label="Sistema" labelPosition="center" my="sm" />

            {/* Configurações */}
            <NavLink
              href="#"
              label="Configurações"
              description="Preferências do sistema"
              leftSection={<IconSettings size="1rem" />}
              rightSection={
                <kbd className="px-2 py-1 bg-gray-100 text-xs rounded">
                  F9
                </kbd>
              }
              active={isActive('/configuracoes')}
              onClick={(e) => {
                e.preventDefault();
                handleNavigation('/configuracoes');
              }}
              className="hover:bg-gray-50 rounded-md"
            />

            {/* Ajuda */}
            <NavLink
              href="#"
              label="Ajuda"
              description="Suporte e documentação"
              leftSection={<IconHelp size="1rem" />}
              rightSection={
                <kbd className="px-2 py-1 bg-gray-100 text-xs rounded">
                  F12
                </kbd>
              }
              active={isActive('/ajuda')}
              onClick={(e) => {
                e.preventDefault();
                handleNavigation('/ajuda');
              }}
              className="hover:bg-gray-50 rounded-md"
            />
          </Stack>

          {/* Tenant Info */}
          <div className="mt-auto pt-6">
            <div className="p-3 bg-gray-50 rounded-lg">
              <Text size="xs" fw={600} c="gray.7" mb="xs">
                Tenant Atual
              </Text>
              <Text size="xs" c="gray.6">
                ID: {currentTenant?.id}
              </Text>
              <Text size="xs" c="gray.6">
                Vertical: {currentTenant?.verticalType}
              </Text>
              <Badge
                size="xs"
                color={currentTenant?.status === 'ACTIVE' ? 'green' : 'gray'}
                mt="xs"
              >
                {currentTenant?.status}
              </Badge>
            </div>
          </div>
        </ScrollArea>
      </AppShell.Navbar>

      {/* Main Content */}
      <AppShell.Main>
        <div className="min-h-full">
          {children}
        </div>
      </AppShell.Main>
    </AppShell>
  );
};