/**
 * Hook centralizado para gerenciamento de atalhos de teclado
 * Previne comportamentos padrão do navegador e redireciona para ações da aplicação
 */
import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

/**
 * Configuração de atalhos por módulo
 */
const KEYBOARD_SHORTCUTS = {
  // Teclas de função F1-F12
  F1: { action: 'help', module: 'current', label: 'Ajuda contextual' },
  F2: { action: 'navigate', module: '/vendas', label: 'Vendas' },
  F3: { action: 'navigate', module: '/clientes', label: 'Clientes' },
  F4: { action: 'navigate', module: '/produtos', label: 'Produtos' },
  F5: { action: 'navigate', module: '/estoque', label: 'Estoque' },
  F6: { action: 'navigate', module: '/fornecedores', label: 'Fornecedores' },
  F7: { action: 'navigate', module: '/promocoes', label: 'Promoções' },
  F8: { action: 'navigate', module: '/relatorios', label: 'Relatórios' },
  F9: { action: 'navigate', module: '/configuracoes', label: 'Configurações' },
  F10: { action: 'navigate', module: '/', label: 'Dashboard' },
  F11: { action: 'browser', module: 'fullscreen', label: 'Tela cheia (sistema)' },
  F12: { action: 'navigate', module: '/auditoria', label: 'Auditoria' },
  
  // Atalhos especiais
  'CTRL+F': { action: 'search', module: 'current', label: 'Busca inteligente' },
} as const;

/**
 * Mapeamento de busca inteligente por rota
 */
const SEARCH_FOCUS_BY_ROUTE = {
  '/clientes': () => {
    const searchInput = document.querySelector('[data-search-input]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  },
  '/vendas': () => {
    const searchInput = document.querySelector('[data-search="produtos-venda"]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  },
  '/produtos': () => {
    const searchInput = document.querySelector('[data-search="produtos"]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  },
  '/estoque': () => {
    const searchInput = document.querySelector('[data-search="estoque"]') as HTMLInputElement;
    if (searchInput) {
      searchInput.focus();
      searchInput.select();
    }
  },
} as const;

/**
 * Interface para callbacks do hook
 */
interface UseKeyboardShortcutsProps {
  onHelp?: () => void;
  onSearch?: () => void;
}

/**
 * Hook para gerenciamento global de atalhos de teclado
 * Previne comportamentos do navegador e executa ações da aplicação
 */
export const useKeyboardShortcuts = (props: UseKeyboardShortcutsProps = {}) => {
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    /**
     * Handler principal de eventos de teclado
     * Intercepta todas as teclas e previne comportamento padrão quando necessário
     */
    const handleKeyDown = (event: KeyboardEvent) => {
      // Detectar tecla pressionada
      const key = event.key;
      const ctrlKey = event.ctrlKey;
      const metaKey = event.metaKey; // Cmd no Mac
      
      // Criar identificador da combinação
      let shortcutKey = '';
      if (ctrlKey || metaKey) {
        shortcutKey = `CTRL+${key.toUpperCase()}`;
      } else {
        shortcutKey = key;
      }

      // Verificar se é um atalho mapeado
      const shortcut = KEYBOARD_SHORTCUTS[shortcutKey as keyof typeof KEYBOARD_SHORTCUTS];
      
      if (shortcut) {
        // SEMPRE prevenir comportamento padrão para atalhos mapeados
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        
        // Executar ação baseada no tipo
        switch (shortcut.action) {
          case 'help':
            // F1: Exibir ajuda contextual da tela atual
            if (props.onHelp) {
              props.onHelp();
            } else {
              console.log(`Ajuda contextual para: ${location.pathname}`);
              // TODO: Implementar modal de ajuda
            }
            break;
            
          case 'navigate':
            // F2-F12: Navegar para módulos
            if (shortcut.module && shortcut.module !== location.pathname) {
              navigate(shortcut.module);
            }
            break;
            
          case 'search':
            // Ctrl+F: Focar na busca inteligente do módulo atual
            const searchFunction = SEARCH_FOCUS_BY_ROUTE[location.pathname as keyof typeof SEARCH_FOCUS_BY_ROUTE];
            if (searchFunction) {
              searchFunction();
            } else if (props.onSearch) {
              props.onSearch();
            }
            break;
            
          case 'browser':
            // F11: Permitir tela cheia (não prevenir)
            // Nota: Já foi preventDefault acima, mas F11 pode precisar tratamento especial
            break;
        }
        
        return false; // Garantir que não propague
      }

      // Para F1-F12 não mapeadas, ainda assim prevenir comportamento do navegador
      if (key.startsWith('F') && key.length >= 2 && key.length <= 3) {
        const fNumber = parseInt(key.substring(1));
        if (fNumber >= 1 && fNumber <= 12) {
          event.preventDefault();
          event.stopPropagation();
          event.stopImmediatePropagation();
          return false;
        }
      }

      // Ctrl+F sempre deve ser interceptado, mesmo se não houver busca mapeada
      if (shortcutKey === 'CTRL+F') {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        
        // Fallback: tentar focar em qualquer campo de busca
        const genericSearch = document.querySelector('input[type="search"], input[placeholder*="buscar"], input[placeholder*="pesquisar"]') as HTMLInputElement;
        if (genericSearch) {
          genericSearch.focus();
          genericSearch.select();
        }
        
        return false;
      }
    };

    /**
     * Handler para keypress - segunda linha de defesa
     */
    const handleKeyPress = (event: KeyboardEvent) => {
      const key = event.key;
      const ctrlKey = event.ctrlKey || event.metaKey;
      
      // F1-F12 ou Ctrl+F
      if ((key.startsWith('F') && key.length >= 2 && key.length <= 3) || 
          (ctrlKey && key.toUpperCase() === 'F')) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        return false;
      }
    };

    /**
     * Handler para keyup - limpeza final
     */
    const handleKeyUp = (event: KeyboardEvent) => {
      const key = event.key;
      const ctrlKey = event.ctrlKey || event.metaKey;
      
      // F1-F12 ou Ctrl+F
      if ((key.startsWith('F') && key.length >= 2 && key.length <= 3) || 
          (ctrlKey && key.toUpperCase() === 'F')) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        return false;
      }
    };

    // Registrar eventos com captura (prioridade máxima)
    const options = { capture: true, passive: false };
    
    document.addEventListener('keydown', handleKeyDown, options);
    document.addEventListener('keypress', handleKeyPress, options);
    document.addEventListener('keyup', handleKeyUp, options);
    
    // Também no window como fallback
    window.addEventListener('keydown', handleKeyDown, options);
    window.addEventListener('keypress', handleKeyPress, options);
    window.addEventListener('keyup', handleKeyUp, options);

    // Cleanup
    return () => {
      document.removeEventListener('keydown', handleKeyDown, options);
      document.removeEventListener('keypress', handleKeyPress, options);
      document.removeEventListener('keyup', handleKeyUp, options);
      window.removeEventListener('keydown', handleKeyDown, options);
      window.removeEventListener('keypress', handleKeyPress, options);
      window.removeEventListener('keyup', handleKeyUp, options);
    };
  }, [navigate, location.pathname, props.onHelp, props.onSearch]);

  /**
   * Retorna informações sobre os atalhos disponíveis
   */
  const getShortcutsInfo = () => {
    return Object.entries(KEYBOARD_SHORTCUTS).map(([key, config]) => ({
      key,
      label: config.label,
      action: config.action,
    }));
  };

  /**
   * Retorna atalhos específicos da rota atual
   */
  const getCurrentRouteShortcuts = () => {
    const current = location.pathname;
    const shortcuts = [];
    
    // F1 sempre disponível
    shortcuts.push({ key: 'F1', label: 'Ajuda desta tela', action: 'help' });
    
    // Ctrl+F se houver busca mapeada
    if (SEARCH_FOCUS_BY_ROUTE[current as keyof typeof SEARCH_FOCUS_BY_ROUTE]) {
      shortcuts.push({ key: 'Ctrl+F', label: 'Busca inteligente', action: 'search' });
    }
    
    return shortcuts;
  };

  return {
    getShortcutsInfo,
    getCurrentRouteShortcuts,
  };
};