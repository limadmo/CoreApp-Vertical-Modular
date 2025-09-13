/**
 * AuthGuard - Componente de proteção de rotas
 * Verifica autenticação e redireciona automaticamente se necessário
 */

'use client';

import { useEffect, ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { Container, Center, LoadingOverlay, Stack, Text, Alert } from '@mantine/core';
import { IconShield, IconAlertCircle } from '@tabler/icons-react';
import { useAuthStatus, useAuth } from '@/stores/useAuth';

interface AuthGuardProps {
  children: ReactNode;
  requireAuth?: boolean;
  redirectTo?: string;
  fallback?: ReactNode;
  showLoading?: boolean;
}

export function AuthGuard({
  children,
  requireAuth = true,
  redirectTo = '/login',
  fallback,
  showLoading = true
}: AuthGuardProps) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuthStatus();
  const { checkAuth } = useAuth();

  // Verificar autenticação na montagem do componente
  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      checkAuth();
    }
  }, [isLoading, isAuthenticated, checkAuth]);

  // Redirecionar se não autenticado
  useEffect(() => {
    if (requireAuth && !isLoading && !isAuthenticated) {
      const currentPath = window.location.pathname;
      const redirectUrl = `${redirectTo}?redirect=${encodeURIComponent(currentPath)}`;
      router.replace(redirectUrl);
    }
  }, [requireAuth, isLoading, isAuthenticated, redirectTo, router]);

  // Estados de carregamento e erro
  if (isLoading && showLoading) {
    return fallback || (
      <Container size="sm" py="xl">
        <Center style={{ minHeight: '50vh' }}>
          <Stack gap="md" align="center">
            <LoadingOverlay visible />
            <IconShield size={48} color="var(--mantine-color-blue-6)" />
            <Text size="lg" fw={500}>Verificando autenticação...</Text>
            <Text size="sm" c="dimmed" ta="center">
              Aguarde enquanto validamos suas credenciais
            </Text>
          </Stack>
        </Center>
      </Container>
    );
  }

  // Se requer autenticação mas não está autenticado
  if (requireAuth && !isAuthenticated && !isLoading) {
    return fallback || (
      <Container size="sm" py="xl">
        <Center style={{ minHeight: '50vh' }}>
          <Stack gap="md" align="center">
            <Alert
              icon={<IconAlertCircle size={24} />}
              title="Acesso Negado"
              color="red"
              variant="light"
              style={{ width: '100%', maxWidth: 400 }}
            >
              <Stack gap="sm">
                <Text size="sm">
                  Você precisa estar logado para acessar esta página.
                </Text>
                <Text size="xs" c="dimmed">
                  Redirecionando para a página de login...
                </Text>
              </Stack>
            </Alert>
          </Stack>
        </Center>
      </Container>
    );
  }

  // Se não requer autenticação ou está autenticado, mostrar conteúdo
  return <>{children}</>;
}

// Variação do AuthGuard especificamente para dashboard e páginas administrativas
export function AdminGuard({ children, ...props }: Omit<AuthGuardProps, 'requireAuth'>) {
  const { user } = useAuthStatus();

  // Verificar se o usuário tem permissão de admin
  const hasAdminAccess = user && (
    user.perfil === 'SUPER_ADMIN' ||
    user.perfil === 'GERENTE' ||
    user.permissions.includes('admin')
  );

  if (user && !hasAdminAccess) {
    return (
      <Container size="sm" py="xl">
        <Center style={{ minHeight: '50vh' }}>
          <Alert
            icon={<IconAlertCircle size={24} />}
            title="Acesso Restrito"
            color="orange"
            variant="light"
            style={{ width: '100%', maxWidth: 400 }}
          >
            <Text size="sm">
              Você não tem permissão para acessar esta área administrativa.
            </Text>
          </Alert>
        </Center>
      </Container>
    );
  }

  return (
    <AuthGuard requireAuth={true} {...props}>
      {children}
    </AuthGuard>
  );
}

// Hook utilitário para usar o AuthGuard programaticamente
export function useAuthGuard() {
  const { isAuthenticated, isLoading, user } = useAuthStatus();
  const router = useRouter();

  const redirectToLogin = (returnUrl?: string) => {
    const currentPath = returnUrl || window.location.pathname;
    const redirectUrl = `/login?redirect=${encodeURIComponent(currentPath)}`;
    router.replace(redirectUrl);
  };

  const requireAuth = (callback?: () => void) => {
    if (!isLoading && !isAuthenticated) {
      redirectToLogin();
      return false;
    }

    if (isAuthenticated && callback) {
      callback();
    }

    return isAuthenticated;
  };

  return {
    isAuthenticated,
    isLoading,
    user,
    redirectToLogin,
    requireAuth
  };
}