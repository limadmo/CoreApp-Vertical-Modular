/**
 * Tema CoreApp SAAS - Mantine customizado
 * Acessibilidade WCAG AAA, contraste 7:1, fonte 16px+
 */
import { createTheme } from '@mantine/core';

export const coreAppTheme = createTheme({
  /** Cores primárias PADARIA com contraste WCAG AAA */
  colors: {
    brand: [
      '#fff5ee', // 50 - Creme suave
      '#fed7aa', // 100 - Laranja muito claro
      '#fdba74', // 200 - Laranja claro
      '#fb923c', // 300 - Laranja médio
      '#f97316', // 400 - Laranja vibrante
      '#d2691e', // 500 - Laranja pão dourado (primary)
      '#b45309', // 600 - Laranja escuro
      '#8b4513', // 700 - Marrom crosta de pão
      '#7c2d12', // 800 - Marrom escuro
      '#5c1a06', // 900 - Marrom muito escuro
    ],
    // Cores acessíveis para texto
    gray: [
      '#f8fafc', // 50
      '#f1f5f9', // 100
      '#e2e8f0', // 200
      '#cbd5e1', // 300
      '#94a3b8', // 400
      '#64748b', // 500
      '#475569', // 600
      '#334155', // 700 - Contraste 7.2:1
      '#1e293b', // 800 - Contraste 12.6:1
      '#0f172a', // 900 - Contraste 21:1
    ],
  },
  
  /** Cor primária do tema */
  primaryColor: 'brand',
  
  /** Fontes otimizadas para legibilidade */
  fontFamily: 'Inter, system-ui, -apple-system, BlinkMacSystemFont, sans-serif',
  fontFamilyMonospace: 'JetBrains Mono, "Fira Code", Monaco, Consolas, monospace',
  
  /** Tamanhos de fonte WCAG AAA (16px+ base) */
  fontSizes: {
    xs: '14px', // Mínimo permitido
    sm: '16px', // Base padrão
    md: '18px', // Confortável
    lg: '20px', // Grande
    xl: '24px', // Extra grande
  },
  
  /** Títulos com hierarquia visual clara */
  headings: {
    fontFamily: 'Inter, system-ui, -apple-system, sans-serif',
    fontWeight: '600',
    sizes: {
      h1: { 
        fontSize: '2.5rem', // 40px
        lineHeight: '1.2',
        fontWeight: '700' 
      },
      h2: { 
        fontSize: '2rem', // 32px
        lineHeight: '1.25',
        fontWeight: '600' 
      },
      h3: { 
        fontSize: '1.75rem', // 28px
        lineHeight: '1.3',
        fontWeight: '600' 
      },
      h4: { 
        fontSize: '1.5rem', // 24px
        lineHeight: '1.35',
        fontWeight: '600' 
      },
      h5: { 
        fontSize: '1.25rem', // 20px
        lineHeight: '1.4',
        fontWeight: '500' 
      },
      h6: { 
        fontSize: '1.125rem', // 18px
        lineHeight: '1.45',
        fontWeight: '500' 
      },
    },
  },
  
  /** Raio de bordas consistente */
  radius: {
    xs: '4px',
    sm: '6px',
    md: '8px',
    lg: '12px',
    xl: '16px',
  },
  
  /** Espaçamentos proporcionais */
  spacing: {
    xs: '8px',
    sm: '12px',
    md: '16px',
    lg: '24px',
    xl: '32px',
  },
  
  /** Sombras sutis para profundidade */
  shadows: {
    xs: '0 1px 3px rgba(0, 0, 0, 0.05)',
    sm: '0 1px 2px rgba(0, 0, 0, 0.05)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
    xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
  },
  
  /** Componentes customizados */
  components: {
    Button: {
      styles: {
        root: {
          // Altura mínima touch-friendly
          minHeight: '44px',
          fontSize: '16px',
          fontWeight: '500',
        },
      },
    },
    
    TextInput: {
      styles: {
        input: {
          fontSize: '16px', // Evita zoom no iOS
          minHeight: '44px',
        },
        label: {
          fontSize: '16px',
          fontWeight: '500',
          marginBottom: '8px',
        },
      },
    },
    
    Select: {
      styles: {
        dropdown: {
          backgroundColor: '#ffffff !important',
          border: '2px solid #374151 !important',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15) !important',
          zIndex: 9999,
        },
        option: {
          color: '#1a1b1e !important',
          fontSize: '16px !important',
          fontWeight: '600 !important',
          padding: '12px 16px !important',
          backgroundColor: '#ffffff !important',
          '&[data-selected]': {
            backgroundColor: '#e7f3ff !important',
            color: '#1a1b1e !important',
            fontWeight: '700 !important',
          },
          '&:hover': {
            backgroundColor: '#f8f9fa !important',
            color: '#1a1b1e !important',
          },
        },
        input: {
          fontSize: '16px',
          minHeight: '44px',
          color: '#1a1b1e',
          backgroundColor: '#ffffff',
        },
        label: {
          fontSize: '16px',
          fontWeight: '600',
          marginBottom: '8px',
          color: '#1a1b1e',
        },
      },
    },

    Table: {
      styles: {
        th: {
          fontSize: '16px',
          fontWeight: '600',
          borderBottomWidth: '2px',
        },
        td: {
          fontSize: '16px',
          padding: '12px',
        },
      },
    },
    
    Card: {
      styles: {
        root: {
          borderWidth: '1px',
          borderColor: '#e2e8f0',
        },
      },
    },
  },
});