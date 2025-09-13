/**
 * Banner de identificação de ambiente
 * Mostra apenas em development e staging
 */

'use client';

import { getEnvironment, isDevelopment, isStaging } from '@/utils/environment';

export function EnvironmentBanner() {
  const env = getEnvironment();

  if (isDevelopment()) {
    return (
      <div
        style={{
          background: '#4caf50',
          color: '#fff',
          textAlign: 'center',
          padding: '4px 8px',
          fontSize: '12px',
          fontWeight: '600',
          borderBottom: '1px solid #388e3c',
          zIndex: 9999,
          position: 'relative'
        }}
      >
        🚧 DEVELOPMENT - Dev tools habilitados | F2-F6 para módulos | F1 para tutorial
      </div>
    );
  }

  if (isStaging()) {
    return (
      <div
        style={{
          background: '#ff9800',
          color: '#000',
          textAlign: 'center',
          padding: '4px 8px',
          fontSize: '12px',
          fontWeight: '600',
          borderBottom: '1px solid #f57c00',
          zIndex: 9999,
          position: 'relative'
        }}
      >
        🧪 STAGING - Ambiente de testes | Dev tools habilitados | F2-F6 para módulos
      </div>
    );
  }

  // Production não mostra banner
  return null;
}