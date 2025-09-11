/**
 * Componente Stack customizado para padaria
 * Substitui o Mantine Stack com flexbox vertical
 */
import React from 'react';

export interface StackProps {
  children: React.ReactNode;
  className?: string;
  gap?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | number;
  align?: 'start' | 'center' | 'end' | 'stretch';
  justify?: 'start' | 'center' | 'end' | 'space-between' | 'space-around';
  style?: React.CSSProperties;
}

const gapClasses = {
  xs: 'space-y-1',  // 4px
  sm: 'space-y-2',  // 8px
  md: 'space-y-3',  // 12px
  lg: 'space-y-4',  // 16px
  xl: 'space-y-6'   // 24px
};

const alignClasses = {
  start: 'items-start',
  center: 'items-center',
  end: 'items-end',
  stretch: 'items-stretch'
};

const justifyClasses = {
  start: 'justify-start',
  center: 'justify-center',
  end: 'justify-end',
  'space-between': 'justify-between',
  'space-around': 'justify-around'
};

export const Stack: React.FC<StackProps> = ({
  children,
  className = '',
  gap = 'sm',
  align = 'stretch',
  justify = 'start',
  style
}) => {
  const baseClasses = [
    'flex flex-col',
    typeof gap === 'string' ? gapClasses[gap] : `space-y-[${gap}px]`,
    alignClasses[align],
    justifyClasses[justify]
  ];

  const finalClasses = [...baseClasses, className].filter(Boolean).join(' ');

  return (
    <div className={finalClasses} style={style}>
      {children}
    </div>
  );
};