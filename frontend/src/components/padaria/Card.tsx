/**
 * Componente Card customizado para padaria
 * Substitui o Mantine Card com estilo limpo
 */
import React from 'react';

export interface CardProps {
  children: React.ReactNode;
  className?: string;
  withBorder?: boolean;
  shadow?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  radius?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  p?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | number;
  style?: React.CSSProperties;
  onClick?: () => void;
}

const shadowClasses = {
  xs: 'shadow-sm',
  sm: 'shadow',
  md: 'shadow-md', 
  lg: 'shadow-lg',
  xl: 'shadow-xl'
};

const radiusClasses = {
  xs: 'rounded-sm',
  sm: 'rounded',
  md: 'rounded-md',
  lg: 'rounded-lg', 
  xl: 'rounded-xl'
};

const paddingClasses = {
  xs: 'p-2',   // 8px
  sm: 'p-3',   // 12px
  md: 'p-4',   // 16px
  lg: 'p-6',   // 24px
  xl: 'p-8'    // 32px
};

export const Card: React.FC<CardProps> = ({
  children,
  className = '',
  withBorder = false,
  shadow = 'sm',
  radius = 'md',
  p = 'md',
  style,
  onClick
}) => {
  const baseClasses = [
    'bg-white',
    shadowClasses[shadow],
    radiusClasses[radius],
    typeof p === 'string' ? paddingClasses[p] : `p-[${p}px]`
  ];

  if (withBorder) {
    baseClasses.push('border border-padaria-border-light');
  }

  if (onClick) {
    baseClasses.push('cursor-pointer hover:shadow-md transition-shadow');
  }

  const finalClasses = [...baseClasses, className].filter(Boolean).join(' ');

  return (
    <div className={finalClasses} style={style} onClick={onClick}>
      {children}
    </div>
  );
};