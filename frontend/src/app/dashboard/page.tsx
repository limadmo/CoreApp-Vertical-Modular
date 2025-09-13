/**
 * Página de Dashboard - CoreApp
 * Mostra módulos disponíveis por perfil + navegação por teclas F
 */

'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Container,
  Title,
  Text,
  Card,
  Group,
  Badge,
  Stack,
  Grid,
  Button
} from '@mantine/core';
import {
  IconCash,
  IconUsers,
  IconPackage,
  IconTruck,
  IconBuilding
} from '@tabler/icons-react';
import { useAuthStatus } from '@/stores/useAuth';
import { usePermissions } from '@/hooks/usePermissions';
import { MODULE_NAMES } from '@/types/auth';

const MODULE_ICONS = {
  F2: IconCash,      // Vendas
  F3: IconUsers,     // Clientes
  F4: IconPackage,   // Produtos
  F5: IconTruck,     // Estoque
  F6: IconBuilding   // Fornecedores
};

export default function DashboardPage() {
  const router = useRouter();
  const { isAuthenticated, user, isLoading } = useAuthStatus();
  const { getAllModulesWithStatus, hasModuleAccess } = usePermissions();

  // Redirecionar para login se não autenticado
  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading) {
    return (
      <Container>
        <Text>Carregando...</Text>
      </Container>
    );
  }

  if (!isAuthenticated || !user) {
    return null; // Redirecionando...
  }

  const modulesWithStatus = getAllModulesWithStatus();

  return (
    <Container size="lg" py="xl">
      <Stack spacing="xl">
        {/* Cabeçalho */}
        <div>
          <Title order={1} size="h2" mb="sm">
            Bem-vindo, {user.nome}!
          </Title>
          <Group spacing="md">
            <Badge color="blue" size="lg">
              {user.perfil.replace('_', ' ')}
            </Badge>
            <Text size="sm" c="dimmed">
              Tenant: {user.tenantId}
            </Text>
          </Group>
        </div>

        {/* Módulos Disponíveis */}
        <div>
          <Title order={2} size="h3" mb="md">
            Módulos do Sistema
          </Title>
          <Text size="sm" c="dimmed" mb="lg">
            Use as teclas F2-F6 para navegar rapidamente entre os módulos
          </Text>

          <Grid>
            {modulesWithStatus.map(({ key, name, accessible }) => {
              const IconComponent = MODULE_ICONS[key];

              return (
                <Grid.Col key={key} span={{ base: 12, sm: 6, md: 4 }}>
                  <Card
                    shadow="sm"
                    padding="lg"
                    radius="md"
                    withBorder
                    style={{
                      opacity: accessible ? 1 : 0.5,
                      cursor: accessible ? 'pointer' : 'not-allowed'
                    }}
                  >
                    <Group spacing="sm" mb="md">
                      <IconComponent
                        size={24}
                        color={accessible ? 'var(--mantine-color-blue-6)' : 'gray'}
                      />
                      <Text fw={500} size="lg">
                        {name}
                      </Text>
                    </Group>

                    <Text size="sm" c="dimmed" mb="md">
                      Pressione {key} para acessar
                    </Text>

                    <Badge
                      color={accessible ? 'green' : 'gray'}
                      size="sm"
                    >
                      {accessible ? 'Disponível' : 'Sem Acesso'}
                    </Badge>

                    {accessible && (
                      <Button
                        variant="light"
                        fullWidth
                        mt="md"
                        onClick={() => {
                          console.log(`Navegando para módulo: ${name} (${key})`);
                          // TODO: Implementar navegação real
                        }}
                      >
                        Acessar {name}
                      </Button>
                    )}
                  </Card>
                </Grid.Col>
              );
            })}
          </Grid>
        </div>

        {/* Instruções de Teclado */}
        <Card withBorder padding="lg">
          <Title order={3} size="h4" mb="md">
            Atalhos de Teclado
          </Title>
          <Stack spacing="sm">
            <Group>
              <Badge variant="light" color="blue">F1</Badge>
              <Text size="sm">Tutorial contextual da tela atual</Text>
            </Group>
            {modulesWithStatus
              .filter(m => m.accessible)
              .map(({ key, name }) => (
                <Group key={key}>
                  <Badge variant="light" color="green">{key}</Badge>
                  <Text size="sm">Navegar para {name}</Text>
                </Group>
              ))}
          </Stack>
        </Card>

        {/* Debug Info (desenvolvimento) */}
        {process.env.NODE_ENV === 'development' && (
          <Card withBorder padding="lg" style={{ backgroundColor: '#f8f9fa' }}>
            <Title order={4} size="h5" mb="md">
              Debug Info (Development)
            </Title>
            <Stack spacing="xs">
              <Text size="xs">Login: {user.login}</Text>
              <Text size="xs">Perfil: {user.perfil}</Text>
              <Text size="xs">Módulos: {user.modulos?.join(', ')}</Text>
              <Text size="xs">Permissões: {user.permissions?.join(', ')}</Text>
            </Stack>
          </Card>
        )}
      </Stack>
    </Container>
  );
}