/**
 * Sistema de Logging Estruturado para Monitoramento Proativo
 * Permite que eu (Claude) monitore o sistema e aja automaticamente
 */

'use client';

import { useState, useCallback, useEffect } from 'react';
import { isDevelopment } from '@/utils/environment';

export interface DebugLog {
  id: string;
  timestamp: string;
  level: 'INFO' | 'WARN' | 'ERROR';
  category: 'AUTH' | 'API' | 'UI' | 'PERFORMANCE' | 'INTERACTION';
  event: string;
  message: string;
  context?: {
    user_input?: string;
    api_response?: any;
    time_taken?: number;
    stack_trace?: string;
    user_agent?: string;
    url?: string;
    element?: string;
    component?: string;
    [key: string]: any;
  };
  action_needed?: boolean;
  suggested_fix?: string;
}

// Store global para logs (para persistir entre re-renders)
let globalLogs: DebugLog[] = [];
let logListeners: Set<(logs: DebugLog[]) => void> = new Set();

// Função para notificar todos os listeners
const notifyListeners = () => {
  logListeners.forEach(listener => listener([...globalLogs]));
};

// Interface para eu (Claude) monitorar logs estruturados
const sendToClaudeMonitoring = (log: DebugLog) => {
  // Em desenvolvimento, exibir logs estruturados no console para meu monitoramento
  if (isDevelopment()) {
    console.group(`🤖 CLAUDE MONITORING - ${log.level} - ${log.category}`);
    console.log('📊 Structured Log:', log);
    console.log('🔍 Event:', log.event);
    console.log('💬 Message:', log.message);
    if (log.context) {
      console.log('📝 Context:', log.context);
    }
    if (log.action_needed) {
      console.warn('🚨 ACTION NEEDED:', log.suggested_fix);
    }
    console.groupEnd();

    // Enviar para backend/external monitoring (futuro)
    // fetch('/api/debug/claude-monitoring', { method: 'POST', body: JSON.stringify(log) });
  }
};

export const useDebugLogger = () => {
  const [logs, setLogs] = useState<DebugLog[]>(globalLogs);

  // Registrar listener para updates
  useEffect(() => {
    const updateLogs = (newLogs: DebugLog[]) => setLogs(newLogs);
    logListeners.add(updateLogs);

    return () => {
      logListeners.delete(updateLogs);
    };
  }, []);

  const log = useCallback((
    level: DebugLog['level'],
    category: DebugLog['category'],
    event: string,
    message: string,
    context?: DebugLog['context'],
    actionNeeded = false,
    suggestedFix?: string
  ) => {
    const newLog: DebugLog = {
      id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      timestamp: new Date().toISOString(),
      level,
      category,
      event,
      message,
      context: {
        ...context,
        user_agent: navigator.userAgent,
        url: window.location.pathname
      },
      action_needed: actionNeeded,
      suggested_fix: suggestedFix
    };

    // Adicionar ao store global
    globalLogs.push(newLog);

    // Manter apenas os últimos 50 logs para performance
    if (globalLogs.length > 50) {
      globalLogs = globalLogs.slice(-50);
    }

    // Enviar para meu monitoramento
    sendToClaudeMonitoring(newLog);

    // Notificar listeners
    notifyListeners();
  }, []);

  // Métodos de conveniência
  const logError = useCallback((
    category: DebugLog['category'],
    event: string,
    message: string,
    context?: DebugLog['context'],
    suggestedFix?: string
  ) => {
    log('ERROR', category, event, message, context, true, suggestedFix);
  }, [log]);

  const logWarning = useCallback((
    category: DebugLog['category'],
    event: string,
    message: string,
    context?: DebugLog['context']
  ) => {
    log('WARN', category, event, message, context);
  }, [log]);

  const logInfo = useCallback((
    category: DebugLog['category'],
    event: string,
    message: string,
    context?: DebugLog['context']
  ) => {
    log('INFO', category, event, message, context);
  }, [log]);

  const clearLogs = useCallback(() => {
    globalLogs = [];
    notifyListeners();
  }, []);

  // Função para eu analisar problemas críticos
  const getActionableIssues = useCallback(() => {
    return globalLogs.filter(log => log.action_needed);
  }, [logs]);

  // Análise de performance
  const getPerformanceIssues = useCallback(() => {
    return globalLogs.filter(log =>
      log.category === 'PERFORMANCE' ||
      (log.context?.time_taken && log.context.time_taken > 1000)
    );
  }, [logs]);

  return {
    logs,
    log,
    logError,
    logWarning,
    logInfo,
    clearLogs,
    getActionableIssues,
    getPerformanceIssues
  };
};

// Hook para interceptar e logar automaticamente
export const useAutoLogger = () => {
  const { logError, logWarning, logInfo } = useDebugLogger();

  useEffect(() => {
    // Interceptar erros JavaScript globais
    const handleError = (event: ErrorEvent) => {
      logError(
        'UI',
        'javascript_error',
        event.message,
        {
          stack_trace: event.error?.stack,
          filename: event.filename,
          lineno: event.lineno,
          colno: event.colno
        },
        'Check JavaScript syntax and imports'
      );
    };

    // Interceptar promises rejeitadas não capturadas
    const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
      logError(
        'API',
        'unhandled_promise_rejection',
        `Promise rejected: ${event.reason}`,
        {
          reason: event.reason,
          stack_trace: event.reason?.stack
        },
        'Add proper error handling to async operations'
      );
    };

    window.addEventListener('error', handleError);
    window.addEventListener('unhandledrejection', handleUnhandledRejection);

    return () => {
      window.removeEventListener('error', handleError);
      window.removeEventListener('unhandledrejection', handleUnhandledRejection);
    };
  }, [logError]);

  return { logError, logWarning, logInfo };
};