/**
 * Página do módulo Produtos (F3)
 * Especializada para vertical PADARIA
 */
import React from 'react';
import { useTenant } from '@hooks/useTenant';

export const ProdutosPage: React.FC = () => {
  const { currentTenant } = useTenant();

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          📦 Produtos
        </h1>
        <p className="text-gray-600">
          Catálogo especializado para {currentTenant?.verticalType}
        </p>
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-semibold mb-4">🚧 Em Desenvolvimento</h2>
        <p className="text-gray-600 mb-4">
          Esta página será implementada com:
        </p>
        <ul className="list-disc list-inside space-y-2 text-gray-600">
          <li>Listagem com filtros específicos (categoria, alérgenos, validade)</li>
          <li>CRUD com propriedades especializadas (ValidadeHoras, Alérgenos, PesoMédio)</li>
          <li>Upload de imagens de produtos</li>
          <li>Validações ANVISA automáticas</li>
          <li>Busca em tempo real por nome/ingrediente</li>
          <li>Ações em lote (descarte vencidos, promoção)</li>
        </ul>
      </div>

      <div className="mt-6 p-4 bg-blue-50 rounded-lg">
        <p className="text-blue-800">
          <strong>✨ Navegação:</strong> Pressione <kbd className="bg-blue-200 px-2 py-1 rounded text-xs">ESC</kbd> para voltar
        </p>
      </div>
    </div>
  );
};