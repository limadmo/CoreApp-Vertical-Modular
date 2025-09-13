/**
 * Componente de Login com Mantine 7.0
 * WCAG AAA compliant + Navegação por teclado
 */

'use client';

import { useState, useCallback, useEffect, useRef } from 'react';
import {
  Card,
  TextInput,
  PasswordInput,
  Button,
  Title,
  Text,
  Alert,
  Stack,
  Group,
  Box,
  Divider
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconUser, IconLock, IconAlertCircle } from '@tabler/icons-react';
import { useAuth, useAuthError } from '@/stores/useAuth';
import { LoginCredentials } from '@/types/auth';

interface LoginFormProps {
  onSuccess?: () => void;
}

export function LoginForm({ onSuccess }: LoginFormProps) {
  // HOOKS DE AUTENTICAÇÃO RESTAURADOS
  console.log('✅ Auth hooks restored - Real authentication enabled');

  const { login, isLoading } = useAuth();
  const error = useAuthError();

  // Refs para navegação por teclado
  const loginRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);
  const submitRef = useRef<HTMLButtonElement>(null);

  const form = useForm<LoginCredentials>({
    initialValues: {
      login: '',
      password: ''
    },

    validate: {
      login: (value) => {
        if (!value) return 'Login é obrigatório';
        if (value.length < 3) return 'Login deve ter pelo menos 3 caracteres';
        if (value.length > 20) return 'Login deve ter no máximo 20 caracteres';
        if (!/^[a-zA-Z0-9]+$/.test(value)) return 'Login deve conter apenas letras e números';
        return null;
      },
      password: (value) => {
        if (!value) return 'Senha é obrigatória';
        if (value.length < 6) return 'Senha deve ter pelo menos 6 caracteres';
        return null;
      }
    }
  });

  const handleSubmit = useCallback(async (values: LoginCredentials) => {
    try {
      await login(values);
      onSuccess?.();
    } catch (error) {
      console.error('Erro no login:', error);
    }
  }, [login, onSuccess]);

  // Handler simples apenas para Escape (limpar form)
  const handleFormKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      form.reset();
      loginRef.current?.focus();
    }
    // Tab, Enter e outras teclas: comportamento nativo do browser/Mantine
  }, [form]);

  // Auto-focus no primeiro campo ao montar
  useEffect(() => {
    // Usar timeout para evitar conflitos com hydration
    const timer = setTimeout(() => {
      loginRef.current?.focus();
    }, 100);

    return () => clearTimeout(timer);
  }, []);

  return (
    <Card
        shadow="xl"
        padding="xl"
        radius="md"
        withBorder
        style={{
          maxWidth: 420,
          width: '100%',
          backgroundColor: 'var(--mantine-color-white)',
          // WCAG AAA: Contraste 7:1
          color: 'var(--mantine-color-gray-9)'
        }}
      >
        <Stack spacing="lg">
          {/* Cabeçalho */}
          <Box ta="center">
            <Title
              order={1}
              size="h2"
              fw={600}
              c="gray.9"
              style={{ fontSize: '1.75rem' }} // 28px > 16px mínimo WCAG
            >
              CoreApp Login
            </Title>
            <Text
              size="md"
              c="gray.7"
              mt={4}
              style={{ fontSize: '1rem' }} // 16px mínimo WCAG
            >
              Sistema multi-tenant brasileiro
            </Text>
          </Box>

          <Divider />

          {/* Formulário */}
          <form onSubmit={form.onSubmit(handleSubmit)} onKeyDown={handleFormKeyDown}>
            <Stack spacing="md">
              {/* Campo Login */}
              <TextInput
                ref={loginRef}
                label="Login"
                placeholder="admin, gerente, vendedor, caixa"
                leftSection={<IconUser size={16} />}
                size="md"
                required
                {...form.getInputProps('login')}
                styles={{
                  input: {
                    fontSize: '1rem', // 16px mínimo WCAG
                    minHeight: '44px', // Área clicável mínima WCAG
                  },
                  label: {
                    fontSize: '1rem',
                    fontWeight: 500
                  }
                }}
                data-testid="login-input"
                aria-describedby={form.errors.login ? 'login-error' : undefined}
              />

              {/* Campo Senha */}
              <PasswordInput
                ref={passwordRef}
                label="Senha"
                placeholder="Sua senha"
                leftSection={<IconLock size={16} />}
                size="md"
                required
                {...form.getInputProps('password')}
                styles={{
                  input: {
                    fontSize: '1rem',
                    minHeight: '44px',
                  },
                  label: {
                    fontSize: '1rem',
                    fontWeight: 500
                  }
                }}
                data-testid="password-input"
                aria-describedby={form.errors.password ? 'password-error' : undefined}
              />


              {/* Erro de Autenticação */}
              {error && (
                <Alert
                  icon={<IconAlertCircle size={16} />}
                  title="Erro de Autenticação"
                  color="red"
                  role="alert"
                  aria-live="polite"
                  styles={{
                    root: { fontSize: '1rem' }
                  }}
                >
                  {error.message}
                </Alert>
              )}

              {/* Botão de Login */}
              <Button
                ref={submitRef}
                type="submit"
                loading={isLoading}
                size="md"
                fullWidth
                style={{
                  minHeight: '44px',
                  fontSize: '1rem',
                  fontWeight: 600
                }}
                data-testid="login-button"
                aria-describedby="login-help"
              >
                Entrar
              </Button>

              {/* Ajuda de Teclado */}
              <Text
                id="login-help"
                size="xs"
                c="gray.6"
                ta="center"
                style={{ fontSize: '0.875rem' }}
              >
                Enter para confirmar • Esc para limpar • F1 para tutorial
              </Text>
            </Stack>
          </form>

          <Divider />

          {/* Credenciais Demo */}
          <Box>
            <Text
              size="sm"
              fw={500}
              c="gray.7"
              mb="xs"
              style={{ fontSize: '0.95rem' }}
            >
              Credenciais de Teste:
            </Text>
            <Group spacing="xs">
              <Text size="xs" c="gray.6" style={{ fontSize: '0.875rem' }}>
                admin / admin123 • gerente / gerente123
              </Text>
            </Group>
            <Group spacing="xs" mt={4}>
              <Text size="xs" c="gray.6" style={{ fontSize: '0.875rem' }}>
                vendedor / vendedor123 • caixa / caixa123
              </Text>
            </Group>
          </Box>
        </Stack>
      </Card>
  );
}