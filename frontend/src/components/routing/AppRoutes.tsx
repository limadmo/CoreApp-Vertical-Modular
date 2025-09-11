/**
 * Configuração de rotas da aplicação CoreApp
 * Sistema de roteamento multi-tenant com validação de módulos
 */
import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useTenant } from '@hooks/useTenant';
import { VendasPage, ProdutosPage } from '../../pages';

/**
 * Componente de rota protegida por módulo
 */
interface ProtectedRouteProps {
  moduleCode: string;
  children: React.ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ moduleCode, children }) => {
  const { hasModule } = useTenant();

  if (!hasModule(moduleCode as any)) {
    return (
      <div className="p-6">
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
          <div className="text-yellow-600 text-5xl mb-4">🔒</div>
          <h2 className="text-2xl font-bold text-yellow-800 mb-2">
            Módulo Não Ativo
          </h2>
          <p className="text-yellow-700 mb-4">
            O módulo <strong>{moduleCode}</strong> não está ativo para este tenant.
          </p>
          <p className="text-sm text-yellow-600">
            Entre em contato com o administrador para ativar este módulo.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
};

/**
 * Página inicial/dashboard
 */
const DashboardPage: React.FC = () => {
  const { currentTenant, availableModules } = useTenant();

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          🏠 Dashboard
        </h1>
        <p className="text-gray-600">
          Visão geral do {currentTenant?.nome}
        </p>
      </div>

      <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        {availableModules
          .filter(module => module.isActive)
          .map((module) => (
            <div 
              key={module.code}
              className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-xl font-semibold text-gray-800">
                  {module.name}
                </h3>
                <kbd className="px-2 py-1 bg-gray-100 text-sm rounded">
                  {module.shortcut}
                </kbd>
              </div>
              <p className="text-gray-600 text-sm mb-4">
                {module.description}
              </p>
              <div className="text-xs text-green-600 font-medium">
                ✅ Módulo Ativo
              </div>
            </div>
        ))}
      </div>

      <div className="mt-8 p-4 bg-blue-50 rounded-lg">
        <h3 className="text-lg font-semibold text-blue-800 mb-2">
          🎯 Navegação Rápida
        </h3>
        <p className="text-blue-700 text-sm">
          Use as teclas de função para navegar rapidamente:
          <strong> F1</strong> (Vendas), <strong>F2</strong> (Clientes), 
          <strong> F3</strong> (Produtos), <strong>F4</strong> (Estoque)
        </p>
      </div>
    </div>
  );
};

/**
 * Página de configurações
 */
const ConfiguracoesPage: React.FC = () => {
  const { currentTenant } = useTenant();

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          ⚙️ Configurações
        </h1>
        <p className="text-gray-600">
          Configurações do {currentTenant?.nome}
        </p>
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-semibold mb-4">🚧 Em Desenvolvimento</h2>
        <p className="text-gray-600">
          Aqui serão configuradas as preferências do tenant, módulos ativos, 
          configurações do vertical e personalizações de tema.
        </p>
      </div>

      <div className="mt-6 p-4 bg-blue-50 rounded-lg">
        <p className="text-blue-800">
          <strong>✨ Navegação:</strong> Pressione <kbd className="bg-blue-200 px-2 py-1 rounded text-xs">F10</kbd> para voltar ao menu principal
        </p>
      </div>
    </div>
  );
};

/**
 * Página de ajuda
 */
const AjudaPage: React.FC = () => {
  const { availableModules } = useTenant();

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          ❓ Ajuda
        </h1>
        <p className="text-gray-600">
          Guia de uso do sistema CoreApp
        </p>
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-semibold mb-4 text-blue-800">
            🎯 Atalhos de Teclado
          </h2>
          <div className="space-y-2 text-sm">
            {availableModules
              .filter(m => m.isActive)
              .map((module) => (
                <div key={module.code} className="flex justify-between">
                  <span>{module.name}</span>
                  <kbd className="px-2 py-1 bg-gray-100 rounded text-xs">
                    {module.shortcut}
                  </kbd>
                </div>
              ))}
            <hr className="my-3" />
            <div className="flex justify-between">
              <span>Configurações</span>
              <kbd className="px-2 py-1 bg-gray-100 rounded text-xs">F9</kbd>
            </div>
            <div className="flex justify-between">
              <span>Menu Principal</span>
              <kbd className="px-2 py-1 bg-gray-100 rounded text-xs">F10</kbd>
            </div>
            <div className="flex justify-between">
              <span>Tela Cheia</span>
              <kbd className="px-2 py-1 bg-gray-100 rounded text-xs">F11</kbd>
            </div>
            <div className="flex justify-between">
              <span>Voltar</span>
              <kbd className="px-2 py-1 bg-gray-100 rounded text-xs">ESC</kbd>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-semibold mb-4 text-green-800">
            ♿ Acessibilidade
          </h2>
          <div className="space-y-2 text-sm text-gray-600">
            <div className="flex items-center">
              <span className="text-green-500 mr-2">✅</span>
              <span>Navegação 100% por teclado</span>
            </div>
            <div className="flex items-center">
              <span className="text-green-500 mr-2">✅</span>
              <span>Compatível com screen readers</span>
            </div>
            <div className="flex items-center">
              <span className="text-green-500 mr-2">✅</span>
              <span>Contraste WCAG AAA (7:1)</span>
            </div>
            <div className="flex items-center">
              <span className="text-green-500 mr-2">✅</span>
              <span>Fonte mínima 16px</span>
            </div>
            <div className="flex items-center">
              <span className="text-green-500 mr-2">✅</span>
              <span>Botões touch-friendly (44px)</span>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-6 p-4 bg-blue-50 rounded-lg">
        <p className="text-blue-800">
          <strong>✨ Navegação:</strong> Pressione <kbd className="bg-blue-200 px-2 py-1 rounded text-xs">ESC</kbd> para voltar
        </p>
      </div>
    </div>
  );
};

/**
 * Configuração de todas as rotas da aplicação
 */
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Página inicial */}
      <Route path="/" element={<DashboardPage />} />
      
      {/* Módulos Starter */}
      <Route 
        path="/vendas" 
        element={
          <ProtectedRoute moduleCode="VENDAS">
            <VendasPage />
          </ProtectedRoute>
        } 
      />
      <Route 
        path="/produtos" 
        element={
          <ProtectedRoute moduleCode="PRODUTOS">
            <ProdutosPage />
          </ProtectedRoute>
        } 
      />
      
      {/* TODO: Adicionar outras rotas quando implementadas */}
      <Route 
        path="/clientes" 
        element={
          <ProtectedRoute moduleCode="CLIENTES">
            <div className="p-6">
              <h1 className="text-3xl font-bold">👥 Clientes</h1>
              <p>Em desenvolvimento...</p>
            </div>
          </ProtectedRoute>
        } 
      />
      <Route 
        path="/estoque" 
        element={
          <ProtectedRoute moduleCode="ESTOQUE">
            <div className="p-6">
              <h1 className="text-3xl font-bold">📦 Estoque</h1>
              <p>Em desenvolvimento...</p>
            </div>
          </ProtectedRoute>
        } 
      />
      
      {/* Sistema */}
      <Route path="/configuracoes" element={<ConfiguracoesPage />} />
      <Route path="/ajuda" element={<AjudaPage />} />
      
      {/* Redirecionamento de rotas não encontradas */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};