/**
 * Hook para controlar navegação global por teclado
 * Implementa todos os atalhos F1-F12 conforme CLAUDE.md
 */
import { useEffect, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTenant } from './useTenant';
import { ModuleCode } from '../types/index';

interface KeyboardNavigationConfig {
  /** Mapa de teclas para ações */
  keyMap: Record<string, () => void>;
  /** Se navegação está ativa */
  enabled: boolean;
  /** Registrar um handler customizado */
  registerHandler: (key: string, handler: () => void) => void;
  /** Remover um handler customizado */
  unregisterHandler: (key: string) => void;
}

/**
 * Hook de navegação por teclado F1-F12
 * Implementa padrão de acessibilidade WCAG AAA
 * 
 * @example
 * ```tsx
 * const { keyMap, enabled } = useKeyboardNavigation();
 * 
 * // Automático - funciona globalmente
 * // F1 = Vendas, F2 = Clientes, etc.
 * ```
 * 
 * @returns Configuração e controles da navegação
 */
export const useKeyboardNavigation = (): KeyboardNavigationConfig => {
  const navigate = useNavigate();
  const { hasModule, availableModules } = useTenant();

  // Handlers customizados registrados dinamicamente
  const customHandlers = useMemo(() => new Map<string, () => void>(), []);

  /**
   * Registra handler customizado para uma tecla
   */
  const registerHandler = useCallback((key: string, handler: () => void) => {
    customHandlers.set(key, handler);
  }, [customHandlers]);

  /**
   * Remove handler customizado
   */
  const unregisterHandler = useCallback((key: string) => {
    customHandlers.delete(key);
  }, [customHandlers]);

  /**
   * Navega para módulo se estiver ativo
   */
  const navigateToModule = useCallback((moduleCode: ModuleCode, path: string) => {
    if (hasModule(moduleCode)) {
      navigate(path);
      
      // Anunciar para screen readers
      const announcement = `Navegando para ${moduleCode}`;
      const srElement = document.getElementById('sr-shortcuts');
      if (srElement) {
        srElement.textContent = announcement;
      }
    } else {
      // Mostrar aviso que módulo não está ativo
      const announcement = `Módulo ${moduleCode} não está ativo para este tenant`;
      const srElement = document.getElementById('sr-shortcuts');
      if (srElement) {
        srElement.textContent = announcement;
      }
    }
  }, [hasModule, navigate]);

  /**
   * Mapa de teclas padrão do sistema
   */
  const keyMap = useMemo(() => {
    const defaultMap: Record<string, () => void> = {
      // Módulos Starter (F1-F4) - Sempre disponíveis
      'F1': () => navigateToModule('VENDAS', '/vendas'),
      'F2': () => navigateToModule('CLIENTES', '/clientes'), 
      'F3': () => navigateToModule('PRODUTOS', '/produtos'),
      'F4': () => navigateToModule('ESTOQUE', '/estoque'),
      
      // Módulos Adicionais (F5-F8) - Baseados em assinatura
      'F5': () => navigateToModule('FORNECEDORES', '/fornecedores'),
      'F6': () => navigateToModule('PROMOCOES', '/promocoes'),
      'F7': () => navigateToModule('RELATORIOS_BASICOS', '/relatorios'),
      'F8': () => navigateToModule('AUDITORIA', '/auditoria'),
      
      // Navegação de sistema (F9-F12)
      'F9': () => {
        navigate('/configuracoes');
        const srElement = document.getElementById('sr-shortcuts');
        if (srElement) srElement.textContent = 'Navegando para Configurações';
      },
      'F10': () => {
        navigate('/');
        const srElement = document.getElementById('sr-shortcuts');
        if (srElement) srElement.textContent = 'Navegando para Menu Principal';
      },
      'F11': () => {
        // Toggle fullscreen
        if (!document.fullscreenElement) {
          document.documentElement.requestFullscreen().catch(console.error);
        } else {
          document.exitFullscreen().catch(console.error);
        }
        const srElement = document.getElementById('sr-shortcuts');
        if (srElement) srElement.textContent = 'Alternando tela cheia';
      },
      'F12': () => {
        navigate('/ajuda');
        const srElement = document.getElementById('sr-shortcuts');
        if (srElement) srElement.textContent = 'Abrindo Ajuda';
      },
      
      // Navegação básica
      'Escape': () => {
        window.history.back();
        const srElement = document.getElementById('sr-shortcuts');
        if (srElement) srElement.textContent = 'Voltando à página anterior';
      },
    };

    // Mergear com handlers customizados
    customHandlers.forEach((handler, key) => {
      defaultMap[key] = handler;
    });

    return defaultMap;
  }, [navigateToModule, navigate, customHandlers]);

  /**
   * Handler global de teclado
   */
  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    // Ignorar se em campo de input/textarea
    const target = event.target as HTMLElement;
    if (target?.tagName === 'INPUT' || 
        target?.tagName === 'TEXTAREA' ||
        target?.contentEditable === 'true') {
      return;
    }

    // Ignorar se usando modificadores (Ctrl, Alt, Shift com outras teclas)
    if (event.ctrlKey || event.altKey || event.metaKey) {
      return;
    }

    const action = keyMap[event.key];
    if (action) {
      event.preventDefault();
      event.stopPropagation();
      action();
    }
  }, [keyMap]);

  // Configurar listeners de teclado
  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  // Anunciar módulos disponíveis quando mudarem
  useEffect(() => {
    const activeModules = availableModules
      .filter(m => m.isActive)
      .map(m => `${m.shortcut}: ${m.name}`)
      .join(', ');
      
    const srElement = document.getElementById('sr-shortcuts');
    if (srElement && activeModules) {
      // Delay para não conflitar com outras ações
      setTimeout(() => {
        srElement.textContent = `Módulos disponíveis: ${activeModules}`;
      }, 1000);
    }
  }, [availableModules]);

  return {
    keyMap,
    enabled: true,
    registerHandler,
    unregisterHandler,
  };
};