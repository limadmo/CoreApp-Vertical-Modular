/**
 * Componente principal da aplicação CoreApp
 * Configura providers, roteamento e tema multi-tenant
 */
import React from 'react';
import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { BrowserRouter } from 'react-router-dom';
import { coreAppTheme } from '@theme/theme';
import { TenantProvider, useTenant } from './contexts/TenantContext';
import { useKeyboardNavigation } from '@hooks/useKeyboardNavigation';
import { AppRoutes } from './components/routing/AppRoutes';
import { AppLayout } from '@components/layout';

/**
 * Componente interno que usa os hooks após providers
 */
const AppContent: React.FC = () => {
  const { currentTenant, availableModules, isLoading, error } = useTenant();
  
  // Ativar navegação por teclado (apenas uma chamada)
  useKeyboardNavigation();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-brand-500 mx-auto"></div>
          <p className="mt-4 text-lg text-gray-600">Carregando tenant...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center p-8 max-w-md">
          <div className="text-red-500 text-6xl mb-4">⚠️</div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2">Erro no Tenant</h1>
          <p className="text-gray-600 mb-4">{error}</p>
          <button 
            onClick={() => window.location.reload()} 
            className="bg-brand-500 text-white px-6 py-2 rounded-md hover:bg-brand-600 transition-colors"
          >
            Tentar Novamente
          </button>
        </div>
      </div>
    );
  }

  return (
    <AppLayout>
      <AppRoutes />
    </AppLayout>
  );
};

/**
 * Componente raiz da aplicação CoreApp SAAS
 * Configura todos os providers e contextos necessários
 * 
 * @returns App component com providers configurados
 */
export const App: React.FC = () => {
  return (
    <MantineProvider theme={coreAppTheme}>
      <Notifications position="top-right" />
      <BrowserRouter>
        <TenantProvider>
          <AppContent />
        </TenantProvider>
      </BrowserRouter>
    </MantineProvider>
  );
};