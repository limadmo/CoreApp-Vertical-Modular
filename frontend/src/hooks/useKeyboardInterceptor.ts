/**
 * Hook para interceptar atalhos de teclado do browser
 * Bloqueia teclas F e outros atalhos, permitindo controle total
 */

'use client';

import { useEffect } from 'react';
import { allowDevTools } from '@/utils/environment';
import { MODULE_NAMES } from '@/types/auth';

interface KeyboardInterceptorOptions {
  onSystemShortcut?: (key: string) => void;
  onTutorial?: () => void;
  onBlockedShortcut?: (key: string) => void;
}

export const useKeyboardInterceptor = (options: KeyboardInterceptorOptions = {}) => {
  const {
    onSystemShortcut,
    onTutorial,
    onBlockedShortcut
  } = options;

  useEffect(() => {
    // INTERCEPTADOR INTELIGENTE RESTAURADO - Não interfere com formulários
    console.log('🎹 Smart Keyboard Interceptor ENABLED - Forms protected');

    const getBlockedKeys = (): string[] => {
      const systemKeys = [
        'F1',  // Tutorial contextual
        'F2',  // Vendas
        'F3',  // Clientes
        'F4',  // Produtos
        'F5',  // Estoque
        'F6',  // Fornecedores
        'F7', 'F8', 'F9', 'F10', 'F11', // Futuros módulos
      ];

      const browserKeys = [
        'Ctrl+R',       // Refresh
        'Ctrl+F5',      // Hard refresh
        'Ctrl+Shift+R', // Hard refresh
        'Ctrl+U',       // View source
      ];

      const devToolsKeys = [
        'F12',           // Dev tools
        'Ctrl+Shift+I',  // Dev tools
        'Ctrl+Shift+J',  // Console
        'Ctrl+Shift+C',  // Inspect
      ];

      // PRODUCTION: bloquear tudo
      if (!allowDevTools()) {
        return [...systemKeys, ...browserKeys, ...devToolsKeys];
      }

      // DEVELOPMENT + STAGING: permitir dev tools
      return [...systemKeys, ...browserKeys];
    };

    const isBlockedKey = (event: KeyboardEvent): boolean => {
      const blockedKeys = getBlockedKeys();

      return blockedKeys.some(key => {
        if (key.includes('+')) {
          // Atalhos com modificadores (Ctrl+R)
          const parts = key.split('+');
          return parts.every(part => {
            switch (part) {
              case 'Ctrl': return event.ctrlKey;
              case 'Shift': return event.shiftKey;
              case 'Alt': return event.altKey;
              default: return event.key === part;
            }
          });
        } else {
          // Teclas simples (F1, F2, etc)
          return event.key === key;
        }
      });
    };

    const isSystemShortcut = (key: string): boolean => {
      return ['F1', 'F2', 'F3', 'F4', 'F5', 'F6'].includes(key);
    };

    const isDevToolsShortcut = (event: KeyboardEvent): boolean => {
      return event.key === 'F12' ||
             (event.ctrlKey && event.shiftKey && event.key === 'I') ||
             (event.ctrlKey && event.shiftKey && event.key === 'J') ||
             (event.ctrlKey && event.shiftKey && event.key === 'C');
    };

    const isFormContext = (target: EventTarget | null): boolean => {
      if (!target) return false;

      const element = target as HTMLElement;
      if (!element.tagName) return false;

      // Verificar se o elemento é um input, textarea ou está dentro de um form
      const isInput = ['INPUT', 'TEXTAREA', 'SELECT'].includes(element.tagName);
      const isContentEditable = element.contentEditable === 'true';
      const isInsideForm = element.closest('form') !== null;

      return isInput || isContentEditable || isInsideForm;
    };

    const handleKeyDown = (event: KeyboardEvent) => {
      // CRÍTICO: Não interceptar eventos em contexto de formulário
      if (isFormContext(event.target)) {
        // Apenas bloquear atalhos específicos que realmente precisam ser bloqueados
        const criticalKeys = ['F5', 'Ctrl+R', 'Ctrl+Shift+R', 'Ctrl+U'];
        const isCritical = criticalKeys.some(key => {
          if (key.includes('+')) {
            const parts = key.split('+');
            return parts.every(part => {
              switch (part) {
                case 'Ctrl': return event.ctrlKey;
                case 'Shift': return event.shiftKey;
                case 'Alt': return event.altKey;
                default: return event.key === part;
              }
            });
          } else {
            return event.key === key;
          }
        });

        if (isCritical) {
          event.preventDefault();
          event.stopPropagation();
        }
        return; // Permitir todos os outros eventos em formulários
      }

      // Fora de formulários, aplicar interceptação normal
      if (isBlockedKey(event)) {
        event.preventDefault();
        event.stopPropagation();

        // Processar teclas do sistema
        if (isSystemShortcut(event.key)) {
          if (event.key === 'F1') {
            onTutorial?.();
          } else {
            onSystemShortcut?.(event.key);
          }
        }

        // Dev tools em produção - mostrar aviso
        if (!allowDevTools() && isDevToolsShortcut(event)) {
          onBlockedShortcut?.(event.key);
          if (console && console.warn) {
            console.warn('🔒 Dev tools desabilitados em produção');
          }
        }

        // Log para desenvolvimento
        if (allowDevTools() && console && console.log) {
          const moduleName = MODULE_NAMES[event.key as keyof typeof MODULE_NAMES];
          if (moduleName) {
            console.log(`🎹 Atalho interceptado: ${event.key} → ${moduleName}`);
          } else {
            console.log(`🚫 Atalho bloqueado: ${event.key}`);
          }
        }
      }
    };

    // Aplicar globalmente sem capture para permitir processamento normal dos forms
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [onSystemShortcut, onTutorial, onBlockedShortcut]);
};

// Utilitários para usar com o hook
export const createKeyboardHandlers = () => {
  const handleSystemShortcut = (key: string) => {
    const moduleName = MODULE_NAMES[key as keyof typeof MODULE_NAMES];
    if (moduleName) {
      console.log(`🎹 Tentativa de acesso: ${key} → ${moduleName}`);
      // TODO: Integrar com sistema de permissões e roteamento
      // const { canNavigateToModule, logAccessAttempt } = usePermissions();
      // const access = canNavigateToModule(key as keyof typeof MODULE_NAMES);
      // logAccessAttempt(key as keyof typeof MODULE_NAMES, access.allowed);
      // if (access.allowed) {
      //   router.push(`/modulos/${moduleName.toLowerCase()}`);
      // } else {
      //   showNotification(access.reason, 'warning');
      // }
    }
  };

  const handleTutorial = () => {
    console.log('🆘 Tutorial contextual da tela atual');
    // TODO: Implementar sistema de tutorial
    // Ex: showTutorial(getCurrentRoute());
  };

  const handleBlockedShortcut = (key: string) => {
    console.log(`🔒 Atalho ${key} foi bloqueado`);
    // TODO: Implementar notificação visual
    // Ex: showNotification(`Atalho ${key} não permitido`, 'warning');
  };

  return {
    handleSystemShortcut,
    handleTutorial,
    handleBlockedShortcut
  };
};