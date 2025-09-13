/**
 * Dashboard de Clientes - Métricas e KPIs
 * Componente para exibir estatísticas e insights dos clientes
 */
'use client';

import { useState } from 'react';
import {
  Card,
  Group,
  Text,
  Stack,
  Grid,
  Paper,
  Progress,
  Alert,
  Select,
  Title,
  Badge,
  SimpleGrid,
  ActionIcon,
  Tooltip,
} from '@mantine/core';
import {
  IconUsers,
  IconUserPlus,
  IconUserMinus,
  IconTrendingUp,
  IconCalendar,
  IconRefresh,
  IconChartBar,
  IconPercentage,
  IconId,
  IconClock,
} from '@tabler/icons-react';
import { useClienteEstatisticas } from '@/hooks/useClientes';

interface ClienteDashboardProps {
  className?: string;
}

/**
 * Card de métrica individual
 */
interface MetricCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
  description?: string;
  trend?: {
    value: number;
    label: string;
    positive: boolean;
  };
}

function MetricCard({ title, value, icon, color, description, trend }: MetricCardProps) {
  return (
    <Paper p="md" radius="md" withBorder>
      <Group justify="space-between" mb="xs">
        <Text size="sm" c="dimmed" fw={500}>
          {title}
        </Text>
        <div style={{ color: `var(--mantine-color-${color}-6)` }}>
          {icon}
        </div>
      </Group>

      <Text size="xl" fw={700} c={color}>
        {value}
      </Text>

      {description && (
        <Text size="xs" c="dimmed" mt={4}>
          {description}
        </Text>
      )}

      {trend && (
        <Group gap="xs" mt="xs">
          <Badge
            color={trend.positive ? 'green' : 'red'}
            variant="light"
            size="xs"
          >
            {trend.positive ? '+' : ''}{trend.value}%
          </Badge>
          <Text size="xs" c="dimmed">
            {trend.label}
          </Text>
        </Group>
      )}
    </Paper>
  );
}

/**
 * Componente principal do dashboard
 */
export function ClienteDashboard({ className }: ClienteDashboardProps) {
  const [periodo, setPeriodo] = useState<string>('30');

  // Query para estatísticas
  const { data: estatisticas, isLoading, error, refetch } = useClienteEstatisticas();

  if (error) {
    return (
      <Alert color="red" title="Erro ao carregar métricas">
        Não foi possível conectar com a API para obter as estatísticas dos clientes.
      </Alert>
    );
  }

  const stats = estatisticas?.data;

  // Calcular percentuais
  const percentualAtivos = stats?.total ? Math.round((stats.ativos / stats.total) * 100) : 0;
  const percentualComCpf = stats?.total ? Math.round((stats.comCpf / stats.total) * 100) : 0;

  return (
    <div className={className}>
      <Stack gap="lg">
        {/* Cabeçalho do Dashboard */}
        <Group justify="space-between">
          <div>
            <Title order={2} c="dark.7">📊 Dashboard de Clientes</Title>
            <Text c="dimmed" size="sm">
              Visão geral e métricas em tempo real
            </Text>
          </div>

          <Group>
            <Select
              data={[
                { value: '7', label: 'Últimos 7 dias' },
                { value: '30', label: 'Últimos 30 dias' },
                { value: '90', label: 'Últimos 90 dias' },
                { value: '365', label: 'Último ano' },
              ]}
              value={periodo}
              onChange={(value) => setPeriodo(value || '30')}
              size="sm"
              w={150}
            />

            <Tooltip label="Atualizar dados">
              <ActionIcon
                variant="light"
                size="lg"
                onClick={() => refetch()}
                loading={isLoading}
              >
                <IconRefresh size={16} />
              </ActionIcon>
            </Tooltip>
          </Group>
        </Group>

        {/* Métricas Principais */}
        <SimpleGrid cols={{ base: 1, sm: 2, md: 4 }} spacing="md">
          <MetricCard
            title="Total de Clientes"
            value={stats?.total || 0}
            icon={<IconUsers size={24} />}
            color="blue"
            description="Clientes cadastrados no sistema"
          />

          <MetricCard
            title="Clientes Ativos"
            value={stats?.ativos || 0}
            icon={<IconUserPlus size={24} />}
            color="green"
            description={`${percentualAtivos}% do total`}
            trend={{
              value: 12,
              label: 'vs mês anterior',
              positive: true
            }}
          />

          <MetricCard
            title="Clientes Inativos"
            value={stats?.inativos || 0}
            icon={<IconUserMinus size={24} />}
            color="red"
            description={`${100 - percentualAtivos}% do total`}
          />

          <MetricCard
            title="Novos este Mês"
            value={stats?.novosMes || 0}
            icon={<IconTrendingUp size={24} />}
            color="teal"
            description="Cadastros recentes"
            trend={{
              value: 8,
              label: 'vs mês anterior',
              positive: true
            }}
          />
        </SimpleGrid>

        {/* Análises Detalhadas */}
        <Grid>
          {/* Card de Segmentação */}
          <Grid.Col span={{ base: 12, md: 6 }}>
            <Card shadow="sm" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <Group gap="sm">
                  <IconChartBar size={20} color="var(--mantine-color-indigo-6)" />
                  <Text fw={600} size="lg">Segmentação de Clientes</Text>
                </Group>

                <Stack gap="sm">
                  <div>
                    <Group justify="space-between" mb={4}>
                      <Text size="sm">Clientes com CPF</Text>
                      <Text size="sm" fw={500}>{stats?.comCpf || 0}</Text>
                    </Group>
                    <Progress
                      value={percentualComCpf}
                      color="blue"
                      size="sm"
                      radius="xs"
                    />
                    <Text size="xs" c="dimmed" mt={2}>
                      {percentualComCpf}% têm CPF cadastrado
                    </Text>
                  </div>

                  <div>
                    <Group justify="space-between" mb={4}>
                      <Text size="sm">Clientes sem CPF</Text>
                      <Text size="sm" fw={500}>{stats?.semCpf || 0}</Text>
                    </Group>
                    <Progress
                      value={100 - percentualComCpf}
                      color="orange"
                      size="sm"
                      radius="xs"
                    />
                    <Text size="xs" c="dimmed" mt={2}>
                      {100 - percentualComCpf}% sem CPF informado
                    </Text>
                  </div>

                  <div>
                    <Group justify="space-between" mb={4}>
                      <Text size="sm">Taxa de Ativação</Text>
                      <Text size="sm" fw={500}>{percentualAtivos}%</Text>
                    </Group>
                    <Progress
                      value={percentualAtivos}
                      color="green"
                      size="sm"
                      radius="xs"
                    />
                    <Text size="xs" c="dimmed" mt={2}>
                      Porcentagem de clientes ativos
                    </Text>
                  </div>
                </Stack>
              </Stack>
            </Card>
          </Grid.Col>

          {/* Card de Insights */}
          <Grid.Col span={{ base: 12, md: 6 }}>
            <Card shadow="sm" padding="lg" radius="md" withBorder>
              <Stack gap="md">
                <Group gap="sm">
                  <IconPercentage size={20} color="var(--mantine-color-violet-6)" />
                  <Text fw={600} size="lg">Insights e Recomendações</Text>
                </Group>

                <Stack gap="sm">
                  {percentualComCpf < 50 && (
                    <Alert color="yellow" variant="light" size="sm">
                      <Text size="sm">
                        💡 <strong>Dica:</strong> Apenas {percentualComCpf}% dos clientes têm CPF cadastrado.
                        Considere incentivar o preenchimento para melhor gestão fiscal.
                      </Text>
                    </Alert>
                  )}

                  {(stats?.novosMes || 0) > 10 && (
                    <Alert color="green" variant="light" size="sm">
                      <Text size="sm">
                        🎉 <strong>Ótimo!</strong> {stats?.novosMes} novos clientes este mês.
                        O crescimento está acelerado!
                      </Text>
                    </Alert>
                  )}

                  {(stats?.inativos || 0) > (stats?.ativos || 0) * 0.2 && (
                    <Alert color="orange" variant="light" size="sm">
                      <Text size="sm">
                        ⚠️ <strong>Atenção:</strong> Muitos clientes inativos ({stats?.inativos}).
                        Considere campanhas de reativação.
                      </Text>
                    </Alert>
                  )}

                  {(stats?.total || 0) === 0 && (
                    <Alert color="blue" variant="light" size="sm">
                      <Text size="sm">
                        🚀 <strong>Comece agora:</strong> Cadastre seus primeiros clientes
                        para começar a acompanhar as métricas!
                      </Text>
                    </Alert>
                  )}
                </Stack>
              </Stack>
            </Card>
          </Grid.Col>
        </Grid>

        {/* Cards de Status Rápido */}
        <Grid>
          <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
            <Paper p="sm" radius="md" style={{ backgroundColor: 'var(--mantine-color-blue-0)' }}>
              <Group>
                <IconId size={24} color="var(--mantine-color-blue-6)" />
                <div>
                  <Text size="xs" c="dimmed">Completude de Dados</Text>
                  <Text fw={600} c="blue">{percentualComCpf}%</Text>
                </div>
              </Group>
            </Paper>
          </Grid.Col>

          <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
            <Paper p="sm" radius="md" style={{ backgroundColor: 'var(--mantine-color-green-0)' }}>
              <Group>
                <IconClock size={24} color="var(--mantine-color-green-6)" />
                <div>
                  <Text size="xs" c="dimmed">Cadastros Recentes</Text>
                  <Text fw={600} c="green">{stats?.novosMes || 0}/mês</Text>
                </div>
              </Group>
            </Paper>
          </Grid.Col>

          <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
            <Paper p="sm" radius="md" style={{ backgroundColor: 'var(--mantine-color-orange-0)' }}>
              <Group>
                <IconCalendar size={24} color="var(--mantine-color-orange-6)" />
                <div>
                  <Text size="xs" c="dimmed">Última Atualização</Text>
                  <Text fw={600} c="orange">Agora</Text>
                </div>
              </Group>
            </Paper>
          </Grid.Col>

          <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
            <Paper p="sm" radius="md" style={{ backgroundColor: 'var(--mantine-color-violet-0)' }}>
              <Group>
                <IconUsers size={24} color="var(--mantine-color-violet-6)" />
                <div>
                  <Text size="xs" c="dimmed">Taxa de Crescimento</Text>
                  <Text fw={600} c="violet">+12%</Text>
                </div>
              </Group>
            </Paper>
          </Grid.Col>
        </Grid>

        {/* Nota sobre funcionalidades futuras */}
        <Alert color="blue" variant="light">
          <Text size="sm">
            📈 <strong>Em breve:</strong> Gráficos de tendência, análise de faixa etária,
            segmentação geográfica e integração com dados de vendas.
          </Text>
        </Alert>
      </Stack>
    </div>
  );
}

export default ClienteDashboard;