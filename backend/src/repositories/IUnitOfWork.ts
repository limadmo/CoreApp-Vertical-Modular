/**
 * Interface Unit of Work - Padrão UoW Avançado
 * Centralização de transações e coordenação de repositories
 */

import { PrismaClient, PrismaTransactionClient } from '@prisma/client';
import { IBaseRepository } from './IBaseRepository';

// Tipo para cliente Prisma em transação
export type TransactionClient = Omit<
  PrismaClient,
  '$connect' | '$disconnect' | '$on' | '$transaction' | '$use'
>;

/**
 * Interface principal do Unit of Work
 * Coordena todas as operações de dados em uma única transação
 */
export interface IUnitOfWork {
  // ====================================
  // REPOSITORY ACCESS
  // ====================================

  /**
   * Acesso aos repositories específicos
   * Lazy loading - criados apenas quando necessário
   */
  readonly usuarios: IBaseRepository<any, string>;
  readonly superAdmins: IBaseRepository<any, string>;
  readonly tenants: IBaseRepository<any, string>;
  readonly roles: IBaseRepository<any, string>;
  readonly permissoes: IBaseRepository<any, string>;
  readonly rolePermissoes: IBaseRepository<any, string>;
  readonly clientes: IBaseRepository<any, string>;
  readonly produtos: IBaseRepository<any, string>;
  readonly estoques: IBaseRepository<any, string>;
  readonly movimentacoesEstoque: IBaseRepository<any, string>;
  readonly vendas: IBaseRepository<any, string>;
  readonly itensVenda: IBaseRepository<any, string>;

  // ====================================
  // TRANSACTION MANAGEMENT
  // ====================================

  /**
   * Inicia nova transação
   * @returns Promise que resolve quando transação está pronta
   */
  beginTransaction(): Promise<void>;

  /**
   * Confirma todas as mudanças na transação atual
   * @returns Promise que resolve quando commit é concluído
   */
  commit(): Promise<void>;

  /**
   * Desfaz todas as mudanças na transação atual
   * @returns Promise que resolve quando rollback é concluído
   */
  rollback(): Promise<void>;

  /**
   * Executa operações dentro de uma transação
   * Commit automático em caso de sucesso, rollback em caso de erro
   * @param operation - Operação a ser executada
   * @returns Resultado da operação
   */
  executeInTransaction<TResult>(
    operation: (uow: IUnitOfWork) => Promise<TResult>
  ): Promise<TResult>;

  /**
   * Executa múltiplas operações em batch dentro de uma transação
   * @param operations - Array de operações
   * @returns Array com resultados de cada operação
   */
  executeBatch<TResult>(
    operations: ((uow: IUnitOfWork) => Promise<TResult>)[]
  ): Promise<TResult[]>;

  // ====================================
  // STATE MANAGEMENT
  // ====================================

  /**
   * Verifica se está dentro de uma transação ativa
   */
  readonly isInTransaction: boolean;

  /**
   * Obtém cliente Prisma atual (normal ou em transação)
   */
  readonly client: PrismaClient | TransactionClient;

  /**
   * Obtém estatísticas da transação atual
   */
  getTransactionStats(): {
    isActive: boolean;
    startTime?: Date;
    operationCount: number;
    repositories: string[];
  };

  // ====================================
  // ADVANCED FEATURES
  // ====================================

  /**
   * Cria savepoint na transação atual
   * @param name - Nome do savepoint
   */
  createSavepoint(name: string): Promise<void>;

  /**
   * Volta para um savepoint específico
   * @param name - Nome do savepoint
   */
  rollbackToSavepoint(name: string): Promise<void>;

  /**
   * Remove savepoint
   * @param name - Nome do savepoint
   */
  releaseSavepoint(name: string): Promise<void>;

  /**
   * Executa query RAW dentro da transação atual
   * @param query - Query SQL
   * @param parameters - Parâmetros da query
   */
  executeRawQuery<TResult = any>(query: string, parameters?: any[]): Promise<TResult>;

  /**
   * Executa stored procedure ou função
   * @param procedureName - Nome da procedure
   * @param parameters - Parâmetros da procedure
   */
  executeProcedure<TResult = any>(procedureName: string, parameters?: any[]): Promise<TResult>;

  // ====================================
  // AUDIT & LOGGING
  // ====================================

  /**
   * Registra ação de auditoria
   * @param entityName - Nome da entidade
   * @param entityId - ID da entidade
   * @param action - Ação realizada
   * @param changes - Mudanças realizadas
   * @param userId - ID do usuário
   * @param tenantId - ID do tenant
   */
  logAudit(
    entityName: string,
    entityId: string,
    action: 'CREATE' | 'UPDATE' | 'DELETE' | 'RESTORE',
    changes: any,
    userId: string,
    tenantId: string
  ): Promise<void>;

  /**
   * Obtém log de todas as operações da transação atual
   */
  getOperationLog(): {
    repository: string;
    method: string;
    parameters: any[];
    timestamp: Date;
  }[];

  // ====================================
  // CACHE INTEGRATION
  // ====================================

  /**
   * Invalida cache relacionado às operações da transação
   * @param patterns - Padrões de chave para invalidar
   */
  invalidateCache(patterns: string[]): Promise<void>;

  /**
   * Registra operação para invalidação de cache após commit
   * @param pattern - Padrão de chave para invalidar
   */
  scheduleInvalidateCache(pattern: string): void;

  // ====================================
  // EVENT SOURCING PREPARATION
  // ====================================

  /**
   * Registra evento de domínio
   * @param eventType - Tipo do evento
   * @param aggregateId - ID do agregado
   * @param eventData - Dados do evento
   * @param tenantId - ID do tenant
   */
  publishDomainEvent(
    eventType: string,
    aggregateId: string,
    eventData: any,
    tenantId: string
  ): Promise<void>;

  /**
   * Obtém eventos pendentes para publicação
   */
  getPendingEvents(): {
    id: string;
    eventType: string;
    aggregateId: string;
    eventData: any;
    tenantId: string;
    timestamp: Date;
  }[];

  /**
   * Publica todos os eventos pendentes após commit
   */
  publishPendingEvents(): Promise<void>;

  // ====================================
  // RESOURCE MANAGEMENT
  // ====================================

  /**
   * Libera todos os recursos e fecha conexões
   */
  dispose(): Promise<void>;

  /**
   * Reseta o estado do UoW para reutilização
   */
  reset(): void;
}