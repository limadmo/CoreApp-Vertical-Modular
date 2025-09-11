/**
 * Componente Text customizado para padaria
 * Substitui o Mantine Text com controle total de estilo
 */
import React from 'react';

export interface TextProps {
  children: React.ReactNode;
  className?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  fw?: number;
  c?: string;
  ta?: 'left' | 'center' | 'right';
  mt?: string;
  mb?: string;
  style?: React.CSSProperties;
  onClick?: () => void;
}

const sizeClasses = {
  xs: 'text-xs', // 12px
  sm: 'text-sm', // 14px  
  md: 'text-base', // 16px
  lg: 'text-lg', // 18px
  xl: 'text-xl' // 20px
};

const fontWeightClasses = {
  400: 'font-normal',
  500: 'font-medium',
  600: 'font-semibold',
  700: 'font-bold'
};

const textAlignClasses = {
  left: 'text-left',
  center: 'text-center', 
  right: 'text-right'
};

export const Text: React.FC<TextProps> = ({
  children,
  className = '',
  size = 'md',
  fw = 400,
  c,
  ta = 'left',
  mt,
  mb,
  style,
  onClick
}) => {
  const baseClasses = [
    'text-padaria-primary', // Usa cor padrão da padaria
    sizeClasses[size],
    fontWeightClasses[fw as keyof typeof fontWeightClasses] || `font-[${fw}]`,
    textAlignClasses[ta]
  ];

  // Adicionar classes de cor específicas se necessário
  if (c === 'dimmed') {
    baseClasses.push('text-padaria-muted');
  }

  const finalClasses = [...baseClasses, className].filter(Boolean).join(' ');

  const finalStyle = {
    marginTop: mt,
    marginBottom: mb,
    ...style
  };

  return (
    <span 
      className={finalClasses}
      style={finalStyle}
      onClick={onClick}
    >
      {children}
    </span>
  );
};