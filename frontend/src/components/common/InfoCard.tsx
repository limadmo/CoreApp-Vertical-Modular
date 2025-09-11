/**
 * Card informativo reutilizável para dashboard
 * Implementa acessibilidade e design responsivo
 */
import React from 'react';
import {
  Card,
  Group,
  Text,
  ThemeIcon,
  Stack,
  Badge,
  ActionIcon,
  Tooltip,
} from '@mantine/core';
import { IconTrendingUp, IconTrendingDown, IconMinus, IconInfoCircle } from '@tabler/icons-react';

interface TrendData {
  /** Valor da variação */
  value: number;
  /** Se a tendência é positiva */
  isPositive: boolean;
  /** Período de comparação */
  period?: string;
}

interface InfoCardProps {
  /** Título do card */
  title: string;
  /** Valor principal */
  value: string | number;
  /** Descrição adicional */
  description?: string;
  /** Ícone do card */
  icon: React.ReactNode;
  /** Cor do tema */
  color?: string;
  /** Dados de tendência */
  trend?: TrendData;
  /** Se o card é clicável */
  onClick?: () => void;
  /** Loading state */
  isLoading?: boolean;
  /** Tooltip com mais informações */
  tooltip?: string;
  /** Formato do valor (currency, percentage, number) */
  valueFormat?: 'currency' | 'percentage' | 'number';
  /** Se deve destacar o card */
  highlighted?: boolean;
  /** Conteúdo adicional no rodapé */
  footer?: React.ReactNode;
}

/**
 * Formatar valor baseado no tipo
 */
const formatValue = (value: string | number, format?: 'currency' | 'percentage' | 'number'): string => {
  if (typeof value === 'string') return value;

  switch (format) {
    case 'currency':
      return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL',
      }).format(value);
    case 'percentage':
      return `${value}%`;
    case 'number':
      return new Intl.NumberFormat('pt-BR').format(value);
    default:
      return value.toString();
  }
};

/**
 * Card informativo acessível e responsivo
 * Ideal para dashboards e métricas
 * 
 * @example
 * ```tsx
 * <InfoCard
 *   title="Vendas Hoje"
 *   value={1250.50}
 *   valueFormat="currency"
 *   icon={<IconCash />}
 *   color="green"
 *   trend={{
 *     value: 15,
 *     isPositive: true,
 *     period: "vs ontem"
 *   }}
 *   onClick={() => navigate('/vendas')}
 * />
 * ```
 */
export const InfoCard: React.FC<InfoCardProps> = ({
  title,
  value,
  description,
  icon,
  color = 'blue',
  trend,
  onClick,
  isLoading = false,
  tooltip,
  valueFormat,
  highlighted = false,
  footer,
}) => {
  const formattedValue = formatValue(value, valueFormat);
  const isClickable = Boolean(onClick);

  const cardContent = (
    <Card
      shadow={highlighted ? 'md' : 'sm'}
      padding="lg"
      radius="md"
      withBorder
      className={`
        h-full transition-all duration-200
        ${isClickable ? 'cursor-pointer hover:shadow-md hover:-translate-y-1' : ''}
        ${highlighted ? 'ring-2 ring-blue-200 bg-blue-50' : 'hover:shadow-sm'}
        ${isLoading ? 'animate-pulse' : ''}
      `}
      onClick={onClick}
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      onKeyDown={
        isClickable
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                onClick?.();
              }
            }
          : undefined
      }
      aria-label={
        isClickable
          ? `${title}: ${formattedValue}${trend ? `, tendência ${trend.isPositive ? 'positiva' : 'negativa'}` : ''}. Clique para mais detalhes.`
          : undefined
      }
    >
      <Stack gap="md">
        {/* Header */}
        <Group justify="space-between" align="flex-start">
          <div className="flex-1">
            <Group gap="xs" align="center" mb="xs">
              <Text size="sm" c="dimmed" fw={500}>
                {title}
              </Text>
              {tooltip && (
                <Tooltip label={tooltip} position="top" withArrow>
                  <ActionIcon variant="transparent" size="sm">
                    <IconInfoCircle size={14} />
                  </ActionIcon>
                </Tooltip>
              )}
            </Group>

            {/* Valor Principal */}
            <Text fw={700} size="xl" className="leading-none">
              {isLoading ? '---' : formattedValue}
            </Text>

            {/* Descrição */}
            {description && (
              <Text size="sm" c="dimmed" mt="xs">
                {description}
              </Text>
            )}
          </div>

          {/* Ícone */}
          <ThemeIcon
            color={color}
            variant="light"
            size="xl"
            radius="md"
            className="flex-shrink-0"
          >
            {icon}
          </ThemeIcon>
        </Group>

        {/* Trend */}
        {trend && !isLoading && (
          <Group justify="space-between" align="center">
            <Group gap="xs">
              <ThemeIcon
                color={trend.isPositive ? 'green' : 'red'}
                variant="light"
                size="sm"
                radius="xl"
              >
                {trend.value === 0 ? (
                  <IconMinus size={12} />
                ) : trend.isPositive ? (
                  <IconTrendingUp size={12} />
                ) : (
                  <IconTrendingDown size={12} />
                )}
              </ThemeIcon>

              <Text
                size="sm"
                fw={500}
                c={
                  trend.value === 0
                    ? 'gray'
                    : trend.isPositive
                    ? 'green'
                    : 'red'
                }
              >
                {trend.isPositive && trend.value > 0 ? '+' : ''}
                {trend.value}%
              </Text>
            </Group>

            {trend.period && (
              <Text size="xs" c="dimmed">
                {trend.period}
              </Text>
            )}
          </Group>
        )}

        {/* Footer */}
        {footer && (
          <div className="pt-2 border-t border-gray-100">
            {footer}
          </div>
        )}
      </Stack>
    </Card>
  );

  return cardContent;
};

/**
 * Variante compacta do InfoCard
 */
export const CompactInfoCard: React.FC<Omit<InfoCardProps, 'description' | 'footer'>> = (
  props
) => {
  return (
    <InfoCard 
      {...props}
      description={undefined}
      footer={undefined}
    />
  );
};

/**
 * InfoCard específico para métricas de vendas
 */
export const SalesInfoCard: React.FC<Omit<InfoCardProps, 'valueFormat' | 'color'>> = (
  props
) => {
  return (
    <InfoCard
      {...props}
      valueFormat="currency"
      color="green"
    />
  );
};

/**
 * InfoCard específico para métricas percentuais
 */
export const PercentageInfoCard: React.FC<Omit<InfoCardProps, 'valueFormat'>> = (
  props
) => {
  return (
    <InfoCard
      {...props}
      valueFormat="percentage"
    />
  );
};