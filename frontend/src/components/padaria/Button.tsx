/**
 * Componente Button customizado para padaria
 * Substitui o Mantine Button com estilo temático
 */
import React from 'react';

export interface ButtonProps {
  children: React.ReactNode;
  className?: string;
  variant?: 'filled' | 'outline' | 'subtle' | 'light';
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  color?: string;
  disabled?: boolean;
  loading?: boolean;
  leftSection?: React.ReactNode;
  rightSection?: React.ReactNode;
  fullWidth?: boolean;
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
  style?: React.CSSProperties;
}

const sizeClasses = {
  xs: 'px-2 py-1 text-xs',
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-4 py-2 text-base',
  lg: 'px-6 py-3 text-lg',
  xl: 'px-8 py-4 text-xl'
};

const variantClasses = {
  filled: 'bg-padaria-primary text-white hover:bg-padaria-primary-hover',
  outline: 'border-2 border-padaria-primary text-padaria-primary hover:bg-padaria-primary hover:text-white',
  subtle: 'text-padaria-primary hover:bg-padaria-bg-secondary',
  light: 'bg-padaria-bg-secondary text-padaria-primary hover:bg-padaria-bg-accent'
};

export const Button: React.FC<ButtonProps> = ({
  children,
  className = '',
  variant = 'filled',
  size = 'md',
  color,
  disabled = false,
  loading = false,
  leftSection,
  rightSection,
  fullWidth = false,
  onClick,
  type = 'button',
  style
}) => {
  const baseClasses = [
    'inline-flex items-center justify-center',
    'rounded-md font-medium',
    'transition-all duration-200',
    'focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-padaria-primary',
    'disabled:opacity-50 disabled:cursor-not-allowed',
    sizeClasses[size],
    variantClasses[variant]
  ];

  if (fullWidth) {
    baseClasses.push('w-full');
  }

  if (disabled || loading) {
    baseClasses.push('opacity-50 cursor-not-allowed');
  }

  const finalClasses = [...baseClasses, className].filter(Boolean).join(' ');

  return (
    <button
      className={finalClasses}
      disabled={disabled || loading}
      onClick={onClick}
      type={type}
      style={style}
    >
      {leftSection && (
        <span className="mr-2 flex items-center">
          {leftSection}
        </span>
      )}
      
      {loading ? (
        <span className="animate-spin mr-2">⟳</span>
      ) : null}
      
      <span>{children}</span>
      
      {rightSection && (
        <span className="ml-2 flex items-center">
          {rightSection}
        </span>
      )}
    </button>
  );
};