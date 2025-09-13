/**
 * Modal de Cliente - Mantine 7 + Acessibilidade WCAG AAA
 * Modal responsivo para criar/editar clientes
 */
'use client';

import { Modal, ScrollArea } from '@mantine/core';
import { Cliente } from '@/services/clienteService';
import ClienteForm from './ClienteForm';

interface ClienteModalProps {
  opened: boolean;
  onClose: () => void;
  cliente?: Cliente | null;
  title?: string;
}

/**
 * Modal para operações CRUD de cliente
 */
export function ClienteModal({ opened, onClose, cliente, title }: ClienteModalProps) {
  const isEditing = !!cliente;
  const modalTitle = title || (isEditing ? 'Editar Cliente' : 'Novo Cliente');

  const handleSuccess = () => {
    onClose();
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={modalTitle}
      size="md"
      centered
      overlayProps={{
        opacity: 0.55,
        blur: 3,
      }}
      styles={{
        title: {
          fontSize: '1.25rem',
          fontWeight: 600,
          color: 'var(--mantine-color-dark-7)',
        },
        header: {
          backgroundColor: 'var(--mantine-color-gray-0)',
          borderBottom: '1px solid var(--mantine-color-gray-3)',
          padding: '1rem 1.5rem',
        },
        body: {
          padding: '1.5rem',
        },
      }}
      // Configurações de acessibilidade
      trapFocus
      returnFocus
      closeOnClickOutside={false}
      closeOnEscape={true}
      withCloseButton
      // ARIA
      aria-labelledby="cliente-modal-title"
      aria-describedby="cliente-modal-description"
    >
      <ScrollArea style={{ maxHeight: '70vh' }}>
        {/* Formulário dentro do modal */}
        <ClienteForm
          cliente={cliente}
          onSuccess={handleSuccess}
          onCancel={handleCancel}
        />
      </ScrollArea>
    </Modal>
  );
}

export default ClienteModal;