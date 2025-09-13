/**
 * Providers Client-Side para Next.js 15
 * Separação entre Server e Client Components
 */

'use client';

import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { ModalsProvider } from '@mantine/modals';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { useKeyboardInterceptor, createKeyboardHandlers } from '@/hooks/useKeyboardInterceptor';
import { EnvironmentBanner } from '@/components/ui/EnvironmentBanner';
import { DebugIndicator } from '@/components/debug/DebugIndicator';
import { DebugErrorBoundary } from '@/components/debug/DebugErrorBoundary';
import { useDebugLogger, useAutoLogger } from '@/hooks/useDebugLogger';
import { initializeInterceptors, setDebugLogger } from '@/utils/debugInterceptors';
import { isDevelopment } from '@/utils/environment';

// Tema CoreApp brasileiro
const theme = {
  colors: {
    brasil: [
      '#E8F5E8', // Verde muito claro
      '#C8F7C5', // Verde claro
      '#A8F29A', // Verde médio claro
      '#81E868', // Verde médio
      '#00A859', // Verde Brasil principal
      '#00974F', // Verde escuro
      '#008642', // Verde mais escuro
      '#007A3D', // Verde bem escuro
      '#006D35', // Verde muito escuro
      '#00602E'  // Verde super escuro
    ],
    amarelo: [
      '#FFFEF0', // Amarelo muito claro
      '#FFFDE0', // Amarelo claro
      '#FFFCD0', // Amarelo médio claro
      '#FFFAC0', // Amarelo médio
      '#FFDF00', // Amarelo Brasil principal
      '#E5C900', // Amarelo escuro
      '#CCB400', // Amarelo mais escuro
      '#B39F00', // Amarelo bem escuro
      '#998A00', // Amarelo muito escuro
      '#807500'  // Amarelo super escuro
    ]
  },
  primaryColor: 'brasil',
  fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif',
  fontSizes: {
    xs: '0.875rem', // 14px
    sm: '1rem',     // 16px - WCAG mínimo
    md: '1.125rem', // 18px
    lg: '1.25rem',  // 20px
    xl: '1.5rem'    // 24px
  },
  headings: {
    fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif',
    fontWeight: '600',
    sizes: {
      h1: { fontSize: '2rem' },    // 32px
      h2: { fontSize: '1.75rem' }, // 28px
      h3: { fontSize: '1.5rem' },  // 24px
      h4: { fontSize: '1.25rem' }, // 20px
      h5: { fontSize: '1.125rem' }, // 18px
      h6: { fontSize: '1rem' }     // 16px
    }
  }
};

function DebugSystem({ children }: { children: React.ReactNode }) {
  const { logError, logWarning, logInfo } = useDebugLogger();

  // Inicializar sistema de debug automático
  useAutoLogger();

  // Configurar interceptadores
  useEffect(() => {
    if (isDevelopment()) {
      // Conectar logger aos interceptadores
      setDebugLogger({ logError, logWarning, logInfo });

      // Inicializar interceptadores
      initializeInterceptors({
        api: true,
        console: true,
        performance: true,
        interactions: true
      });

      console.log('🤖 CLAUDE MONITORING SYSTEM ACTIVATED');
      console.log('📊 Full system monitoring enabled for proactive debugging');
    }
  }, [logError, logWarning, logInfo]);

  return (
    <>
      {children}
      {isDevelopment() && <DebugIndicator />}
    </>
  );
}

export function Providers({ children }: { children: React.ReactNode }) {
  // SISTEMA COMPLETO COM DEBUG INTELIGENTE
  console.log('✅ Full System with Proactive Debug Monitoring');

  // QueryClient deve ser criado uma única vez por sessão
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 5 * 60 * 1000, // 5 minutos
        gcTime: 10 * 60 * 1000, // 10 minutos
        retry: 2,
        refetchOnWindowFocus: false,
      },
      mutations: {
        retry: 1,
      },
    },
  }));

  // Handlers para interceptação de teclado
  const keyboardHandlers = createKeyboardHandlers();

  // Interceptar teclas F globalmente (protegendo formulários)
  useKeyboardInterceptor({
    onSystemShortcut: keyboardHandlers.handleSystemShortcut,
    onTutorial: keyboardHandlers.handleTutorial,
    onBlockedShortcut: keyboardHandlers.handleBlockedShortcut
  });

  return (
    <DebugErrorBoundary
      onError={(error, errorInfo) => {
        console.error('🔴 React Error Boundary caught error:', error, errorInfo);
      }}
    >
      <QueryClientProvider client={queryClient}>
        <MantineProvider theme={theme} defaultColorScheme="light">
          <ModalsProvider>
            <DebugSystem>
              <EnvironmentBanner />
              <Notifications
                position="top-right"
                zIndex={9999}
                containerWidth={400}
              />
              {children}
            </DebugSystem>
          </ModalsProvider>
        </MantineProvider>
      </QueryClientProvider>
    </DebugErrorBoundary>
  );
}