/**
 * Componente Select customizado para padaria
 * Substitui o Mantine Select com estilo limpo
 */
import React, { useState, useRef, useEffect } from 'react';

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps {
  label?: string;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  data: SelectOption[] | string[];
  className?: string;
  error?: string;
  disabled?: boolean;
  readOnly?: boolean;
  required?: boolean;
  onChange?: (value: string) => void;
  style?: React.CSSProperties;
}

export const Select: React.FC<SelectProps> = ({
  label,
  placeholder = 'Selecione...',
  value,
  defaultValue,
  data,
  className = '',
  error,
  disabled = false,
  readOnly = false,
  required = false,
  onChange,
  style
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedValue, setSelectedValue] = useState(value || defaultValue || '');
  const selectRef = useRef<HTMLDivElement>(null);

  // Normalizar dados
  const options: SelectOption[] = data.map(item => 
    typeof item === 'string' 
      ? { value: item, label: item }
      : item
  );

  const selectedOption = options.find(opt => opt.value === selectedValue);

  useEffect(() => {
    if (value !== undefined) {
      setSelectedValue(value);
    }
  }, [value]);

  const handleSelect = (optionValue: string) => {
    setSelectedValue(optionValue);
    setIsOpen(false);
    onChange?.(optionValue);
  };

  const selectClasses = [
    'relative w-full',
    'h-9', // 36px - altura compacta
    'px-3 py-2',
    'text-padaria-primary',
    'bg-white',
    'border-2 border-padaria-border-light',
    'rounded-md',
    'text-sm',
    'font-medium',
    'cursor-pointer',
    'transition-all duration-200',
    'focus:border-padaria-primary focus:ring-2 focus:ring-padaria-primary/20',
    'focus:outline-none',
    'flex items-center justify-between'
  ];

  if (error) {
    selectClasses.push('border-red-500');
  }

  if (disabled) {
    selectClasses.push('bg-gray-100 cursor-not-allowed opacity-60');
  }

  const finalSelectClasses = [...selectClasses, className].filter(Boolean).join(' ');

  return (
    <div className="space-y-1" style={style} ref={selectRef}>
      {label && (
        <label className="block text-sm font-semibold text-padaria-primary">
          {label}
          {required && <span className="text-red-500 ml-1">*</span>}
        </label>
      )}
      
      <div className="relative">
        <button
          type="button"
          className={finalSelectClasses}
          onClick={() => !disabled && setIsOpen(!isOpen)}
          disabled={disabled}
        >
          <span className={selectedOption ? 'text-padaria-primary' : 'text-padaria-muted'}>
            {selectedOption?.label || placeholder}
          </span>
          <span className={`transform transition-transform ${isOpen ? 'rotate-180' : ''}`}>
            ▼
          </span>
        </button>
        
        {isOpen && (
          <div className="absolute z-50 w-full mt-1 bg-white border-2 border-padaria-border-light rounded-md shadow-lg max-h-60 overflow-auto">
            {options.map((option) => (
              <button
                key={option.value}
                type="button"
                className={`w-full px-3 py-2 text-left text-sm hover:bg-padaria-bg-secondary transition-colors ${
                  option.value === selectedValue 
                    ? 'bg-padaria-bg-secondary text-padaria-primary font-semibold' 
                    : 'text-padaria-primary'
                }`}
                onClick={() => handleSelect(option.value)}
              >
                {option.label}
              </button>
            ))}
          </div>
        )}
      </div>
      
      {error && (
        <p className="text-sm text-red-600 font-medium">{error}</p>
      )}
    </div>
  );
};