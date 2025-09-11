/**
 * Componente padronizado para itens de navegação
 * Design system padaria - Visual consistente para todos cartões
 */
import React from 'react';
import { NavLink, Text } from '@mantine/core';

export interface NavigationItemProps {
  /** Ícone do item */
  icon: React.ReactNode;
  /** Título principal */
  label: string;
  /** Descrição/subtítulo */
  description: string;
  /** Atalho de teclado (F1, F2, etc) */
  shortcut: string;
  /** Se o item está ativo/selecionado */
  isActive: boolean;
  /** Função executada ao clicar */
  onClick: () => void;
}

/**
 * Item de navegação padronizado para sidebar
 * Unifica visual entre módulos dinâmicos e sistema fixo
 */
export const NavigationItem: React.FC<NavigationItemProps> = ({
  icon,
  label,
  description,
  shortcut,
  isActive,
  onClick,
}) => {
  return (
    <NavLink
      href="#"
      label={label}
      // description removida para layout ultra-compacto
      leftSection={icon}
      rightSection={
        <div className="padaria-navigation-shortcut">
          {shortcut}
        </div>
      }
      active={isActive}
      onClick={(e) => {
        e.preventDefault();
        onClick();
      }}
      className="hover:bg-gray-50 rounded-md padaria-navigation-compact"
      styles={{
        root: {
          minHeight: '32px',
          padding: '6px 12px',
        },
        label: {
          fontSize: '12px',
          fontWeight: 500,
        },
        section: {
          marginRight: '8px',
        }
      }}
    />
  );
};