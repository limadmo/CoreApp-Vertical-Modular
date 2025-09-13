/**
 * Página de Login
 * Layout responsivo com background brasileiro
 */

'use client';

import { useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  Container,
  Center,
  Stack,
  Text,
  Overlay,
  Group,
  Badge
} from '@mantine/core';
import { IconFlag, IconShield, IconCreditCard } from '@tabler/icons-react';
import { LoginForm } from '@/components/auth/LoginForm';
import { useAuthStatus } from '@/stores/useAuth';

export default function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isAuthenticated, isLoading } = useAuthStatus();

  // Redirecionar se já estiver autenticado
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      const redirect = searchParams.get('redirect') || '/dashboard';
      router.push(redirect);
    }
  }, [isAuthenticated, isLoading, router, searchParams]);

  const handleLoginSuccess = () => {
    const redirect = searchParams.get('redirect') || '/dashboard';
    router.push(redirect);
  };

  if (isAuthenticated && !isLoading) {
    return null; // Evita flash durante redirecionamento
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #00A859 0%, #FFDF00 50%, #00A859 100%)',
        backgroundSize: '400% 400%',
        animation: 'brazilGradient 8s ease infinite'
      }}
    >
      <style jsx>{`
        @keyframes brazilGradient {
          0% { background-position: 0% 50%; }
          50% { background-position: 100% 50%; }
          100% { background-position: 0% 50%; }
        }
      `}</style>
      <Overlay color="#000" opacity={0.2} />

      <Container size="sm" style={{ position: 'relative', zIndex: 1 }}>
        <Center style={{ minHeight: '100vh' }}>
          <Stack gap="xl" align="center" style={{ width: '100%' }}>

            {/* Cabeçalho Brasileiro */}
            <Stack gap="md" align="center">
              <Group gap="xs">
                <IconFlag size={20} color="#00A859" />
                <Text
                  size="lg"
                  fw={700}
                  c="white"
                  style={{
                    fontSize: '1.25rem',
                    textShadow: '2px 2px 4px rgba(0,0,0,0.8)'
                  }}
                >
                  CoreApp Brasil
                </Text>
                <IconFlag size={20} color="#FFDF00" />
              </Group>

              <Text
                size="md"
                c="white"
                ta="center"
                style={{
                  fontSize: '1rem',
                  textShadow: '1px 1px 2px rgba(0,0,0,0.8)',
                  maxWidth: 400
                }}
              >
                Sistema SAAS 100% brasileiro para gestão comercial multi-tenant
              </Text>

              {/* Badges de Compliance */}
              <Group gap="sm">
                <Badge
                  leftSection={<IconShield size={12} />}
                  color="green"
                  size="sm"
                  style={{ fontSize: '0.875rem' }}
                >
                  LGPD Compliant
                </Badge>
                <Badge
                  leftSection={<IconCreditCard size={12} />}
                  color="blue"
                  size="sm"
                  style={{ fontSize: '0.875rem' }}
                >
                  PIX + Boleto
                </Badge>
              </Group>
            </Stack>

            {/* Formulário de Login */}
            <LoginForm onSuccess={handleLoginSuccess} />

            {/* Rodapé */}
            <Stack gap="xs" align="center">
              <Text
                size="xs"
                c="white"
                style={{
                  fontSize: '0.875rem',
                  textShadow: '1px 1px 2px rgba(0,0,0,0.8)'
                }}
              >
                © 2024 CoreApp SAAS - Desenvolvido no Brasil
              </Text>
              <Group gap="md">
                <Text
                  size="xs"
                  c="gray.3"
                  style={{
                    fontSize: '0.8rem',
                    textShadow: '1px 1px 2px rgba(0,0,0,0.8)'
                  }}
                >
                  Dados processados em território nacional
                </Text>
              </Group>
            </Stack>

          </Stack>
        </Center>
      </Container>
    </div>
  );
}