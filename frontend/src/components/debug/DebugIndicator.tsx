/**
 * Sistema de Debug Visual Inteligente
 * - Verde: Sistema perfeito (apenas ícone discreto)
 * - Amarelo: Avisos (expansão automática)
 * - Vermelho: Erros críticos (painel completo)
 */

'use client';

import React, { useState, useEffect } from 'react';
import { useDebugLogger } from '@/hooks/useDebugLogger';

interface DebugLog {
  id: string;
  timestamp: string;
  level: 'INFO' | 'WARN' | 'ERROR';
  category: 'AUTH' | 'API' | 'UI' | 'PERFORMANCE';
  message: string;
  context?: any;
}

export function DebugIndicator() {
  const { logs, clearLogs } = useDebugLogger();
  const [isExpanded, setIsExpanded] = useState(false);
  const [currentStatus, setCurrentStatus] = useState<'perfect' | 'warning' | 'error'>('perfect');

  // Determinar status baseado nos logs
  useEffect(() => {
    if (logs.length === 0) {
      setCurrentStatus('perfect');
      setIsExpanded(false);
      return;
    }

    const hasErrors = logs.some(log => log.level === 'ERROR');
    const hasWarnings = logs.some(log => log.level === 'WARN');

    if (hasErrors) {
      setCurrentStatus('error');
      setIsExpanded(true);
    } else if (hasWarnings) {
      setCurrentStatus('warning');
      setIsExpanded(true);
    } else {
      setCurrentStatus('perfect');
      setIsExpanded(false);
    }
  }, [logs]);

  // Auto-collapse warnings after 5 seconds if resolved
  useEffect(() => {
    if (currentStatus === 'warning') {
      const timer = setTimeout(() => {
        const recentWarnings = logs.filter(log =>
          log.level === 'WARN' &&
          Date.now() - new Date(log.timestamp).getTime() < 5000
        );

        if (recentWarnings.length === 0) {
          setIsExpanded(false);
          setCurrentStatus('perfect');
        }
      }, 5000);

      return () => clearTimeout(timer);
    }
  }, [currentStatus, logs]);

  const getStatusIcon = () => {
    switch (currentStatus) {
      case 'perfect': return '🟢';
      case 'warning': return '🟡';
      case 'error': return '🔴';
      default: return '🟢';
    }
  };

  const getStatusColor = () => {
    switch (currentStatus) {
      case 'perfect': return '#4CAF50';
      case 'warning': return '#FF9800';
      case 'error': return '#f44336';
      default: return '#4CAF50';
    }
  };

  if (!isExpanded && currentStatus === 'perfect') {
    // Modo discreto: apenas ícone verde pequeno
    return (
      <div
        style={{
          position: 'fixed',
          bottom: '20px',
          right: '20px',
          zIndex: 10000,
          fontSize: '14px',
          opacity: 0.7,
          cursor: 'pointer',
          transition: 'opacity 0.2s'
        }}
        onClick={() => setIsExpanded(true)}
        title="Debug Status - System OK"
      >
        {getStatusIcon()}
      </div>
    );
  }

  // Painel expandido para avisos e erros
  return (
    <div
      style={{
        position: 'fixed',
        bottom: '20px',
        right: '20px',
        zIndex: 10000,
        backgroundColor: 'white',
        border: `2px solid ${getStatusColor()}`,
        borderRadius: '8px',
        boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
        minWidth: '300px',
        maxWidth: '400px',
        maxHeight: '300px',
        overflowY: 'auto',
        fontSize: '13px'
      }}
    >
      {/* Header */}
      <div
        style={{
          padding: '8px 12px',
          backgroundColor: getStatusColor(),
          color: 'white',
          fontWeight: 'bold',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <span>{getStatusIcon()} Debug Status</span>
        <button
          onClick={() => setIsExpanded(false)}
          style={{
            background: 'none',
            border: 'none',
            color: 'white',
            cursor: 'pointer',
            fontSize: '16px'
          }}
        >
          ×
        </button>
      </div>

      {/* Logs */}
      <div style={{ padding: '8px' }}>
        {logs.slice(-5).map((log) => (
          <div
            key={log.id}
            style={{
              marginBottom: '6px',
              padding: '4px 6px',
              backgroundColor: log.level === 'ERROR' ? '#ffebee' : '#fff3e0',
              borderRadius: '4px',
              borderLeft: `3px solid ${log.level === 'ERROR' ? '#f44336' : '#ff9800'}`
            }}
          >
            <div style={{ fontWeight: 'bold', fontSize: '11px', color: '#666' }}>
              {log.timestamp.split('T')[1]?.split('.')[0]} - {log.category}
            </div>
            <div style={{ marginTop: '2px' }}>
              {log.level === 'ERROR' ? '🔴' : '🟡'} {log.message}
            </div>
          </div>
        ))}

        {logs.length === 0 && (
          <div style={{ textAlign: 'center', color: '#666', padding: '20px' }}>
            🟢 Sistema funcionando perfeitamente
          </div>
        )}
      </div>

      {/* Actions */}
      {logs.length > 0 && (
        <div
          style={{
            padding: '8px 12px',
            borderTop: '1px solid #eee',
            display: 'flex',
            gap: '8px'
          }}
        >
          <button
            onClick={clearLogs}
            style={{
              padding: '4px 8px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              background: 'white',
              cursor: 'pointer',
              fontSize: '11px'
            }}
          >
            Clear
          </button>
          <button
            onClick={() => {
              const logData = JSON.stringify(logs, null, 2);
              navigator.clipboard.writeText(logData);
            }}
            style={{
              padding: '4px 8px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              background: 'white',
              cursor: 'pointer',
              fontSize: '11px'
            }}
          >
            Copy
          </button>
        </div>
      )}
    </div>
  );
}