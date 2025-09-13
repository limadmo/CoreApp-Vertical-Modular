/**
 * Página Principal Next.js 15 - CoreApp SAAS
 * Landing page com status do sistema
 */
'use client';

import { Container, Title, Text, Button, Stack, Card, Group, Badge } from '@mantine/core';
import { IconDatabase, IconServer, IconBrandNextjs } from '@tabler/icons-react';

export default function HomePage() {
  return (
    <Container size="md" py="xl">
      <Stack gap="xl">
        <div style={{ textAlign: 'center' }}>
          <Title order={1} size="3rem" c="blue">
            🚀 CoreApp SAAS
          </Title>
          <Text size="xl" c="dimmed" mt="md">
            Sistema Multi-tenant Next.js 15 + PostgreSQL
          </Text>
        </div>

        <Card shadow="sm" padding="lg" radius="md" withBorder>
          <Stack gap="md">
            <Group justify="space-between">
              <Title order={2}>Status do Sistema</Title>
              <Badge color="green" variant="filled">
                ✅ Ativo
              </Badge>
            </Group>

            <Group gap="md">
              <IconBrandNextjs size={24} color="blue" />
              <Text>Next.js 15 com App Router + Turbopack</Text>
            </Group>

            <Group gap="md">
              <IconDatabase size={24} color="green" />
              <Text>PostgreSQL 17 com dados reais</Text>
            </Group>

            <Group gap="md">
              <IconServer size={24} color="orange" />
              <Text>API Routes integradas</Text>
            </Group>
          </Stack>
        </Card>

        <Group justify="center" gap="md">
          <Button 
            size="lg" 
            component="a" 
            href="/api/health" 
            target="_blank"
            variant="filled"
          >
            🔍 Health Check API
          </Button>
          
          <Button 
            size="lg" 
            component="a" 
            href="/clientes" 
            variant="outline"
          >
            👥 Clientes Padaria
          </Button>
        </Group>

        <Text size="sm" c="dimmed" ta="center">
          🏗️ Arquitetura: Multi-tenant + JWT + PostgreSQL + Vercel Ready
        </Text>
      </Stack>
    </Container>
  );
}
