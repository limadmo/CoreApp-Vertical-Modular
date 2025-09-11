/**
 * Componente base para todos os formulários
 * Inclui validação, loading states e acessibilidade
 */
import React, { useCallback } from 'react';
import {
  Stack,
  Group,
  Button,
  Paper,
  Title,
  Text,
  LoadingOverlay,
} from '@mantine/core';
import { useForm, UseFormReturnType } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconDeviceFloppy, IconX } from '@tabler/icons-react';

interface BaseFormProps<T = Record<string, any>> {
  /** Título do formulário */
  title?: string;
  /** Descrição do formulário */
  description?: string;
  /** Dados iniciais do formulário */
  initialData?: T;
  /** Esquema de validação */
  validate?: Record<string, (value: any) => string | null>;
  /** Callback de submit */
  onSubmit: (data: T) => Promise<void>;
  /** Callback de cancelamento */
  onCancel?: () => void;
  /** Se está carregando */
  isLoading?: boolean;
  /** Se o formulário está desabilitado */
  disabled?: boolean;
  /** Children - campos do formulário */
  children: (form: any) => React.ReactNode;
  /** Texto do botão submit */
  submitText?: string;
  /** Texto do botão cancelar */
  cancelText?: string;
  /** Se deve exibir como card */
  withCard?: boolean;
}

/**
 * Formulário base reutilizável para toda aplicação
 * Implementa padrões de UX e acessibilidade WCAG AAA
 * 
 * @example
 * ```tsx
 * <BaseForm
 *   title="Cadastro de Produto"
 *   onSubmit={async (data) => await createProduct(data)}
 *   validate={{
 *     nome: (value) => value ? null : 'Nome é obrigatório'
 *   }}
 * >
 *   {(form) => (
 *     <>
 *       <TextInput
 *         label="Nome"
 *         {...form.getInputProps('nome')}
 *       />
 *     </>
 *   )}
 * </BaseForm>
 * ```
 */
export function BaseForm<T extends Record<string, any> = Record<string, any>>({
  title,
  description,
  initialData,
  validate,
  onSubmit,
  onCancel,
  isLoading = false,
  disabled = false,
  children,
  submitText = 'Salvar',
  cancelText = 'Cancelar',
  withCard = true,
}: BaseFormProps<T>) {
  const form = useForm<T>({
    initialValues: initialData || ({} as T),
    validate: validate as any,
  });

  /**
   * Handler de submit com notificações automáticas
   */
  const handleSubmit = useCallback(async (data: T) => {
    try {
      await onSubmit(data);
      
      notifications.show({
        title: 'Sucesso',
        message: 'Dados salvos com sucesso!',
        color: 'green',
        autoClose: 3000,
      });
    } catch (error) {
      console.error('Erro no formulário:', error);
      
      notifications.show({
        title: 'Erro',
        message: error instanceof Error ? error.message : 'Erro ao salvar dados',
        color: 'red',
        autoClose: 5000,
      });
    }
  }, [onSubmit]);

  /**
   * Handler de reset/cancelamento
   */
  const handleReset = useCallback(() => {
    form.reset();
    if (onCancel) {
      onCancel();
    }
  }, [form, onCancel]);

  /**
   * Conteúdo do formulário
   */
  const formContent = (
    <div className="relative">
      <LoadingOverlay 
        visible={isLoading}
        overlayProps={{ radius: 'sm', blur: 2 }}
        loaderProps={{ color: 'brand', type: 'dots' }}
      />
      
      <form onSubmit={form.onSubmit(handleSubmit as any)}>
        <Stack gap="lg">
          {/* Header */}
          {(title || description) && (
            <div>
              {title && (
                <Title order={2} size="h3" mb="xs">
                  {title}
                </Title>
              )}
              {description && (
                <Text c="dimmed" size="sm">
                  {description}
                </Text>
              )}
            </div>
          )}

          {/* Form Fields */}
          <Stack gap="md">
            {children(form as any)}
          </Stack>

          {/* Actions */}
          <Group justify="flex-end" mt="xl">
            {onCancel && (
              <Button
                type="button"
                variant="subtle"
                color="gray"
                onClick={handleReset}
                disabled={isLoading || disabled}
                leftSection={<IconX size={16} />}
                className="min-h-11"
              >
                {cancelText} (ESC)
              </Button>
            )}
            
            <Button
              type="submit"
              loading={isLoading}
              disabled={disabled}
              leftSection={<IconDeviceFloppy size={16} />}
              className="min-h-11"
            >
              {submitText} (F9)
            </Button>
          </Group>
        </Stack>
      </form>
    </div>
  );

  if (withCard) {
    return (
      <Paper shadow="sm" radius="md" p="lg" withBorder>
        {formContent}
      </Paper>
    );
  }

  return formContent;
}

/**
 * Variante compacta do formulário para modals
 */
export function CompactForm<T extends Record<string, any> = Record<string, any>>(
  props: Omit<BaseFormProps<T>, 'withCard' | 'title' | 'description'>
) {
  return (
    <BaseForm<T>
      {...props} 
      withCard={false}
      submitText={props.submitText || 'Salvar'}
      cancelText={props.cancelText || 'Cancelar'}
    />
  );
}