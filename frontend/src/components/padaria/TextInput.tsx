/**
 * Componente TextInput customizado para padaria
 * Substitui o Mantine TextInput com estilo limpo
 */
import React, { forwardRef } from 'react';

export interface TextInputProps {
  label?: string;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  className?: string;
  error?: string;
  disabled?: boolean;
  readOnly?: boolean;
  required?: boolean;
  type?: 'text' | 'email' | 'tel' | 'password' | 'number';
  maxLength?: number;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (event: React.FocusEvent<HTMLInputElement>) => void;
  onFocus?: (event: React.FocusEvent<HTMLInputElement>) => void;
  style?: React.CSSProperties;
  leftSection?: React.ReactNode;
  rightSection?: React.ReactNode;
}

export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(({
  label,
  placeholder,
  value,
  defaultValue,
  className = '',
  error,
  disabled = false,
  readOnly = false,
  required = false,
  type = 'text',
  maxLength,
  onChange,
  onBlur,
  onFocus,
  style,
  leftSection,
  rightSection
}, ref) => {
  const inputClasses = [
    'w-full',
    'h-9', // 36px - altura compacta
    'px-3 py-2',
    'text-padaria-primary',
    'bg-white',
    'border-2 border-padaria-border-light',
    'rounded-md',
    'text-sm',
    'font-medium',
    'transition-all duration-200',
    'focus:border-padaria-primary focus:ring-2 focus:ring-padaria-primary/20',
    'focus:outline-none',
    'placeholder:text-padaria-muted'
  ];

  if (error) {
    inputClasses.push('border-red-500 focus:border-red-500 focus:ring-red-500/20');
  }

  if (disabled) {
    inputClasses.push('bg-gray-100 cursor-not-allowed opacity-60');
  }

  if (readOnly) {
    inputClasses.push('bg-gray-50 cursor-default');
  }

  const finalInputClasses = [...inputClasses, className].filter(Boolean).join(' ');

  return (
    <div className="space-y-1" style={style}>
      {label && (
        <label className="block text-sm font-semibold text-padaria-primary">
          {label}
          {required && <span className="text-red-500 ml-1">*</span>}
        </label>
      )}
      
      <div className="relative">
        {leftSection && (
          <div className="absolute left-3 top-1/2 transform -translate-y-1/2 text-padaria-muted">
            {leftSection}
          </div>
        )}
        
        <input
          ref={ref}
          type={type}
          value={value}
          defaultValue={defaultValue}
          placeholder={placeholder}
          disabled={disabled}
          readOnly={readOnly}
          required={required}
          maxLength={maxLength}
          onChange={onChange}
          onBlur={onBlur}
          onFocus={onFocus}
          className={`${finalInputClasses} ${leftSection ? 'pl-10' : ''} ${rightSection ? 'pr-10' : ''}`}
        />
        
        {rightSection && (
          <div className="absolute right-3 top-1/2 transform -translate-y-1/2 text-padaria-muted">
            {rightSection}
          </div>
        )}
      </div>
      
      {error && (
        <p className="text-sm text-red-600 font-medium">{error}</p>
      )}
    </div>
  );
});