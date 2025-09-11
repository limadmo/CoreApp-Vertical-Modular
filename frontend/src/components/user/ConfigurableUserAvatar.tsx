/**
 * Avatar Premium com login identificador configurável
 * Sistema enterprise para identificação visual única de usuários
 */
import React from 'react';
import { Avatar, Tooltip, Text, Group } from '@mantine/core';

export interface ConfigurableUserAvatarProps {
  /** Nome completo do usuário */
  userName: string;
  /** Login identificador configurável (ex: JS01234A) */
  userLogin: string;
  /** Cargo/função do usuário */
  userRole: string;
  /** URL da foto do usuário (opcional) */
  photoUrl?: string;
  /** Status online/offline */
  isOnline?: boolean;
  /** Tamanho do avatar */
  size?: 'sm' | 'md' | 'lg' | 'xl';
}


/**
 * Extrai iniciais do nome completo
 * Primeira letra do primeiro nome + primeira letra do segundo nome
 */
const getInitials = (name: string): string => {
  const parts = name.trim().split(' ').filter(part => part.length > 0);
  if (parts.length === 1) {
    // Se só tem um nome, usa as duas primeiras letras
    return parts[0].substring(0, 2).toUpperCase();
  }
  // Primeira letra do primeiro nome + primeira letra do segundo nome
  return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
};

/**
 * Avatar simples e funcional para funcionários
 */
export const ConfigurableUserAvatar: React.FC<ConfigurableUserAvatarProps> = ({
  userName,
  userLogin,
  userRole,
  photoUrl,
  isOnline = true,
  size = 'md',
}) => {
  const initials = getInitials(userName);

  return (
    <Tooltip
      label={
        <div>
          <Text size="sm" fw={600} c="white">
            {userName}
          </Text>
          <Text size="xs" c="gray.3">
            Login: {userLogin}
          </Text>
          <Text size="xs" c="gray.3">
            Cargo: {userRole}
          </Text>
          <Text size="xs" c={isOnline ? 'green.3' : 'red.3'}>
            Status: {isOnline ? 'Online' : 'Offline'}
          </Text>
        </div>
      }
      position="bottom"
      withArrow
    >
      <div className="relative">
        <Avatar
          size={size}
          color="brand"
          src={photoUrl}
          style={{ cursor: 'pointer' }}
        >
          {!photoUrl && initials}
        </Avatar>
        
        {/* Indicador de status online/offline */}
        <div
          className="absolute -bottom-1 -right-1 w-3 h-3 rounded-full border-2 border-white"
          style={{
            backgroundColor: isOnline ? '#40c057' : '#fa5252',
            boxShadow: '0 2px 4px rgba(0,0,0,0.2)',
          }}
        />
      </div>
    </Tooltip>
  );
};