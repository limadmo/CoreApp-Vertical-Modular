/**
 * Componente Group customizado para padaria  
 * Substitui o Mantine Group com flexbox limpo
 */
import React from 'react';

export interface GroupProps {
  children: React.ReactNode;
  className?: string;
  justify?: 'start' | 'center' | 'end' | 'space-between' | 'space-around';
  align?: 'start' | 'center' | 'end' | 'stretch';
  gap?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | number;
  wrap?: 'wrap' | 'nowrap';
  style?: React.CSSProperties;
}

const justifyClasses = {
  start: 'justify-start',
  center: 'justify-center',
  end: 'justify-end',
  'space-between': 'justify-between',
  'space-around': 'justify-around'
};

const alignClasses = {
  start: 'items-start',
  center: 'items-center', 
  end: 'items-end',
  stretch: 'items-stretch'
};

const gapClasses = {
  xs: 'gap-1',  // 4px
  sm: 'gap-2',  // 8px
  md: 'gap-3',  // 12px
  lg: 'gap-4',  // 16px
  xl: 'gap-6'   // 24px
};

export const Group: React.FC<GroupProps> = ({
  children,
  className = '',
  justify = 'start',
  align = 'center',
  gap = 'sm',
  wrap = 'nowrap',
  style
}) => {
  const baseClasses = [
    'flex',
    justifyClasses[justify],
    alignClasses[align],
    typeof gap === 'string' ? gapClasses[gap] : `gap-[${gap}px]`,
    wrap === 'wrap' ? 'flex-wrap' : 'flex-nowrap'
  ];

  const finalClasses = [...baseClasses, className].filter(Boolean).join(' ');

  return (
    <div className={finalClasses} style={style}>
      {children}
    </div>
  );
};