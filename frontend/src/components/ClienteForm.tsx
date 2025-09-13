/**
 * Formulário de Cliente - Mantine 7 + WCAG AAA
 * Validações completas com CPF brasileiro e navegação por teclado
 */
'use client';

import { useEffect, useRef } from 'react';
import {
  TextInput,
  Button,
  Group,
  Stack,
  Text,
  Box,
  Alert,
  LoadingOverlay
} from '@mantine/core';
import { useForm, hasLength, isNotEmpty, matches } from '@mantine/form';
import { IconUser, IconId, IconCalendar, IconAlertCircle } from '@tabler/icons-react';
import { Cliente, ClienteCreateRequest, validarCpf, formatarCpf } from '@/services/clienteService';
import { useCreateCliente, useUpdateCliente } from '@/hooks/useClientes';

interface ClienteFormProps {
  cliente?: Cliente | null;
  onSuccess?: () => void;
  onCancel?: () => void;
}

/**
 * Componente de formulário com validações brasileiras
 */
export function ClienteForm({ cliente, onSuccess, onCancel }: ClienteFormProps) {
  const isEditing = !!cliente;
  const firstInputRef = useRef<HTMLInputElement>(null);

  // Mutations para criar/atualizar
  const createCliente = useCreateCliente();
  const updateCliente = useUpdateCliente();

  const isLoading = createCliente.isPending || updateCliente.isPending;

  // Configuração do formulário com validações
  const form = useForm<ClienteCreateRequest>({
    initialValues: {
      nome: cliente?.nome || '',
      sobrenome: cliente?.sobrenome || '',
      cpf: cliente?.cpf || '',
      dataNascimento: cliente?.dataNascimento?.split('T')[0] || '',
    },
    validate: {
      nome: hasLength({ min: 2, max: 50 }, 'Nome deve ter entre 2 e 50 caracteres'),
      sobrenome: hasLength({ min: 2, max: 50 }, 'Sobrenome deve ter entre 2 e 50 caracteres'),
      cpf: (value) => {
        if (!value) return null; // CPF é opcional
        const cleaned = value.replace(/\D/g, '');
        if (cleaned.length !== 11) return 'CPF deve ter 11 dígitos';
        if (!validarCpf(value)) return 'CPF inválido';
        return null;
      },
      dataNascimento: (value) => {
        if (!value) return null; // Data é opcional
        const date = new Date(value);
        const today = new Date();
        if (date > today) return 'Data de nascimento não pode ser futura';
        const age = today.getFullYear() - date.getFullYear();
        if (age > 120) return 'Data de nascimento muito antiga';
        return null;
      },
    },
  });

  // Focar primeiro campo ao montar
  useEffect(() => {
    const timer = setTimeout(() => {
      firstInputRef.current?.focus();
    }, 100);
    return () => clearTimeout(timer);
  }, []);

  // Handler para formatação automática do CPF
  const handleCpfChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    const cleaned = value.replace(/\D/g, '');

    if (cleaned.length <= 11) {
      const formatted = formatarCpf(cleaned);
      form.setFieldValue('cpf', formatted);
    }
  };

  // Handler de submissão
  const handleSubmit = async (values: ClienteCreateRequest) => {
    try {
      if (isEditing && cliente) {
        await updateCliente.mutateAsync({
          id: cliente.id,
          dados: values
        });
      } else {
        await createCliente.mutateAsync(values);
      }

      onSuccess?.();
    } catch (error) {
      // Erro já tratado nos hooks
      console.error('Erro no formulário:', error);
    }
  };

  // Navegação por teclado
  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      event.preventDefault();
      onCancel?.();
    }

    if (event.key === 'F1') {
      event.preventDefault();
      firstInputRef.current?.focus();
    }
  };

  return (
    <Box
      component="form"
      onSubmit={form.onSubmit(handleSubmit)}
      onKeyDown={handleKeyDown}
      pos="relative"
    >
      <LoadingOverlay visible={isLoading} overlayProps={{ radius: 'sm', blur: 2 }} />

      <Stack gap="md">
        {/* Cabeçalho do formulário */}
        <Group gap="xs">
          <IconUser size={20} />
          <Text size="lg" fw={600} c="dark.7">
            {isEditing ? 'Editar Cliente' : 'Novo Cliente'}
          </Text>
        </Group>

        {/* Alert de instruções de acessibilidade */}
        <Alert
          icon={<IconAlertCircle size={16} />}
          title="Navegação por Teclado"
          color="blue"
          variant="light"
          style={{ fontSize: '14px' }}
        >
          <Text size="sm">
            • <kbd>TAB</kbd> - Próximo campo • <kbd>SHIFT+TAB</kbd> - Campo anterior<br />
            • <kbd>ENTER</kbd> - Salvar • <kbd>ESC</kbd> - Cancelar • <kbd>F1</kbd> - Primeiro campo
          </Text>
        </Alert>

        {/* Campo Nome */}
        <TextInput
          ref={firstInputRef}
          label="Nome *"
          placeholder="Digite o nome do cliente"
          leftSection={<IconUser size={16} />}
          {...form.getInputProps('nome')}
          error={form.errors.nome}
          required
          autoComplete="given-name"
          style={{ fontSize: '16px' }}
          data-testid="cliente-nome"
          aria-describedby={form.errors.nome ? 'nome-error' : undefined}
        />

        {/* Campo Sobrenome */}
        <TextInput
          label="Sobrenome *"
          placeholder="Digite o sobrenome do cliente"
          leftSection={<IconUser size={16} />}
          {...form.getInputProps('sobrenome')}
          error={form.errors.sobrenome}
          required
          autoComplete="family-name"
          style={{ fontSize: '16px' }}
          data-testid="cliente-sobrenome"
          aria-describedby={form.errors.sobrenome ? 'sobrenome-error' : undefined}
        />

        {/* Campo CPF */}
        <TextInput
          label="CPF (opcional)"
          placeholder="000.000.000-00"
          leftSection={<IconId size={16} />}
          value={form.values.cpf}
          onChange={handleCpfChange}
          error={form.errors.cpf}
          autoComplete="off"
          style={{ fontSize: '16px' }}
          data-testid="cliente-cpf"
          aria-describedby={form.errors.cpf ? 'cpf-error' : 'cpf-help'}
          maxLength={14}
        />
        <Text id="cpf-help" size="xs" c="dimmed">
          Formato: 000.000.000-00 (apenas números são aceitos)
        </Text>

        {/* Campo Data de Nascimento */}
        <TextInput
          type="date"
          label="Data de Nascimento (opcional)"
          leftSection={<IconCalendar size={16} />}
          {...form.getInputProps('dataNascimento')}
          error={form.errors.dataNascimento}
          autoComplete="bday"
          style={{ fontSize: '16px' }}
          data-testid="cliente-data-nascimento"
          aria-describedby={form.errors.dataNascimento ? 'data-error' : 'data-help'}
          max={new Date().toISOString().split('T')[0]}
        />
        <Text id="data-help" size="xs" c="dimmed">
          Data não pode ser futura nem anterior a 1900
        </Text>

        {/* Botões de ação */}
        <Group justify="flex-end" mt="lg" gap="sm">
          <Button
            variant="light"
            color="gray"
            onClick={onCancel}
            disabled={isLoading}
            style={{ minWidth: '100px', fontSize: '16px' }}
            data-testid="btn-cancelar"
          >
            Cancelar
          </Button>

          <Button
            type="submit"
            loading={isLoading}
            style={{ minWidth: '100px', fontSize: '16px' }}
            data-testid="btn-salvar"
          >
            {isEditing ? 'Atualizar' : 'Criar'} Cliente
          </Button>
        </Group>

        {/* Indicador de campos obrigatórios */}
        <Text size="xs" c="dimmed" ta="center">
          * Campos obrigatórios
        </Text>
      </Stack>
    </Box>
  );
}

export default ClienteForm;